namespace LCSC.Http.Models.Pulse;

public record Member
{
    public int? TerranGamesPlayed { get; set; }
    public int? ProtossGamesPlayed { get; set; }
    public int? ZergGamesPlayed { get; set; }
    public int? RandomGamesPlayed { get; set; }
    public Character? Character { get; set; }
    public Account? Account { get; set; }
    public Clan? Clan { get; set; }
}