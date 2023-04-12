using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public class JeffRpgCommand : BotCommandBase
    {
        #region StreamElementsClient
        public StreamElementsClient StreamElementsClient { get; set; }
        #endregion

        #region Constructor
        public JeffRpgCommand(BotCommandSettings botCommandSettings, JeffBot jeffBot) : base(botCommandSettings, jeffBot)
        {
            StreamElementsClient = new StreamElementsClient { ChannelId = jeffBot.StreamerSettings.StreamElementsChannelId, JwtTokenString = jeffBot.StreamerSettings.StreamElementsJwtToken };
        }
        #endregion

        #region ProcessMessage - Override
        public override Task<bool> ProcessMessage(ChatMessage chatMessage)
        {
            return null;
            // TODO: Handle each possible command for the RPG such as:
            // join
            // leave
            // explore
            // stats
            // party
            // ability
        }
        #endregion
        #region Initialize - Override
        public override void Initialize()
        {
            // TODO: Fetch existing player states from Amazon dynamo db (maybe?)
        }
        #endregion
    }
}