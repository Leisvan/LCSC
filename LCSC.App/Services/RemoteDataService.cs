//using LCSC.App.Models;
//using LCSC.App.ObservableObjects;
//using LCSC.Common.Models;
//using LCSC.Common.Models.Airtable;
//using LCSC.Http.Services;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace LCSC.App.Services
//{
//    public class RemoteDataService
//    {
//        private const string DefaultRegionParameter = "US";

//        private readonly AirtableHttpService _airtableHttpService;
//        private readonly List<MemberObservableObject> _members;
//        private readonly PulseHttpService _pulseHttpService;
//        private readonly List<TournamentObservableObject> _tournaments;

//        public RemoteDataService(
//            AirtableHttpService airtableHttpService,
//            PulseHttpService pulseHttpService)
//        {
//            _members = [];
//            _tournaments = [];
//            _airtableHttpService = airtableHttpService;
//            _pulseHttpService = pulseHttpService;
//        }

//        public Task<string?> CreateBattleNetProfile(string? battleTag, string? pulseId, string? profileRealm, string? profileId, bool mainProfile, string? notes, string memberId)
//        {
//            var record = new BattleNetProfileRecord(
//                string.Empty,
//                0,
//                battleTag,
//                pulseId,
//                profileRealm,
//                profileId,
//                mainProfile,
//                notes,
//                [memberId]);
//            return _airtableHttpService.CreateBattleNetProfile(record);
//        }

//        public async Task<List<MemberObservableObject>> GetMembersAsync(bool forceRefresh = false)
//        {
//            if (forceRefresh || _members.Count == 0)
//            {
//                var members = await _airtableHttpService.GetMemberRecordsAsync();
//                if (members != null && members.Any())
//                {
//                    _members.Clear();
//                    var bnetProfiles = await _airtableHttpService.GetBattleNetProfilesAsync();
//                    if (bnetProfiles == null)
//                    {
//                        _members.AddRange(members
//                            .Select(m => new MemberObservableObject(m, []))
//                            .OrderBy(x => x.Nick));
//                    }
//                    else
//                    {
//                        foreach (var member in members)
//                        {
//                            var profiles = bnetProfiles?
//                                .Where(profile => profile.Members?.Any(m => m == member.Id) ?? false)
//                                .OrderByDescending(x => x.MainProfile)
//                                .ToList() ?? [];

//                            _members.Add(new MemberObservableObject(member, profiles));
//                        }
//                        _members.Sort();
//                    }
//                }
//            }
//            return _members;
//        }

//        public async Task<List<TournamentObservableObject>> GetTournamentsAsync(bool forceRefresh = false)
//        {
//            if (forceRefresh || _tournaments.Count == 0)
//            {
//                var tournaments = await _airtableHttpService.GetTournamentRecordsAsync();
//                if (tournaments != null && tournaments.Any())
//                {
//                    var results = new List<TournamentObservableObject>();
//                    foreach (var item in tournaments)
//                    {
//                        var p1 = GetPlayerById(GetFirstItemFromArray(item.Place1));
//                        var p2 = GetPlayerById(GetFirstItemFromArray(item.Place2));
//                        var p3 = GetPlayerById(GetFirstItemFromArray(item.Place3));
//                        var p4 = GetPlayerById(GetFirstItemFromArray(item.Place4));
//                        var participants = GetAllParticipants(item);
//                        var matches = GetMatchModels(item);
//                        var tournament = new TournamentObservableObject(item, p1, p2, p3, p4, participants, matches);
//                        results.Add(tournament);
//                    }
//                    _tournaments.Clear();
//                    _tournaments.AddRange(results.OrderByDescending(t => t.Record.Date));
//                }
//            }
//            return _tournaments;
//        }

//        public async Task<ProfileSearchResult?> SearchProfileByBattleTag(string? battleTag)
//        {
//            if (battleTag == null)
//            {
//                return null;
//            }
//            var results = await _pulseHttpService.CharacterSearchAsync(battleTag);
//            if (results != null)
//            {
//                var result = results
//                    .Where(x =>
//                    string.Equals(x.Members?.Character?.Region, DefaultRegionParameter, StringComparison.InvariantCultureIgnoreCase) &&
//                    string.Equals(x.Members?.Account?.BattleTag, battleTag, StringComparison.InvariantCultureIgnoreCase))
//                    .FirstOrDefault();
//                if (result != null)
//                {
//                    return new ProfileSearchResult(
//                        result.Members?.Account?.BattleTag,
//                        result.Members?.Account?.Id.ToString(),
//                        result.Members?.Character?.Realm.ToString(),
//                        result.Members?.Character?.BattlenetId.ToString());
//                }
//            }
//            return null;
//        }

//        public async Task UpdateTournamentMatchesAsync(TournamentObservableObject tournament)
//        {
//            if (tournament == null || tournament.Matches == null)
//            {
//                return;
//            }
//            var matches = tournament.Matches.Select(match => new MatchJsonModel
//            {
//                LoserId = match.Loser.Record.Id,
//                LoserRace = (int)match.LoserRace,
//                LoserScore = match.LoserScore,
//                WinnerId = match.Winner.Record.Id,
//                WinnerRace = (int)match.WinnerRace,
//                WinnerScore = match.WinnerScore,
//                Stage = (int)match.Stage,
//                Notes = match.Notes,
//            });
//            try
//            {
//                var jsonText = System.Text.Json.JsonSerializer.Serialize(matches);
//                var result = await _airtableHttpService.UpdateTournamentMatchesData(tournament.Record.Id, jsonText);
//            }
//            catch (Exception)
//            {
//            }
//        }

//        private static string? GetFirstItemFromArray(string[]? array)
//        {
//            if (array == null || array.Length == 0)
//            {
//                return null;
//            }
//            return array[0];
//        }

//        private List<MemberObservableObject>? GetAllParticipants(TournamentRecord record)
//        {
//            if (record?.Participants == null || record.Participants.Length == 0)
//            {
//                return [];
//            }
//            var list = new List<MemberObservableObject>();
//            foreach (var item in record.Participants)
//            {
//                var player = GetPlayerById(item);
//                if (player != null)
//                {
//                    list.Add(player);
//                }
//            }
//            return list;
//        }

//        private List<MatchObservableObject> GetMatchModels(TournamentRecord record)
//        {
//            if (string.IsNullOrEmpty(record?.MatchesData))
//            {
//                return [];
//            }

//            MatchJsonModel[]? matchesArray = null;
//            try
//            {
//                matchesArray = JsonConvert.DeserializeObject<MatchJsonModel[]>(record.MatchesData);
//            }
//            catch (Exception)
//            {
//            }
//            if (matchesArray == null)
//            {
//                return [];
//            }

//            var list = new List<MatchObservableObject>();
//            foreach (var match in matchesArray)
//            {
//                var winner = GetPlayerById(match.WinnerId);
//                var loser = GetPlayerById(match.LoserId);
//                if (winner != null && loser != null)
//                {
//                    list.Add(new MatchObservableObject(
//                        winner,
//                        loser,
//                        (Race)match.WinnerRace,
//                        (Race)match.LoserRace,
//                        match.WinnerScore,
//                        match.LoserScore,
//                        (MatchStage)match.Stage,
//                        match.Notes));
//                }
//            }
//            list.Sort();
//            return list;
//        }

//        private MemberObservableObject? GetPlayerById(string? id)
//        {
//            if (!string.IsNullOrEmpty(id) && _members.Count > 0)
//            {
//                return _members.FirstOrDefault(m => m.Record.Id == id);
//            }
//            return null;
//        }
//    }
//}