using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams.CreateStreamMarker;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    public class MarkCommand : BotCommandBase
    {
        #region Constructor
        public MarkCommand(BotCommandSettings botCommandSettings, TwitchAPI twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSub, StreamerSettings streamerSettings) : base(botCommandSettings, twitchApiClient, twitchChatClient, twitchPubSub, streamerSettings)
        {
        }
        #endregion

        #region MarkStream
        private async Task MarkStream(ChatMessage chatMessage, string markMessage = "Marked from bot.")
        {
            try
            {
                if (chatMessage.IsVip || chatMessage.IsModerator || chatMessage.IsBroadcaster)
                {
                    var mark = await TwitchApiClient.Helix.Streams.CreateStreamMarkerAsync(new CreateStreamMarkerRequest { Description = markMessage, UserId = StreamerSettings.StreamerId });
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
                    TwitchChatClient.SendMessage(chatMessage.Channel, $"Sorry {chatMessage.Username}, only {chatMessage.Channel}, VIPS, and Moderators can mark the stream.");
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

        #region ProcessMessage - IBotCommand Member
        public override async Task ProcessMessage(ChatMessage chatMessage)
        {
            #region Mark
            var isMarkMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{BotCommandSettings.TriggerWord}$");
            if (isMarkMessage.Captures.Count > 0)
            {
                await MarkStream(chatMessage);
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
                }
            }
            #endregion
        }
        #endregion
        #region Initialize - IBotCommand Member
        public override void Initialize()
        {
        } 
        #endregion
    }
}