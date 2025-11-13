using LCSC.Core.Helpers;
using LCSC.Core.Models;
using LCSC.Http.Services;
using LCSC.Models;
using LCSC.Models.Airtable;
using LCSC.Models.Pulse;
using LCTWorks.Core.Extensions;
using LCTWorks.Core.Helpers;
using Newtonsoft.Json;
using System.Diagnostics;

namespace LCSC.Core.Services;

public class CommunityDataService(LadderService ladderService, CacheService cacheService, string? airtableToken, string? baseId, string? cachePath)
{
    private const string DefaultRegionParameter = "US";
    private const string MembersCacheFileName = "Members.json";
    private const string SettingsFileName = "Settings.json";
    private const string TournamentsCacheFileName = "Tournaments.json";

    private readonly AirtableHttpService _airtableHttpService = new(airtableToken, baseId);
    private readonly string? _cachePath = cachePath;
    private readonly CacheService _cacheService = cacheService;
    private readonly List<GuildSettingsModel> _guildSettings = [];
    private readonly LadderService _ladderService = ladderService;
    private readonly List<MemberModel> _members = [];
    private readonly PulseHttpService _pulseHttpService = new();
    private readonly List<TournamentModel> _tournaments = [];

    public Task<string?> CreateBattleNetProfile(
        string? battleTag,
        string? pulseId,
        string? profileRealm,
        string? profileId,
        bool mainProfile,
        string? notes,
        string memberId)
    {
        var record = new BattleNetProfileRecord(
            string.Empty,
            0,
            battleTag,
            pulseId,
            profileRealm,
            profileId,
            mainProfile,
            notes,
            [memberId]);
        return _airtableHttpService.CreateBattleNetProfile(record);
    }

    public async Task<List<GuildSettingsModel>?> GetAllGuildSettingsAsync(bool forceRefresh = false)
    {
        if (forceRefresh || _guildSettings.Count == 0)
        {
            var guildSettings = await _airtableHttpService.GetDiscordBotGuildsSettingsAsync();
            if (guildSettings != null && guildSettings.Any())
            {
                _guildSettings.Clear();
                _guildSettings.AddRange(guildSettings.Select(item => new GuildSettingsModel(item)));
                await SaveToCacheAsync(false, false, true);
            }
        }
        return _guildSettings;
    }

    public GuildSettingsModel? GetGuildSettings(ulong guildId)
    {
        if (_guildSettings == null || _guildSettings.Count == 0)
        {
            return default;
        }
        return _guildSettings.FirstOrDefault(x => x.GuildId == guildId);
    }

    public async Task<List<MemberModel>> GetMembersAsync(bool forceRefresh = false)
    {
        if (forceRefresh || _members.Count == 0)
        {
            await RefreshAllAsync();
        }

        return _members;
    }

    public LeagueStatsModel? GetMemberStats(string? memberId)
    {
        if (string.IsNullOrWhiteSpace(memberId))
        {
            return null;
        }
        var member = GetMemberById(memberId);
        if (member is null)
        {
            return null;
        }

        var p1 = _tournaments.Where(x => x.Place1?.Record.Id == memberId).Count();
        var p2 = _tournaments.Where(x => x.Place2?.Record.Id == memberId).Count();
        var p3 = _tournaments.Where(x => x.Place3?.Record.Id == memberId).Count();
        var p4 = _tournaments.Where(x => x.Place4?.Record.Id == memberId).Count();
        var pcount = _tournaments
            .Where(x => x.Participants != null && x.Participants.Any(m => m.Record.Id == memberId))
            .Count();

        var allMatches = _tournaments.SelectMany(m => m.Matches ?? []);
        var membermatches = allMatches.Where(m => m.Winner.Record.Id == memberId || m.Loser.Record.Id == memberId);
        var winCount = membermatches.Where(m => m.Winner.Record.Id == member.Record.Id).Select(m => m.WinnerScore).Sum();
        var totalCount = membermatches.Select(m => m.WinnerScore + m.LoserScore).Sum();

        var winrate = totalCount == 0 ? 0 : winCount * 100 / totalCount;

        var twinrate = RacePercent(membermatches, memberId, Race.Terran);
        var zwinrate = RacePercent(membermatches, memberId, Race.Zerg);
        var pwinrate = RacePercent(membermatches, memberId, Race.Protoss);

        return new LeagueStatsModel(p1, p2, p3, p4, pcount, winrate, twinrate, zwinrate, pwinrate);
    }

    public async Task<List<TournamentModel>> GetTournamentsAsync(bool forceRefresh = false)
    {
        if (forceRefresh || _tournaments.Count == 0)
        {
            await RefreshAllAsync();
        }
        return _tournaments;
    }

