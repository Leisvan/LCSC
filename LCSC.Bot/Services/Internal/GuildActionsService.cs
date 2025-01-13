using DSharpPlus.Entities;
using LCSC.Core.Services;

namespace LCSC.Discord.Services.Internal
{
    internal class GuildActionsService(MembersService membersService, DiscordBotService botService, SettingsService settingsService)
    {
        private readonly DiscordBotService _botService = botService;
        private readonly MembersService _membersService = membersService;
        private readonly SettingsService _settingsService = settingsService;

        public async Task UpdateMemberRegionsAsync(
            bool forceUpdate = false,
            ulong guildId = 0,
            ulong channelId = 0,
            DiscordMessage? message = null)
        {
            TimeSpan? updateTime = null;
            if (!forceUpdate)
            {
                var regionUpdateMinutesThreshold = _settingsService.GetIntValue(SettingKey.RegionUpdateThresholdInMinutes, guildId);
                if (regionUpdateMinutesThreshold.HasValue && regionUpdateMinutesThreshold.Value > 0)
                {
                    updateTime = TimeSpan.FromMinutes(regionUpdateMinutesThreshold.Value);
                }
            }
            if (channelId == 0)
            {
                var channelIdSettingValue = _settingsService.GetStringValue(SettingKey.RankingChannel, guildId);
                if (channelIdSettingValue == null || !ulong.TryParse(channelIdSettingValue, out channelId) || channelId == 0)
                {
                    LogNotifier.NotifyError("ChannelId not found in UpdateMemberRegionsAsync");
                    return;
                }
            }

            await _membersService.UpdateAllRegionsAsync(updateTime,
                async (number, count, tag) =>
                {
                    string content = $"Actualizando {number} de {count} perfiles: {tag}";
                    message = await UpdateMessageAsync(content, channelId, message);
                });
        }

        private async Task<DiscordMessage?> UpdateMessageAsync(string content, ulong channelId, DiscordMessage? message = null)
        {
            try
            {
                if (message == null)
                {
                    var channel = await _botService.Client.GetChannelAsync(channelId);
                    return await channel.SendMessageAsync(content);
                }
                return await message.ModifyAsync(content);
            }
            catch (Exception e)
            {
                LogNotifier.Notify(e.Message);
                return null;
            }
        }
    }
}