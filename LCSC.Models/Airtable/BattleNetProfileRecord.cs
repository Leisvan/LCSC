namespace LCSC.Models.Airtable;

public record class BattleNetProfileRecord(
    string Id,
    int Number = 0,
    string? BattleTag = null,
    string? PulseId = null,
    string? ProfileRealm = null,
    string? ProfileId = null,
    bool MainProfile = false,
    string? Notes = null,
    string[]? Members = null,
    string[]? LadderRegion = null);