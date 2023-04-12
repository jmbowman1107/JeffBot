using CommandLine;

namespace JeffBot
{
    public class RemoveCommandOptions
    {
        #region TriggerWord
        [Value(0, Required = true, HelpText = "Trigger word for the command.")]
        public string TriggerWord { get; set; }
        #endregion
    }
}