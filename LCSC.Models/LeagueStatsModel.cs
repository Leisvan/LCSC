
namespace LCSC.Models;

public record class LeagueStatsModel(
    int Place1Count, 
    int Place2Count, 
    int Place3Count, 
    int Place4Count, 
    int Participations,
    double TotalWinrate,
    double TerranWinrate,
    double ZergWinrate,
    double ProtossWinrate)
{
}
