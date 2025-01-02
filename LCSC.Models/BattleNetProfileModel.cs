using LCSC.Models.Airtable;

namespace LCSC.Models;

public record class BattleNetProfileModel(
    BattleNetProfileRecord Record,
    LadderRegionRecord? LadderRegion) : IComparable<BattleNetProfileModel>
{
    public int CompareTo(BattleNetProfileModel? other)
    {
        if (other == null) 
            return 1;
        return other.Record.MainProfile.CompareTo(Record.MainProfile);
    }
}
