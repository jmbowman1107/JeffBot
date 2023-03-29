using System;
#if DEBUG
using Amazon.Runtime.CredentialManagement;
#endif
using System.Threading;
using System.Threading.Tasks;
#if RELEASE
using Amazon;
#endif
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;


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

    public class DataConverter : IPropertyConverter
    {
        public object FromEntry(DynamoDBEntry entry)
        {
            var primitive = entry as Primitive;
            if (primitive is not { Value: string value } || string.IsNullOrEmpty(value))
                throw new ArgumentOutOfRangeException();
            var ret = JsonConvert.DeserializeObject(value);
            return ret;
        }

        public DynamoDBEntry ToEntry(object value)
        {
            var jsonString = JsonConvert.SerializeObject(value);
            DynamoDBEntry ret = new Primitive(jsonString);
            return ret;
        }
    }

    public class DataConverter<T> : IPropertyConverter
    {
        public object FromEntry(DynamoDBEntry entry)
        {
            var primitive = entry as Primitive;
            if (primitive is not { Value: string value } || string.IsNullOrEmpty(value))
                throw new ArgumentOutOfRangeException();
            var ret = JsonConvert.DeserializeObject<T>(value);
            return ret;
        }

        public DynamoDBEntry ToEntry(object value)
        {
            var jsonString = JsonConvert.SerializeObject(value);
            DynamoDBEntry ret = new Primitive(jsonString);
            return ret;
        }
    }
}