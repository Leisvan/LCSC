using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using LCSC.Core.Services;
using LCSC.Discord.Helpers;
using LCSC.Discord.Models;
using LCSC.Discord.Services.Internal;
using LCSC.Discord.Strings;
using System.Threading.Channels;

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
            => throw new Exception("Cannot Init"); //_client.ConnectAsync();

        public async Task DisconnectAsync()
        {
            await _client.DisconnectAsync();
        }

        public Task DisplayRankAsync(bool includeBanned = false, ulong guildId = 0)
        {
            var channelIdSettingValue = _settingsService.GetStringValue(SettingKey.RankingChannel, guildId);
            if (channelIdSettingValue == null || !ulong.TryParse(channelIdSettingValue, out ulong channelId) || channelId == 0)
            {
                var errorMessage = MessageResources.ChannelIdNotFoundErrorMessage;
                LogNotifier.NotifyError(errorMessage);
                return Task.CompletedTask;
            }
            return _guildActions.DisplayRankAsync(includeBanned, guildId, channelId);
        }

        public IEnumerable<DiscordGuildModel> GetSettingServers()
                    => _settingsService.GetAllGuilds();

        public Task UpdateMemberRegionsAsync(bool forceUpdate = false, ulong guildId = 0)
        {
            var channelIdSettingValue = _settingsService.GetStringValue(SettingKey.RankingChannel, guildId);
            if (channelIdSettingValue == null || !ulong.TryParse(channelIdSettingValue, out ulong channelId) || channelId == 0)
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