using LCSC.Http.Services;
using LCSC.Models;
using LCSC.Models.BattleNet;
using LCSC.Models.Pulse;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.Core.Services
{
    internal class LadderService
    {
        private int _seasonId = 0;
        private List<LadderTierModel> _tiers = [];
        private readonly PulseHttpService _pulseHttpService;
        private readonly CacheService _cacheService;
        private readonly BattleNetHttpService _battleNetHttpService;

        private const string SeasonCacheFileName = "Seasons.json";
        private const string TiersCacheFileName = "Tiers.json";

        public LadderService(
            PulseHttpService pulseHttpService,
            BattleNetHttpService battleNetHttpService,
            CacheService cacheService)
        {
            _pulseHttpService = pulseHttpService;
            _cacheService = cacheService;
            _battleNetHttpService = battleNetHttpService;
        }

        public async Task TryLadder()
        {
            var results = await GetLadderTiersAsync();
            if (results != null)
            {
            }
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
                        && (cachedData.LastUpdated + TimeSpan.FromDays(2)) > DateTime.Now
                        && cachedData.Tiers.Count > 0)
                    {
                        _tiers = new List<LadderTierModel>(cachedData.Tiers);
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
                    await _cacheService.CacheFileAsync(TiersCacheFileName, text);
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

        private async Task<int> GetSeasonIdAsync()
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
                    if (season != null && (season.LastUpdated + TimeSpan.FromDays(30)) > DateTime.Now)
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
                    await _cacheService.CacheFileAsync(SeasonCacheFileName, data);
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

        private record class TierCacheData(
            int SeasonId, 
            List<LadderTierModel> Tiers,
            DateTime LastUpdated);
    }
}
