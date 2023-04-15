using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public class HttpGetCommandVariable : CommandVariableBase
    {
        private static readonly HttpClient HttpClient = new();

        #region Keyword - Override
        public override string Keyword { get; set; } = "httpget";
        #endregion
        #region Description - Override
        public override string Description { get; set; } = "Use this to send a get request to a URL, will return the text response if successful, else will return an error.";
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
            (string jsonTokenPath, string url) = SplitCommand(command);

            if (!IsWellFormedUrl(url)) return "Not a valid URL";

            try
            {
                string result = await FetchData(url);
                return jsonTokenPath == null ? result : ExtractJsonTokenValue(result, jsonTokenPath);
            }
            catch (Exception ex)
            {
                BotCommand.Logger.LogError($"Error processing command: {command}", ex);
                return "Error getting response from server.";
            }
        } 
        #endregion

        #region SplitCommand
        private (string, string) SplitCommand(string command)
        {
            var nonUrlColonPattern = @"(?<!https?):|(?<=\s):|(^:)";
            var parts = Regex.Split(command, nonUrlColonPattern);

            return parts.Length > 1 ? (parts[0].Trim(), parts[1].Trim()) : (null, command);
        }
        #endregion
        #region IsWellFormedUrl
        private bool IsWellFormedUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute)
                   && (new Uri(url).Scheme == Uri.UriSchemeHttps || new Uri(url).Scheme == Uri.UriSchemeHttp);
        }
        #endregion
        #region FetchData
        private async Task<string> FetchData(string url)
        {
            return await HttpClient.GetStringAsync(new Uri(url));
        }
        #endregion
        #region ExtractJsonTokenValue
        private string ExtractJsonTokenValue(string json, string tokenPath)
        {
            try
            {
                var jObject = JObject.Parse(json);
                return (string)jObject.SelectToken(tokenPath, true);
            }
            catch (JsonException ex)
            {
                BotCommand.Logger.LogError($"Error parsing JSON output: {json}", ex);
                return $"Error parsing JSON output. - {ex.Message}";
            }
            catch (Exception ex)
            {
                BotCommand.Logger.LogError($"Error parsing JSON output: {json}", ex);
                return $"Error parsing JSON output.";
            }
        } 
        #endregion
    }
}