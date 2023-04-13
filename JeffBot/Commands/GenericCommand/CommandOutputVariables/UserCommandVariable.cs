using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public class UserCommandVariable : CommandOutputVariableBase
    {
        #region Keyword - Override
        public override string Keyword { get; set; } = "user";
        #endregion
        #region Description - Override
        public override string Description { get; set; } = "Use this variable to show the user who activated the command";
        #endregion
        #region UsageExample - Override
        public override string UsageExample { get; set; } = "{ user }";
        #endregion

        #region Constructor
        public UserCommandVariable(BotCommandBase botCommand) : base(botCommand)
        {
        }
        #endregion

        #region ProcessVariable - Override
        public override async Task<string> ProcessVariable(string command, ChatMessage chatMessage)
        {
            await Task.Run(() => chatMessage.DisplayName);
            return chatMessage.DisplayName;
        } 
        #endregion
    }
}