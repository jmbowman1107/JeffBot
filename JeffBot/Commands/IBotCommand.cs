using TwitchLib.Client.Models;

namespace JeffBot
{
    public interface IBotCommand
    {
        void CheckExecutionPermissionsAndProcessMessage(ChatMessage chatMessage);

        void Initialize();
    }
}