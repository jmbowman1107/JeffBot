using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using TwitchLib.Client.Models;

namespace JeffBot
{
    public class CommandsManagementCommand : BotCommandBase
    {
        #region Constructor
        public CommandsManagementCommand(BotCommandSettings botCommandSettings, JeffBot jeffBot) : base(botCommandSettings, jeffBot)
        { }
        #endregion

        #region ProcessMessage - Override
        public override async Task<bool> ProcessMessage(ChatMessage chatMessage)
        {
            var message = chatMessage.Message;
            var match = Regex.Match(message, $@"^!{BotCommandSettings.TriggerWord}\s+(\w+)\s?(.*)");

            if (!match.Success) return false;

            var command = match.Groups[1].Value;
            var parameters = match.Groups[2].Value;

            var args = parameters.SplitArgs();

            switch (command)
            {
                case "add":
                    var addOptions = Parser.Default.ParseArguments<AddCommandOptions>(args);
                    await AddCommand(chatMessage, addOptions);
                    break;
                case "remove":
                    var removeOptions = Parser.Default.ParseArguments<RemoveCommandOptions>(args);
                    await RemoveCommand(chatMessage, removeOptions);
                    break;
                case "edit":
                    var editOptions = Parser.Default.ParseArguments<EditCommandOptions>(args);
                    await EditCommand(chatMessage, editOptions);
                    break;
                default:
                    return false;
            }

            return true;
        }
        #endregion

        #region AddCommand
        private async Task AddCommand(ChatMessage chatMessage, ParserResult<AddCommandOptions> commandOptions)
        {
            if (commandOptions.Errors.Any())
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"{string.Join(',', commandOptions.Errors)}");
                return;
            }

            var commandTriggerWord = commandOptions.Value.TriggerWord.StartsWith("!") ? commandOptions.Value.TriggerWord.Replace("!", string.Empty) : commandOptions.Value.TriggerWord;

            if (StreamerSettings.BotFeatures.Any(x => x.TriggerWord.Equals(commandTriggerWord, StringComparison.OrdinalIgnoreCase)))
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"!{commandTriggerWord} already exists.");
                // Return error message: Command already exists
                return;
            }

            var newCommand = new BotCommandSettings
            {
                Name = $"GenericCommand",
                TriggerWord = commandTriggerWord,
                Output = commandOptions.Value.Output,
            };

            if (commandOptions.Value.PermissionLevel.HasValue) newCommand.PermissionLevel = commandOptions.Value.PermissionLevel.Value;
            if (commandOptions.Value.UserCooldown.HasValue) newCommand.UserCooldown = commandOptions.Value.UserCooldown.Value;
            if (commandOptions.Value.GlobalCooldown.HasValue) newCommand.GlobalCooldown = commandOptions.Value.GlobalCooldown.Value;
            if (commandOptions.Value.CommandAvailability.HasValue) newCommand.CommandAvailability = commandOptions.Value.CommandAvailability.Value;

            StreamerSettings.BotFeatures.Add(newCommand);
            await AwsUtilities.DynamoDb.PopulateOrUpdateStreamerSettings(StreamerSettings);
            TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"!{commandTriggerWord} command successfully added.");
        }
        #endregion
        #region RemoveCommand
        private async Task RemoveCommand(ChatMessage chatMessage, ParserResult<RemoveCommandOptions> commandOptions)
        {
            if (commandOptions.Errors.Any())
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"{string.Join(',', commandOptions.Errors)}");
                return;
            }

            var commandTriggerWord = commandOptions.Value.TriggerWord.StartsWith("!") ? commandOptions.Value.TriggerWord.Replace("!", string.Empty) : commandOptions.Value.TriggerWord;

            if (StreamerSettings.BotFeatures.Any(x => x.TriggerWord.Equals(commandTriggerWord, StringComparison.OrdinalIgnoreCase)))
            {
                var existingBotFeature = StreamerSettings.BotFeatures.FirstOrDefault(x => x.TriggerWord.Equals(commandTriggerWord, StringComparison.OrdinalIgnoreCase));
                if (existingBotFeature is { Name: nameof(GenericCommand) })
                {
                    StreamerSettings.BotFeatures.Remove(existingBotFeature);
                    await AwsUtilities.DynamoDb.PopulateOrUpdateStreamerSettings(StreamerSettings);
                    TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"!{commandTriggerWord} command successfully removed.");
                }
            }
            else
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"!{commandTriggerWord} does not exist.");
            }
        }
        #endregion
        #region EditCommand
        private async Task EditCommand(ChatMessage chatMessage, ParserResult<EditCommandOptions> commandOptions)
        {
            if (commandOptions.Errors.Any())
            {
                TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"{string.Join(',', commandOptions.Errors)}");
                return;
            }
            var commandTriggerWord = commandOptions.Value.TriggerWord.StartsWith("!") ? commandOptions.Value.TriggerWord.Replace("!", string.Empty) : commandOptions.Value.TriggerWord;

            if (StreamerSettings.BotFeatures.Any(x => x.TriggerWord.Equals(commandTriggerWord, StringComparison.OrdinalIgnoreCase)))
            {
                var existingBotFeature = StreamerSettings.BotFeatures.FirstOrDefault(x => x.TriggerWord.Equals(commandTriggerWord, StringComparison.OrdinalIgnoreCase));
                if (existingBotFeature is { Name: nameof(GenericCommand) })
                {
                    existingBotFeature.Output = commandOptions.Value.Output;
                    if (commandOptions.Value.PermissionLevel.HasValue) existingBotFeature.PermissionLevel = commandOptions.Value.PermissionLevel.Value;
                    if (commandOptions.Value.UserCooldown.HasValue) existingBotFeature.UserCooldown = commandOptions.Value.UserCooldown.Value;
                    if (commandOptions.Value.GlobalCooldown.HasValue) existingBotFeature.GlobalCooldown = commandOptions.Value.GlobalCooldown.Value;
                    if (commandOptions.Value.CommandAvailability.HasValue) existingBotFeature.CommandAvailability = commandOptions.Value.CommandAvailability.Value;
                    await AwsUtilities.DynamoDb.PopulateOrUpdateStreamerSettings(StreamerSettings);
                    TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"!{commandTriggerWord} command successfully edited.");
                }
                else
                {
                    TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"!{commandTriggerWord} cannot be edited.");
                }
                return;
            }
            TwitchChatClient.SendReply(chatMessage.Channel, chatMessage.Id, $"!{commandTriggerWord} does not exist.");
        }
        #endregion

        #region Initialize - Override
        public override void Initialize()
        {
        }
        #endregion
    }
}