using System;
using System.Text.RegularExpressions;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams.CreateStreamMarker;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    public class MarkCommand : BotCommandBase
    {
        #region BotFeature - Override
        public override BotFeatures BotFeature => BotFeatures.Mark;
        #endregion
        #region DefaultKeyword - Override
        public override string DefaultKeyword => "mark";
        #endregion

        #region Constructor
        public MarkCommand(TwitchAPI twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSub, StreamerSettings streamerSettings) : base(twitchApiClient, twitchChatClient, twitchPubSub, streamerSettings)
        {
        }
        #endregion

        #region MarkStream
        private void MarkStream(ChatMessage chatMessage, string markMessage = "Marked from bot.")
        {
            try
            {
                if (chatMessage.IsVip || chatMessage.IsModerator || chatMessage.IsBroadcaster)
                {
                    var mark = TwitchApiClient.Helix.Streams.CreateStreamMarkerAsync(new CreateStreamMarkerRequest { Description = markMessage, UserId = StreamerSettings.StreamerId }).Result;
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
        public override void ProcessMessage(ChatMessage chatMessage)
        {
            #region Mark
            var isMarkMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{CommandKeyword}$");
            if (isMarkMessage.Captures.Count > 0)
            {
                MarkStream(chatMessage);
            }
            #endregion

            #region Mark Message
            var isMarkWithMessage = Regex.Match(chatMessage.Message.ToLower(), @$"^!{CommandKeyword} .*$");
            if (isMarkWithMessage.Captures.Count > 0)
            {
                var markDescription = Regex.Match(chatMessage.Message.ToLower(), @" .*$");
                if (markDescription.Captures.Count > 0)
                {
                    MarkStream(chatMessage, markDescription.Captures[0].Value.Trim());
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