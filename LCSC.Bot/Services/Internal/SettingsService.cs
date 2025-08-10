using LCSC.Core.Services;
using LCSC.Models;
using LCSC.Models.Airtable;

namespace LCSC.Discord.Services.Internal;

internal class SettingsService
{
    private readonly MembersService _membersService;

    private List<DiscordBotGuildSettingsRecord>? _records;

    public SettingsService(MembersService membersService)
    {
        _membersService = membersService;
        InitializeAsync();
    }

    public IEnumerable<GuildSettingsModel> GetAllGuilds(bool includeDebugGuilds = true)
    {
        if (_records == null)
        {
            return [];
        }
        var matches = includeDebugGuilds ? _records : [.. _records.Where(x => !x.IsDebugGuild)];

        List<GuildSettingsModel> models = [];
        foreach (var item in matches)
        {
            var id = item.GetIdNumber();
            if (id > 0)
            {
                models.Add(new GuildSettingsModel(item));
            }
        }
        return models;
    }

    public GuildSettingsModel? GetGuildSettings(ulong guildId)
    {
        if (_records == null)
        {
            return null;
        }
        var record = _records.FirstOrDefault(x => x.GuildId == guildId.ToString());
        if (record == null)
        {
            return null;
        }
        return new GuildSettingsModel(record);
    }

    private async void InitializeAsync()
    {
        var results = await _membersService.GetDiscordBotGuildsSettingsAsync();
        if (results != null)
        {
            _records = [.. results];
        }
    }
}