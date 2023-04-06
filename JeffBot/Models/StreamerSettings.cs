using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace JeffBot
{
    [DynamoDBTable("JeffBotStreamerSettings")]
    public class StreamerSettings : BotSettingsBase
    {
        #region UseDefaultBot
        [DynamoDBIgnore]
        public bool UseDefaultBot
        {
            get
            {
                if (string.IsNullOrEmpty(this.StreamerBotId) || string.IsNullOrEmpty(this.StreamerBotOauthToken))
                {
                    StreamerBotName = Singleton<GlobalSettings>.Instance.DefaultBotName;
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region StreamerId
        [DynamoDBHashKey()]
        public string StreamerId { get; set; }
        #endregion
        #region IsActive
        [RequiresBotRestart]
        public bool IsActive { get; set; } = true;
        #endregion
        #region StreamerName
        public string StreamerName { get; set; }
        #endregion
        #region StreamerOauthToken
        [RequiresBotRestart]
        public string StreamerOauthToken { get; set; }
        #endregion
        #region StreamerOauthTokenExpiration
        public DateTime StreamerOauthTokenExpiration { get; set; }
        #endregion
        #region StreamerRefreshToken
        [RequiresBotRestart]
        public string StreamerRefreshToken { get; set; }
        #endregion

        #region StreamerBotId
        public string StreamerBotId { get; set; }
        #endregion
        #region StreamerBotName
        public string StreamerBotName { get; set; }
        #endregion
        #region StreamerBotOauthToken
        [RequiresBotRestart]
        public string StreamerBotOauthToken { get; set; }
        #endregion
        #region StreamerBotOauthTokenExpiration
        public DateTime StreamerBotOauthTokenExpiration { get; set; }
        #endregion
        #region StreamerBotRefreshToken
        [RequiresBotRestart]
        public string StreamerBotRefreshToken { get; set; }
        #endregion

        #region StreamElementsChannelId
        [RequiresBotRestart]
        public string StreamElementsChannelId { get; set; }
        #endregion
        #region StreamElementsJwtToken
        [RequiresBotRestart]
        public string StreamElementsJwtToken { get; set; }
        #endregion
        #region SpotifyRefreshToken 
        [RequiresBotRestart]
        public string SpotifyRefreshToken { get; set; }
        #endregion
        #region BotFeatures
        public List<BotCommandSettings> BotFeatures { get; set; }
        #endregion

        #region Constructor
        public StreamerSettings()
        {
            var init = UseDefaultBot;
        }
        #endregion
    }
}