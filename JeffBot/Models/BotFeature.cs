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
        #region PermissionLevel
        public FeaturePermissionLevels PermissionLevel { get; set; }
        #endregion

        #region Constructor
        public BotFeature(BotFeatures name, FeaturePermissionLevels permissionLevel, string command)
        {
            Name = name;
            PermissionLevel = permissionLevel;
            Command = command;

        }
        public BotFeature(BotFeatures name, FeaturePermissionLevels permissionLevel)
        {
            Name = name;
            PermissionLevel = permissionLevel;
        }
        public BotFeature()
        { } 
        #endregion
    }
}