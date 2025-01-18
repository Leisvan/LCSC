using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.EventArgs;
using DSharpPlus.Extensions;
using DSharpPlus.Interactivity.Extensions;
using LCSC.Discord.Commands;
using LCSC.Discord.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace LCSC.Discord.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDiscordClient(
                    this IServiceCollection services, string? token)
        {
            ArgumentNullException.ThrowIfNull(token);

            services
                .AddDiscordClient(token, DiscordIntents.Guilds | DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | SlashCommandProcessor.RequiredIntents)
                .AddCommandsExtension((IServiceProvider serviceProvider, CommandsExtension extension) =>
                {
                    extension.AddCommands([typeof(LadderCommand)]);
                })
                .AddInteractivityExtension()
                .ConfigureEventHandlers(b => b.AddEventHandlers<ComponentInteractionCreatedEventHandler>(ServiceLifetime.Singleton));

            return services;
        }
    }
}