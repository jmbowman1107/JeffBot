using System;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot
{
    internal class StreamManagementCommand : BotCommandBase
    {
        public StreamManagementCommand(BotCommandSettings botCommandSettings, ManagedTwitchApi twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSubClient, StreamerSettings streamerSettings) : base(botCommandSettings, twitchApiClient, twitchChatClient, twitchPubSubClient, streamerSettings)
        {
        }

        public override Task<bool> ProcessMessage(ChatMessage chatMessage)
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
