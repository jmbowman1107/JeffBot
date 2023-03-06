using System.Linq;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    public abstract class BotCommandBase : IBotCommand
    {
        #region BotFeature - Abstract
        public abstract BotFeatures BotFeature { get; }
        #endregion
        #region DefaultKeyword - Abstract
        public abstract string DefaultKeyword { get; }
        #endregion
        #region CommandKeyword
        public string CommandKeyword
        {
            get
            {
                if (StreamerSettings.BotFeatures.Any(a => a.Name == BotFeature))
                {
                    var feature = StreamerSettings.BotFeatures.FirstOrDefault(a => a.Name == BotFeature);
                    if (!string.IsNullOrWhiteSpace(feature?.Command))
                    {
                        return feature.Command;
                    }
                }
                return DefaultKeyword;
            }
        }
        #endregion
        #region CommandPermissionLevel
        public FeaturePermissionLevels CommandPermissionLevel
        {
            get
            {
                if (StreamerSettings.BotFeatures.Any(a => a.Name == BotFeature))
                {
                    var feature = StreamerSettings.BotFeatures.FirstOrDefault(a => a.Name == BotFeature);
                    return feature.PermissionLevel;
                }
                return FeaturePermissionLevels.Broadcaster;
            }
        }
        #endregion
        #region IsCommandEnabled
        public bool IsCommandEnabled
        {
            get
            {
                return StreamerSettings.BotFeatures.Any(a => a.Name == BotFeature);
            }
        }
        #endregion
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

        #region CheckExecutionPermissionsAndProcessCommand
        public virtual void CheckExecutionPermissionsAndProcessMessage(ChatMessage chatMessage)
        {
            if (!IsCommandEnabled) return;
            var canExecuteCommand = false;
            switch (CommandPermissionLevel)
            {
                case FeaturePermissionLevels.Everyone:
                    canExecuteCommand = true;
                    break;
                case FeaturePermissionLevels.LoyalUser:
                    // TODO: Implement when points system is enabled.. (over X hours watched, can use command etc..)
                    break;
                case FeaturePermissionLevels.Subscriber:
                    if (chatMessage.IsSubscriber || chatMessage.IsVip || chatMessage.IsModerator || chatMessage.IsBroadcaster) canExecuteCommand = true;
                    break;
                case FeaturePermissionLevels.Vip:
                    if (chatMessage.IsVip || chatMessage.IsModerator || chatMessage.IsBroadcaster) canExecuteCommand = true;
                    break;
                case FeaturePermissionLevels.Mod:
                    if (chatMessage.IsModerator || chatMessage.IsBroadcaster) canExecuteCommand = true;
                    break;
                case FeaturePermissionLevels.SuperMod:
                    // TODO: Implement when SuperMod (e.g. editor) functionality is implemented.
                    break;
                case FeaturePermissionLevels.Broadcaster:
                    if (chatMessage.IsBroadcaster) canExecuteCommand = true;
                    break;
            }
            if (canExecuteCommand) ProcessMessage(chatMessage);
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