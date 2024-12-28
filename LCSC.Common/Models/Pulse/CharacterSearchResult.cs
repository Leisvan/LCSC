using System.Text.Json.Serialization;

namespace LCSC.Common.Models.Pulse;

public record class CharacterSearchResult
{
    [JsonPropertyName("leagueMax")]
    //public int? LeagueMax { get; set; }
    //public int? RatingMax { get; set; }
    //public int? TotalGamesPlayed { get; set; }
    //public Stats? PreviousStats { get; set; }
    //public Stats? CurrentStats { get; set; }
    public Member? Members { get; set; }
}