using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public class AiCommandVariable : CommandOutputVariableBase
    {
        #region Keyword - Override
        public override string Keyword { get; set; } = "ai";
        #endregion
        #region Description - Override
        public override string Description { get; set; } = "Use this variable to have the AI generate text based on an output";
        #endregion

        #region Constructor
        public AiCommandVariable(BotCommandBase botCommand) : base(botCommand)
        {
        }

        #endregion

        public override async Task<string> ProcessVariable(string command, ChatMessage chatMessage)
        {
            string aiPrompt = string.Empty;
            // TODO: For this command variable, the variable should be a quoted string, if it is not a quoted string, then just return and empty string

            var askMeAnythingCommand = BotCommand.JeffBot.BotCommands.FirstOrDefault(a => a.BotCommandSettings.Name == nameof(AskMeAnythingCommand));
            if (askMeAnythingCommand != null)
            {
                return string.Empty;
            }
            else
            {
                var test = (AskMeAnythingCommand)askMeAnythingCommand;

                await test.AskAnything(chatMessage, aiPrompt);
            }

            return string.Empty;;

            // TODO: Else invoke the AskMeAnythingCommand
        }
    }
}