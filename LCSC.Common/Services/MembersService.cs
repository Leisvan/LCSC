using LCSC.Http.Services;
using LCSC.Models;
using LCSC.Models.Airtable;
using LCSC.Models.Pulse;
using Newtonsoft.Json;

namespace LCSC.Core.Services
{
    public class MembersService(
       AirtableHttpService airtableHttpService,
       PulseHttpService pulseHttpService,
       BattleNetHttpService battleNetHttpService,
       CacheService cacheService)
    {
        private const string DefaultRegionParameter = "US";

        private readonly AirtableHttpService _airtableHttpService = airtableHttpService;
        private readonly List<MemberModel> _members = [];
        private readonly PulseHttpService _pulseHttpService = pulseHttpService;
        private readonly List<TournamentModel> _tournaments = [];
        private readonly LadderService _ladderService = new(pulseHttpService, battleNetHttpService, cacheService);

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

        private async Task RefreshAllAsync()
        {
            await _ladderService.TryLadder();

            //Refresh members:
            var members = await _airtableHttpService.GetMemberRecordsAsync();
            if (members != null && members.Any())
            {
                _members.Clear();
                var bnetProfiles = await _airtableHttpService.GetBattleNetProfilesAsync();
                if (bnetProfiles == null)
                {
                    _members.AddRange(members
                        .Select(m => new MemberModel(m, []))
                        .OrderBy(x => x.Record.Nick));
                }
                else
                {
                    foreach (var member in members)
                    {
                        var profiles = bnetProfiles?
                            .Where(profile => profile.Members?.Any(m => m == member.Id) ?? false)
                            .OrderByDescending(x => x.MainProfile)
                            .ToList() ?? [];

                        _members.Add(new MemberModel(member, profiles));
                    }
                    _members.Sort();
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
        }

        public async Task<List<MemberModel>> GetMembersAsync(bool forceRefresh = false)
        {
            if (forceRefresh || _members.Count == 0)
            {
                await RefreshAllAsync();
                foreach (var member in _members)
                {
                    member.Stats = GetMemberStats(member.Record.Id);
                }
            }
            return _members;
        }

        public async Task<List<TournamentModel>> GetTournamentsAsync(bool forceRefresh = false)
        {
            if (forceRefresh || _tournaments.Count == 0)
            {
                await RefreshAllAsync();
            }
            return _tournaments;
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
                        result.Members?.Account?.Id.ToString(),
                        result.Members?.Character?.Realm.ToString(),
                        result.Members?.Character?.BattlenetId.ToString());
                }
            }
            return null;
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

        private static string? GetFirstItemFromArray(string[]? array)
        {
            if (array == null || array.Length == 0)
            {
                return null;
            }
            return array[0];
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

        private double RacePercent(IEnumerable<MatchModel> matches, string playerId, Race race)
        {
            int winCount = matches
                .Where(x => x.Winner.Record.Id == playerId && x.LoserRace == race)
                .Select(x => x.WinnerScore)
                .Sum();
            int allCount = matches.Where(x => (x.Winner.Record.Id == playerId && x.LoserRace == race) || (x.Loser.Record.Id == playerId && x.WinnerRace == race))
                .Select(x => x.WinnerScore + x.LoserScore).Sum();
            return allCount == 0 ? 0 : (winCount * 100) / allCount;
        }
    }
}