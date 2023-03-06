namespace JeffBot
{
    public class BotFeature
    {
        #region Name
        public BotFeatures Name { get; set; }
        #endregion
        #region Command
        public string Command { get; set; }
        #endregion

        #region Constructor
        public BotFeature(BotFeatures name, string command)
        {
            Name = name;
            Command = command;
        }
        public BotFeature(BotFeatures name)
        {
            Name = name;
        }
        public BotFeature()
        { } 
        #endregion
    }
    public enum BotFeatures
    {
        BanHate,
        Heist,
        JeffRpg,
        Clip,
        AdvancedClip,
        Mark,
        AskMeAnything
    }
}