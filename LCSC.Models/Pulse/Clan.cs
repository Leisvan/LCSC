namespace LCSC.Models.Pulse;

public record Clan
{
    public string? Tag { get; set; }
    public int? Id { get; set; }
    public string? Region { get; set; }
    public string? Name { get; set; }
    public int? Members { get; set; }
    public int? ActiveMembers { get; set; }
    public int? AvgRating { get; set; }
    public string? AvgLeagueType { get; set; }
    public int? Games { get; set; }
}