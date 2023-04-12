using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Api.Helix.Models.Streams.CreateStreamMarker;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    public class MarkCommand : BotCommandBase
    {
        #region Constructor
        public MarkCommand(BotCommandSettings botCommandSettings, ManagedTwitchApi twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSub, StreamerSettings streamerSettings, ILogger<JeffBot> logger) : base(botCommandSettings, twitchApiClient, twitchChatClient, twitchPubSub, streamerSettings, logger)
        {
        }
        #endregion

        #region ProcessMessage - Override
        public override async Task<bool> ProcessMessage(ChatMessage chatMessage)
        {
            #region Mark
            var isMarkMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{BotCommandSettings.TriggerWord}$");
            if (isMarkMessage.Captures.Count > 0)
            {
                await MarkStream(chatMessage);
                return true;
            }
            #endregion

            #region Mark Message
            var isMarkWithMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{BotCommandSettings.TriggerWord} .*$");
            if (isMarkWithMessage.Captures.Count > 0)
            {
                var markDescription = Regex.Match(chatMessage.Message.ToLower(), @" .*$");
                if (markDescription.Captures.Count > 0)
                {
                    await MarkStream(chatMessage, markDescription.Captures[0].Value.Trim());
                    return true;
                }
            }
            #endregion

            return false;
        }
        #endregion
        #region Initialize - Override
        public override void Initialize()
        {
        }
        #endregion

        #region MarkStream
        private async Task MarkStream(ChatMessage chatMessage, string markMessage = "Marked from bot.")
        {
            try
            {
                if (!await IsStreamLive())
                {
                    TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"Cannot mark an offline stream.");
                    return;
                }
                var mark = await TwitchApiClient.ExecuteRequest(async api => await api.Helix.Streams.CreateStreamMarkerAsync(new CreateStreamMarkerRequest { Description = markMessage, UserId = StreamerSettings.StreamerId }));
                if (markMessage != "Marked from bot.")
                {
                    TwitchChatClient.SendMessage(chatMessage.Channel, $"Stream successfully marked with description: \"{markMessage}\"");
                }
                else
                {
                    TwitchChatClient.SendMessage(chatMessage.Channel, "Stream successfully marked.");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Source == "Newtonsoft.Json")
                {
                    if (markMessage != "Marked from bot.")
                    {
                        TwitchChatClient.SendMessage(chatMessage.Channel, $"Stream successfully marked with description: \"{markMessage}\"");
                    }
                    else
                    {
                        TwitchChatClient.SendMessage(chatMessage.Channel, "Stream successfully marked.");
                    }
                }
                else
                {
                    TwitchChatClient.SendMessage(chatMessage.Channel, "Stream was NOT successfully marked.. Someone tell Jeff..");
                }
            }
        }
        #endregion
    }
}