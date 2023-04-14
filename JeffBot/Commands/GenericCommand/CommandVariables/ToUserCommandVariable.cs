using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public class ToUserCommandVariable : CommandVariableBase
    {
        #region Keyword - Override
        public override string Keyword { get; set; } = "touser";
        #endregion
        #region Description - Override
        public override string Description { get; set; } = "Use this variable to show the user who is mentioned in this command, or if there is none, then return the name of the sender.";
        #endregion
        #region UsageExample - Override
        public override string UsageExample { get; set; } = "{ touser }";
        #endregion

        #region Constructor
        public UserCommandVariable(BotCommandBase botCommand) : base(botCommand)
        {
        }
        #endregion

        #region ProcessVariable - Override
        public override async Task<string> ProcessVariable(string command, ChatMessage chatMessage)
        {
            var userName = chatMessage.Message.Split(' ').Skip(1).FirstOrDefault();
            await Task.Run(() => userName ?? chatMessage.DisplayName);
            return userName ?? chatMessage.DisplayName;
        } 
        #endregion
    }
}