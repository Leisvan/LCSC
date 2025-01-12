namespace LCSC.Models.Airtable;

public record class DiscordBotSettingsRecord(
    string Id,
    int Number = 0,
    string? Key = "",
    string? Value = "",
    string? GuildId = "",
    string? Description = "")
{
    public ulong GetIdNumber()
    {
        if (!string.IsNullOrEmpty(GuildId) && ulong.TryParse(GuildId, out var value))
        {
            return value;
        }
        return 0;
    }
}