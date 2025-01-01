namespace LCSC.Models.Airtable;

public record class LadderRecord(
    string Id,
    int Number = 0,
    DateTime? LastUpdated = null,
    int SeasonId = 0,
    string? Race = null,
    int CurrentMMR = 0,
    int PreviousMMR = 0,
    int League = 0,
    int Tier = 0,
    int Wins = 0,
    int TotalMatches = 0,
    string[]? Members = null);
