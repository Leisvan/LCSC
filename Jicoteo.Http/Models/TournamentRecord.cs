namespace LCSC.Http.Models;

public record class TournamentRecord(
    string Id,
    int Number,
    string? SeriesName = null,
    string? SeriesNumber = null,
    DateTime? Date = null,
    string? BracketsUrl = null,
    string? Description = null,
    string? DetailsUrl = null,
    string? LogoUrl = null,
    string? SpecialUrl = null,
    string? SpecialUrlDescription = null,
    string[]? Place1 = null,
    string[]? Place2 = null,
    string[]? Place3 = null,
    string[]? Place4 = null,
    string[]? Participants = null,
    string? MatchesData = null)
{
}