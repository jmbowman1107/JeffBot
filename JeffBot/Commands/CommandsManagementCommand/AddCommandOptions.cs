using CommandLine;

namespace JeffBot
{
    public class AddCommandOptions : CommandOptionsBase
    {
        #region Output
        [Value(1, Required = true, HelpText = "Output text for the command.")]
        public string Output { get; set; }
        #endregion
    }
}