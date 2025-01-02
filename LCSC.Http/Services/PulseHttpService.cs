using LCSC.Models.Pulse;
using Newtonsoft.Json;

namespace LCSC.Http.Services;

public class PulseHttpService
{
    private const string CharacterSearchUrl = "https://sc2pulse.nephest.com/sc2/api/character/search?term={0}";
    private const string CharacterCommonUrl = "https://sc2pulse.nephest.com/sc2/api/character/{0}/common?matchType=_1V1&mmrHistoryDepth=0"; //"https://www.nephest.com/sc2/api/character/{0}/common/_1V1";
    private const string SeasonList = "https://sc2pulse.nephest.com/sc2/api/season/list";

    public Task<List<CharacterSearchResult>?> SearchCharacterAsync(string searchTerm)
        => GetAsync<List<CharacterSearchResult>>(string.Format(CharacterSearchUrl, searchTerm));

    public Task<CharacterCommon?> GetCharacterCommonDataAsync(string characterId)
        => GetAsync<CharacterCommon?>(string.Format(CharacterCommonUrl, characterId));

    public Task<List<Season>?> GetSeasonsDataAsync()
        => GetAsync<List<Season>?>(SeasonList);

    private async Task<T?> GetAsync<T>(string url)
    {
        using var httpClient = new HttpClient();
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
        catch (Exception)
        {
        }
        return default;
    }
}