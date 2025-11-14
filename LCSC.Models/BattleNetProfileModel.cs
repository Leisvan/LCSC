using LCSC.Models.Airtable;

namespace LCSC.Models;

public record class BattleNetProfileModel(
    BattleNetProfileRecord Record,
    List<LadderRegionRecord>? LadderRegions) : IComparable<BattleNetProfileModel>
{
    public int CompareTo(BattleNetProfileModel? other)
    {
        if (other == null)
            return 1;
        return other.Record.MainProfile.CompareTo(Record.MainProfile);
    }

    public string BestMMR
    {
        get
        {
            if (LadderRegions?.Count > 0)
            {
                return LadderRegions.Max(r => r.CurrentMMR).ToString();
            }
            return 0.ToString();
        }
    }
}