using CommandLine.Text;
using CommandLine;
using Newtonsoft.Json;

namespace JeffBot
{
    public class AddCommandOptions : CommandOptionsBase
    {
        [Value(1, Required = true, HelpText = "Output text for the command.")]
        public string Output { get; set; }
    }
}
