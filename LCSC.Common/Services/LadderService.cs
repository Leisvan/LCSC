using LCSC.Http.Services;
using LCSC.Models;
using LCSC.Models.Pulse;
using Newtonsoft.Json;

namespace LCSC.Core.Services
{
    public class LadderService(CacheService cacheService, string? clientId, string? clientSecret)
    {
        private const string SeasonCacheFileName = "Seasons.json";
        private const string TiersCacheFileName = "Tiers.json";
        private static readonly TimeSpan SeasonCacheTimeout = TimeSpan.FromDays(1);
        private static readonly TimeSpan TiersCacheTimout = TimeSpan.FromDays(1);
        private readonly BattleNetHttpService _battleNetHttpService = new(clientId, clientSecret);
        private readonly CacheService _cacheService = cacheService;
        private readonly PulseHttpService _pulseHttpService = new();
        private int _seasonId = 0;
        private List<LadderTierModel> _tiers = [];

        public async Task<int> GetSeasonIdAsync()
        {
            if (_seasonId != 0)
            {
                return _seasonId;
            }

            //Load from cache:
            Season? season = null;
            var loadedCacheData = await _cacheService.GetCachedTextAsync(SeasonCacheFileName);
            if (!string.IsNullOrEmpty(loadedCacheData))
            {
                try
                {
                    season = JsonConvert.DeserializeObject<Season>(loadedCacheData);
                    if (season != null && (season.LastUpdated + SeasonCacheTimeout) > DateTime.Now)
                    {
                        return _seasonId = season.BattlenetId;
                    }
                }
                catch (Exception)
                {
                }
            }

            //Retrieve API data:
            var seasons = await _pulseHttpService.GetSeasonsDataAsync();
            var currentSeason = seasons?.OrderByDescending(season => season.BattlenetId)
                    .FirstOrDefault();
            if (currentSeason != null)
            {
                try
                {
                    currentSeason.LastUpdated = DateTime.UtcNow;
                    var data = JsonConvert.SerializeObject(currentSeason);
                    await _cacheService.CacheTextFileAsync(SeasonCacheFileName, data);
                }
                catch (Exception)
                {
                }
                return _seasonId = currentSeason.BattlenetId;
            }
            //If we are still here, there's a cached season and it hasn't ended, we use that
            if (season != null && season.End > DateTimeOffset.Now)
            {
                return season.BattlenetId;
            }
            return 0;
        }

        internal async Task<Team?> Get1v1TeamAsync(string? pulseId)
        {
            if (string.IsNullOrEmpty(pulseId))
            {
                return null;
            }
            var result = await _pulseHttpService.GetCharacterCommonDataAsync(pulseId);
            if (result == null)
            {
                return null;
            }
            var seasonId = await GetSeasonIdAsync();
            var validTeam = result.Teams?
                .Where(t => t.QueueType == 201 && t.Season == seasonId)
                .FirstOrDefault();

            if (validTeam == null)
            {
                return null;
            }
            await SetLadderTiersAsync(validTeam);
            return validTeam;
        }

        private async Task<List<LadderTierModel>?> GetLadderTiersAsync()
        {
            if (_tiers.Count != 0)
            {
                return _tiers;
            }
            var seasonId = await GetSeasonIdAsync();
            if (seasonId == 0)
            {
                return null;
            }

            TierCacheData? cachedData = null;
            var loadedCacheData = await _cacheService.GetCachedTextAsync(TiersCacheFileName);

            //Load from cache:
            if (!string.IsNullOrEmpty(loadedCacheData))
            {
                try
                {
                    cachedData = JsonConvert.DeserializeObject<TierCacheData>(loadedCacheData);
                    if (cachedData != null
                        && cachedData.SeasonId == seasonId
                        && (cachedData.LastUpdated + TiersCacheTimout) > DateTime.Now
                        && cachedData.Tiers.Count > 0)
                    {
                        _tiers = [.. cachedData.Tiers];
                        return _tiers;
                    }
                }
                catch (Exception)
                {
                }
            }

            //Load from API:
            var values = Enum.GetValues<LadderLeague>().Except([LadderLeague.Grandmaster]);
            var list = new List<LadderTierModel>();
            foreach (var value in values)
            {
                var tiers = await _battleNetHttpService.GetLadderTiersForLeague(_seasonId, (int)value);
                if (tiers != null && tiers.Count > 0)
                {
                    list.AddRange(tiers);
                }
            }
            //Cache results:
            if (list.Count > 0)
            {
                try
                {
                    var dataToCache = new TierCacheData(_seasonId, list, DateTime.Now);
                    var text = JsonConvert.SerializeObject(dataToCache);
                    await _cacheService.CacheTextFileAsync(TiersCacheFileName, text);
                    _tiers = list;
                    return _tiers;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            if (cachedData != null && cachedData.SeasonId == _seasonId)
            {
                return cachedData.Tiers;
            }
            return null;
        }

        private async Task SetLadderTiersAsync(Team? team)
        {
            if (team == null)
            {
                return;
            }
            var bnetTiers = await GetLadderTiersAsync();
            if (bnetTiers == null)
            {
                return;
            }
            bnetTiers.Reverse();
            foreach (var tier in bnetTiers)
            {
                if (team.Rating > tier.MinMMR && team.Rating <= tier.MaxMMR)
                {
                    team.LeagueType = (int)tier.League;
                    team.TierType = (int)tier.Tier;
                }
            }
        }

        private record class TierCacheData(
            int SeasonId,
            List<LadderTierModel> Tiers,
            DateTime LastUpdated);
    }
}