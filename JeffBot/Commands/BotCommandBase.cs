using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    public abstract class BotCommandBase : IBotCommand
    {
        #region TwitchApiClient
        public TwitchAPI TwitchApiClient { get; set; }
        #endregion
        #region TwitchChatClient
        public TwitchClient TwitchChatClient { get; set; }
        #endregion
        #region TwitchPubSubClient
        public TwitchPubSub TwitchPubSubClient { get; set; } 
        #endregion
        #region StreamerSettings
        public StreamerSettings StreamerSettings { get; set; } 
        #endregion

        #region Constructor
        protected BotCommandBase(TwitchAPI twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSubClient, StreamerSettings streamerSettings)
        {
            TwitchApiClient = twitchApiClient;
            TwitchChatClient = twitchChatClient;
            TwitchPubSubClient = twitchPubSubClient;
            StreamerSettings = streamerSettings;
        }
        #endregion

        #region ProcessMessage - IBotCommand Member
        public abstract void ProcessMessage(ChatMessage chatMessage);
        #endregion
        #region Initialize - IBotCommand Member
        public abstract void Initialize();
        #endregion
    }
}