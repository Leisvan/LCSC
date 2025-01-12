using DSharpPlus;
using DSharpPlus.Entities;
using LCSC.Core.Services;
using LCSC.Discord.Models;

namespace LCSC.Discord.Services
{
    public partial class DiscordBotService
    {
        private readonly DiscordClient _client;
        private readonly MembersService _membersService;
        private readonly DiscordBotSettingsService _settingsService;

        public DiscordBotService(
            DiscordClient client,
            DiscordBotSettingsService settingsService,
            MembersService membersService)
        {
            _client = client;
            _settingsService = settingsService;
            _membersService = membersService;
        }

        public DiscordClient Client => _client;

        public Task ConnectAsync()
            => _client.ConnectAsync();

        public async Task DisconnectAsync()
        {
            await _client.DisconnectAsync();
            LogNotifier.Notify("Bot Disconnected");
        }

        public IEnumerable<DiscordGuildModel> GetSettingServers()
            => _settingsService.GetAllGuilds();

        private async Task<DiscordMessage?> SendMessageAsync(string content, ulong channelId, DiscordMessage? message = null)
        {
            try
            {
                if (message == null)
                {
                    var channel = await Client.GetChannelAsync(channelId);
                    return await channel.SendMessageAsync(content);
                }
                return await message.ModifyAsync(content);
            }
            catch (Exception e)
            {
                LogNotifier.Notify(e.Message);
                return null;
            }
        }
    }
}