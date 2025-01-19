using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LCSC.Core.Models;
using LCSC.Core.Services;
using LCSC.Discord.Extensions;
using LCSC.Discord.Helpers;
using LCSC.Discord.Strings;
using LCSC.Models;
using LCSC.Models.Airtable;
using System.Text;

namespace LCSC.Discord.Services.Internal
{
    internal class GuildActionsService(
        MembersService membersService,
        DiscordBotService botService,
        SettingsService settingsService,
        LadderService ladderService,
        InteractivityExtension interactivity)
    {
        private const string BlankSpace = " ";
        private const string DoubleSpaceCode = "`  `";
        private const int NickTrimSizeInRanking = 10;
        private const int RankingMessageChunkSize = 8;
        private readonly DiscordBotService _botService = botService;
        private readonly InteractivityExtension _interactivity = interactivity;
        private readonly LadderService _ladderService = ladderService;
        private readonly MembersService _membersService = membersService;
        private readonly SettingsService _settingsService = settingsService;
        private CancellationTokenSource? _updateLadderTokenSource;

        public void CancelUpdateMemberRegions()
            => _updateLadderTokenSource?.Cancel();

        public async Task<string?> DisplayRankAsync(
            ulong guildId = 0,
            ulong channelId = 0,
            CommandContext? context = null)
        {
            var seasonId = await _ladderService.GetSeasonIdAsync();
            if (seasonId == 0)
            {
                return MessageResources.SeasonInfoNotAvailableErrorMessage;
            }
            var members = await _membersService.GetMembersAsync();
            var entries = new List<(string Nick, LadderRegionRecord Region)>();
            foreach (var member in members)
            {
                if (member.Record?.Nick == null)
                {
                    continue;
                }
                var region = GetValidLadderRegion(member, seasonId);
                if (region != null)
                {
                    entries.Add((member.Record.Nick, region));
                }
            }

            if (entries.Count == 0)
            {
                return MessageResources.NoProfileToShowErrorMessage;
            }

            //Setup ranking messages
            var channel = context == null
                ? await _botService.Client.GetChannelAsync(channelId)
                : context.Channel;

            int seqNumber = 1;
            var nameCapTool = new StringLengthCapTool(NickTrimSizeInRanking);//entries.Select(x => x.Nick)
            var header = GetRankingHeaderString(nameCapTool);

            //1. Ranking header:
            await channel.SendMessageAsync(header);
            //2. Ranking lines:
            foreach (var sublist in entries.OrderByDescending(e => e.Region.CurrentMMR).Chunk(RankingMessageChunkSize))
            {
                var stringBuilder = new StringBuilder();
                foreach (var (Nick, Region) in sublist)
                {
                    var line = GetRankingEntryLine(Nick, Region, seqNumber++, ref nameCapTool);
                    stringBuilder.AppendLine(line);
                }
                await channel.SendMessageAsync(stringBuilder.ToString());
            }
            //3. Embedded
            //4. Emoji reaction (GG)

            return null;
        }

        public async Task<string?> UpdateMemberRegionsAsync(
                    bool forceUpdate = false,
                    ulong guildId = 0,
                    ulong channelId = 0,
                    CommandContext? context = null)
        {
            if (_updateLadderTokenSource != null)
            {
                return MessageResources.OperationAlredyInProgressMessage;
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
                    var errorMessage = MessageResources.ChannelIdNotFoundErrorMessage;
                    LogNotifier.NotifyError(errorMessage);
                    return errorMessage;
                }
            }
            DiscordMessage? message = null;
            if (context != null)
            {
                var builder = new DiscordMessageBuilder()
                    .WithContent(MessageResources.AccessingMembersListMessage)
                    .AddComponents(InteractionsHelper.GetCancelUpdateRankButton());
                message = await context.FollowupAsync(builder);
            }
            RegionUpdateProgressReportData? lastUpdate = null;
            var result = await _membersService.UpdateAllRegionsAsync(
                updateTime, async (data) =>
                {
                    lastUpdate = data;
                    var content = data.ErrorMessage ?? MessageResources.UpdatingProfilesReportFormat.Format(data.Number, data.Total, data?.EntryName ?? string.Empty);
                    message = await UpdateMessageAsync(content, channelId, message);
                }, _updateLadderTokenSource.Token);

            if (lastUpdate?.ErrorMessage == null)
            {
                await UpdateMessageAsync(MessageResources.UpdatedProfilesCountFormat.Format(result), channelId, message);
            }

            return null;
        }

        private string GetRankingEntryLine(string nick, LadderRegionRecord region, int seqNumber, ref StringLengthCapTool nameCapTool)
        {
            return string.Empty;
        }

        private string GetRankingHeaderString(StringLengthCapTool nameCapTool)
        {
            return
                "`##`" + BlankSpace //Number
                + DoubleSpaceCode + BlankSpace // Race logo
                + $"`{nameCapTool.GetString("NICK")}`" + BlankSpace + BlankSpace // Nick
                + "`  ` | " // Country flag
                + "` MMR`" + BlankSpace // MMR
                + "` ↕MMR`" + BlankSpace // MMR Diff
                + DoubleSpaceCode + BlankSpace //League icon
                + "`  WR`" + BlankSpace // Winrate
                + "`TOTAL`"; //Total games played
        }

        private LadderRegionRecord? GetValidLadderRegion(MemberModel? model, int seasonId)
        {
            if (model?.Profiles == null)
            {
                return null;
            }
            var profile = model.Profiles
                .Where(p => p.LadderRegion != null)
                .Where(p => p.LadderRegion?.SeasonId == seasonId)
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