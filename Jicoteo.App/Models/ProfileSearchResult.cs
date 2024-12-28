namespace LCSC.App.Models;

public record class ProfileSearchResult(
    string? BattleTag, 
    string? PulseId, 
    string? ProfileRealm, 
    string? ProfileId)
{
}