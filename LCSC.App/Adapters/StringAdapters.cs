using LCSC.Models;
using LCSC.Models.Airtable;
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
        return participants?.Count > 0
            ? string.Format("Tournaments-ParticipantsCountFormat".GetTextLocalized(), participants.Count)
            : "Tournaments-NoParticipants".GetTextLocalized();
    }

    public static string ToRegionDisplayName(LadderRegionRecord? record)
    {
        if (record == null)
        {
            return string.Empty;
        }
        string regionLeague = record.League switch
        {
            0 => "Ladder-RankBronze",
            1 => "Ladder-RankSilver",
            2 => "Ladder-RankGold",
            3 => "Ladder-RankPlatinum",
            4 => "Ladder-RankDiamond",
            5 => "Ladder-RankMaster",
            6 => "Ladder-RankGrandMaster",
            _ => "Ladder-RankUnknown",
        };
        if (record.League < 0 || record.League > 6)
        {
            return regionLeague.GetTextLocalized();
        }
        return $"{record.Race} | {regionLeague.GetTextLocalized()} {record.Tier + 1}";
    }

    public static string ToTournamentAffiliationDescription(TournamentAffiliation affiliation)
    {
        return affiliation switch
        {
            TournamentAffiliation.Official => "Tournament-AffiliationOfficial".GetTextLocalized(),
            TournamentAffiliation.Partner => "Tournament-AffiliationPartner".GetTextLocalized(),
            TournamentAffiliation.Community => "Tournament-AffiliationCommunity".GetTextLocalized(),
            _ => string.Empty,
        };
    }

    public static string TournamentDate(DateTime? date)
                => date == null ? string.Empty : date.Value.ToShortDateString();

    public static string TournamentDisplayName(string? seriesName, string? seriesNumber)
            => $"{seriesName} {seriesNumber}";
}