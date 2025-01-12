using LCSC.Core.Services;
using LCSC.Discord.Models;
using LCSC.Http.Services;
using LCSC.Models.Airtable;

namespace LCSC.Discord.Services
{
    public enum DiscordBotSettingKey
    {
        RankingChannel,
        GuildName,
        RegionUpdateThresholdInMinutes,
    }

    public class DiscordBotSettingsService
    {
        private readonly AirtableHttpService _airtableHttpService;

        private List<DiscordBotSettingsRecord>? _records;

        public DiscordBotSettingsService(AirtableHttpService airtableHttpService)
        {
            _airtableHttpService = airtableHttpService;
            InitializeAsync();
        }

        public IEnumerable<DiscordGuildModel> GetAllGuilds()
        {
            if (_records == null)
            {
                return [];
            }
            var keyText = DiscordBotSettingKey.GuildName.ToString();
            var matches = _records.Where(r => r.Key == keyText);
            List<DiscordGuildModel> models = [];
            foreach (var item in matches)
            {
                var id = item.GetIdNumber();
                if (id > 0)
                {
                    models.Add(new DiscordGuildModel(item.Value, id));
                }
            }
            return models;
        }

        public double? GetDoubleValue(DiscordBotSettingKey key, ulong guildId = 0)
        {
            var value = GetSettingValue(key.ToString(), guildId);
            return value != null ? Convert.ToDouble(value) : null;
        }

        public int? GetIntValue(DiscordBotSettingKey key, ulong guildId = 0)
        {
            var value = GetSettingValue(key.ToString(), guildId);
            return value != null ? Convert.ToInt32(value) : null;
        }

        public string? GetStringValue(DiscordBotSettingKey key, ulong guildId = 0)
                    => GetSettingValue(key.ToString(), guildId);

        public ulong? GetUlongValue(DiscordBotSettingKey key, ulong guildId = 0)
        {
            var value = GetSettingValue(key.ToString(), guildId);
            return value != null ? Convert.ToUInt64(value) : null;
        }

        private string? GetSettingValue(string key, ulong guildId = 0)
        {
            if (_records == null)
            {
                return null;
            }
            var settings = _records.Where(setting => setting.Key == key);
            if (guildId != 0)
            {
                var guildText = guildId.ToString();
                settings = settings.Where(setting => setting.GuildId == guildText);
            }
            var match = settings.FirstOrDefault();
            return match?.Value;
        }

        private async void InitializeAsync()
        {
            var results = await _airtableHttpService.GetDiscordBotSettingsAsync();
            if (results != null)
            {
                _records = new List<DiscordBotSettingsRecord>(results);
            }
        }
    }
}