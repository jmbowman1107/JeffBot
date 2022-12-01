using TwitchLib.Client.Models;

namespace JeffBot
{
    public interface IBotCommand
    {
        void ProcessMessage(ChatMessage chatMessage);

        void Initialize();
    }
}