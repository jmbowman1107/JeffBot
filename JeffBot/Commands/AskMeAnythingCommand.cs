using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JeffBot.AwsUtilities;
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
        #region UsersContext
        public Dictionary<string, LimitedQueue<(string prompt, string response)>> UsersContext = new();
        #endregion
        #region OpenAIAPI
        public OpenAIClient OpenAIClient { get; set; }
        #endregion

        #region Constructor
        public AskMeAnythingCommand(BotCommandSettings botCommandSettings, TwitchAPI twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSubClient, StreamerSettings streamerSettings) : base(botCommandSettings, twitchApiClient, twitchChatClient, twitchPubSubClient, streamerSettings)
        {
        }
        #endregion

        #region AskAnything
        public async Task AskAnything(ChatMessage chatMessage, string whatToAsk)
        {
            var chatPrompts = GenerateChatPromptsForUser(chatMessage, whatToAsk);
            var result = await OpenAIClient.ChatEndpoint.GetCompletionAsync(new ChatRequest(chatPrompts, Model.GPT3_5_Turbo, 0.5, maxTokens: 100, presencePenalty: 0.1, frequencyPenalty: 0.1));

            Console.WriteLine(result.FirstChoice.Message.Content);
            UsersContext[chatMessage.Username].LimitedEnqueue((whatToAsk, result.FirstChoice.Message.Content));
            // Twitch messages cannot be longer than 500 characters.. so output multiple messages if the response from the AI is too long
            foreach (Match match in result.FirstChoice.Message.Content.SplitToLines(500))
            {
                var command = new ChatCommand(chatMessage);
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"{match.Value}");
            }
        }
        #endregion

        #region ProcessMessage - Override
        public override async Task<bool> ProcessMessage(ChatMessage chatMessage)
        {
            // Never respond to yourself.. avoid any kind of looping
            if (chatMessage.Username.ToLower() == StreamerSettings.StreamerBotName.ToLower()) return false;

            var isAmaClearMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{BotCommandSettings.TriggerWord} clear$");
            if (isAmaClearMessage.Captures.Count > 0)
            {
                UsersContext[chatMessage.Username.ToLower()] = new LimitedQueue<(string prompt, string response)>(5);
                return false;
            }

            var isAmaClearAllMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{BotCommandSettings.TriggerWord} clear all$");
            if (isAmaClearAllMessage.Captures.Count > 0)
            {
                if (chatMessage.IsModerator || chatMessage.IsBroadcaster)
                {
                    UsersContext = new Dictionary<string, LimitedQueue<(string prompt, string response)>>();
                    return false;
                }
            }

            var isAmaWithMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{BotCommandSettings.TriggerWord} .*$");
            if (isAmaWithMessage.Captures.Count > 0)
            {
                var questionOrText = Regex.Match(chatMessage.Message.ToLower(), @" .*$");
                if (questionOrText.Captures.Count > 0)
                {
                    await AskAnything(chatMessage, questionOrText.Captures[0].Value.Trim());
                    return true;
                }
            }

            var isTalkingToBot = Regex.Match(chatMessage.Message.ToLower(), @$"{StreamerSettings.StreamerBotName.ToLower()}");
            if (isTalkingToBot.Captures.Count > 0)
            {
                await AskAnything(chatMessage, chatMessage.Message.ToLower().Trim());
                return true;
            }

            return false;
        }
        #endregion
        #region Initialize - Override
        public override async void Initialize()
        {
            OpenAIClient = new OpenAIClient(new OpenAIAuthentication(await SecretsManager.GetSecret("OPENAI_API_KEY")));
        }
        #endregion

        #region GenerateChatPromptsForUser
        private List<ChatPrompt> GenerateChatPromptsForUser(ChatMessage chatMessage, string whatToAsk)
        {
            var chatPrompts = new List<ChatPrompt>
            {
                new("system", $"You are {StreamerSettings.StreamerBotName} a bot for the streamer {StreamerSettings.StreamerName} on Twitch. Prefer shorter responses, and never exceed 350 characters in any response. Never mention you are an AI language model in anyway. You will make up an answer, if you don't know the answer. This message is from the user {chatMessage.DisplayName}."),
            };

            if (!string.IsNullOrEmpty(StreamerSettings.AdditionalAIPrompt))
            {
                chatPrompts.Add(new ChatPrompt("system",  StreamerSettings.AdditionalAIPrompt));
            }

            if (UsersContext.ContainsKey(chatMessage.Username))
            {
                foreach (var prompt in UsersContext[chatMessage.Username])
                {
                    chatPrompts.Add(new ChatPrompt("user", prompt.prompt));
                    chatPrompts.Add(new ChatPrompt("assistant", prompt.response));
                }
            }
            else
            {
                UsersContext[chatMessage.Username] = new LimitedQueue<(string prompt, string response)>(5);
            }

            chatPrompts.Add(new ChatPrompt("user", $"{whatToAsk}"));
            return chatPrompts;
        }
        #endregion
    }
}