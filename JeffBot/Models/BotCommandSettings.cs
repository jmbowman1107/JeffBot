using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using JeffBot.AwsUtilities;
using Newtonsoft.Json.Linq;

namespace JeffBot
{
    public class BotCommandSettings : IBotCommandSettings
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
        public List<string> TriggerRegexes { get; set; } = new();
        #endregion
        #region PermissionLevel
        public FeaturePermissionLevel PermissionLevel { get; set; } = FeaturePermissionLevel.Broadcaster;
        #endregion
        #region GlobalCooldown
        public int GlobalCooldown { get; set; } = 0;
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
        #region CustomSettings
        [DynamoDBProperty(typeof(DataConverter))]
        public dynamic CustomSettings { get; set; }
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

    public class BotCommandSettings<T> : BotCommandSettings, IBotCommandSettings<T> where T : new()
    {
        #region CustomSettings
        public new T CustomSettings { get; set; }
        #endregion

        #region Constructor
        public BotCommandSettings(BotCommandSettings botCommandSettings)
        {
            Name = botCommandSettings.Name;
            Description = botCommandSettings.Description;
            Output = botCommandSettings.Output;
            TriggerWord = botCommandSettings.TriggerWord;
            AdditionalTriggerWords = botCommandSettings.AdditionalTriggerWords;
            TriggerRegexes = botCommandSettings.TriggerRegexes;
            PermissionLevel = botCommandSettings.PermissionLevel;
            GlobalCooldown = botCommandSettings.GlobalCooldown;
            UserCooldown = botCommandSettings.UserCooldown;
            CommandAvailability = botCommandSettings.CommandAvailability;
            IsEnabled = botCommandSettings.IsEnabled;
            CustomSettings = ((JObject)botCommandSettings.CustomSettings) == null ? new T() : ((JObject)botCommandSettings.CustomSettings).ToObject<T>();
        }
        #endregion
    }
}