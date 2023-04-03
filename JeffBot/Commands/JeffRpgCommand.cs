using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot;

public class JeffRpgCommand : BotCommandBase
{
    #region StreamElementsClient
    public StreamElementsClient StreamElementsClient { get; set; }
    #endregion

    #region Constructor
    public JeffRpgCommand(BotCommandSettings botCommandSettings, ManagedTwitchApi twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSubClient, StreamerSettings streamerSettings, ILogger<JeffBot> logger) : base(botCommandSettings, twitchApiClient, twitchChatClient, twitchPubSubClient, streamerSettings, logger)
    {
        StreamElementsClient = new StreamElementsClient { ChannelId = streamerSettings.StreamElementsChannelId, JwtTokenString = streamerSettings.StreamElementsJwtToken };
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