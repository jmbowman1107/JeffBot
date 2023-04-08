using CommandLine;

namespace JeffBot
{
    public class CommandOptionsBase
    {
        [Value(0, Required = true, HelpText = "Trigger word for the command.")]
        public string TriggerWord { get; set; }

        [Option('p', "permission", Required = false, Default=FeaturePermissionLevel.Everyone, HelpText = "Permission level for the command.")]
        public FeaturePermissionLevel? PermissionLevel { get; set; }

        [Option('a', "availability", HelpText = "Availability for the command.")]
        public CommandAvailability? CommandAvailability { get; set; }

        [Option('u', "usercooldown", HelpText = "User cooldown for the command.")]
        public int? UserCooldown { get; set; }

        [Option('g', "globalcooldown", HelpText = "Global cooldown for the command.")]
        public int? GlobalCooldown { get; set; }
    }
}
