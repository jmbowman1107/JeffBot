using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public class AiCommandVariable : CommandVariableBase
    {
        #region Keyword - Override
        public override string Keyword { get; set; } = "ai";
        #endregion
        #region Description - Override
        public override string Description { get; set; } = "Use this variable to have the AI generate text based on an input.";
        #endregion
        #region UsageExample - Override
        public override string UsageExample { get; set; } = "{ ai: tell me a random dad joke }";
        #endregion

        #region Constructor
        public AiCommandVariable(BotCommandBase botCommand) : base(botCommand)
        {
        }

        #endregion

        #region ProcessVariable - Override
        public override async Task<string> ProcessVariable(string command, ChatMessage chatMessage)
        {
            var askMeAnythingCommand = BotCommand.JeffBot.BotCommands.FirstOrDefault(a => a is AskMeAnythingCommand);
            if (askMeAnythingCommand == null) return string.Empty;

            try
            {
                return await ((AskMeAnythingCommand)askMeAnythingCommand).AskAnything(chatMessage, command);
            }
            catch
            {
                return string.Empty;
            }
        } 
        #endregion
    }
}