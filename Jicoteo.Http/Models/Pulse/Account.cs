namespace LCSC.Http.Models.Pulse;

public record Account
{
    public string? BattleTag { get; set; }
    public int? Id { get; set; }
    public string? Partition { get; set; }
    public bool? Hidden { get; set; }
}