using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Api.Helix.Models.Moderation.BanUser;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    public class BanHateCommand : BotCommandBase
    {
        #region Constructor
        public BanHateCommand(BotCommandSettings botCommandSettings, ManagedTwitchApi twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSub, StreamerSettings streamerSettings, ILogger<JeffBot> logger) : base(botCommandSettings, twitchApiClient, twitchChatClient, twitchPubSub, streamerSettings, logger)
        {
            BotCommandSettings.GlobalCooldown = 0;
            BotCommandSettings.UserCooldown = 0;
        }
        #endregion

        #region ProcessMessage - IBotCommand Member
        public override async Task<bool> ProcessMessage(ChatMessage chatMessage)
        {
            if (chatMessage.Username.Contains("hoss00312") || chatMessage.Username.Contains("idwt_"))
                await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Moderation.BanUserAsync(StreamerSettings.StreamerId, StreamerSettings.StreamerBotId ?? Singleton<GlobalSettings>.Instance.DefaultBotId, new BanUserRequest { Reason = "We don't tolerate hate in this channel. Goodbye.", UserId = chatMessage.UserId }));

            if (chatMessage.IsFirstMessage && (chatMessage.Message.ToLower().Contains("buy followers") ||
                                               chatMessage.Message.ToLower().Contains(" followers") ||
                                               chatMessage.Message.ToLower().Contains(" viewers") ||
                                               chatMessage.Message.ToLower().Contains(" views")))
            {
                var test = await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Users.GetUsersFollowsAsync(fromId: chatMessage.UserId, toId: StreamerSettings.StreamerId));
                if (test.Follows != null && !test.Follows.Any())
                {
                    await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Moderation.BanUserAsync(StreamerSettings.StreamerId, StreamerSettings.StreamerBotId ?? Singleton<GlobalSettings>.Instance.DefaultBotId, new BanUserRequest { Reason = "We don't want what you are selling.. go away.", UserId = chatMessage.UserId }));
                }
            }

            return false;
        }
        #endregion
        #region Initialize - IBotCommand Member
        public override void Initialize()
        {
            if (!IsCommandEnabled) return;
            TwitchPubSubClient.OnFollow += TwitchPubSubClient_OnFollow;
        }
        #endregion

        #region GetRecentFollowersAndBanHate
        private async Task GetRecentFollowersAndBanHate()
        {
            string pagination = null;
            var followers = await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Users.GetUsersFollowsAsync(first: 100, toId: StreamerSettings.StreamerId, after: pagination));
            foreach (var follower in followers.Follows)
            {
                if (follower.FromUserName.Contains("hoss00312"))
                {
                    Logger.LogInformation($"Banning this MOFO {follower.FromUserName}");
                    await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Moderation.BanUserAsync(StreamerSettings.StreamerId, StreamerSettings.StreamerBotId ?? Singleton<GlobalSettings>.Instance.DefaultBotId, new BanUserRequest { Reason = "We don't tolerate hate in this channel. Goodbye.", UserId = follower.FromUserId }));
                }
            }
            pagination = followers.Pagination.Cursor;
        }
        #endregion
        #region TwitchPubSubClient_OnFollow
        private async void TwitchPubSubClient_OnFollow(object sender, TwitchLib.PubSub.Events.OnFollowArgs e)
        {
            if (e.Username.ToLower().Contains("hoss00312") || e.Username.ToLower().Contains("h0ss00312") || e.Username.Contains("idwt_"))
                await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Moderation.BanUserAsync(StreamerSettings.StreamerId, StreamerSettings.StreamerBotId ?? Singleton<GlobalSettings>.Instance.DefaultBotId, new BanUserRequest { Reason = "We don't tolerate hate in this channel. Goodbye.", UserId = e.UserId }));
        }
        #endregion
    }
}