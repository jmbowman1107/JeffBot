using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace JeffBot
{
    [DynamoDBTable("JeffBotStreamerSettings")]
    public class StreamerSettings
    {
        #region UseDefaultBot
        [DynamoDBIgnore]
        public bool UseDefaultBot
        {
            get
            {
                if (string.IsNullOrEmpty(this.StreamerBotId) || string.IsNullOrEmpty(this.StreamerBotOauthToken))
                {
                    StreamerBotName = GlobalSettingsSingleton.Instance.DefaultBotName;
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
        #region StreamerName
        public string StreamerName { get; set; }
        #endregion
        #region StreamerOauthToken
        public string StreamerOauthToken { get; set; }
        #endregion
        #region StreamerRefreshToken
        public string StreamerRefreshToken { get; set; }
        #endregion

        #region StreamerBotId
        public string StreamerBotId { get; set; }
        #endregion
        #region StreamerBotName
        public string StreamerBotName { get; set; }
        #endregion
        #region StreamerBotOauthToken
        public string StreamerBotOauthToken { get; set; }
        #endregion
        #region StreamerBotRefreshToken
        public string StreamerBotRefreshToken { get; set; }
        #endregion

        #region StreamElementsChannelId
        public string StreamElementsChannelId { get; set; }
        #endregion
        #region StreamElementsJwtToken
        public string StreamElementsJwtToken { get; set; }
        #endregion
        #region SpotifyRefreshToken 
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