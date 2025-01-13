using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.EventArgs;
using DSharpPlus.Extensions;
using LCSC.Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace LCSC.Discord.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDiscordClient(
                    this IServiceCollection services, string? token)
        {
            ArgumentNullException.ThrowIfNull(token);

            services.AddDiscordClient(token, DiscordIntents.Guilds | DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | SlashCommandProcessor.RequiredIntents);

            services.AddCommandsExtension((IServiceProvider serviceProvider, CommandsExtension extension) =>
            {
                extension.AddCommands([typeof(LadderCommand)]);
            });
            //var builder = DiscordClientBuilder.CreateDefault(token,
            //    DiscordIntents.Guilds |
            //    DiscordIntents.AllUnprivileged |
            //    DiscordIntents.MessageContents |
            //    SlashCommandProcessor.RequiredIntents,
            //    services);

            //builder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
            //{
            //    extension.AddCommands([typeof(LadderCommand)]);
            //});

            //services
            //    .AddSingleton(builder.Build());
            return services;
        }

        private static Task OnClientMessageCreated(DiscordClient sender, MessageCreatedEventArgs args)
        {
            //if (args.Author.Id != _appUserId)
            //{
            //    //_messageListener?.HandleMessageAsync(sender, args);
            //}
            return Task.CompletedTask;
        }
    }
}