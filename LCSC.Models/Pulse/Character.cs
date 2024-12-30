namespace LCSC.Models.Pulse;

public record Character
{
    public int? Realm { get; set; }
    public string? Name { get; set; }
    public long? Id { get; set; }
    public long? AccountId { get; set; }
    public string? Region { get; set; }
    public int? BattlenetId { get; set; }
}