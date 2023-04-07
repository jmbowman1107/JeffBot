using System.Diagnostics;
using AspNet.Security.OAuth.Spotify;
using AspNet.Security.OAuth.Twitch;
using JeffBot.AwsUtilities;
using JeffBotWebApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using TwitchLib.Api;

namespace JeffBotWebApi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                var accessToken = _httpContextAccessor.HttpContext.GetTokenAsync(SpotifyAuthenticationDefaults.AuthenticationScheme, "access_token").Result;
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    _logger.LogInformation($"Spotify Access Token: {accessToken}");
                    _logger.LogInformation($"Spotify Refresh Token: {_httpContextAccessor.HttpContext.GetTokenAsync(SpotifyAuthenticationDefaults.AuthenticationScheme, "refresh_token").Result}");
                }

                accessToken = _httpContextAccessor.HttpContext.GetTokenAsync(TwitchAuthenticationDefaults.AuthenticationScheme, "access_token").Result;
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    var api = new TwitchAPI();
                    api.Settings.ClientId = SecretsManager.GetSecret("TWITCH_API_CLIENT_ID").Result;
                    api.Settings.AccessToken = accessToken;
                    var user = api.Helix.Users.GetUsersAsync().Result;
                    _logger.LogInformation($"Twitch Id: {user.Users[0].Id}");
                    _logger.LogInformation($"Twitch User: {user.Users[0].DisplayName}");
                    _logger.LogInformation($"Twitch Access Token: {accessToken}");
                    _logger.LogInformation($"Twitch Refresh Token: {_httpContextAccessor.HttpContext.GetTokenAsync(TwitchAuthenticationDefaults.AuthenticationScheme, "refresh_token").Result}");
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}