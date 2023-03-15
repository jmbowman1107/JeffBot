using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using Amazon;
using Newtonsoft.Json;


namespace JeffBot.AwsUtilities
{
    public static class SecretsManager
    {
        public static async Task<string> GetSecret(string name)
        {
            var secretName = "JeffBotSecrets";
            var region = "us-east-1";
            #if DEBUG
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials("jeff-personal", out var awsCredentials))
            {
                Console.WriteLine("Could not find AWS Credentials");
                throw new ArgumentException("No AWS credential profile called 'jeff-personal' was found");
            }
            using IAmazonSecretsManager client = new AmazonSecretsManagerClient(awsCredentials, RegionEndpoint.GetBySystemName(region));
#else
            using IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
            #endif

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };

            GetSecretValueResponse response;

            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                // For a list of the exceptions thrown, see
                // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
                throw e;
            }

            var secretDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.SecretString);
            if (secretDictionary != null && secretDictionary.ContainsKey(name))
            {
                return secretDictionary[name];
            }
            else
            {
                return string.Empty;
                // Throw error?
            }
        }
    }
}
