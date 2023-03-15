using AspNet.Security.OAuth.Spotify;
using AspNet.Security.OAuth.Twitch;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;
using JeffBot.AwsUtilities;

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
