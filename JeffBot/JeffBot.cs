using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using OnLogArgs = TwitchLib.Client.Events.OnLogArgs;

namespace JeffBot
{
    public class JeffBot
    {
        #region ILogger
        protected ILogger<JeffBot> Logger { get; set; }
        #endregion

        #region BotCommands
        public List<IBotCommand> BotCommands { get; set; } = new();
        #endregion
        #region ManagedTwitchApi
        protected ManagedTwitchApi TwitchApi { get; set; }
        #endregion
        #region TwitchChatClient
        protected TwitchClient TwitchChatClient { get; set; }
        #endregion
        #region TwitchPubSubClient
        protected TwitchPubSub TwitchPubSubClient { get; set; }
        #endregion
        #region WebsocketClient
        protected WebSocketClient WebsocketClient { get; set; }
        #endregion
        #region StreamerSettings
        public StreamerSettings StreamerSettings { get; set; }
        #endregion

        #region Constructor
        public JeffBot(StreamerSettings streamerSettings, ILogger<JeffBot> logger)
        {
            StreamerSettings = streamerSettings;
            Logger = logger;
            InitializeBotForStreamer();
        }
        #endregion

        #region InitializeBotForStreamer
        private void InitializeBotForStreamer()
        {
            InitializeTwitchApi();
            InitializeChat();
            InitializePubSub();

            foreach (var botFeature in StreamerSettings.BotFeatures)
            {
                try
                {
                    switch (botFeature.Name)
                    {
                        case nameof(BotFeatureName.BanHate):
                            BotCommands.Add(new BanHateCommand(botFeature, TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings, Logger));
                            break;
                        case nameof(BotFeatureName.Heist):
                            BotCommands.Add(new HeistCommand(new BotCommandSettings<HeistCommandSettings>(botFeature), TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings, Logger));
                            break;
                        case nameof(BotFeatureName.JeffRpg):
                            BotCommands.Add(new BanHateCommand(botFeature, TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings, Logger));
                            break;
                        case nameof(BotFeatureName.Clip):
                            BotCommands.Add(new AdvancedClipCommand(new BotCommandSettings<AdvancedClipCommandSettings>(botFeature), TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings, Logger));
                            break;
                        case nameof(BotFeatureName.AdvancedClip):
                            BotCommands.Add(new AdvancedClipCommand(new BotCommandSettings<AdvancedClipCommandSettings>(botFeature), TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings, Logger));
                            break;
                        case nameof(BotFeatureName.Mark):
                            BotCommands.Add(new MarkCommand(botFeature, TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings, Logger));
                            break;
                        case nameof(BotFeatureName.AskMeAnything):
                            BotCommands.Add(new AskMeAnythingCommand(new BotCommandSettings<AskMeAnythingSettings>(botFeature), TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings, Logger));
                            break;
                        case nameof(BotFeatureName.SongManagement):
                            BotCommands.Add(new SongManagementCommand(new BotCommandSettings<SongManagementCommandSettings>(botFeature), TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings, Logger));
                            break;
                        case nameof(BotFeatureName.StreamManagement):
                            BotCommands.Add(new StreamManagementCommand(botFeature, TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings, Logger));
                            break;
                        default:
                            BotCommands.Add(new GenericCommand(botFeature, TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings, Logger));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogInformation($"Failed to setup command for streamer: {StreamerSettings.StreamerName}");
                    Logger.LogError(JsonConvert.SerializeObject(botFeature), ex);
                }
            }
            InitializeBotCommands();
        }
        #endregion
        #region ShutdowmBotForStreamer
        public void ShutdownBotForStreamer()
        {
            try
            {
                Logger.LogInformation($"Disconnecting the bot {StreamerSettings.StreamerBotName} from {StreamerSettings.StreamerName}'s stream.");
                if (TwitchChatClient.IsConnected)
                {
                    TwitchChatClient.OnLog -= ChatClient_OnLog;
                    TwitchChatClient.OnJoinedChannel -= ChatClient_OnJoinedChannel;
                    TwitchChatClient.OnConnected -= ChatClient_OnConnected;
                    TwitchChatClient.OnMessageReceived -= ChatClient_OnMessageReceived;
                    TwitchChatClient.OnDisconnected -= ChatClient_OnDisconnected;
                    WebsocketClient.OnStateChanged -= WebSocketClient_OnStateChanged;
                    TwitchChatClient.Disconnect();
                }

                TwitchPubSubClient.Disconnect();
            }
            catch (Exception ex)
            {
                // TODO: How do we handle this?
                Logger.LogError(ex.ToString());
            }
        }
        #endregion

        #region InitializePubSub
        private void InitializePubSub()
        {
            TwitchPubSubClient = new TwitchPubSub();
            TwitchPubSubClient.OnPubSubServiceConnected += PubSubClient_OnPubSubServiceConnected;
            TwitchPubSubClient.OnListenResponse += PubSubClient_OnListenResponse;
            TwitchPubSubClient.ListenToFollows(StreamerSettings.StreamerId);
            if (!string.IsNullOrWhiteSpace(StreamerSettings.StreamerOauthToken))
            {
                TwitchPubSubClient.ListenToRaid(StreamerSettings.StreamerId);
                TwitchPubSubClient.ListenToBitsEventsV2(StreamerSettings.StreamerId);
            }
            TwitchPubSubClient.Connect();
        }
        #endregion
        #region InitializeChat
        private void InitializeChat()
        {
            Logger.LogInformation($"Initialize {StreamerSettings.StreamerName}'s chat as {StreamerSettings.StreamerBotName}");
            var credentials = new ConnectionCredentials((StreamerSettings.StreamerBotName), $"oauth:{(!StreamerSettings.UseDefaultBot ? StreamerSettings.StreamerBotOauthToken : GlobalSettingsSingleton.Instance.DefaultBotOauthToken)}");
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebsocketClient = new WebSocketClient(clientOptions);
            TwitchChatClient = new TwitchClient(WebsocketClient);
            TwitchChatClient.Initialize(credentials, StreamerSettings.StreamerName.ToLower());
            TwitchChatClient.OnIncorrectLogin += ChatClient_OnIncorrectLogin;

            TwitchChatClient.OnLog += ChatClient_OnLog;
            TwitchChatClient.OnJoinedChannel += ChatClient_OnJoinedChannel;
            TwitchChatClient.OnConnected += ChatClient_OnConnected;
            TwitchChatClient.OnMessageReceived += ChatClient_OnMessageReceived;
            TwitchChatClient.OnDisconnected += ChatClient_OnDisconnected;
            WebsocketClient.OnStateChanged += WebSocketClient_OnStateChanged;
            if (!TwitchChatClient.Connect())
            {
                Logger.LogError($"Failed to connect to {StreamerSettings.StreamerName}'s chat as {StreamerSettings.StreamerBotName}");
                WaitAndAttemptReconnection();
            }
        }
        #endregion
        #region InitializeTwitchApi
        private async void InitializeTwitchApi()
        {
            TwitchApi = new ManagedTwitchApi(await AwsUtilities.SecretsManager.GetSecret("TWITCH_API_CLIENT_ID"), await AwsUtilities.SecretsManager.GetSecret("TWITCH_API_CLIENT_SECRET"), StreamerSettings);
        }
        #endregion
        #region InitializeBotCommands
        private void InitializeBotCommands()
        {
            BotCommands.ForEach(a => a.Initialize());
        }
        #endregion
        #region ChatClient_OnLog
        private void ChatClient_OnLog(object sender, OnLogArgs e)
        {
            // Logger.LogInformation($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }
        #endregion
        #region ChatClient_OnDisconnected
        private void ChatClient_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            WaitAndAttemptReconnection();
        }
        #endregion
        #region ChatClient_OnConnected
        private void ChatClient_OnConnected(object sender, OnConnectedArgs e)
        {
            Logger.LogInformation($"Connected to {e.BotUsername}");
        }
        #endregion
        #region ChatClient_OnJoinedChannel
        private void ChatClient_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Logger.LogInformation($"Hey guys! I am a bot connected via TwitchLib to {StreamerSettings.StreamerName}'s chat as the user {StreamerSettings.StreamerBotName}");
        }
        #endregion
        #region ChatClient_OnMessageReceived
        private void ChatClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            BotCommands.AsParallel().ForAll(a => a.CheckExecutionPermissionsAndExecuteCommand(e.ChatMessage));
        }
        #endregion
        #region ChatClient_OnIncorrectLogin
        private async void ChatClient_OnIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {
            if (e.Exception == null) return;
            Logger.LogInformation($"Incorrect chat login for streamer {StreamerSettings.StreamerName} with error message {e.Exception.Message}");
            if (e.Exception.Message.Contains("Login authentication failed"))
            {
                try
                {
                    await TwitchApi.RefreshAccessTokenAsync();
                }
                catch (Exception)
                {
                    // TODO: How to handle this? Just let it fail for now..
                }
            }
        }
        #endregion

