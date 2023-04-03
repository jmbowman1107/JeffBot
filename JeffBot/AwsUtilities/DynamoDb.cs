using System;
using System.Linq;
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
        #region DbContext
        private static DynamoDBContext _dbContext;
        public static DynamoDBContext DbContext
        {
            get
            {
                if (_dbContext != null) return _dbContext;
#if DEBUG
                var chain = new CredentialProfileStoreChain();
                if (!chain.TryGetAWSCredentials("jeff-personal", out var awsCredentials))
                {
                    throw new ArgumentException("No AWS credential profile called 'jeff-personal' was found");
                }
                var client = new AmazonDynamoDBClient(awsCredentials);
#else
                var client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.USEast1 });
#endif

                var dbContext = new DynamoDBContext(client);
                _dbContext = dbContext;
                return _dbContext;
            }
        }
        #endregion

        #region PopulateOrUpdateStreamerSettings
        public static async Task PopulateOrUpdateStreamerSettings(StreamerSettings streamerSettings, CancellationToken stoppingToken = default)
        {
            await DbContext.SaveAsync(streamerSettings, stoppingToken);
        }
        #endregion
        #region GetGlobalSettings
        public static async Task<GlobalSettings> GetGlobalSettings(CancellationToken stoppingToken = default)
        {
            var globalSettings = await DbContext.FromScanAsync<GlobalSettings>(new ScanOperationConfig()).GetRemainingAsync(stoppingToken);
            return globalSettings != null ? globalSettings.FirstOrDefault() : new GlobalSettings();
        }
        #endregion
        #region UpdateGlobalSettings
        public static async Task UpdateGlobalSettings(GlobalSettings globalSettings, CancellationToken stoppingToken = default)
        {
            await DbContext.SaveAsync(globalSettings, stoppingToken);
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