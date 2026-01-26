using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using LCSC.Core.Services;
using LCSC.Discord.Helpers;
using LCSC.Discord.Services.Internal;
using LCSC.Discord.Strings;
using LCSC.Models;
using LCTWorks.Core.Interops;

namespace LCSC.Discord.Services
{
    public class DiscordBotService
    {
        private readonly DiscordClient _client;
        private readonly CommunityDataService _communityDataService;
        private readonly GuildActionsService _guildActions;

        public DiscordBotService(
            DiscordClient client,
            InteractivityExtension interactivity,
            CommunityDataService communityDataService,
            LadderService ladderService)
        {
            _client = client;
            _communityDataService = communityDataService;
            _guildActions = new GuildActionsService(communityDataService, this, ladderService, interactivity);
        }

        public DiscordClient Client => _client;

        internal GuildActionsService Actions => _guildActions;

        public void CancelUpdateMemberRegions()
            => _guildActions.CancelUpdateMemberRegions();

        public async Task<bool> ConnectAsync()
        {
            try
            {
                ConsoleInterops.ClearConsole();
                await _client.ConnectAsync();
                return true;
            }
            catch (Exception e)
            {
                ConsoleInterops.WriteErrorLine(e.Message);
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await _client.DisconnectAsync();
                ConsoleInterops.ClearConsole();
                ConsoleInterops.WriteLine(MessageResources.BotDisconnectedMessage);
            }
            catch (Exception e)
            {
                ConsoleInterops.WriteErrorLine(e.Message);
            }
        }

        public Task DisplayRankAsync(bool includeBanned = false, ulong guildId = 0)
        {
            var guildSettings = _communityDataService.GetGuildSettings(guildId);
            if (guildSettings == null || !ulong.TryParse(guildSettings.RankingChannelId, out ulong channelId) || channelId == 0)
            {
                var errorMessage = MessageResources.ChannelIdNotFoundErrorMessage;
                ConsoleInterops.WriteErrorLine(errorMessage);
                return Task.CompletedTask;
            }
            return _guildActions.DisplayRankAsync(includeBanned, channelId);
        }

        public async Task<List<GuildSettingsModel>?> GetSettingServersAsync(bool includeDebugGuilds, bool forceRefresh = false)
        {
            var members = await _communityDataService.GetAllGuildSettingsAsync(forceRefresh);
            if (!includeDebugGuilds)
            {
                members = members?.Where(x => !x.Record.IsDebugGuild).ToList();
            }
            return members;
        }

        public Task PruneRegionsAsync()
            => _communityDataService.PruneRegionsAsync();

        public async Task<bool> UpdateMemberRegionsAsync(bool forceUpdate = false, ulong guildId = 0)
        {
            var guildSettings = _communityDataService.GetGuildSettings(guildId);
            if (guildSettings == null || !ulong.TryParse(guildSettings.RankingChannelId, out ulong channelId) || channelId == 0)
            {
                var errorMessage = MessageResources.ChannelIdNotFoundErrorMessage;
                ConsoleInterops.WriteErrorLine(errorMessage);
                return false;
            }
            var errorResult = await _guildActions.UpdateMemberRegionsAsync(forceUpdate, guildId, channelId);
            return errorResult == null;
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