using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace JeffBot
{
    [DynamoDBTable("JeffBotStreamerSettings")]
    public class StreamerSettings
    {
        #region StreamerId
        [DynamoDBHashKey()]
        public string StreamerId { get; set; }
        #endregion
        #region StreamerName
        public string StreamerName { get; set; }
        #endregion
        #region StreamerBotName
        public string StreamerBotName { get; set; }
        #endregion
        #region StreamerBotId
        public string StreamerBotId { get; set; }
        #endregion
        #region StreamerBotChatOauthToken
        public string StreamerBotChatOauthToken { get; set; }
        #endregion
        #region StreamerBotApiOauthToken
        public string StreamerBotApiOauthToken { get; set; }
        #endregion
        #region StreamElementsChannelId
        public string StreamElementsChannelId { get; set; }
        #endregion
        #region StreamElementsJwtToken
        public string StreamElementsJwtToken { get; set; }
        #endregion
        #region AdditionalAIPrompt 
        public string AdditionalAIPrompt { get; set; }
        #endregion
        #region BotFeatures
        public List<BotCommandSettings> BotFeatures { get; set; } 
        #endregion
    }
}