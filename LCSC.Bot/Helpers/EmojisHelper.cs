using DSharpPlus;
using LCSC.Discord.Strings;
using LCSC.Models;

namespace LCSC.Discord.Helpers
{
    internal static class EmojisHelper
    {
        private const string DefaultFlagEmojiString = ":globe_with_meridians:";

        private static readonly Dictionary<string, string> CacheMap = [];

        private static readonly Dictionary<string, string> FlagMaps = new()
        {
            { "Mexico", ":flag_mx:"},
            { "Belize", ":flag_bz:"},
            { "Guatemala", ":flag_gt:"},
            { "El Salvador", ":flag_sv:"},
            { "Honduras", ":flag_hn:"},
            { "Nicaragua", ":flag_ni:"},
            { "Costa Rica", ":flag_cr:"},
            { "Panama", ":flag_pa:"},
            { "Cuba", ":flag_cu:"},
            { "Jamaica", ":flag_jm:"},
            { "Haiti", ":flag_ht:"},
            { "Republica Dominicana", ":flag_do:"},
            { "Puerto Rico", ":flag_pr:"},
            { "Bahamas", ":flag_bs:"},
            { "Colombia", ":flag_co:"},
            { "Venezuela", ":flag_ve:"},
            { "Ecuador", ":flag_ec:"},
            { "Peru", ":flag_pe:"},
            { "Bolivia", ":flag_bo:"},
            { "Chile", ":flag_cl:"},
            { "Paraguay", ":flag_py:"},
            { "Uruguay", ":flag_uy:"},
            { "Argentina", ":flag_ar:"},
            { "España", ":flag_es:"},
            { "US", ":flag_us:"},
            { "Alemania", ":flag_de:"},
            { "Holanda", ":flag_nl:"},
        };

        public static string GetFlagEmojiString(string? countryTag)
        {
            if (!string.IsNullOrWhiteSpace(countryTag) && FlagMaps.TryGetValue(countryTag, out string? value))
            {
                return value;
            }
            return DefaultFlagEmojiString;
        }

        public static Task<string> GetLeagueEmojiStringAsync(DiscordClient client, int league, int tier)
        {
            var emojiId = (league, tier) switch
            {
                (0, 0) => EmojiResources.LadderBronze1,
                (0, 1) => EmojiResources.LadderBronze2,
                (0, 2) => EmojiResources.LadderBronze3,
                (1, 0) => EmojiResources.LadderSilver1,
                (1, 1) => EmojiResources.LadderSilver2,
                (1, 2) => EmojiResources.LadderSilver3,
                (2, 0) => EmojiResources.LadderGold1,
                (2, 1) => EmojiResources.LadderGold2,
                (2, 2) => EmojiResources.LadderGold3,
                (3, 0) => EmojiResources.LadderPlatinum1,
                (3, 1) => EmojiResources.LadderPlatinum2,
                (3, 2) => EmojiResources.LadderPlatinum3,
                (4, 0) => EmojiResources.LadderDiamond1,
                (4, 1) => EmojiResources.LadderDiamond2,
                (4, 2) => EmojiResources.LadderDiamond3,
                (5, 0) => EmojiResources.LadderMaster1,
                (5, 1) => EmojiResources.LadderMaster2,
                (5, 2) => EmojiResources.LadderMaster3,
                (0, _) => EmojiResources.LadderBronze,
                (1, _) => EmojiResources.LadderSilver,
                (2, _) => EmojiResources.LadderGold,
                (3, _) => EmojiResources.LadderPlatinum,
                (4, _) => EmojiResources.LadderDiamond,
                (5, _) => EmojiResources.LadderMaster,
                (6, _) => EmojiResources.LadderGrandmaster,
                _ => EmojiResources.LadderPlacement,
            };
            return GetEmojiStringAsync(client, emojiId);
        }

        public static Task<string> GetRaceEmojiStringAsync(DiscordClient client, string? race)
        {
            race = race == null ? string.Empty : race.ToLowerInvariant();
            var raceId = race switch
            {
                "protoss" => EmojiResources.RaceProtoss,
                "zerg" => EmojiResources.RaceZerg,
                "terran" => EmojiResources.RaceTerran,
                "random" => EmojiResources.RaceRandom,
                _ => EmojiResources.RaceUnknown
            };
            return GetEmojiStringAsync(client, raceId);
        }

        private static async Task<string> GetEmojiStringAsync(DiscordClient client, string emojiId)
        {
            if (CacheMap.TryGetValue(emojiId, out var value))
            {
                return value;
            }
            if (ulong.TryParse(emojiId, out ulong id))
            {
                var emoji = await client.GetApplicationEmojiAsync(id);
                var messageFormat = emoji.ToString();
                CacheMap.TryAdd(emojiId, messageFormat);
                return messageFormat;
            }
            return string.Empty;
        }
    }
}