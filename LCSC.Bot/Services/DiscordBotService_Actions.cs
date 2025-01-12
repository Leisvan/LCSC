using DSharpPlus.Entities;

namespace LCSC.Discord.Services;

public partial class DiscordBotService
{
    public async Task UpdateMemberRegionsAsync(bool forceUpdate = false, ulong guildId = 0)
    {
        TimeSpan? updateTime = null;
        if (!forceUpdate)
        {
            var regionUpdateMinutesThreshold = _settingsService.GetIntValue(DiscordBotSettingKey.RegionUpdateThresholdInMinutes, guildId);
            if (regionUpdateMinutesThreshold.HasValue && regionUpdateMinutesThreshold.Value > 0)
            {
                updateTime = TimeSpan.FromMinutes(regionUpdateMinutesThreshold.Value);
            }
        }
        var channelId = _settingsService.GetStringValue(DiscordBotSettingKey.RankingChannel, guildId);
        if (channelId == null || !ulong.TryParse(channelId, out var channelIdNumber) || channelIdNumber == 0)
        {
            LogNotifier.NotifyError("ChannelId not found in UpdateMemberRegionsAsync");
            return;
        }
        DiscordMessage? updateMessage = null;
        await _membersService.UpdateAllRegionsAsync(updateTime,
            async (number, count, tag) =>
            {
                string content = $"Actualizando {number} de {count} perfiles: {tag}";
                updateMessage = await SendMessageAsync(content, channelIdNumber, updateMessage);
            });
    }
}