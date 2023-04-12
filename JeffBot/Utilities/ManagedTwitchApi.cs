using System;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core.Exceptions;

namespace JeffBot
{
    public class ManagedTwitchApi
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string _refreshToken;

        #region StreamerSettings
        public StreamerSettings StreamerSettings { get; set; }
        #endregion
        #region TwitchApiClient
        public TwitchAPI TwitchApi { get; set; }
        #endregion

        #region Constructor
        public ManagedTwitchApi(string clientId, string clientSecret, StreamerSettings streamerSettings)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            TwitchApi = new TwitchAPI();
            TwitchApi.Settings.ClientId = clientId;
            TwitchApi.Settings.Secret = clientSecret;
            StreamerSettings = streamerSettings;
            TwitchApi.Settings.AccessToken = !StreamerSettings.UseDefaultBot ? StreamerSettings.StreamerBotOauthToken : Singleton<GlobalSettings>.Instance.DefaultBotOauthToken;
            _refreshToken = !StreamerSettings.UseDefaultBot ? StreamerSettings.StreamerBotRefreshToken : Singleton<GlobalSettings>.Instance.DefaultBotRefreshToken;
        }
        #endregion

        #region RefreshAccessTokenAsync
        public async Task RefreshAccessTokenAsync()
        {
            var botToken = await TwitchApi.Auth.RefreshAuthTokenAsync(_refreshToken, _clientSecret, _clientId);
            RefreshResponse streamerToken = null;
            if (!string.IsNullOrWhiteSpace(StreamerSettings.StreamerRefreshToken))
            {
                streamerToken = await TwitchApi.Auth.RefreshAuthTokenAsync(StreamerSettings.StreamerRefreshToken, _clientSecret, _clientId);
            }
            await UpdateTokenDetails(botToken, streamerToken);
        }
        #endregion
        #region ExecuteRequest
        public async Task ExecuteRequest(Func<TwitchAPI, Task> apiCall)
        {
            try
            {
                await EnsureAccessTokenAsync();
                await apiCall(TwitchApi);
            }
            catch (Exception ex) when (ex is TokenExpiredException or BadScopeException)
            {
                await RefreshAccessTokenAsync();
                await apiCall(TwitchApi);
            }
        }
        public async Task<T> ExecuteRequest<T>(Func<TwitchAPI, Task<T>> apiCall)
        {
            try
            {
                await EnsureAccessTokenAsync();
                return await apiCall(TwitchApi);
            }
            catch (TokenExpiredException)
            {
                await RefreshAccessTokenAsync();
                return await apiCall(TwitchApi);
            }
        }
        #endregion

        #region EnsureAccessTokenAsync
        private async Task EnsureAccessTokenAsync()
        {
            var botTokenExpiration = !StreamerSettings.UseDefaultBot ? StreamerSettings.StreamerBotOauthTokenExpiration : Singleton<GlobalSettings>.Instance.DefaultBotOauthTokenExpiration;
            if (DateTime.Now >= botTokenExpiration) await RefreshAccessTokenAsync();
        }
        #endregion
        #region UpdateTokenDetails
        private async Task UpdateTokenDetails(RefreshResponse botToken, RefreshResponse streamerToken)
        {
            TwitchApi.Settings.AccessToken = botToken.AccessToken;
            _refreshToken = botToken.RefreshToken;
            var accessTokenExpiration = DateTime.Now.AddSeconds(botToken.ExpiresIn - 1800); // Subtract 30 minutes as a buffer

            if (streamerToken != null)
            {
                StreamerSettings.StreamerOauthToken = streamerToken.AccessToken;
                StreamerSettings.StreamerRefreshToken = streamerToken.RefreshToken;
                StreamerSettings.StreamerOauthTokenExpiration = accessTokenExpiration;
            }

            if (!StreamerSettings.UseDefaultBot)
            {
                StreamerSettings.StreamerBotOauthToken = botToken.AccessToken;
                StreamerSettings.StreamerBotRefreshToken = botToken.RefreshToken;
                StreamerSettings.StreamerBotOauthTokenExpiration = accessTokenExpiration;
                await AwsUtilities.DynamoDb.PopulateOrUpdateStreamerSettings(StreamerSettings);
            }
            else
            {
                Singleton<GlobalSettings>.Instance.DefaultBotOauthToken = botToken.AccessToken;
                Singleton<GlobalSettings>.Instance.DefaultBotRefreshToken = botToken.RefreshToken;
                Singleton<GlobalSettings>.Instance.DefaultBotOauthTokenExpiration = accessTokenExpiration;
                await AwsUtilities.DynamoDb.UpdateGlobalSettings(Singleton<GlobalSettings>.Instance);
                await AwsUtilities.DynamoDb.PopulateOrUpdateStreamerSettings(StreamerSettings);
            }
        }
        #endregion
    }
}