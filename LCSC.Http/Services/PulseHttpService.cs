using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LCSC.Common.Models.Pulse;

namespace LCSC.Http.Services;

public class PulseHttpService
{
    private const string CharacterSearchUrl = "https://sc2pulse.nephest.com/sc2/api/character/search?term={0}";

    public async Task<List<CharacterSearchResult>?> CharacterSearchAsync(string searchTerm)
    {
        return await GetAsync<List<CharacterSearchResult>>(string.Format(CharacterSearchUrl, searchTerm));
    }

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
        catch (Exception e)
        {
        }
        return default;
    }
}