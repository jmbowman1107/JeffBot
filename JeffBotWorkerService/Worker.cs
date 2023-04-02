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

        #region StreamerSettings
        public Dictionary<string, (StreamerSettings StreamerSettings, JeffBot.JeffBot JeffBot)> StreamerSettings = new();
        #endregion

        #region Constructor
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
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
            JeffBot.GlobalSettingsSingleton.Initialize(globalSettings);
            // TODO: Segment this if we ever need to have a lot of streamers..
            var streamerSettings = JeffBot.AwsUtilities.DynamoDb.DbContext.FromScanAsync<StreamerSettings>(new ScanOperationConfig());
            foreach (var streamer in await streamerSettings.GetRemainingAsync(stoppingToken))
            {
                _logger.LogInformation($"Starting bot {streamer.StreamerBotName} for streamer {streamer.StreamerName}");
                StreamerSettings[streamer.StreamerId] = (streamer, new JeffBot.JeffBot(streamer));
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
                    await LoadDynamoDbStreamShards(stoppingToken, shardIterators, dynamoDbStreamsClient, stream);
                    await CheckForUpdatesAndUpdateStreamers(stoppingToken, shardIterators, dynamoDbStreamsClient, JeffBot.AwsUtilities.DynamoDb.DbContext);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
        #endregion

        #region LoadDynamoDbStreamShards
        private static async Task LoadDynamoDbStreamShards(CancellationToken stoppingToken, Dictionary<string, string> shardIterators, AmazonDynamoDBStreamsClient dynamoDbStreamsClient, StreamSummary stream)
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
        #region CheckForUpdatesAndUpdateStreamers
        private async Task CheckForUpdatesAndUpdateStreamers(CancellationToken stoppingToken, Dictionary<string, string> shardIterators, AmazonDynamoDBStreamsClient dynamoDbStreamsClient, DynamoDBContext dbContext)
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
                    // TODO: For now just hardcode this, need to do this in a better way, and probably make it a property in some way..
                    // Perhaps look at changes between the settings, if certain settings change, need to reboot, if not, it's fine
                    _logger.LogInformation($"Updated setting for streamer {newStreamerSettings.StreamerName}");

                    //var differencesInSettings = newStreamerSettings.DetailedCompare(StreamerSettings[streamerThatWasUpdated].StreamerSettings);
                    //foreach (var difference in differencesInSettings)
                    //{
                    //    switch (difference.Prop)
                    //    {

                    //    }
                    //}

                    if (StreamerSettings[streamerThatWasUpdated].StreamerSettings.SpotifyRefreshToken != newStreamerSettings.SpotifyRefreshToken)
                    {
                        StreamerSettings[streamerThatWasUpdated].StreamerSettings.SpotifyRefreshToken = newStreamerSettings.SpotifyRefreshToken;
                        continue;
                    }
                    StreamerSettings[streamerThatWasUpdated].JeffBot.ShutdownBotForStreamer();
                    StreamerSettings[streamerThatWasUpdated] = (newStreamerSettings, new JeffBot.JeffBot(newStreamerSettings));
                }
            }
        }
        #endregion
    }
}