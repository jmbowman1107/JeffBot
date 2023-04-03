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
        private DateTime _accessTokenExpiration;
        private string _refreshToken;

        #region StreamerSettings
        public StreamerSettings StreamerSettings { get; set; }
        #endregion
        #region TwitchApi
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
            TwitchApi.Settings.AccessToken = !StreamerSettings.UseDefaultBot ? StreamerSettings.StreamerBotOauthToken : GlobalSettingsSingleton.Instance.DefaultBotOauthToken;
            _refreshToken = !StreamerSettings.UseDefaultBot ? StreamerSettings.StreamerBotRefreshToken : GlobalSettingsSingleton.Instance.DefaultBotRefreshToken; ;
            _accessTokenExpiration = DateTime.Now.AddMinutes(235); // Tokens are typically good for 4 hours..
        }
        #endregion

        #region RefreshAccessTokenAsync
        public async Task<RefreshResponse> RefreshAccessTokenAsync()
        {
            var newToken = await TwitchApi.Auth.RefreshAuthTokenAsync(_refreshToken, _clientSecret, _clientId);
            await UpdateTokenDetails(newToken);
            return newToken;
        }
        #endregion
        #region ExecuteRequest
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
            if (DateTime.Now >= _accessTokenExpiration)
            {
                await RefreshAccessTokenAsync();
            }
        }
        #endregion
        #region UpdateTokenDetails
        private async Task UpdateTokenDetails(RefreshResponse newToken)
        {
            TwitchApi.Settings.AccessToken = newToken.AccessToken;
            _refreshToken = newToken.RefreshToken;
            _accessTokenExpiration = DateTime.Now.AddSeconds(newToken.ExpiresIn - 1800); // Subtract 30 minutes as a buffer

            if (!StreamerSettings.UseDefaultBot)
            {
                StreamerSettings.StreamerBotOauthToken = newToken.AccessToken;
                StreamerSettings.StreamerBotRefreshToken = newToken.RefreshToken;
                await AwsUtilities.DynamoDb.PopulateOrUpdateStreamerSettings(StreamerSettings);
            }
            else
            {
                GlobalSettingsSingleton.Instance.DefaultBotOauthToken = newToken.AccessToken;
                GlobalSettingsSingleton.Instance.DefaultBotRefreshToken = newToken.RefreshToken;
                await AwsUtilities.DynamoDb.UpdateGlobalSettings(GlobalSettingsSingleton.Instance);
            }
        }
        #endregion
    }
}