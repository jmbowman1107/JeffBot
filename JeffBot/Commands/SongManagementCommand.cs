using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JeffBot.AwsUtilities;
using SpotifyAPI.Web;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    public class SongManagementCommand : BotCommandBase<SongManagementCommandSettings>
    {
        #region SpotifyClient
        protected SpotifyClient SpotifyClient { get; set; } 
        #endregion

        #region Constructor
        public SongManagementCommand(BotCommandSettings<SongManagementCommandSettings> botCommandSettings, TwitchAPI twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSubClient, StreamerSettings streamerSettings) : base(botCommandSettings, twitchApiClient, twitchChatClient, twitchPubSubClient, streamerSettings)
        {}
        #endregion

        #region ProcessMessage - Override
        public override async Task<bool> ProcessMessage(ChatMessage chatMessage)
        {
            var isSongMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{BotCommandSettings.TriggerWord}$");
            if (isSongMessage.Captures.Count > 0)
            {
                await GetCurrentSong(chatMessage);
                return true;
            }

            return false;
        }
        #endregion
        #region Initialize - Override
        public override async void Initialize()
        {
            if (string.IsNullOrEmpty(StreamerSettings.SpotifyRefreshToken))
            {
                Console.WriteLine("Cannot initialize spotify as there is no token!");
                return;
            }
            var authenticator = new AuthorizationCodeAuthenticator(
                await AwsUtilities.SecretsManager.GetSecret("SPOTIFY_CLIENT_ID"), 
                await AwsUtilities.SecretsManager.GetSecret("SPOTIFY_CLIENT_SECRET"),
                new AuthorizationCodeTokenResponse()
                {
                    RefreshToken = StreamerSettings.SpotifyRefreshToken,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            });
            authenticator.TokenRefreshed += async (sender, response) =>
            {
                Console.Write("Refreshing token.");
                StreamerSettings.SpotifyRefreshToken = response.RefreshToken;
                await DynamoDb.PopulateOrUpdateStreamerSettings(StreamerSettings);
            };

            var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);
            SpotifyClient = new SpotifyClient(config);
        }
        #endregion

        #region GetCurrentSong
        private async Task GetCurrentSong(ChatMessage chatMessage)
        {
            if (SpotifyClient == null)
            {
                Console.WriteLine("Spotify is not connected, cannot get current song");
                return;
            }
            var currentSong = await SpotifyClient.Player.GetCurrentPlayback();
            if (currentSong is { Item: FullTrack track })
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"{BotCommandSettings.CustomSettings.MessageBeforeSong} {track.Name} by {string.Join(" and ", track.Artists.Select(a => a.Name))}");
            }
            else
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, "Spotify is not currently playing any songs.");
            }
        }
        #endregion
    }
}
