using System.Collections.Generic;

namespace JeffBot
{
    public class BotCommandSettings
    {
        #region Name
        public string Name { get; set; }
        #endregion
        #region Description
        public string Description { get; set; }
        #endregion
        #region Output
        public string Output { get; set; }
        #endregion
        #region TriggerWord
        public string TriggerWord { get; set; }
        #endregion
        #region AdditionalTriggerWords
        public List<string> AdditionalTriggerWords { get; set; } = new();
        #endregion
        #region TriggerRegexes
        public List<string> TriggerRegexes{ get; set; } = new();
        #endregion
        #region PermissionLevel
        public FeaturePermissionLevel PermissionLevel { get; set; } = FeaturePermissionLevel.Broadcaster;
        #endregion
        #region GlobalCooldown
        public int GlobalCooldown { get; set; } = 5;
        #endregion
        #region UserCooldown
        public int UserCooldown { get; set; } = 0;
        #endregion
        #region CommandAvailability
        public CommandAvailability CommandAvailability { get; set; } = CommandAvailability.Both;
        #endregion
        #region IsEnabled
        public bool IsEnabled { get; set; } = true;
        #endregion

        #region Constructor
        public BotCommandSettings(string name, string triggerWord, FeaturePermissionLevel permissionLevel)
        {
            Name = name;
            TriggerWord = triggerWord; 
            PermissionLevel = permissionLevel;
        }
        public BotCommandSettings()
        { } 
        #endregion
    }
}