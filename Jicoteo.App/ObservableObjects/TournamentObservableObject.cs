using CommunityToolkit.Mvvm.ComponentModel;
using LCSC.Http.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LCSC.App.ObservableObjects
{
    public partial class TournamentObservableObject(
        TournamentRecord record,
        MemberObservableObject? place1 = null,
        MemberObservableObject? place2 = null,
        MemberObservableObject? place3 = null,
        MemberObservableObject? place4 = null,
        List<MemberObservableObject>? participants = null,
        List<MatchObservableObject>? matches = null) : ObservableObject
    {
        private readonly TournamentRecord _record = record;

        public string Date => Record.Date == null ? string.Empty : Record.Date.Value.ToShortDateString();

        public string DisplayName => $"{Record.SeriesName} {Record.SeriesNumber}";

        public string? ExtraParticipantsCount
        {
            get
            {
                if (Participants == null || Participants.Count <= 4)
                {
                    return null;
                }
                var count = (new[] { HasPlace1, HasPlace2, HasPlace3, HasPlace4 }).Count(b => b);
                return $"+{((Participants?.Count - count) ?? 0)}";
            }
        }

        public bool HasPlace1 => Place1 != null;

        public bool HasPlace2 => Place2 != null;

        public bool HasPlace3 => Place3 != null;

        public bool HasPlace4 => Place4 != null;

        public string? LogoUrl => Record.LogoUrl;

        public List<MatchObservableObject>? Matches { get; } = matches;

        public List<MemberObservableObject>? Participants { get; } = participants;

        public string ParticipantsDisplayCount => Participants?.Count > 0 ? $"{Participants.Count} participantes" : "0 participantes";

        public MemberObservableObject? Place1 { get; } = place1;

        public MemberObservableObject? Place2 { get; } = place2;

        public MemberObservableObject? Place3 { get; } = place3;

        public MemberObservableObject? Place4 { get; } = place4;

        public TournamentRecord Record => _record;

        public string? SeriesName => _record.SeriesName;
    }
}