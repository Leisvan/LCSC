using DSharpPlus;
using LCSC.Core.Services;
using LCSC.Discord.Models;
using LCSC.Discord.Services.Internal;

namespace LCSC.Discord.Services
{
    public class DiscordBotService
    {
        private readonly DiscordClient _client;
        private readonly GuildActionsService _guildActions;
        private readonly SettingsService _settingsService;

        public DiscordBotService(
            DiscordClient client,
            MembersService membersService)
        {
            _client = client;
            _settingsService = new SettingsService(membersService);
            _guildActions = new GuildActionsService(membersService, this, _settingsService);
        }

        public DiscordClient Client => _client;

        internal GuildActionsService Actions => _guildActions;

        public void CancelUpdateMemberRegions()
            => _guildActions.CancelUpdateMemberRegions();

        public Task ConnectAsync()
                    => _client.ConnectAsync();

        public async Task DisconnectAsync()
        {
            await _client.DisconnectAsync();
            LogNotifier.Notify("Bot Disconnected");
        }

        public IEnumerable<DiscordGuildModel> GetSettingServers()
            => _settingsService.GetAllGuilds();

        public Task UpdateMemberRegionsAsync(bool forceUpdate = false, ulong guildId = 0)
            => _guildActions.UpdateMemberRegionsAsync(forceUpdate, guildId);
    }
}