    public async Task InitializeFromCacheAsync()
    {
        if (string.IsNullOrEmpty(_cachePath))
        {
            return;
        }

        //Load members
        var cachedMembersTextData = await _cacheService.GetCachedTextAsync(MembersCacheFileName);
        var cachedMembers = Json.ToObject<List<MemberModel>>(cachedMembersTextData);
        if (cachedMembers != null && cachedMembers.Count > 0)
        {
            _members.Clear();
            _members.AddRange(cachedMembers);
        }

        //Load members
        var cachedTournamentsTextData = await _cacheService.GetCachedTextAsync(TournamentsCacheFileName);
        var cachedTournaments = Json.ToObject<List<TournamentModel>>(cachedTournamentsTextData);
        if (cachedTournaments != null && cachedTournaments.Count > 0)
        {
            _tournaments.Clear();
            _tournaments.AddRange(cachedTournaments);
        }

        //Load settings
        var cachedSettingsTextData = await _cacheService.GetCachedTextAsync(SettingsFileName);
        var cachedSettings = Json.ToObject<List<GuildSettingsModel>>(cachedSettingsTextData);
        if (cachedSettings != null && cachedSettings.Count > 0)
        {
            _guildSettings.Clear();
            _guildSettings.AddRange(cachedSettings);
        }
    }

    public async Task<int> PruneRegionsAsync()
    {
        var currentSeasonId = await _ladderService.GetSeasonIdAsync();
        if (currentSeasonId == 0)
        {
            return 0;
        }
        var regions = (await _airtableHttpService.GetLadderRegionsAsync());
        if (regions == null || !regions.Any())
        {
            return 0;
        }
        var regionsToDelete = new List<string>();

        foreach (var region in regions)
        {
            if (region.SeasonId < currentSeasonId)
            {
                regionsToDelete.Add(region.Id);
            }
        }
        if (regionsToDelete.Count > 0)
        {
            var successCount = await _airtableHttpService.DeleteRegionsAsync(regionsToDelete);
            await RefreshAllAsync();
            return successCount;
        }
        return 0;
    }

