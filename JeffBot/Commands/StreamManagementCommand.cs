using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    internal class StreamManagementCommand : BotCommandBase
    {
        #region Constructor
        public StreamManagementCommand(BotCommandSettings botCommandSettings, ManagedTwitchApi twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSubClient, StreamerSettings streamerSettings, ILogger<JeffBot> logger) : base(botCommandSettings, twitchApiClient, twitchChatClient, twitchPubSubClient, streamerSettings, logger)
        {
        } 
        #endregion

        #region PrcoessMessage - Override
        public override async Task<bool> ProcessMessage(ChatMessage chatMessage)
        {
            var message = chatMessage.Message;
            var match = Regex.Match(message, @"^!(title|game|tags)(?:\s+(.*))?$");

            if (!match.Success) return false;

            var command = match.Groups[1].Value;
            var parameter = match.Groups[2].Value;

            switch (command)
            {
                case "title":
                    await ChangeTitle(chatMessage, parameter);
                    break;
                case "game":
                    await ChangeGame(chatMessage, parameter);
                    break;
                case "tags":
                    await AddOrRemoveTag(chatMessage, parameter);
                    break;
                default:
                    return false;
            }

            return true;
        }
        #endregion
        #region Initialize
        public override void Initialize()
        {
        } 
        #endregion

        #region ChangeTitle
        private async Task ChangeTitle(ChatMessage chatMessage, string title)
        {
            await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Channels.ModifyChannelInformationAsync(StreamerSettings.StreamerId, new ModifyChannelInformationRequest() { Title = title }, StreamerSettings.StreamerOauthToken));
            TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"Stream title changed to: {title}");
        }
        #endregion
        #region ChangeGame
        private async Task ChangeGame(ChatMessage chatMessage, string gameName)
        {
            var games = await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Games.GetGamesAsync(gameNames: new List<string> { gameName }));
            if (games.Games != null && games.Games.Any())
            {
                // For now we just grab the first..
                await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Channels.ModifyChannelInformationAsync(StreamerSettings.StreamerId, new ModifyChannelInformationRequest() {GameId = games.Games.First().Id}, StreamerSettings.StreamerOauthToken));
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"Game changed to: {games.Games.First().Name}");
            }
            else
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"The game {gameName} does not exist.");
            }
        }
        #endregion
        #region AddOrRemoveTag
        private async Task AddOrRemoveTag(ChatMessage chatMessage, string parameter)
        {
            var currentChannelInfoResponse = await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Channels.GetChannelInformationAsync(StreamerSettings.StreamerId));
            if (currentChannelInfoResponse.Data is not { Length: 1 })
            {
                Logger.LogError($"No streamer with id {StreamerSettings.StreamerId}");
                return;
            }

            var currentChannelInfo = currentChannelInfoResponse.Data[0];
            var currentTags = currentChannelInfo.Tags.ToList();
            var tagMatch = Regex.Match(parameter, @"^(add|remove)\s+([a-zA-Z0-9]+)$");
            if (!tagMatch.Success) return;

            var tagAction = tagMatch.Groups[1].Value;
            var tagName = tagMatch.Groups[2].Value;

            if (tagAction == "add")
            {
                await AddTag(chatMessage, currentTags, tagName);
            }
            else if (tagAction == "remove")
            {
                await RemoveTag(chatMessage, currentTags, tagName);
            }
        } 
        #endregion
        #region AddTag
        private async Task AddTag(ChatMessage chatMessage, List<string> currentTags, string tagName)
        {
            if (currentTags.Contains(tagName))
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"The tag '{tagName}' already exists.");
                return;
            }

            if (currentTags.Count == 10)
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"There are already 10 tags, please remove a tag before adding a new one.");
                return;
            }

            currentTags.Add(tagName);
            await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Channels.ModifyChannelInformationAsync(StreamerSettings.StreamerId, new ModifyChannelInformationRequest() {Tags = currentTags.ToArray()}, StreamerSettings.StreamerOauthToken));
            TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"{tagName} has been successfully added.");
        }
        #endregion
        #region RemoveTag
        private async Task RemoveTag(ChatMessage chatMessage, List<string> currentTags, string tagName)
        {
            if (!currentTags.Contains(tagName))
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"The tag {tagName} does not exist.");
                return;
            }

            currentTags.Remove(tagName);
            await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Channels.ModifyChannelInformationAsync(StreamerSettings.StreamerId, new ModifyChannelInformationRequest() {Tags = currentTags.ToArray()}, StreamerSettings.StreamerOauthToken));
            TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"{tagName} has been successfully removed.");
        }
        #endregion
    }
}
