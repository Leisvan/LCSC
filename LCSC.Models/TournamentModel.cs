using LCSC.Models.Airtable;

namespace LCSC.Models;

public record class TournamentModel(
    TournamentRecord Record,
    MemberModel? Place1 = null,
    MemberModel? Place2 = null,
    MemberModel? Place3 = null,
    MemberModel? Place4 = null,
    List<MemberModel>? Participants = null,
    List<MatchModel>? Matches = null)
{
    public bool HasPlace1 => Place1 != null;

    public bool HasPlace2 => Place2 != null;

    public bool HasPlace3 => Place3 != null;

    public bool HasPlace4 => Place4 != null;
}