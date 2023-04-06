using System;
using Amazon.DynamoDBv2.DataModel;

namespace JeffBot
{
    [DynamoDBTable("JeffBotGlobalSettings")]
    public class GlobalSettings : BotSettingsBase
    {
        #region Id 
        public string Id { get; set; }
        #endregion
        #region DefaultBotName
        public string DefaultBotName { get; set; }
        #endregion
        #region DefaultBotId
        public string DefaultBotId { get; set; }
        #endregion
        #region DefaultBotOauthToken
        public string DefaultBotOauthToken { get; set; }
        #endregion
        #region DefaultBotOauthTokenExpiration
        public DateTime DefaultBotOauthTokenExpiration { get; set; }
        #endregion
        #region DefaultBotRefreshToken
        public string DefaultBotRefreshToken { get; set; }
        #endregion
    }
}