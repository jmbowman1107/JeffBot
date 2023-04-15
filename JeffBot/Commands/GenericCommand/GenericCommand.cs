using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Client.Models;

namespace JeffBot;

public class GenericCommand : BotCommandBase
{
    #region CommandOutputVariables

    private List<ICommandVariable> _commandOutputVariables;

    public List<ICommandVariable> CommandOutputVariables
    {
        get
        {
            if (_commandOutputVariables == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var types = assembly.GetTypes().Where(t => typeof(ICommandVariable).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToList();
                try
                {
                    _commandOutputVariables = types.Select(t => (ICommandVariable)Activator.CreateInstance(t, this)).ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error instantiating command variable types.", ex);
                }
            }
            return _commandOutputVariables;
        }
        set => _commandOutputVariables = value;
    }
    #endregion

    #region Constructor
    public GenericCommand(BotCommandSettings botCommandSettings, JeffBot jeffBot) : base(botCommandSettings, jeffBot)
    {
    }
    #endregion

    #region ProcessMessage - Override
    public override async Task<bool> ProcessMessage(ChatMessage chatMessage)
    {
        // TODO: We will need to take into account variables when generating output (e.g. users, counters, whatever else we want to be a dynamic feature)
        if (chatMessage.Message.StartsWith($"!{BotCommandSettings.TriggerWord}", StringComparison.InvariantCultureIgnoreCase))
        {
            TwitchChatClient.SendMessage(chatMessage.Channel, await ProcessOutput(chatMessage));
            return true;
        }

        foreach (var additionalTriggerWord in BotCommandSettings.AdditionalTriggerWords)
        {
            if (chatMessage.Message.StartsWith($"!{additionalTriggerWord}", StringComparison.InvariantCultureIgnoreCase))
            {
                TwitchChatClient.SendMessage(chatMessage.Channel, await ProcessOutput(chatMessage));
                return true;
            }
        }

        foreach (var regex in BotCommandSettings.TriggerRegexes)
        {
            if (Regex.IsMatch(chatMessage.Message, regex, RegexOptions.IgnoreCase))
            {
                TwitchChatClient.SendMessage(chatMessage.Channel, await ProcessOutput(chatMessage));
                return true;
            }
        }

        // If no trigger words or regex matches are found, return false
        return false;
    }
    #endregion
    #region Initialize - Override
    public override void Initialize()
    {
    }
    #endregion

    #region ProcessOutput
    public async Task<string> ProcessOutput(ChatMessage chatMessage)
    {
        // Define a pattern to match the {variablename: sometext} or {variablename} format
        var pattern = @"\{\s*(\w+)(?:\s*:\s*([\s\S]*?))?\s*\}"; ;

        // Create a local variable to store the processed output
        var processedOutput = BotCommandSettings.Output;

        // Look for instances in the output that match the pattern
        var matches = Regex.Matches(processedOutput, pattern);

        // Iterate through each match
        foreach (Match match in matches)
        {
            // Get the output variable name and the text after the colon (if present)
            var outputVariableName = match.Groups[1].Value;
            var someText = match.Groups[2].Value;

            // Find the ICommandOutputVariable that matches the output variable name
            var commandOutputVariable = CommandOutputVariables.FirstOrDefault(outputVariable => outputVariable.Keyword == outputVariableName);

            if (commandOutputVariable == null)
            {
                // Replace the whole {} with an empty string if no matching ICommandOutputVariable is found
                processedOutput = processedOutput.Replace(match.Value, "");
            }
            else
            {
                // Process the variable and replace the {} with the variable's output
                var processedVariable = await commandOutputVariable.ProcessVariable(someText, chatMessage);
                processedOutput = processedOutput.Replace(match.Value, processedVariable);
            }
        }

        return processedOutput.Length > 500 ? processedOutput[..500] : processedOutput;
    }
    #endregion
}