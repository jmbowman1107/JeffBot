using System;
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
    public override Task ProcessMessage(ChatMessage chatMessage)
    {
        // TODO: Check if trigger word
        // TODO: Check if any of the additional trigger words
        // TODO: Check if matches any regex
        // TODO: Execute what it should return
        throw new NotImplementedException();
    }
    #endregion
    #region Override
    public override void Initialize()
    {
    } 
    #endregion
}