using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    public class AskMeAnythingCommand : BotCommandBase
    {
        #region OpenAIAPI
        public OpenAIClient OpenAIClient { get; set; }
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
            var chatPrompts = new List<ChatPrompt>
            {
                new("system", $"You are {StreamerSettings.StreamerBotName} a bot for the streamer {StreamerSettings.StreamerName} on Twitch. Do NOT exceed 500 characters in any response. {StreamerSettings.AdditionalAIPrompt}."),
                new("system", $"This message is from the user {chatMessage.DisplayName}."),
                new("system", $"You will make up an answer, if you don't know the answer."),
                new("user", $"{whatToAsk}")
            };
            var result = await OpenAIClient.ChatEndpoint.GetCompletionAsync(new ChatRequest(chatPrompts, Model.GPT3_5_Turbo, 0.5, maxTokens: 200, presencePenalty: 0.1, frequencyPenalty: 0.1 ));
            Console.WriteLine(result.FirstChoice.Message.Content);
            // Twitch messages cannot be longer than 500 characters.. so output multiple messages if the response from the AI is too long
            foreach (Match match in SplitToLines(result.FirstChoice.Message.Content, 500))
            {
                TwitchChatClient.SendMessage(chatMessage.Channel, $"{match.Value}");
            }
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
            OpenAIClient = new OpenAIClient(new OpenAIAuthentication("ADD THIS"));
        }
        #endregion

        #region SplitToLines
        public MatchCollection SplitToLines(string stringToSplit, int maximumLineLength)
        {
            return Regex.Matches(stringToSplit, @"(.{1," + maximumLineLength + @"})(?:\s|$)");
        } 
        #endregion
    }
}