        #region WebSocketClient_OnStateChanged
        private void WebSocketClient_OnStateChanged(object sender, OnStateChangedEventArgs e)
        {
            Logger.LogInformation($"Chat client websocket had a change in state in {StreamerSettings.StreamerName}'s chat with bot {StreamerSettings.StreamerBotName}: IsConnected = {e.IsConnected}");
            if (e.IsConnected) return;
            try
            {
                TwitchChatClient.OnLog -= ChatClient_OnLog;
                TwitchChatClient.OnJoinedChannel -= ChatClient_OnJoinedChannel;
                TwitchChatClient.OnConnected -= ChatClient_OnConnected;
                TwitchChatClient.OnMessageReceived -= ChatClient_OnMessageReceived;
                TwitchChatClient.OnDisconnected -= ChatClient_OnDisconnected;
                WebsocketClient.OnStateChanged -= WebSocketClient_OnStateChanged;
                TwitchChatClient.Disconnect();
            }
            catch
            {
                // Swallow this as we are gonna just create a new Client anyways
            }

            try
            {
                TwitchChatClient = null;
                InitializeChat();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error when trying to reconnect to twitch chat for {StreamerSettings.StreamerName} as {StreamerSettings.StreamerBotName}.");
                Logger.LogError(ex.ToString());
            }
        }
        #endregion

