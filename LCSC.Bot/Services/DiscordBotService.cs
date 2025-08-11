using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using LCSC.Core.Interactivity;
using LCSC.Core.Services;
using LCSC.Discord.Helpers;
using LCSC.Discord.Services.Internal;
using LCSC.Discord.Strings;
using LCSC.Models;

namespace LCSC.Discord.Services
{
    public class DiscordBotService
    {
        private readonly DiscordClient _client;
        private readonly GuildActionsService _guildActions;
        private readonly SettingsService _settingsService;

        public DiscordBotService(
            DiscordClient client,
            InteractivityExtension interactivity,
            MembersService membersService,
            LadderService ladderService)
        {
            _client = client;
            _settingsService = new SettingsService(membersService);
            _guildActions = new GuildActionsService(membersService, this, _settingsService, ladderService, interactivity);
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
            ConsoleInteractionsHelper.WriteLine(MessageResources.BotDisconnectedMessage, ConsoleColor.Red);
        }

        public Task DisplayRankAsync(bool includeBanned = false, ulong guildId = 0)
        {
            var guildSettings = _settingsService.GetGuildSettings(guildId);
            if (guildSettings == null || !ulong.TryParse(guildSettings.RankingChannelId, out ulong channelId) || channelId == 0)
            {
                var errorMessage = MessageResources.ChannelIdNotFoundErrorMessage;
                LogNotifier.NotifyError(errorMessage);
                return Task.CompletedTask;
            }
            return _guildActions.DisplayRankAsync(includeBanned, guildId, channelId);
        }

        public IEnumerable<GuildSettingsModel> GetSettingServers(bool includeDebugGuilds)
                    => _settingsService.GetAllGuilds(includeDebugGuilds);

        public Task UpdateMemberRegionsAsync(bool forceUpdate = false, ulong guildId = 0)
        {
            var guildSettings = _settingsService.GetGuildSettings(guildId);
            if (guildSettings == null || !ulong.TryParse(guildSettings.RankingChannelId, out ulong channelId) || channelId == 0)
            {
                var errorMessage = MessageResources.ChannelIdNotFoundErrorMessage;
                LogNotifier.NotifyError(errorMessage);
                return Task.CompletedTask;
            }
            return _guildActions.UpdateMemberRegionsAsync(forceUpdate, guildId, channelId);
        }

        internal async Task RespondToInteractionAsync(ComponentInteractionCreatedEventArgs args)
        {
            if (args.Id == InteractionsHelper.CancelRegionUpdateButtonId)
            {
                CancelUpdateMemberRegions();
                var builder = new DiscordInteractionResponseBuilder()
                    .WithContent(MessageResources.OperationCancelledMessage)
                    .AddComponents(InteractionsHelper.GetCancelUpdateRankButton(true));

                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, builder);
            }
        }
    }
}