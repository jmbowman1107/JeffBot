using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Runtime.CredentialManagement;
using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon;

namespace JeffBot
{
    public static class DynamoDbUtilities
    {
        public static async Task PopulateOrUpdateStreamerSettings(StreamerSettings streamerSettings, CancellationToken stoppingToken=default)
        {
            #if DEBUG
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials("jeff-personal", out var awsCredentials))
            {
                //_logger.LogCritical("Could not find AWS Credentials");
                throw new ArgumentException("No AWS credential profile called 'jeff-personal' was found");
            }
            using var client = new AmazonDynamoDBClient(awsCredentials);
            #else
            using var client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.USEast1 });
            #endif

            using var dbContext = new DynamoDBContext(client);
            await dbContext.SaveAsync(streamerSettings, stoppingToken);
        }
    }
}
