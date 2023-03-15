namespace JeffBot
{
    public enum BotFeatureName
    {
        BanHate,
        Heist,
        JeffRpg,
        Clip,
        AdvancedClip,
        Mark,
        AskMeAnything,
        SongManagement
    }

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

    public enum CommandAvailability
    {
        Online,
        Offline,
        Both
    }

    public enum MusicServiceProvider
    {
        Spotify,
        YouTube,
        AppleMusic,
        SoundCloud,
        AmazonMusic,
        None
    }
}
