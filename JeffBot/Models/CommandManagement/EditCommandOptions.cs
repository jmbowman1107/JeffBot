using CommandLine.Text;
using CommandLine;
using Newtonsoft.Json;

namespace JeffBot
{
    public class EditCommandOptions : CommandOptionsBase
    {
        [Value(1, Required = false, HelpText = "Output text for the command.")]
        [Option('o', "output", Required = false, HelpText = "Output text for the command.")]
        public string Output { get; set; }
    }
}