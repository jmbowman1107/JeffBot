using CommandLine;

namespace JeffBot
{
    public class EditCommandOptions : CommandOptionsBase
    {
        #region Output
        [Value(1, Required = false, HelpText = "Output text for the command.")]
        [Option('o', "output", Required = false, HelpText = "Output text for the command.")]
        public string Output { get; set; }
        #endregion
    }
}