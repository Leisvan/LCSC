using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jicoteo.Bot
{
    public class BotManager
    {
        private const ulong DiscordBotUserId = 1303150451893600276;
        private MessageListener? _messageListener;

        public DiscordClient? Client { get; private set; }

        public SlashCommandsExtension? SlashCommands { get; private set; }

        public void ConfigureBot(IServiceProvider services, string token)
        {
            if (services == null)
            {
                return;
            }
            var config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
            };
            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.MessageCreated += OnClientMessageCreated;
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(5)
            });

            var slashCommandsConfig = new SlashCommandsConfiguration()
            {
                Services = services,
            };
            SlashCommands = Client.UseSlashCommands(slashCommandsConfig);

            InitializeSlashCommands();
            SlashCommands.SlashCommandErrored += async (sender, e) =>
            {
                Client.Logger.LogError(e.Exception.ToString());
                await Task.CompletedTask;
            };

            _messageListener = services.GetService<MessageListener>();
        }

        public async Task ConnectAsync()
        {
            if (Client == null)
            {
                return;
            }
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        public async Task DisconnectAsync()
        {
            if (Client == null)
            {
                return;
            }
            await Client.DisconnectAsync();
        }

        private void InitializeSlashCommands()
        {
        }

        private Task OnClientMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (args.Author.Id != DiscordBotUserId)
            {
                _messageListener?.HandleMessageAsync(sender, args);
            }
            return Task.CompletedTask;
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs args)
            => Task.CompletedTask;
    }
}