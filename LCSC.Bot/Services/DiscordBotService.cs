using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using LCSC.Core.Services;
using LCSC.Discord.Helpers;
using LCSC.Discord.Models;
using LCSC.Discord.Services.Internal;
using LCSC.Discord.Strings;

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
        }

        public IEnumerable<DiscordGuildModel> GetSettingServers()
            => _settingsService.GetAllGuilds();

        public Task UpdateMemberRegionsAsync(bool forceUpdate = false, ulong guildId = 0)
            => _guildActions.UpdateMemberRegionsAsync(forceUpdate, guildId);

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