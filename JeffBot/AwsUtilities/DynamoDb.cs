#if DEBUG
using System;
using Amazon.Runtime.CredentialManagement;
#endif
using System.Threading;
using System.Threading.Tasks;
#if RELEASE
using Amazon;
#endif
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;


namespace JeffBot.AwsUtilities
{
    public static class DynamoDb
    {
#region PopulateOrUpdateStreamerSettings
        public static async Task PopulateOrUpdateStreamerSettings(StreamerSettings streamerSettings, CancellationToken stoppingToken = default)
        {
#if DEBUG
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials("jeff-personal", out var awsCredentials))
            {
                throw new ArgumentException("No AWS credential profile called 'jeff-personal' was found");
            }
            using var client = new AmazonDynamoDBClient(awsCredentials);
#else
            using var client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.USEast1 });
#endif

            using var dbContext = new DynamoDBContext(client);
            await dbContext.SaveAsync(streamerSettings, stoppingToken);
        } 
#endregion
    }
}
