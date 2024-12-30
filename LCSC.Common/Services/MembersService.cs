using LCSC.Http.Services;
using LCSC.Models;
using LCSC.Models.Airtable;
using Newtonsoft.Json;

namespace LCSC.Core.Services
{
    public class MembersService(
       AirtableHttpService airtableHttpService,
       PulseHttpService pulseHttpService)
    {
        private const string DefaultRegionParameter = "US";

        private readonly AirtableHttpService _airtableHttpService = airtableHttpService;
        private readonly List<MemberModel> _members = [];
        private readonly PulseHttpService _pulseHttpService = pulseHttpService;
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

        public async Task<List<MemberModel>> GetMembersAsync(bool forceRefresh = false)
        {
            if (forceRefresh || _members.Count == 0)
            {
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
            }
            return _members;
        }

        public async Task<List<TournamentModel>> GetTournamentsAsync(bool forceRefresh = false)
        {
            if (forceRefresh || _tournaments.Count == 0)
            {
                var tournaments = await _airtableHttpService.GetTournamentRecordsAsync();
                if (tournaments != null && tournaments.Any())
                {
                    var results = new List<TournamentModel>();
                    foreach (var item in tournaments)
                    {
                        var p1 = GetPlayerById(GetFirstItemFromArray(item.Place1));
                        var p2 = GetPlayerById(GetFirstItemFromArray(item.Place2));
                        var p3 = GetPlayerById(GetFirstItemFromArray(item.Place3));
                        var p4 = GetPlayerById(GetFirstItemFromArray(item.Place4));
                        var participants = GetAllParticipants(item);
                        var matches = GetMatchModels(item);
                        var tournament = new TournamentModel(item, p1, p2, p3, p4, participants, matches);
                        results.Add(tournament);
                    }
                    _tournaments.Clear();
                    _tournaments.AddRange(results.OrderByDescending(t => t.Record.Date));
                }
            }
            return _tournaments;
        }

        public async Task<ProfileSearchResult?> SearchProfileByBattleTag(string? battleTag)
        {
            if (battleTag == null)
            {
                return null;
            }
            var results = await _pulseHttpService.CharacterSearchAsync(battleTag);
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
                var player = GetPlayerById(item);
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
                var winner = GetPlayerById(match.WinnerId);
                var loser = GetPlayerById(match.LoserId);
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

        private MemberModel? GetPlayerById(string? id)
        {
            if (!string.IsNullOrEmpty(id) && _members.Count > 0)
            {
                return _members.FirstOrDefault(m => m.Record.Id == id);
            }
            return null;
        }
    }
}