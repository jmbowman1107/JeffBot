using AspNet.Security.OAuth.Spotify;
using AspNet.Security.OAuth.Twitch;
using JeffBot.AwsUtilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;

namespace JeffBotWebApi;

public class Startup
{
    public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
    {
        Configuration = configuration;
        HostingEnvironment = hostingEnvironment;
    }

    public IConfiguration Configuration { get; }

    private IHostEnvironment HostingEnvironment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();
        services.AddHttpContextAccessor();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
            options.KnownProxies.Clear();
            options.KnownNetworks.Clear();
        });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/signin";
            options.LogoutPath = "/signout";
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        })
        .AddOAuth<SpotifyAuthenticationOptions, DebugSpotifyOauthHandler>(SpotifyAuthenticationDefaults.AuthenticationScheme, SpotifyAuthenticationDefaults.DisplayName, options =>
        {
            options.ClientId = SecretsManager.GetSecret("SPOTIFY_CLIENT_ID").Result;
            options.ClientSecret = SecretsManager.GetSecret("SPOTIFY_CLIENT_SECRET").Result;
            options.SaveTokens = true;
            options.Scope.Add("user-read-currently-playing");
            options.Scope.Add("user-read-playback-state");
            options.CorrelationCookie.HttpOnly = true;
            options.CorrelationCookie.SameSite = SameSiteMode.None;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        })
        //.AddSpotify(options =>
        //{
        //    options.ClientId = SecretsManager.GetSecret("SPOTIFY_CLIENT_ID").Result;
        //    options.ClientSecret = SecretsManager.GetSecret("SPOTIFY_CLIENT_SECRET").Result;
        //    options.SaveTokens = true;
        //    options.Scope.Add("user-read-currently-playing");
        //    options.Scope.Add("user-read-playback-state");
        //    options.CorrelationCookie.HttpOnly = true;
        //    options.CorrelationCookie.SameSite = SameSiteMode.None;
        //    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        //    options.Events.OnRemoteFailure += OnRemoteFailure;
        //})
        //.AddTwitch(options =>
        //{
        //    options.ClientId = SecretsManager.GetSecret("TWITCH_API_CLIENT_ID").Result;
        //    options.ClientSecret = SecretsManager.GetSecret("TWITCH_API_CLIENT_SECRET").Result;
        //    options.ForceVerify = true;
        //    options.SaveTokens = true;
        //    options.CorrelationCookie.HttpOnly = true;
        //    options.CorrelationCookie.SameSite = SameSiteMode.None;
        //    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        //})
        .AddOAuth<TwitchAuthenticationOptions, DebugTwitchOauthHandler>(TwitchAuthenticationDefaults.AuthenticationScheme, TwitchAuthenticationDefaults.DisplayName, options =>
        {
            options.ClientId = SecretsManager.GetSecret("TWITCH_API_CLIENT_ID").Result;
            options.ClientSecret = SecretsManager.GetSecret("TWITCH_API_CLIENT_SECRET").Result;
            options.Scope.Add("bits:read");
            options.Scope.Add("channel:manage:broadcast");
            options.Scope.Add("channel:manage:polls");
            options.Scope.Add("channel:manage:predictions");
            options.Scope.Add("channel:manage:moderators");
            options.Scope.Add("channel:manage:vips");
            options.Scope.Add("channel:read:hype_train");
            options.Scope.Add("channel:read:polls");
            options.Scope.Add("channel:read:predictions");
            options.Scope.Add("channel:read:subscriptions");
            options.Scope.Add("channel:read:redemptions");
            options.Scope.Add("channel:read:vips");
            options.Scope.Add("clips:edit");
            options.Scope.Add("moderation:read");
            options.Scope.Add("moderator:manage:automod");
            options.Scope.Add("moderator:manage:chat_messages");
            options.Scope.Add("moderator:manage:shoutouts");
            options.Scope.Add("moderator:manage:banned_users");
            options.Scope.Add("moderator:manage:announcements");
            options.Scope.Add("moderator:read:chat_settings");
            options.Scope.Add("moderator:manage:chat_settings");
            options.Scope.Add("moderator:read:chatters");
            options.Scope.Add("moderator:read:followers");
            options.Scope.Add("moderator:read:shoutouts");
            options.Scope.Add("channel:moderate");
            options.Scope.Add("chat:edit");
            options.Scope.Add("chat:read");
            options.Scope.Add("whispers:read");
            options.Scope.Add("whispers:edit");
            options.ForceVerify = true;
            options.SaveTokens = true;
            options.CorrelationCookie.HttpOnly = true;
            options.CorrelationCookie.SameSite = SameSiteMode.None;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        services.AddMvc();
    }

    public void Configure(IApplicationBuilder app)
    {
        if (HostingEnvironment.IsDevelopment())
        {
            IdentityModelEventSource.ShowPII = true;
        }

        // Required to serve files with no extension in the .well-known folder
        var options = new StaticFileOptions()
        {
            ServeUnknownFileTypes = true,
        };

        app.UseHttpsRedirection();
        app.UseStaticFiles(options);

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseForwardedHeaders();

        app.Use((context, next) =>
        {
            context.Request.Scheme = "https";
            return next(context);
        });

        app.UseCookiePolicy(new CookiePolicyOptions
        {
            HttpOnly = HttpOnlyPolicy.Always,
            MinimumSameSitePolicy = SameSiteMode.None,
            Secure = CookieSecurePolicy.Always
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });
    }
}
