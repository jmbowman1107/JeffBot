using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot.Commands
{
    public class AskMeAnythingCommand : BotCommandBase
    {
        #region OpenAIAPI
        public OpenAIAPI OpenAIAPI { get; set; }
        #endregion
        #region BotFeature - Override
        public override BotFeatures BotFeature => BotFeatures.AskMeAnything;
        #endregion
        #region DefaultKeyword - Override
        public override string DefaultKeyword => "ama";
        #endregion

        #region Constructor
        public AskMeAnythingCommand(TwitchAPI twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSubClient, StreamerSettings streamerSettings) : base(twitchApiClient, twitchChatClient, twitchPubSubClient, streamerSettings)
        {
        }
        #endregion

        #region AskAnything
        public async Task AskAnything(ChatMessage chatMessage, string whatToAsk)
        {

            var test = new CompletionRequest($"You are {StreamerSettings.StreamerBotName} a bot for the streamer {StreamerSettings.StreamerName} on Twitch. All of your responses will be in 500 characters or less. /n/nI am {StreamerSettings.StreamerBotName} here to serve {StreamerSettings.StreamerName}", model: Model.DavinciText, 200, 0.5, presencePenalty: 0.1, frequencyPenalty: 0.1);
            test.Prompt = String.Join(" /n/n", test.Prompt, whatToAsk);
            var result = await OpenAIAPI.Completions.CreateCompletionAsync(test);
            TwitchChatClient.SendMessage(chatMessage.Channel, $"{result.Completions[0].Text}");
        } 
        #endregion

        #region ProcessMessage - Override
        public override void ProcessMessage(ChatMessage chatMessage)
        {
            if (IsCommandEnabled)
            {
                #region Ask Me Anything
                var isMarkWithMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{CommandKeyword} .*$");
                if (isMarkWithMessage.Captures.Count > 0)
                {
                    var questionOrText = Regex.Match(chatMessage.Message.ToLower(), @" .*$");
                    if (questionOrText.Captures.Count > 0)
                    {
                        AskAnything(chatMessage, questionOrText.Captures[0].Value.Trim()).Wait();
                    }
                }
                #endregion
            }
        }
        #endregion

        #region Initialize - Override
        public override void Initialize()
        {
            OpenAIAPI = new OpenAIAPI("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");
        } 
        #endregion
    }
}