
namespace LCSC.Models.BattleNet;

public record class BattleNetToken(
    string Access_Token, 
    string Token_Type, 
    int Expires_In, 
    string Score)
{
    public DateTime IssuedTime { get; set; }

    public bool IsTokenExpired() 
    { 
        var expirationTime = IssuedTime.AddSeconds(Expires_In); 
        return DateTime.UtcNow > expirationTime; 
    }
}
