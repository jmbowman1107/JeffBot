using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public abstract class CommandOutputVariableBase : ICommandOutputVariable
    {
        #region BotCommand
        public BotCommandBase BotCommand { get; set; }
        #endregion

        #region Keyword - Abstract
        public abstract string Keyword { get; set; }
        #endregion
        #region Description - Abstract
        public abstract string Description { get; set; }
        #endregion

        #region Constructor
        protected CommandOutputVariableBase(BotCommandBase botCommand)
        {
            BotCommand = botCommand;
        }
        #endregion

        #region ProcessVariable - Abstract
        public abstract Task<string> ProcessVariable(string variable, ChatMessage chatMessage); 
        #endregion
    }
}