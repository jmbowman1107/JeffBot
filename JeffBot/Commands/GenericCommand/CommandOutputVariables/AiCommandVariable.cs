using System.Linq;
using System.Text.RegularExpressions;
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
            // Check if the input string is enclosed in quotes
            var pattern = @"^""([^""]+)""$";
            var match = Regex.Match(command, pattern);

            if (!match.Success)
            {
                // If it is not a quoted string, then just return an empty string
                return string.Empty;
            }

            var aiPrompt = match.Groups[1].Value;

            var askMeAnythingCommand = BotCommand.JeffBot.BotCommands.FirstOrDefault(a => a.BotCommandSettings.Name == nameof(AskMeAnythingCommand));
            if (askMeAnythingCommand != null)
            {
                return string.Empty;
            }
            else
            {
                return await ((AskMeAnythingCommand)askMeAnythingCommand).AskAnything(chatMessage, aiPrompt);
            }

            // TODO: Else invoke the AskMeAnythingCommand
        }
    }
}