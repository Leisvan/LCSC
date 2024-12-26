using LCSC.App.Models;
using LCSC.App.ObservableObjects;
using LCSC.Http.Helpers;
using LCSC.Http.Models;
using LCSC.Http.Models.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Isolation;

namespace LCSC.App.Services
{
    public class AirtableService
    {
        private readonly List<MemberObservableObject> _members;
        private readonly List<TournamentObservableObject> _tournaments;

        public AirtableService()
        {
            _members = [];
            _tournaments = [];
        }

        public async Task<List<MemberObservableObject>> GetMembersAsync(bool forceRefresh = false)
        {
            if (forceRefresh || _members.Count == 0)
            {
                var members = await AirtableHttpHelper.GetMemberRecordsAsync();
                if (members != null && members.Any())
                {
                    _members.Clear();
                    _members.AddRange(members.OrderBy(m => m.Nick).Select(m => new MemberObservableObject(m)));
                }
            }
            return _members;
        }

        public async Task<List<TournamentObservableObject>> GetTournamentsAsync(bool forceRefresh = false)
        {
            if (forceRefresh || _tournaments.Count == 0)
            {
                var tournaments = await AirtableHttpHelper.GetTournamentRecordsAsync();
                if (tournaments != null && tournaments.Any())
                {
                    var results = new List<TournamentObservableObject>();
                    foreach (var item in tournaments)
                    {
                        var p1 = GetPlayerById(GetFirstItemFromArray(item.Place1));
                        var p2 = GetPlayerById(GetFirstItemFromArray(item.Place2));
                        var p3 = GetPlayerById(GetFirstItemFromArray(item.Place3));
                        var p4 = GetPlayerById(GetFirstItemFromArray(item.Place4));
                        var participants = GetAllParticipants(item);
                        var matches = GetMatchModels(item);
                        var tournament = new TournamentObservableObject(item, p1, p2, p3, p4, participants, matches);
                        results.Add(tournament);
                    }
                    _tournaments.Clear();
                    _tournaments.AddRange(results.OrderByDescending(t => t.Record.Date));
                }
            }
            return _tournaments;
        }

        public async Task UpdateTournamentMatchesAsync(TournamentObservableObject tournament)
        {
            if (tournament == null || tournament.Matches == null)
            {
                return;
            }
            var matches = tournament.Matches.Select(match => new MatchJsonModel
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
                var jsonText = System.Text.Json.JsonSerializer.Serialize(matches);
                var result = await AirtableHttpHelper.UpdateTournamentMatchesData(tournament.Record.Id, jsonText);
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

        private List<MemberObservableObject>? GetAllParticipants(TournamentRecord record)
        {
            if (record?.Participants == null || record.Participants.Length == 0)
            {
                return [];
            }
            var list = new List<MemberObservableObject>();
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

        private List<MatchObservableObject> GetMatchModels(TournamentRecord record)
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

            var list = new List<MatchObservableObject>();
            foreach (var match in matchesArray)
            {
                var winner = GetPlayerById(match.WinnerId);
                var loser = GetPlayerById(match.LoserId);
                if (winner != null && loser != null)
                {
                    list.Add(new MatchObservableObject(
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

        private MemberObservableObject? GetPlayerById(string? id)
        {
            if (!string.IsNullOrEmpty(id) && _members.Count > 0)
            {
                return _members.FirstOrDefault(m => m.Record.Id == id);
            }
            return null;
        }
    }
}