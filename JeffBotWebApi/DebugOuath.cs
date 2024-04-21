using System.Text.Encodings.Web;
using AspNet.Security.OAuth.Spotify;
using AspNet.Security.OAuth.Twitch;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;

namespace JeffBotWebApi
{
    public class DebugSpotifyOauthHandler : SpotifyAuthenticationHandler
    {
        public DebugSpotifyOauthHandler(IOptionsMonitor<SpotifyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            context = new OAuthCodeExchangeContext(context.Properties, context.Code, context.RedirectUri.Replace("http://", "https://"));
            return await base.ExchangeCodeAsync(context);
        }
    }

    public class DebugTwitchOauthHandler : TwitchAuthenticationHandler
    {
        public DebugTwitchOauthHandler(IOptionsMonitor<TwitchAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            context = new OAuthCodeExchangeContext(context.Properties, context.Code, context.RedirectUri.Replace("http://", "https://"));
            return await base.ExchangeCodeAsync(context);
        }
    }
}
