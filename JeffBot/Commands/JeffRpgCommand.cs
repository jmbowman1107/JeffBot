using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace JeffBot;

public class JeffRpgCommand : BotCommandBase
{
    #region BotFeature - Override
    public override BotFeatures BotFeature => BotFeatures.JeffRpg;
    #endregion
    #region DefaultKeyword - Override
    public override string DefaultKeyword => "rpg";
    #endregion
    #region StreamElementsClient
    public StreamElementsClient StreamElementsClient { get; set; }
    #endregion

    #region Constructor
    public JeffRpgCommand(TwitchAPI twitchApiClient, TwitchClient twitchChatClient, TwitchPubSub twitchPubSubClient, StreamerSettings streamerSettings) : base(twitchApiClient, twitchChatClient, twitchPubSubClient, streamerSettings)
    {
        StreamElementsClient = new StreamElementsClient { ChannelId = streamerSettings.StreamElementsChannelId, JwtTokenString = streamerSettings.StreamElementsJwtToken };
    }
    #endregion

    #region ProcessMessage - Override
    public override void ProcessMessage(ChatMessage chatMessage)
    {
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