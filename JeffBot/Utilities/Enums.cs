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
        AskMeAnything
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
}
