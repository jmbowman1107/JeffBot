#if DEBUG
using Amazon.Runtime.CredentialManagement;
#endif
#if RELEASE
using Amazon;
#endif
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using JeffBot;

namespace JeffBotWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ILoggerFactory _loggerFactory;

        #region StreamerSettings
        public Dictionary<string, (StreamerSettings StreamerSettings, JeffBot.JeffBot JeffBot)> StreamerSettings = new();
        #endregion

        #region Constructor
        public Worker(ILogger<Worker> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }
        #endregion

        #region ExecuteAsync - Override
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            #if DEBUG
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials("jeff-personal", out var awsCredentials))
            {
                _logger.LogCritical("Could not find AWS Credentials");
                throw new ArgumentException("No AWS credential profile called 'jeff-personal' was found");
            }
            using var dynamoDbStreamsClient = new AmazonDynamoDBStreamsClient(awsCredentials);
            #else
            using var dynamoDbStreamsClient = new AmazonDynamoDBStreamsClient(new AmazonDynamoDBStreamsConfig{ RegionEndpoint = RegionEndpoint.USEast1 });
            #endif

            var globalSettings = await JeffBot.AwsUtilities.DynamoDb.GetGlobalSettings(stoppingToken);

            Singleton<GlobalSettings>.Initialize(globalSettings);
            // TODO: Segment this if we ever need to have a lot of streamers..
            var streamerSettings = JeffBot.AwsUtilities.DynamoDb.DbContext.FromScanAsync<StreamerSettings>(new ScanOperationConfig());
            foreach (var streamer in await streamerSettings.GetRemainingAsync(stoppingToken))
            {
                if (!streamer.IsActive)
                {
                    _logger.LogInformation($"{streamer.StreamerName}'s bot is set not to be active, skipping..");
                    continue;
                }
                _logger.LogInformation($"Starting bot {streamer.StreamerBotName} for streamer {streamer.StreamerName}");
                StreamerSettings[streamer.StreamerId] = (streamer, new JeffBot.JeffBot(streamer, _loggerFactory.CreateLogger<JeffBot.JeffBot>()));
            }

            // For watching for changes to bot settings.
            var shardIterators = new Dictionary<string, string>();
            var streams = await dynamoDbStreamsClient.ListStreamsAsync(stoppingToken);
            var stream = streams.Streams.FirstOrDefault(a => a.TableName == "JeffBotStreamerSettings");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Watch for settings changes from DynamoDb
                if (stream == null)
                {
                    _logger.LogCritical("Could not find stream for DynamoDB Table 'JeffBotStreamerSettings'");
                }
                else
                {
                    // TODO: When this scales beyond need of a single container, can we utilize a combination of an update (date time?) and System.Environment.MachineName to determine if a bot should be added?
                    await LoadDynamoDbStreamShards(shardIterators, dynamoDbStreamsClient, stream, stoppingToken);
                    await CheckForStreamerUpdatesAndUpdateStreamerSettings(shardIterators, dynamoDbStreamsClient, JeffBot.AwsUtilities.DynamoDb.DbContext, stoppingToken);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
        #endregion

        #region LoadDynamoDbStreamShards
        private static async Task LoadDynamoDbStreamShards(Dictionary<string, string> shardIterators, AmazonDynamoDBStreamsClient dynamoDbStreamsClient, StreamSummary stream, CancellationToken stoppingToken = default)
        {
            if (shardIterators.Count == 0)
            {
                var streamInfo = await dynamoDbStreamsClient.DescribeStreamAsync(stream.StreamArn, stoppingToken);
                foreach (var shard in streamInfo.StreamDescription.Shards)
                {
                    var iterator = await dynamoDbStreamsClient.GetShardIteratorAsync(new GetShardIteratorRequest
                    {
                        StreamArn = streamInfo.StreamDescription.StreamArn,
                        ShardId = shard.ShardId,
                        ShardIteratorType = ShardIteratorType.LATEST
                    }, stoppingToken);
                    shardIterators[shard.ShardId] = iterator.ShardIterator;
                }
            }
        }
        #endregion
        #region CheckForStreamerUpdatesAndUpdateStreamerSettings
        private async Task CheckForStreamerUpdatesAndUpdateStreamerSettings(Dictionary<string, string> shardIterators, AmazonDynamoDBStreamsClient dynamoDbStreamsClient, DynamoDBContext dbContext, CancellationToken stoppingToken = default)
        {
            foreach (var shardIterator in shardIterators)
            {
                var updates = await dynamoDbStreamsClient.GetRecordsAsync(shardIterator.Value, stoppingToken);
                if (string.IsNullOrWhiteSpace(updates.NextShardIterator))
                {
                    shardIterators.Remove(shardIterator.Key);
                    continue;
                }

                shardIterators[shardIterator.Key] = updates.NextShardIterator;
                foreach (var update in updates.Records)
                {
                    var streamerThatWasUpdated = update.Dynamodb.Keys["StreamerId"].S;
                    var newStreamerSettings = await dbContext.LoadAsync<StreamerSettings>(streamerThatWasUpdated, stoppingToken);

                    _logger.LogInformation($"Updated setting for streamer {newStreamerSettings.StreamerName}");
                    var jeffBotLogger = _loggerFactory.CreateLogger<JeffBot.JeffBot>();
                    if (StreamerSettings.ContainsKey(streamerThatWasUpdated))
                    {
                        if (ShouldRestartBot(StreamerSettings[streamerThatWasUpdated].StreamerSettings, newStreamerSettings))
                        {
                            StreamerSettings[streamerThatWasUpdated].JeffBot.ShutdownBotForStreamer();
                            if (newStreamerSettings.IsActive)
                            {
                                StreamerSettings[streamerThatWasUpdated] = (newStreamerSettings, new JeffBot.JeffBot(newStreamerSettings, jeffBotLogger));
                            }
                        }
                        else
                        {
                            // TODO: How to only update bot command settings, and not instantiate a new instance of a command? Or maybe just say fuck it?
                            StreamerSettings[streamerThatWasUpdated] = (newStreamerSettings, StreamerSettings[streamerThatWasUpdated].JeffBot);
                            StreamerSettings[streamerThatWasUpdated].JeffBot.StreamerSettings = newStreamerSettings;
                            StreamerSettings[streamerThatWasUpdated].JeffBot.BotCommands = new List<IBotCommand>();
                            StreamerSettings[streamerThatWasUpdated].JeffBot.InitializeBotCommands();
                        }
                    }
                    else
                    {
                        // TODO: When scaling work is done, don't automatically create it, determine if should be doing so.
                        if (newStreamerSettings.IsActive)
                        {
                            StreamerSettings[streamerThatWasUpdated] = (newStreamerSettings, new JeffBot.JeffBot(newStreamerSettings, jeffBotLogger));
                        }
                    }
                }
            }
        }
        #endregion
        #region ShouldRestartBot
        private bool ShouldRestartBot(StreamerSettings oldSettings, StreamerSettings newSettings)
        {
            return oldSettings.HaveRebootRequiredPropertiesChanged(newSettings) || HaveNewBotFeatures(oldSettings.BotFeatures, newSettings.BotFeatures);
        }
        #endregion
        #region HaveNewBotFeatures
        private bool HaveNewBotFeatures(List<BotCommandSettings> oldFeatures, List<BotCommandSettings> newFeatures)
        {
            var oldFeaturesDictionary = oldFeatures.ToDictionary(f => f.Id);
            var newFeaturesDictionary = newFeatures.ToDictionary(f => f.Id);

            var allFeatureNames = oldFeaturesDictionary.Keys.Concat(newFeaturesDictionary.Keys).Distinct();

            foreach (var featureName in allFeatureNames)
            {
                if (oldFeaturesDictionary.TryGetValue(featureName, out var oldFeature)
                    && newFeaturesDictionary.TryGetValue(featureName, out var newFeature))
                {
                    if (oldFeature.HaveRebootRequiredPropertiesChanged(newFeature)) return true;
                }
                else
                {
                    // If the feature is present in only one of the dictionaries, it's either added or removed
                    return true;
                }
            }
            return false;
        } 
        #endregion
    }
}