namespace JeffBot
{
    #region BotFeatureName
    public enum BotFeatureName
    {
        BanHate,
        Heist,
        JeffRpg,
        Clip,
        AdvancedClip,
        Mark,
        AskMeAnything,
        SongManagement,
        StreamManagement
    }
    #endregion

    #region FeaturePermissionLevel
    public enum FeaturePermissionLevel
    {
        Everyone,
        LoyalUser,
        Subscriber,
        Vip,
        Mod,
        SuperMod,
        Broadcaster
    }
    #endregion

    #region CommandAvailability
    public enum CommandAvailability
    {
        Online,
        Offline,
        Both
    }
    #endregion

    #region MusicServiceProvider
    public enum MusicServiceProvider
    {
        Spotify,
        YouTube,
        AppleMusic,
        SoundCloud,
        AmazonMusic,
        None
    } 
    #endregion
}