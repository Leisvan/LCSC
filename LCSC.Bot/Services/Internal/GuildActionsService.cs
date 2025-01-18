using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LCSC.Core.Models;
using LCSC.Core.Services;
using LCSC.Discord.Helpers;
using LCSC.Models;
using LCSC.Models.Airtable;

namespace LCSC.Discord.Services.Internal
{
    internal class GuildActionsService(
        MembersService membersService,
        DiscordBotService botService,
        SettingsService settingsService,
        LadderService ladderService,
        InteractivityExtension interactivity)
    {
        private readonly DiscordBotService _botService = botService;
        private readonly InteractivityExtension _interactivity = interactivity;
        private readonly LadderService _ladderService = ladderService;
        private readonly MembersService _membersService = membersService;
        private readonly SettingsService _settingsService = settingsService;
        private CancellationTokenSource? _updateLadderTokenSource;

        public void CancelUpdateMemberRegions()
            => _updateLadderTokenSource?.Cancel();

        public async Task<string?> DisplayRank()
        {
            var seasonId = await _ladderService.GetSeasonIdAsync();
            if (seasonId == 0)
            {
                return "No se puede acceder a la información de temporada";
            }
            var members = await _membersService.GetMembersAsync();
            var entries = new List<(string, LadderRegionRecord)>();
            foreach (var member in members)
            {
                var region = GetLadderRegion(member, seasonId);
            }

            if (entries.Count == 0)
            {
                return "No hay perfiles que mostrar";
            }
            return null;
        }

        public async Task<string> UpdateMemberRegionsAsync(
                    bool forceUpdate = false,
                    ulong guildId = 0,
                    ulong channelId = 0,
                    CommandContext? context = null)
        {
            if (_updateLadderTokenSource != null)
            {
                return "La operación ya está en progreso.";
            }
            _updateLadderTokenSource = new CancellationTokenSource();

            TimeSpan? updateTime = null;
            if (!forceUpdate)
            {
                var regionUpdateMinutesThreshold = _settingsService.GetIntValue(SettingKey.RegionUpdateThresholdInMinutes, guildId);
                if (regionUpdateMinutesThreshold.HasValue && regionUpdateMinutesThreshold.Value > 0)
                {
                    updateTime = TimeSpan.FromMinutes(regionUpdateMinutesThreshold.Value);
                }
            }
            if (channelId == 0)
            {
                var channelIdSettingValue = _settingsService.GetStringValue(SettingKey.RankingChannel, guildId);
                if (channelIdSettingValue == null || !ulong.TryParse(channelIdSettingValue, out channelId) || channelId == 0)
                {
                    var errorMessage = "Id del canal no encontrado";
                    LogNotifier.NotifyError(errorMessage);
                    return errorMessage;
                }
            }
            DiscordMessage? message = null;
            if (context != null)
            {
                var builder = new DiscordMessageBuilder()
                    .WithContent("Accediendo a la lista de miembros")
                    .AddComponents(InteractionsHelper.GetCancelUpdateRankButton());
                message = await context.FollowupAsync(builder);
            }
            RegionUpdateProgressReportData? lastUpdate = null;
            var result = await _membersService.UpdateAllRegionsAsync(
                updateTime, async (data) =>
                {
                    lastUpdate = data;
                    var content = data.ErrorMessage ?? $"Actualizando {data.Number} de {data.Total} perfiles: {data.EntryName}";
                    message = await UpdateMessageAsync(content, channelId, message);
                }, _updateLadderTokenSource.Token);

            if (lastUpdate?.ErrorMessage == null)
            {
                await UpdateMessageAsync($"{result} perfiles actualizados.", channelId, message);
            }

            return string.Empty;
        }

        private LadderRegionRecord? GetLadderRegion(MemberModel? model, int seasonId)
        {
            if (model?.Profiles == null)
            {
                return null;
            }
            var profile = model.Profiles
                .Where(p => p.LadderRegion != null)
                .Select(p => p.LadderRegion)
                .OrderBy(r => r?.CurrentMMR ?? 0)
                .FirstOrDefault();

            return profile;
        }

        private async Task<DiscordMessage?> UpdateMessageAsync(string content, ulong channelId, DiscordMessage? message = null)
        {
            try
            {
                if (message == null)
                {
                    var channel = await _botService.Client.GetChannelAsync(channelId);
                    return await channel.SendMessageAsync(content);
                }
                return message = await message.ModifyAsync(content);
            }
            catch (Exception e)
            {
                LogNotifier.Notify(e.Message);
                return null;
            }
        }
    }
}