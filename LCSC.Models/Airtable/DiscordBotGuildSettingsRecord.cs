namespace LCSC.Models.Airtable;

public record class DiscordBotGuildSettingsRecord(
    string Id,
    int Number = 0,
    string? GuildId = "",
    string? GuildName = "",
    bool IsDebugGuild = false,
    int RegionUpdateThresholdInMinutes = 0,
    string? RankingChannelId = "",
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