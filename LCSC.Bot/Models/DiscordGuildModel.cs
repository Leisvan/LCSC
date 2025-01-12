namespace LCSC.Discord.Models;

public record class DiscordGuildModel(string? Name = "", ulong Id = 0)
{
    public override string ToString()
        => Name ?? string.Empty;
}