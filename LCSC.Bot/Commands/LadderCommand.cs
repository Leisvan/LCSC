using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using LCSC.Discord.Services;

namespace LCSC.Discord.Commands;

[Command("ladder")]
[AllowedProcessors(typeof(SlashCommandProcessor))]
public class LadderCommand(DiscordBotService service)
{
    private readonly DiscordBotService _service = service;

    [Command("ranges")]
    public async ValueTask RangesAsync(CommandContext context)
    {
        await context.DeferResponseAsync();
        await _service.Actions.DisplayRangesAsync(context.Channel.Id, context);
    }

    [Command("rank")]
    [RequirePermissions(DiscordPermission.Administrator)]
    public async ValueTask RankAsync(CommandContext context)
    {
        await context.DeferResponseAsync();
        await _service.Actions.DisplayRankAsync(false, context.Channel.Id, context);
    }

    [Command("update")]
    [RequirePermissions(DiscordPermission.Administrator)]
    public async ValueTask UpdateAsync(CommandContext context, bool forceUpdate = false)
    {
        var guildId = context.Guild?.Id ?? 0;

        var result = await _service.Actions.UpdateMemberRegionsAsync(forceUpdate, guildId, context.Channel.Id, context);
        if (result != null)
        {
            await context.EditResponseAsync(result);
        }
    }
}