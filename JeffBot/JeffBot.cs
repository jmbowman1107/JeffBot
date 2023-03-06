using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Api;
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
        #region BotCommands
        public List<IBotCommand> BotCommands { get; set; } = new List<IBotCommand>();
        #endregion
        #region TwitchApi
        protected TwitchAPI TwitchApi { get; set; } 
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
        public JeffBot(StreamerSettings streamerSettings)
        {
            StreamerSettings = streamerSettings;
            InitializePubSub();
            InitializeChat();
            InitializeTwitchApi();
            // TODO: This should be done when creating the bot for each streamer (e.g. in the Web Project). This is so eventually there can be a backend that feeds this.
            BotCommands = new List<IBotCommand>()
            {
                new AdvancedClipCommand(TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings),
                new BanHateCommand(TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings),
                new HeistCommand(TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings),
                new MarkCommand(TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings),
                new AskMeAnythingCommand(TwitchApi, TwitchChatClient, TwitchPubSubClient, StreamerSettings)
            };
            InitializeBotCommands();
        }
        #endregion

        #region InitializePubSub
        private void InitializePubSub()
        {
            TwitchPubSubClient = new TwitchPubSub();
            TwitchPubSubClient.OnPubSubServiceConnected += PubSubClient_OnPubSubServiceConnected;
            TwitchPubSubClient.OnListenResponse += PubSubClient_OnListenResponse;
            TwitchPubSubClient.ListenToFollows(StreamerSettings.StreamerId);
            TwitchPubSubClient.Connect();
        }
        #endregion
        #region InitializeChat
        private void InitializeChat()
        {
            Console.WriteLine($"Initialize {StreamerSettings.StreamerName}'s chat as {StreamerSettings.StreamerBotName}");
            ConnectionCredentials credentials = new ConnectionCredentials(StreamerSettings.StreamerBotName, StreamerSettings.StreamerBotChatOauthToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebsocketClient = new WebSocketClient(clientOptions);
            TwitchChatClient = new TwitchClient(WebsocketClient);
            TwitchChatClient.Initialize(credentials, StreamerSettings.StreamerName.ToLower());

            TwitchChatClient.OnLog += ChatClient_OnLog;
            TwitchChatClient.OnJoinedChannel += ChatClient_OnJoinedChannel;
            TwitchChatClient.OnConnected += ChatClient_OnConnected;
            TwitchChatClient.OnMessageReceived += ChatClient_OnMessageReceived;
            TwitchChatClient.OnDisconnected += ChatClient_OnDisconnected;
            WebsocketClient.OnStateChanged += WebSocketClient_OnStateChanged;
            if (!TwitchChatClient.Connect())
            {
                Console.WriteLine($"Failed to connect to {StreamerSettings.StreamerName}'s chat as {StreamerSettings.StreamerBotName}");
                WaitAndAttemptReconnection();
            }
        }
        #endregion
        #region InitializeTwitchApi
        private void InitializeTwitchApi()
        {
            TwitchApi = new TwitchAPI();

            // throw new NotImplementedException("Enter your Twitch API Client ID and Access Token below.");
            //_twitchApi.Settings.ClientId = "YOUR_TWITCH_API_CLIENT_ID";
            //_twitchApi.Settings.AccessToken = "YOUR_TWITCH_API_ACCESS_TOKEN";
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
            Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }
        #endregion
        #region ChatClient_OnDisconnected
        private void ChatClient_OnDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            WaitAndAttemptReconnection();
        }
        #endregion
        #region ChatClient_OnConnected
        private void ChatClient_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.BotUsername}");
        }
        #endregion
        #region ChatClient_OnJoinedChannel
        private void ChatClient_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine($"Hey guys! I am a bot connected via TwitchLib to {StreamerSettings.StreamerName}'s chat as the user {StreamerSettings.StreamerBotName}");
        }
        #endregion
        #region ChatClient_OnMessageReceived
        private void ChatClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            BotCommands.ForEach(a => a.ProcessMessage(e.ChatMessage));
        }
        #endregion

        #region WebSocketClient_OnStateChanged
        private void WebSocketClient_OnStateChanged(object sender, OnStateChangedEventArgs e)
        {
            Console.WriteLine($"Chat client websocket had a change in state in {StreamerSettings.StreamerName}'s chat with bot {StreamerSettings.StreamerBotName}: IsConnected = {e.IsConnected}");
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
                Console.WriteLine($"Error when trying to reconnect to twitch chat for {StreamerSettings.StreamerName} as {StreamerSettings.StreamerBotName}.");
                Console.WriteLine(ex);
            }
        }
        #endregion

        #region PubSubClient_OnPubSubServiceConnected
        private void PubSubClient_OnPubSubServiceConnected(object sender, EventArgs e)
        {
            // SendTopics accepts an oauth optionally, which is necessary for some topics
            //_twitchPubSubClient.SendTopics("YOUR_OAUTH_TOKEN_FOR_BEING_ABLE_TO_MARK_STREAMS");
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
            Console.WriteLine($"Disconnected, trying to reconnect..");
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