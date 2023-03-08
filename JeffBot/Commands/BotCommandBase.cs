using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Exceptions;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Interfaces;

namespace JeffBot
{
    public abstract class BotCommandBase : IBotCommand
    {
        private DateTimeOffset _lastExecuted;
        private readonly Dictionary<string, DateTimeOffset>  _usersLastExecuted;

        #region BotCommandSettings - IBotCommand Member
        public BotCommandSettings BotCommandSettings { get; set; }
        #endregion
        #region IsCommandEnabled - IBotCommand Member
        public bool IsCommandEnabled
        {
            get
            {
                return StreamerSettings.BotFeatures.Any(a => a.Name == BotCommandSettings.Name && a.IsEnabled);
            }
        }
        #endregion
        #region TwitchApiClient - IBotCommand Member
        public ITwitchAPI TwitchApiClient { get; set; }
        #endregion
        #region TwitchChatClient - IBotCommand Member
        public ITwitchClient TwitchChatClient { get; set; }
        #endregion
        #region TwitchPubSubClient - IBotCommand Member
        public ITwitchPubSub TwitchPubSubClient { get; set; }
        #endregion
        #region StreamerSettings - IBotCommand Member
        public StreamerSettings StreamerSettings { get; set; } 
        #endregion

        #region Constructor
        protected BotCommandBase(BotCommandSettings botCommandSettings, TwitchAPI twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSubClient, StreamerSettings streamerSettings)
        {
            BotCommandSettings = botCommandSettings;
            TwitchApiClient = twitchApiClient;
            TwitchChatClient = twitchChatClient;
            TwitchPubSubClient = twitchPubSubClient;
            StreamerSettings = streamerSettings;
            _lastExecuted = DateTimeOffset.MinValue;
            _usersLastExecuted = new Dictionary<string, DateTimeOffset>();
        }
        #endregion

        #region CheckExecutionPermissionsAndExecuteCommand - IBotCommand Member
        public virtual void CheckExecutionPermissionsAndExecuteCommand(ChatMessage chatMessage)
        {
            if (!IsCommandEnabled) return;
            var canExecuteCommand = UserHasPermission(chatMessage);
            if (canExecuteCommand) canExecuteCommand = CheckCooldowns(chatMessage, canExecuteCommand);
            if (canExecuteCommand) ExecuteCommand(chatMessage);
        }
        #endregion

        #region ProcessMessage - IBotCommand Member - Abstract
        public abstract Task ProcessMessage(ChatMessage chatMessage);
        #endregion
        #region Initialize - IBotCommand Member - Abstract
        public abstract void Initialize();
        #endregion

        #region UserHasPermission
        private bool UserHasPermission(ChatMessage chatMessage)
        {
            var canExecuteCommand = false;
            switch (BotCommandSettings.PermissionLevel)
            {
                case FeaturePermissionLevel.Everyone:
                    canExecuteCommand = true;
                    break;
                case FeaturePermissionLevel.LoyalUser:
                    // TODO: Implement when points system is enabled.. (over X hours watched, can use command etc..)
                    break;
                case FeaturePermissionLevel.Subscriber:
                    if (chatMessage.IsSubscriber || chatMessage.IsVip || chatMessage.IsModerator || chatMessage.IsBroadcaster)
                        canExecuteCommand = true;
                    break;
                case FeaturePermissionLevel.Vip:
                    if (chatMessage.IsVip || chatMessage.IsModerator || chatMessage.IsBroadcaster) canExecuteCommand = true;
                    break;
                case FeaturePermissionLevel.Mod:
                    if (chatMessage.IsModerator || chatMessage.IsBroadcaster) canExecuteCommand = true;
                    break;
                case FeaturePermissionLevel.SuperMod:
                    // TODO: Implement when SuperMod (e.g. editor) functionality is implemented.
                    break;
                case FeaturePermissionLevel.Broadcaster:
                    if (chatMessage.IsBroadcaster) canExecuteCommand = true;
                    break;
            }

            return canExecuteCommand;
        }
        #endregion
        #region CheckCooldowns
        private bool CheckCooldowns(ChatMessage chatMessage, bool canExecuteCommand)
        {
            if (DateTimeOffset.UtcNow >= _lastExecuted.AddSeconds(BotCommandSettings.GlobalCooldown))
            {
                if (_usersLastExecuted.ContainsKey(chatMessage.Username.ToLower()) && DateTimeOffset.UtcNow <
                    _usersLastExecuted[chatMessage.Username.ToLower()].AddSeconds(BotCommandSettings.UserCooldown))
                {
                    canExecuteCommand = false;
                }
            }
            else
            {
                canExecuteCommand = false;
            }

            return canExecuteCommand;
        }
        #endregion
        #region ExecuteCommand
        private void ExecuteCommand(ChatMessage chatMessage)
        {
            try
            {
                _lastExecuted = DateTimeOffset.UtcNow;
                _usersLastExecuted[chatMessage.Username.ToLower()] = DateTimeOffset.UtcNow;
                ProcessMessage(chatMessage);
            }
            catch (BadStateException)
            {
                // TODO: Root cause fix this at some point.. Seems to be something weird going on in twitchlib.. sometimes it intiializes.. but doesn't join according to the library..
                TwitchChatClient.JoinChannel($"{StreamerSettings.StreamerName.ToLower()}");
            }
            catch (AggregateException aex)
            {
                if (aex.InnerException is BadStateException)
                {
                    // TODO: Root cause fix this at some point.. Seems to be something weird going on in twitchlib.. sometimes it intiializes.. but doesn't join according to the library..
                    TwitchChatClient.JoinChannel($"{StreamerSettings.StreamerName.ToLower()}");
                }
            }
            catch (Exception ex)
            {
                // In case we bubble up an exception..
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion
    }
}