    public async Task<ProfileSearchResult?> SearchProfileByBattleTag(string? battleTag)
    {
        if (battleTag == null)
        {
            return null;
        }
        var results = await _pulseHttpService.SearchCharacterAsync(battleTag);
        if (results != null)
        {
            var result = results
                .Where(x =>
                string.Equals(x.Members?.Character?.Region, DefaultRegionParameter, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(x.Members?.Account?.BattleTag, battleTag, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
            if (result != null)
            {
                return new ProfileSearchResult(
                    result.Members?.Account?.BattleTag,
                    result.Members?.Character?.Id.ToString(),
                    result.Members?.Character?.Realm.ToString(),
                    result.Members?.Character?.BattlenetId.ToString());
            }
        }
        return null;
    }

    public async Task<int> UpdateAllRegionsAsync(
        bool includeBannedPlayers = true,
        TimeSpan? regionUpdateThreshold = null,
        Func<RegionUpdateProgressReportData, Task>? progressReport = null,
        CancellationToken? token = null)
    {
        var profilesList = _members
            .Where(m => includeBannedPlayers || !m.Record.Banned)
            .Where(m => m.Profiles != null && m.Profiles.Count > 0)
            .SelectMany(m => m.Profiles!)
            .ToList();
        var updatedRegions = new List<LadderRegionModel>();
        for (int i = 0; i < profilesList.Count; i++)
        {
            if (token?.IsCancellationRequested == true)
            {
                if (progressReport != null)
                {
                    await progressReport.Invoke(RegionUpdateProgressReportData.Message("Operación cancelada"));
                }
                return -1;
            }

            var profile = profilesList[i];
            if (progressReport != null)
            {
                await progressReport.Invoke(new RegionUpdateProgressReportData(i + 1, profilesList.Count, $"`{profile.Record.BattleTag}`"));
            }
            var regions = await UpdateRegionsInternalAsync(profile, regionUpdateThreshold);
            foreach (var item in regions.Where(r => r.IsUpdated))
            {
                updatedRegions.Add(item);
            }
        }
        if (updatedRegions.Count > 0)
        {
            if (progressReport != null)
            {
                await progressReport.Invoke(RegionUpdateProgressReportData.Message("Finalizando operación"));
            }
            if (await _airtableHttpService.UpdateOrCreateRegionsAsync(updatedRegions))
            {
                await RefreshAllAsync();
            }
            else
            {
                if (progressReport != null)
                {
                    await progressReport.Invoke(RegionUpdateProgressReportData.Message("Error"));
                }
            }
        }
        return updatedRegions.Count;
    }

    public async Task<bool> UpdateRegionsAsync(IEnumerable<BattleNetProfileModel> profiles)
    {
        List<LadderRegionModel> regions = [];
        if (profiles == null || !profiles.Any())
        {
            return false;
        }
        foreach (var profile in profiles)
        {
            var profileRegions = await UpdateRegionsInternalAsync(profile);
            foreach (var item in profileRegions.Where(r => !r.IsUpdated))
            {
                regions.Add(item);
            }
        }
        if (regions.Count == 0)
        {
            return false;
        }
        if (await _airtableHttpService.UpdateOrCreateRegionsAsync(regions))
        {
            await SaveToCacheAsync();
            return true;
        }
        return false;
    }

    public async Task UpdateTournamentMatchesAsync(string tournamentId, List<MatchModel>? matches)
    {
        if (string.IsNullOrWhiteSpace(tournamentId) || matches == null || matches.Count == 0)
        {
            return;
        }

        var models = matches.Select(match => new MatchJsonModel
        {
            LoserId = match.Loser.Record.Id,
            LoserRace = (int)match.LoserRace,
            LoserScore = match.LoserScore,
            WinnerId = match.Winner.Record.Id,
            WinnerRace = (int)match.WinnerRace,
            WinnerScore = match.WinnerScore,
            Stage = (int)match.Stage,
            Notes = match.Notes,
        });
        try
        {
            var jsonText = System.Text.Json.JsonSerializer.Serialize(models);
            var result = await _airtableHttpService.UpdateTournamentMatchesData(tournamentId, jsonText);
        }
        catch (Exception)
        {
        }
    }

    private static bool AreRegionsEqual(LadderRegionRecord? ladderRegion, Team team)
    {
        if (ladderRegion == null)
        {
            return false;
        }
        if (ladderRegion.SeasonId != team.Season)
        {
            return false;
        }
        return ladderRegion.TotalMatches == (team.Wins + team.Losses + team.Ties);
    }

    private static string? GetFirstItemFromArray(string[]? array)
    {
        if (array == null || array.Length == 0)
        {
            return null;
        }
        return array[0];
    }

    private static double RacePercent(IEnumerable<MatchModel> matches, string playerId, Race race)
    {
        int winCount = matches
            .Where(x => x.Winner.Record.Id == playerId && x.LoserRace == race)
            .Select(x => x.WinnerScore)
            .Sum();
        int allCount = matches.Where(x => (x.Winner.Record.Id == playerId && x.LoserRace == race) || (x.Loser.Record.Id == playerId && x.WinnerRace == race))
            .Select(x => x.WinnerScore + x.LoserScore).Sum();
        return allCount == 0 ? 0 : (winCount * 100) / allCount;
    }

    private List<MemberModel>? GetAllParticipants(TournamentRecord record)
    {
        if (record?.Participants == null || record.Participants.Length == 0)
        {
            return [];
        }
        var list = new List<MemberModel>();
        foreach (var item in record.Participants)
        {
            var player = GetMemberById(item);
            if (player != null)
            {
                list.Add(player);
            }
        }
        return list;
    }

    private async Task<Dictionary<string, List<BattleNetProfileModel>>> GetBattleNetProfilesAsync()
    {
        var bnetProfiles = await _airtableHttpService.GetBattleNetProfilesAsync();
        if (bnetProfiles != null)
        {
            var ladderRegions = await _airtableHttpService.GetLadderRegionsAsync() ?? [];
            var results = new Dictionary<string, List<BattleNetProfileModel>>();

            var regionsLookup = ladderRegions
                .Where(x => x.BattleNetProfiles?.Length > 0)
                .ToLookup(x => x.BattleNetProfiles!.FirstOrDefault() ?? string.Empty);

            foreach (var item in bnetProfiles)
            {
                var id = item.Members?.FirstOrDefault();
                if (id == null)
                {
                    continue;
                }
                if (!results.TryGetValue(id, out List<BattleNetProfileModel>? value))
                {
                    value = [];
                    results[id] = value;
                }
                var allRegions = regionsLookup[item.Id];
                value.Add(new BattleNetProfileModel(item, [.. allRegions]));
            }
            return results;
        }
        return [];
    }

    private List<MatchModel> GetMatchModels(TournamentRecord record)
    {
        if (string.IsNullOrEmpty(record?.MatchesData))
        {
            return [];
        }

        MatchJsonModel[]? matchesArray = null;
        try
        {
            matchesArray = JsonConvert.DeserializeObject<MatchJsonModel[]>(record.MatchesData);
        }
        catch (Exception)
        {
        }
        if (matchesArray == null)
        {
            return [];
        }

        var list = new List<MatchModel>();
        foreach (var match in matchesArray)
        {
            var winner = GetMemberById(match.WinnerId);
            var loser = GetMemberById(match.LoserId);
            if (winner != null && loser != null)
            {
                list.Add(new MatchModel(
                    winner,
                    loser,
                    (Race)match.WinnerRace,
                    (Race)match.LoserRace,
                    match.WinnerScore,
                    match.LoserScore,
                    (MatchStage)match.Stage,
                    match.Notes));
            }
        }
        list.Sort();
        return list;
    }

    private MemberModel? GetMemberById(string? id)
    {
        if (!string.IsNullOrEmpty(id) && _members.Count > 0)
        {
            return _members.FirstOrDefault(m => m.Record.Id == id);
        }
        return null;
    }

    private async Task RefreshAllAsync()
    {
        //Refresh members:
        var members = await _airtableHttpService.GetMemberRecordsAsync();
        if (members != null && members.Any())
        {
            _members.Clear();
            var bnetProfiles = await GetBattleNetProfilesAsync();
            if (bnetProfiles.Count == 0)
            {
                _members.AddRange(members
                    .Select(m => new MemberModel(m, []))
                    .OrderBy(x => x.Record.Nick));
            }
            else
            {
                foreach (var member in members)
                {
                    bnetProfiles.TryGetValue(member.Id, out var profiles);
                    profiles?.Sort();
                    _members.Add(new MemberModel(member, profiles));
                }
                _members.Sort();
            }
            foreach (var member in _members)
            {
                member.Stats = GetMemberStats(member.Record.Id);
            }
        }

        //Refresh tournaments:
        var tournaments = await _airtableHttpService.GetTournamentRecordsAsync();
        if (tournaments != null && tournaments.Any())
        {
            var results = new List<TournamentModel>();
            foreach (var item in tournaments)
            {
                var p1 = GetMemberById(GetFirstItemFromArray(item.Place1));
                var p2 = GetMemberById(GetFirstItemFromArray(item.Place2));
                var p3 = GetMemberById(GetFirstItemFromArray(item.Place3));
                var p4 = GetMemberById(GetFirstItemFromArray(item.Place4));
                var participants = GetAllParticipants(item);
                var matches = GetMatchModels(item);
                var tournament = new TournamentModel(item, p1, p2, p3, p4, participants, matches);
                results.Add(tournament);
            }
            _tournaments.Clear();
            _tournaments.AddRange(results.OrderByDescending(t => t.Record.Date));
        }
        await SaveToCacheAsync();
    }

    private async Task SaveToCacheAsync(bool saveMembers = true, bool saveTournaments = true, bool saveSettings = false)
    {
        if (string.IsNullOrEmpty(_cachePath))
        {
            return;
        }
        try
        {
            if (saveMembers && _members != null && _members.Count > 0)
            {
                var json = await Json.StringifyAsync(_members);
                await _cacheService.CacheTextFileAsync(MembersCacheFileName, json);
            }
            if (saveTournaments && _tournaments != null && _tournaments.Count > 0)
            {
                var json = await Json.StringifyAsync(_tournaments);
                await _cacheService.CacheTextFileAsync(TournamentsCacheFileName, json);
            }
            if (saveSettings && _guildSettings != null && _guildSettings.Count > 0)
            {
                var json = await Json.StringifyAsync(_guildSettings);
                await _cacheService.CacheTextFileAsync(SettingsFileName, json);
            }
        }
        catch (Exception)
        {
        }
    }

    private async Task<List<LadderRegionModel>> UpdateRegionsInternalAsync(BattleNetProfileModel profile, TimeSpan? regionUpdateThreshold = null)
    {
        if (profile == null || profile.Record == null)
        {
            return [];
        }

        var teams = await _ladderService.Get1v1TeamsAsync(profile.Record.PulseId);
        if (teams.Count == 0)
        {
            return [];
        }
        var list = new List<LadderRegionModel>();
        foreach (var item in teams)
        {
            var itemRace = LadderHelper.GetRaceFromTeamResult(item);
            string itemRaceText = itemRace == Race.Unknown ? string.Empty : itemRace.ToString();

            var storedRegion = profile.LadderRegions?.FirstOrDefault(r => r.SeasonId == item.Season && r.Race == itemRaceText);

            if (storedRegion != null && regionUpdateThreshold != null)
            {
                if (regionUpdateThreshold != null)
                {
                    if (storedRegion.LastUpdated + regionUpdateThreshold > DateTime.UtcNow)
                    {
                        continue;
                    }
                }
            }

            var id = storedRegion?.Id ?? string.Empty;
            var previousMMR = storedRegion?.CurrentMMR ?? 0;
            var regionUpdated = !AreRegionsEqual(storedRegion, item);

            var region = new LadderRegionModel(
                id,
                item.Season,
                item.Region,
                itemRaceText,
                item.Rating,
                previousMMR,
                item.LeagueType,
                item.TierType,
                item.Wins,
                (item.Wins + item.Losses + item.Ties),
                profile.Record.Id,
                regionUpdated);

            list.Add(region);
        }

        return list;
    }
}