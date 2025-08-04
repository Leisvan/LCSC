using LCSC.Models.Airtable;

namespace LCSC.Models;

public record class MemberModel(
    MemberRecord Record,
    List<BattleNetProfileModel>? Profiles) : IComparable<MemberModel>
{
    public Race Race
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Record.KnownRace))
            {
                return Race.Unknown;
            }
            return Enum.Parse<Race>(Record.KnownRace, true);
        }
    }

    public bool ValidProfiles => Profiles != null && Profiles.Count > 0;

    public LeagueStatsModel? Stats { get; set; }

    public string Nick => Record.Nick ?? string.Empty;

    public int CompareTo(MemberModel? other)
    {
        if (other == null)
        {
            return 1;
        }
        if (Record.Nick == null)
        {
            return -1;
        }
        return Record.Nick.CompareTo(other.Record.Nick);
    }

    public string MMR
    {
        get
        {
            if (Profiles != null)
            {
                return Profiles.Select(x => x.LadderRegion?.CurrentMMR).Max()?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }
    }
}