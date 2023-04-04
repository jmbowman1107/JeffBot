using System.Collections.Generic;

namespace JeffBot
{
    public interface IBotCommandSettings
    {
        #region Name
        string Name { get; set; }
        #endregion
        #region Description
        string Description { get; set; }
        #endregion
        #region Output
        string Output { get; set; }
        #endregion
        #region TriggerWord
        string TriggerWord { get; set; }
        #endregion
        #region AdditionalTriggerWords
        List<string> AdditionalTriggerWords { get; set; }
        #endregion
        #region TriggerRegexes
        List<string> TriggerRegexes { get; set; }
        #endregion
        #region PermissionLevel
        FeaturePermissionLevel PermissionLevel { get; set; }
        #endregion
        #region GlobalCooldown
        int GlobalCooldown { get; set; }
        #endregion
        #region UserCooldown
        int UserCooldown { get; set; }
        #endregion
        #region CommandAvailability
        CommandAvailability CommandAvailability { get; set; }
        #endregion
        #region IsEnabled
        bool IsEnabled { get; set; }
        #endregion
        #region CustomSettings
        object CustomSettings { get; set; } 
        #endregion
    }

    public interface IBotCommandSettings<T> : IBotCommandSettings
    {
        #region CustomSettings
        new T CustomSettings { get; set; }
        #endregion
    }
}