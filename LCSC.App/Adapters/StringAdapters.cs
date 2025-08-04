using LCSC.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LCSC.App.Adapters;

public static class StringAdapters
{
    public static string ExtraParticipantsCount(List<MemberModel>? participants, bool hasP1, bool hasP2, bool hasP3, bool hasP4)
    {
        if (participants == null || participants.Count <= 4)
        {
            return string.Empty;
        }
        var count = (new[] { hasP1, hasP2, hasP3, hasP4 }).Count(b => b);
        return $"+{((participants?.Count - count) ?? 0)}";
    }

    public static string ParticipantsDisplayCount(List<MemberModel>? participants)
    {
        if (participants == null)
        {
            return string.Empty;
        }
        return participants?.Count > 0 ? $"{participants.Count} participantes" : "0 participantes";
    }

    public static string ToTournamentAffiliationDescription(TournamentAffiliation affiliation)
    {
        return affiliation switch
        {
            TournamentAffiliation.Official => "Torneo oficial",
            TournamentAffiliation.Partner => "Torneo en colaboración",
            TournamentAffiliation.Community => "Torneo de la comunidad",
            _ => string.Empty,
        };
    }

    public static string TournamentDate(DateTime? date)
                => date == null ? string.Empty : date.Value.ToShortDateString();

    public static string TournamentDisplayName(string? seriesName, string? seriesNumber)
            => $"{seriesName} {seriesNumber}";
}