using JeffBotWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using AspNet.Security.OAuth.Spotify;
using AspNet.Security.OAuth.Twitch;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

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
                _logger.LogInformation($"Spotify Access Token: {_httpContextAccessor.HttpContext.GetTokenAsync(SpotifyAuthenticationDefaults.AuthenticationScheme, "access_token").Result}");
                _logger.LogInformation($"Spotify Refresh Token: {_httpContextAccessor.HttpContext.GetTokenAsync(SpotifyAuthenticationDefaults.AuthenticationScheme, "refresh_token").Result}");
                _logger.LogInformation($"Twitch Access Token: {_httpContextAccessor.HttpContext.GetTokenAsync(TwitchAuthenticationDefaults.AuthenticationScheme, "access_token").Result}");
                _logger.LogInformation($"Twitch Refresh Token: {_httpContextAccessor.HttpContext.GetTokenAsync(TwitchAuthenticationDefaults.AuthenticationScheme, "refresh_token").Result}");
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