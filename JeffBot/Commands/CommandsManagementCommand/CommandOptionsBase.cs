using CommandLine;

namespace JeffBot
{
    public class CommandOptionsBase
    {
        #region TriggerWord
        [Value(0, Required = true, HelpText = "Trigger word for the command.")]
        public string TriggerWord { get; set; }
        #endregion
        #region PermissionLevel
        [Option('p', "permission", Required = false, Default = FeaturePermissionLevel.Everyone, HelpText = "Permission level for the command.")]
        public FeaturePermissionLevel? PermissionLevel { get; set; }
        #endregion
        #region CommandAvailability
        [Option('a', "availability", HelpText = "Availability for the command.")]
        public CommandAvailability? CommandAvailability { get; set; }
        #endregion
        #region UserCooldown
        [Option('u', "usercooldown", HelpText = "User cooldown for the command.")]
        public int? UserCooldown { get; set; }
        #endregion
        #region GlobalCooldown
        [Option('g', "globalcooldown", HelpText = "Global cooldown for the command.")]
        public int? GlobalCooldown { get; set; }
        #endregion
    }
}