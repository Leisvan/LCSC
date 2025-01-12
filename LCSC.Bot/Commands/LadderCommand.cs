using DSharpPlus.Commands;
using DSharpPlus.Entities;
using LCSC.Discord.Services;

namespace LCSC.Discord.Commands
{
    [Command("ladder")]
    public class LadderCommand
    {
        public LadderCommand(DiscordBotService service)
        {
        }

        [Command("rank")]
        public static async ValueTask RankAsync(CommandContext context, int a, int b)
            => await context.RespondAsync($"{a} + {b} = {a + b}");

        private async Task Emojis(DiscordBotService service)
        {
            var xx = await service.Client.GetApplicationEmojisAsync();
        }
    }
}