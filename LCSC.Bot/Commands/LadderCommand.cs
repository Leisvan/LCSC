using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Trees.Metadata;
using LCSC.Discord.Services;

namespace LCSC.Discord.Commands
{
    [Command("ladder")]
    [AllowedProcessors(typeof(SlashCommandProcessor))]
    public class LadderCommand
    {
        private readonly DiscordBotService _service;

        public LadderCommand(DiscordBotService service)
        {
            _service = service;
        }

        [Command("rank")]
        public async ValueTask RankAsync(CommandContext context)
        {
        }

        [Command("update")]
        public async ValueTask UpdateAsync(CommandContext context, bool forceUpdate = false)
        {
            var guildId = context.Guild?.Id ?? 0;

            await context.RespondAsync("...");

            var message = await context.GetResponseAsync();

            await _service.Actions.UpdateMemberRegionsAsync(forceUpdate, guildId, context.Channel.Id, message);
        }

        private async Task Emojis(DiscordBotService service)
        {
            var xx = await service.Client.GetApplicationEmojisAsync();
        }
    }
}