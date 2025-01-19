﻿using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using LCSC.Core.Models;
using LCSC.Core.Services;
using LCSC.Discord.Extensions;
using LCSC.Discord.Helpers;
using LCSC.Discord.Strings;
using LCSC.Models;
using LCSC.Models.Airtable;
using LCSC.Models.Pulse;
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
            bool includeBanned = false,
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
            var entries = new List<(MemberRecord Member, LadderRegionRecord Region)>();
            foreach (var member in members.Where(x => includeBanned || !x.Record.Banned))
            {
                if (member.Record?.Nick == null)
                {
                    continue;
                }
                var region = GetValidLadderRegion(member, seasonId);
                if (region != null)
                {
                    entries.Add((member.Record, region));
                }
            }

            if (entries.Count == 0)
            {
                return MessageResources.NoProfileToShowErrorMessage;
            }

            if (context != null)
            {
                await context.FollowupAsync(MessageResources.AccessingMembersListMessage);
            }

            //Setup ranking messages
            var channel = context == null
                ? await _botService.Client.GetChannelAsync(channelId)
                : context.Channel;

            int seqNumber = 1;
            var header = GetRankingHeaderString();

            //1. Ranking header:
            await channel.SendMessageAsync(header);
            //2. Ranking lines:
            foreach (var sublist in entries.OrderByDescending(e => e.Region.CurrentMMR).Chunk(RankingMessageChunkSize))
            {
                var stringBuilder = new StringBuilder();
                foreach (var (Record, Region) in sublist)
                {
                    var line = await GetRankingEntryLineAsync(Record, Region, seqNumber++);
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
            var result = await _membersService.UpdateAllRegionsAsync(false, updateTime,
                async (data) =>
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

        private async Task<string> GetRankingEntryLineAsync(MemberRecord record, LadderRegionRecord region, int seqNumber)
        {
            if (record.Nick == null || region == null)
            {
                return string.Empty;
            }

            var raceEmoji = await EmojisHelper.GetRaceEmojiStringAsync(_botService.Client, region.Race);
            var flagEmoji = EmojisHelper.GetFlagEmojiString(record.CountryTag);
            var leagueEmoji = await EmojisHelper.GetLeagueEmojiStringAsync(_botService.Client, region.League, region.Tier);

            var newPlayer = (region.PreviousMMR == 0);
            var mmrDiffValue = newPlayer ? 0 : region.CurrentMMR - region.PreviousMMR;
            var mmrsign = mmrDiffValue == 0 ? " " : (mmrDiffValue > 0) ? "↑" : "↓";
            string mmrDifText = mmrsign + StringLengthCapTool.InvertedFourSpaces.GetString(mmrDiffValue == 0 ? " " : Math.Abs(mmrDiffValue));
            var winrate = (double)region.Wins / region.TotalMatches * 100;

            var builder = new StringBuilder();
            var i3ct = StringLengthCapTool.InvertedThreeSpaces;
            var i5ct = StringLengthCapTool.InvertedFiveSpaces;

            builder.Append($"`{seqNumber:00}` ");
            builder.Append($"{raceEmoji} ");
            builder.Append($"`{StringLengthCapTool.Default.GetString(record.Nick)}` ");
            builder.Append($"{flagEmoji} | ");
            builder.Append($"`{region.CurrentMMR}` ");
            builder.Append($"`{mmrDifText}` ");
            builder.Append($"{leagueEmoji} ");
            builder.Append($"`{i3ct.GetString((int)winrate)}%` ");
            builder.Append($"`{i5ct.GetString(region.TotalMatches)}` ");

            if (newPlayer)
            {
                builder.Append(":new: ");
            }

            return builder.ToString();
        }

        private string GetRankingHeaderString()
        {
            return
                "`##`" + BlankSpace //Number
                + DoubleSpaceCode + BlankSpace // Race logo
                + $"`{StringLengthCapTool.Default.GetString("NICK")}`" + BlankSpace // Nick
                + "`  `| " // Country flag
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