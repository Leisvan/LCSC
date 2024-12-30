namespace LCSC.Models;

public record class MatchModel(
    MemberModel Winner,
    MemberModel Loser,
    Race WinnerRace = Race.Unknown,
    Race LoserRace = Race.Unknown,
    int WinnerScore = 0,
    int LoserScore = 0,
    MatchStage Stage = MatchStage.Round7,
    string? Notes = "") : IComparable<MatchModel>
{
    public int CompareTo(MatchModel? other)
    {
        if (other == null)
        {
            return 1;
        }
        return this.Stage.CompareTo(other.Stage);
    }
}