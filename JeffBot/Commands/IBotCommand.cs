using System.Threading.Tasks;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using TwitchLib.PubSub.Interfaces;

namespace JeffBot
{
    public interface IBotCommand
    {
        BotCommandSettings BotCommandSettings { get; set; }
        bool IsCommandEnabled { get; }
        ManagedTwitchApi TwitchApiClient { get; set; }
        ITwitchClient TwitchChatClient { get; set; }
        ITwitchPubSub TwitchPubSubClient { get; set; }
        StreamerSettings StreamerSettings { get; set; }

        Task<bool> ProcessMessage(ChatMessage chatMessage);
        Task CheckExecutionPermissionsAndExecuteCommand(ChatMessage chatMessage);
        void Initialize();
    }
}