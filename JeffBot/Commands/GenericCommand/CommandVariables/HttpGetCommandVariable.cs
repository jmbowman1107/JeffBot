using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public class HttpGetCommandVariable : CommandVariableBase
    {
        #region Keyword - Override
        public override string Keyword { get; set; } = "httpget";
        #endregion
        #region Description - Override
        public override string Description { get; set; } = "Use this to send a get request to a URL, will return string (first 100 characters) if string, else will return an error.";
        #endregion
        #region UsageExample - Override
        public override string UsageExample { get; set; } = "{ httpget:https://api.jeffbot.com/samplehttpget }";
        #endregion

        #region Constructor
        public HttpGetCommandVariable(BotCommandBase botCommand) : base(botCommand)
        {
        }
        #endregion

        #region ProcessVariable - Override
        public override async Task<string> ProcessVariable(string command, ChatMessage chatMessage)
        {
            if (Uri.IsWellFormedUriString(command, UriKind.Absolute))
            {
                var uri = new Uri(command);
                if (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                {
                    var httpClient = new HttpClient();
                    try
                    {
                        var result = await httpClient.GetStringAsync(uri);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        BotCommand.Logger.LogError($"Error sending GET request to {command}", ex.ToString());
                        return "Error getting response from server.";
                    }
                }
            }
            return "Not a valid URL";
        } 
        #endregion
    }
}