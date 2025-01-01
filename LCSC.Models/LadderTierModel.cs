namespace LCSC.Models;

public record class LadderTierModel(
    LadderLeague League,
    LadderTier Tier, 
    int MinMMR, 
    int MaxMMR) : IComparable<LadderTierModel>
{
    public int CompareTo(LadderTierModel? other)
    {
        return MaxMMR.CompareTo(other?.MaxMMR);
    }
}
