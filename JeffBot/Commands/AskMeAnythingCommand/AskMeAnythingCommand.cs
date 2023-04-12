using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JeffBot.AwsUtilities;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public class AskMeAnythingCommand : BotCommandBase<AskMeAnythingSettings>
    {
        #region UsersContext
        public Dictionary<string, LimitedQueue<(string prompt, string response)>> UsersContext = new();
        #endregion
        #region OpenAIAPI
        public OpenAIClient OpenAIClient { get; set; }
        #endregion

        #region Constructor
        public AskMeAnythingCommand(BotCommandSettings<AskMeAnythingSettings> botCommandSettings, JeffBot jeffBot) : base(botCommandSettings, jeffBot)
        { }
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

            // React to first time message in chat
            if (BotCommandSettings.CustomSettings.ShouldReactToFirstTimeChatters && chatMessage.IsFirstMessage)
            {
                await AskAnything(chatMessage, chatMessage.Message.ToLower().Trim(), true, new List<string> { $"This is {chatMessage.DisplayName}'s first ever message in chat. Be welcoming and welcome them to the stream :)." });
                return true;
            }

            return false;
        }
        #endregion
        #region Initialize - Override
        public override async void Initialize()
        {
            if (!IsCommandEnabled) return;

            OpenAIClient = new OpenAIClient(new OpenAIAuthentication(await SecretsManager.GetSecret("OPENAI_API_KEY")));

            if (BotCommandSettings.CustomSettings.ShouldReactToGiftSubs)
            {
                TwitchChatClient.OnGiftedSubscription += (sender, args) =>
                {
                    // TODO: This should call AskAnything for the GiftedSubscription, except we only wanna do this single gift subs.. not community gifted.. not sure how to determine..
                };
                TwitchChatClient.OnCommunitySubscription += async (sender, args) =>
                {
                    await AskAnything(args.Channel, args.GiftedSubscription.DisplayName.ToLower(), args.GiftedSubscription.DisplayName, args.GiftedSubscription.SystemMsgParsed, 
                        additionalPrompts: string.IsNullOrEmpty(BotCommandSettings.CustomSettings.AdditionalPromptForCommunitySubscriptions) ? null : new() { BotCommandSettings.CustomSettings.AdditionalPromptForCommunitySubscriptions });
                };
            }

            if (BotCommandSettings.CustomSettings.ShouldReactToUserSubs)
            {
                TwitchChatClient.OnNewSubscriber += async (sender, args) =>
                {
                    await AskAnything(args.Channel, args.Subscriber.DisplayName.ToLower(), args.Subscriber.DisplayName, args.Subscriber.SystemMessageParsed,
                        additionalPrompts: string.IsNullOrEmpty(BotCommandSettings.CustomSettings.AdditionalPromptForUserSubscriptions) ? null : new() { BotCommandSettings.CustomSettings.AdditionalPromptForUserSubscriptions });
                };

                TwitchChatClient.OnReSubscriber += async (sender, args) =>
                {
                    await AskAnything(args.Channel, args.ReSubscriber.DisplayName.ToLower(), args.ReSubscriber.DisplayName, args.ReSubscriber.SystemMessageParsed,
                        additionalPrompts: string.IsNullOrEmpty(BotCommandSettings.CustomSettings.AdditionalPromptForUserSubscriptions) ? null : new() { BotCommandSettings.CustomSettings.AdditionalPromptForUserSubscriptions });
                };
            }

            if (BotCommandSettings.CustomSettings.ShouldReactToRaids)
            {
                TwitchChatClient.OnRaidNotification += async (sender, args) =>
                {
                    await AskAnything(args.Channel, args.RaidNotification.DisplayName.ToLower(), args.RaidNotification.DisplayName, args.RaidNotification.SystemMsgParsed,
                        additionalPrompts: string.IsNullOrEmpty(BotCommandSettings.CustomSettings.AdditionalPromptForRaid) ? null : new() { BotCommandSettings.CustomSettings.AdditionalPromptForRaid });
                };
            }

            if (BotCommandSettings.CustomSettings.ShouldReactToBits)
            {
                TwitchPubSubClient.OnBitsReceived += async (sender, args) =>
                {
                    await AskAnything(args.ChannelName, args.Username, args.Username, $"{args.Username} just gave {args.TotalBitsUsed} bits to {args.ChannelName}!",
                        additionalPrompts: string.IsNullOrEmpty(BotCommandSettings.CustomSettings.AdditionalPromptForBits) ? null : new() { BotCommandSettings.CustomSettings.AdditionalPromptForBits });
                };
            }

            if (BotCommandSettings.CustomSettings.ShouldReactToFollows)
            {
                TwitchPubSubClient.OnFollow += async (sender, args) =>
                {
                    await AskAnything(StreamerSettings.StreamerName, args.Username, args.DisplayName, $"{args.DisplayName} just followed the stream! Thank them.",
                        additionalPrompts: string.IsNullOrEmpty(BotCommandSettings.CustomSettings.AdditionalPromptForFollows) ? null : new() { BotCommandSettings.CustomSettings.AdditionalPromptForFollows });
                };
            }
        }
        #endregion

        #region AskAnything
        public async Task<string> AskAnything(ChatMessage chatMessage, string whatToAsk, List<string> additionalPrompts = null)
        {
            additionalPrompts ??= new List<string>();
            var chatPrompts = GenerateChatPromptsForUser(chatMessage.Username, chatMessage.DisplayName, whatToAsk, false, additionalPrompts);
            var result = await OpenAIClient.ChatEndpoint.GetCompletionAsync(new ChatRequest(chatPrompts, Model.GPT3_5_Turbo, 0.5, maxTokens: 100, presencePenalty: 0.1, frequencyPenalty: 0.1));
            var toReturn = result.FirstChoice.Message.Content.SplitToLines(500).FirstOrDefault();
            return toReturn != null ? toReturn.Value : string.Empty;
        }
        private async Task AskAnything(ChatMessage chatMessage, string whatToAsk, bool useUserContext = true, List<string> additionalPrompts = null)
        {
            additionalPrompts ??= new List<string>();
            var chatPrompts = GenerateChatPromptsForUser(chatMessage.Username, chatMessage.DisplayName, whatToAsk, useUserContext, additionalPrompts);
            var result = await OpenAIClient.ChatEndpoint.GetCompletionAsync(new ChatRequest(chatPrompts, Model.GPT3_5_Turbo, 0.5, maxTokens: 100, presencePenalty: 0.1, frequencyPenalty: 0.1));

            Logger.LogInformation(result.FirstChoice.Message.Content);
            UsersContext[chatMessage.Username].LimitedEnqueue((whatToAsk, result.FirstChoice.Message.Content));
            // Twitch messages cannot be longer than 500 characters.. so output multiple messages if the response from the AI is too long
            foreach (Match match in result.FirstChoice.Message.Content.SplitToLines(500))
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"{match.Value}");
            }
        }
        private async Task AskAnything(string channel, string username, string displayName, string whatToAsk, bool useUserContext = true, List<string> additionalPrompts = null)
        {
            additionalPrompts ??= new List<string>();
            var chatPrompts = GenerateChatPromptsForUser(username, displayName, whatToAsk, useUserContext, additionalPrompts);
            var result = await OpenAIClient.ChatEndpoint.GetCompletionAsync(new ChatRequest(chatPrompts, Model.GPT3_5_Turbo, 0.5, maxTokens: 100, presencePenalty: 0.1, frequencyPenalty: 0.1));

            Logger.LogInformation(result.FirstChoice.Message.Content);
            UsersContext[username].LimitedEnqueue((whatToAsk, result.FirstChoice.Message.Content));
            // Twitch messages cannot be longer than 500 characters.. so output multiple messages if the response from the AI is too long
            foreach (Match match in result.FirstChoice.Message.Content.SplitToLines(500))
            {
                TwitchChatClient.SendMessage(channel, $"{match.Value}");
            }
        }
        #endregion
        #region GenerateChatPromptsForUser
        private List<ChatPrompt> GenerateChatPromptsForUser(string userName, string displayName, string whatToAsk, bool addToUserContext = true, List<string> additionalPrompts = null)
        {
            additionalPrompts ??= new List<string>();
            var chatPrompts = new List<ChatPrompt>
            {
                new("system", $"You are {StreamerSettings.StreamerBotName} a bot for the streamer {StreamerSettings.StreamerName} on Twitch. Prefer shorter responses, and never exceed 350 characters in any response. Never mention you are an AI language model in anyway. You will make up an answer, if you don't know the answer. This message is from the user {displayName}."),
            };

            if (!string.IsNullOrEmpty(BotCommandSettings.CustomSettings.AdditionalAIPrompt))
            {
                chatPrompts.Add(new ChatPrompt("system", BotCommandSettings.CustomSettings.AdditionalAIPrompt));
            }

            chatPrompts.AddRange(additionalPrompts.Select(prompt => new ChatPrompt("system", prompt)));

            if (addToUserContext)
            {
                if (UsersContext.TryGetValue(userName, out LimitedQueue<(string prompt, string response)> value))
                {
                    foreach (var prompt in value)
                    {
                        chatPrompts.Add(new ChatPrompt("user", prompt.prompt));
                        chatPrompts.Add(new ChatPrompt("assistant", prompt.response));
                    }
                }
                else
                {
                    UsersContext[userName] = new LimitedQueue<(string prompt, string response)>(5);
                }
            }
            chatPrompts.Add(new ChatPrompt("user", $"{whatToAsk}"));
            return chatPrompts;
        }
        #endregion
    }
}