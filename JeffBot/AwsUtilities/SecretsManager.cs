using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;

namespace JeffBot.AwsUtilities
{
    public static class SecretsManager
    {
        #region GetSecret
        public static async Task<string> GetSecret(string name)
        {
            var secretName = "JeffBotSecrets";
            var region = "us-east-1";
#if DEBUG
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials("jeff-personal", out var awsCredentials))
            {
                throw new ArgumentException("No AWS credential profile called 'jeff-personal' was found");
            }
            using IAmazonSecretsManager client = new AmazonSecretsManagerClient(awsCredentials, RegionEndpoint.GetBySystemName(region));
#else
            using IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
#endif

            var request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };

            GetSecretValueResponse response = null;
            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception)
            {
                // TODO: Catch and handle this.
                // For a list of the exceptions thrown, see
                // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
                throw;
            }

            var secretDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.SecretString);
            if (secretDictionary != null && secretDictionary.TryGetValue(name, out string value))
            {
                return value;
            }

            // Error will be caught later if empty..
            return string.Empty;
        }
        #endregion
    }
}
