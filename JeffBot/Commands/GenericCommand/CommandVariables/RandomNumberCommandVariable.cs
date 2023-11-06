using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public class RandomNumberCommandVariable : CommandVariableBase
    {
        #region Keyword - Override
        public override string Keyword { get; set; } = "random";
        #endregion
        #region Description - Override
        public override string Description { get; set; } = "Use this variable to generate a random number between a range.";
        #endregion
        #region UsageExample - Override
        public override string UsageExample { get; set; } = "{ random: 1-100 }";
        #endregion

        #region Constructor
        public RandomNumberCommandVariable(BotCommandBase botCommand) : base(botCommand)
        {
        }
        #endregion

        #region ProcessVariable - Override
        public override async Task<string> ProcessVariable(string command, ChatMessage chatMessage)
        {
            // Use regex to match floating-point numbers as well as integers
            var match = Regex.Match(command, @"^\s*([+-]?(\d+(\.\d+)?|\.\d+))\s*-\s*([+-]?(\d+(\.\d+)?|\.\d+))\s*$");
            if (!match.Success)
            {
                throw new ArgumentException("The input string must be in the format 'x - y', where x and y are numbers.");
            }

            // Parse the numbers using the invariant culture to avoid issues with number format in different cultures
            if (double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var start) &&
                double.TryParse(match.Groups[4].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var end))
            {
                if (start >= end)
                {
                    throw new ArgumentException("The first number must be less than the second number.");
                }

                // Generate a random double between start and end (inclusive)
                var random = new Random();
                await Task.Run(() => random.Next(Convert.ToInt32(start), Convert.ToInt32(end)));
                return random.Next(Convert.ToInt32(start), Convert.ToInt32(end)).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                throw new ArgumentException("The input string must contain two valid numbers.");
            }
        } 
        #endregion
    }
}