namespace LCSC.Models.Airtable
{
    public record class MemberRecord(
        string Id,
        int Number = 0,
        string? Nick = null,
        string? RealName = null,
        string? KnownRace = null,
        string? CountryTag = null,
        string? DiscordId = null,
        bool Verified = false,
        bool Banned = false,
        string? PictureUrl = null,
        string? ContactEmail = null,
        string? ChallongeEmail = null,
        string? Phone = null,
        string[]? Tournaments = null,
        string[]? Tournaments1st = null,
        string[]? Tournaments2nd = null,
        string[]? Tournaments3rd = null,
        string[]? Tournaments4th = null,
        string[]? BattleNetProfiles = null,
        string? LadderStats = null)
    {
    }
}