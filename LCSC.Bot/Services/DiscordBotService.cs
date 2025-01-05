namespace LCSC.Discord.Services
{
    public class DiscordBotService
    {
        private readonly BotManager _bot;

        public DiscordBotService(string? discordToken, ulong appId)
        {
            if (string.IsNullOrWhiteSpace(discordToken))
            {
                throw new ArgumentException(null, nameof(discordToken));
            }
            _bot = new BotManager(discordToken, appId);
        }

        public Task ConnectAsync()
            => _bot.ConnectAsync();
    }
}