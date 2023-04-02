using Amazon.DynamoDBv2.DataModel;

namespace JeffBot
{
    [DynamoDBTable("JeffBotGlobalSettings")]
    public class GlobalSettings
    {
        public string Id { get; set; }
        public string DefaultBotName { get; set; }
        public string DefaultBotId { get; set; }
        public string DefaultBotOauthToken { get; set; }
        public string DefaultBotRefreshToken { get; set; }
    }
}
