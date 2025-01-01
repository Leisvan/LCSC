namespace LCSC.Models.Pulse;

public class Season
{
    public int Number { get; set; }
    public int Year { get; set; }
    public int Id { get; set; }
    public int BattlenetId { get; set; }
    public string? Region { get; set; }
    public DateTimeOffset? Start { get; set; }
    public DateTimeOffset? End { get; set; }

    public DateTime LastUpdated { get; set; }
}
