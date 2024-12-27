namespace LCSC.Http.Models;

public record class BattleNetProfileRecord(string Id,
    int Number = 0,
    string? BattleTag = null,
    string? PulseId = null,
    string? ProfileRealm = null,
    string? ProfileId = null,
    string? AccountId = null,
    bool MainProfile = false,
    string? Path = null,
    string? Notes = null,
    string[]? Members = null)
{
}