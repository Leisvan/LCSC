namespace LCSC.Models;

public record class LadderRegionModel(
    string Id,
    int SeasonId,
    string? Region,
    string Race,
    int CurrentMMR,
    int PreviousMMR,
    int League,
    int Tier,
    int Wins,
    int TotalMatches,
    string BattleNetProfileId,
    bool IsUpdated = false);