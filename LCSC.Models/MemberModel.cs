using LCSC.Models.Airtable;

namespace LCSC.Models;

public record class MemberModel(
    MemberRecord Record,
    List<BattleNetProfileRecord>? Profiles) : IComparable<MemberModel>
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
}