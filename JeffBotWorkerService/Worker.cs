#if DEBUG
using Amazon.Runtime.CredentialManagement;
#endif
using Amazon.DynamoDBv2;
using JeffBot;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;

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
            using var client = new AmazonDynamoDBClient(awsCredentials);
            #else
            using var client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.APEast1 });
            #endif

            using var dbContext = new DynamoDBContext(client);
            // TODO: Segment this if we ever need to have a lot of streamers..
            var streamerSettings = dbContext.FromScanAsync<StreamerSettings>(new ScanOperationConfig());
            foreach (var streamer in await streamerSettings.GetRemainingAsync(stoppingToken))
            {
                _logger.LogInformation($"Starting bot {streamer.StreamerBotName} for streamer {streamer.StreamerName}");
                StreamerSettings[streamer.StreamerId] = (streamer, new JeffBot.JeffBot(streamer));
            }

            // For watching for changes to bot settings.
            var shardIterators = new Dictionary<string, string>();
            var dynamoDbStreamsClient = new AmazonDynamoDBStreamsClient(awsCredentials);
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
                    await CheckForUpdatesAndUpdateStreamers(stoppingToken, shardIterators, dynamoDbStreamsClient, dbContext);
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
                // TODO: Technically shards can show up whenever.. make this more dynamic.
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
                shardIterators[shardIterator.Key] = updates.NextShardIterator;
                foreach (var update in updates.Records)
                {
                    var streamerThatWasUpdated = update.Dynamodb.Keys["StreamerId"].S;
                    var newStreamerSettings =
                        await dbContext.LoadAsync<StreamerSettings>(streamerThatWasUpdated, stoppingToken);

                    StreamerSettings[streamerThatWasUpdated].JeffBot.ShutdownBotForStreamer();
                    StreamerSettings[streamerThatWasUpdated] = (newStreamerSettings, new JeffBot.JeffBot(newStreamerSettings));
                    Console.WriteLine(JsonConvert.SerializeObject(newStreamerSettings));
                }
            }
        }
        #endregion

        #region PopulateOrUpdateStreamerSettings
        /// <summary>
        /// Will update DynamoDB with settings set from in the code, just placing this code here for now, it probably will got elsewhere at somepoint
        /// </summary>
        /// <returns></returns>
        private async Task PopulateOrUpdateStreamerSettings(CancellationToken stoppingToken)
        {
            #if DEBUG
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials("jeff-personal", out var awsCredentials))
            {
                _logger.LogCritical("Could not find AWS Credentials");
                throw new ArgumentException("No AWS credential profile called 'jeff-personal' was found");
            }
            using var client = new AmazonDynamoDBClient(awsCredentials);
            #else
            using var client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.APEast1 });
            #endif

            using var dbContext = new DynamoDBContext(client);

            foreach (var streamer in StreamerSettings)
            {
                await dbContext.SaveAsync(streamer.Value.StreamerSettings, stoppingToken);
            }
        }
        #endregion
    }
}