        #region PubSubClient_OnPubSubServiceConnected
        private void PubSubClient_OnPubSubServiceConnected(object sender, EventArgs e)
        {
            // SendTopics accepts an oauth optionally, which is necessary for some topics
            TwitchPubSubClient.SendTopics(string.IsNullOrEmpty(StreamerSettings.StreamerOauthToken) ? StreamerSettings.StreamerBotOauthToken : StreamerSettings.StreamerOauthToken);
        }
        #endregion
        #region PubSubClient_OnListenResponse
        private void PubSubClient_OnListenResponse(object sender, OnListenResponseArgs e)
        {
            if (!e.Successful)
                throw new Exception($"Failed to listen! Response: {e.Response}");
        }
        #endregion

        #region WaitAndAttemptReconnection
        private void WaitAndAttemptReconnection()
        {
            // If we disconnect, wait 30 seconds, cleanup and reconnect.
            Logger.LogInformation($"Disconnected, trying to reconnect..");
            Task.Delay(30000).Wait();
            TwitchChatClient.OnLog -= ChatClient_OnLog;
            TwitchChatClient.OnJoinedChannel -= ChatClient_OnJoinedChannel;
            TwitchChatClient.OnConnected -= ChatClient_OnConnected;
            TwitchChatClient.OnMessageReceived -= ChatClient_OnMessageReceived;
            TwitchChatClient.OnDisconnected -= ChatClient_OnDisconnected;
            InitializeChat();
        }
        #endregion
    }
}