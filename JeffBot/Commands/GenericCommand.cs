using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot;

public class GenericCommand : BotCommandBase
{
    #region Constructor
    public GenericCommand(BotCommandSettings botCommandSettings, TwitchAPI twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSubClient, StreamerSettings streamerSettings) : base(botCommandSettings, twitchApiClient, twitchChatClient, twitchPubSubClient, streamerSettings)
    {
    }
    #endregion

    #region ProcessMessage - Override
    public override async Task<bool> ProcessMessage(ChatMessage chatMessage)
    {
        // TODO: We will need to take into account variables when generating output (e.g. users, counters, whatever else we want to be a dynamic feature)
        if (chatMessage.Message.StartsWith($"!{BotCommandSettings.TriggerWord}", StringComparison.InvariantCultureIgnoreCase))
        {
            TwitchChatClient.SendMessage(chatMessage.Channel, BotCommandSettings.Output);
            return true;
        }

        foreach (var additionalTriggerWord in BotCommandSettings.AdditionalTriggerWords)
        {
            if (chatMessage.Message.StartsWith($"!{additionalTriggerWord}", StringComparison.InvariantCultureIgnoreCase))
            {
                TwitchChatClient.SendMessage(chatMessage.Channel, BotCommandSettings.Output);
                return true;
            }
        }

        foreach (var regex in BotCommandSettings.TriggerRegexes)
        {
            if (Regex.IsMatch(chatMessage.Message, regex, RegexOptions.IgnoreCase))
            {
                TwitchChatClient.SendMessage(chatMessage.Channel, BotCommandSettings.Output);
                return true;
            }
        }

        // If no trigger words or regex matches are found, return false
        return false;
    }
    #endregion
    #region Initialize - Override
    public override void Initialize()
    {
    }
    #endregion
}