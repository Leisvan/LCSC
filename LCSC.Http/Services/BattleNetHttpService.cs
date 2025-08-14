using LCSC.Models;
using LCSC.Models.BattleNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace LCSC.Http.Services;

public class BattleNetHttpService(string? clientId, string? clientSecret)
{
    private const string AccessTokenUrl = "https://oauth.battle.net/token";
    private const string LeagueData1v1NAUrl = @"https://us.api.blizzard.com/data/sc2/league/{0}/201/0/{1}";

    private readonly string? _clientId = clientId;
    private readonly string? _clientSecret = clientSecret;
    private BattleNetToken? _token;

    private bool IsConfigured
        => !string.IsNullOrEmpty(_clientId) && !string.IsNullOrEmpty(_clientSecret);

    public async Task<List<LadderTierModel>?> GetLadderTiersForLeague(int season, int leagueId)
    {
        if (!IsConfigured)
        {
            return default;
        }
        var list = new List<LadderTierModel>();
        try
        {
            if (!await UpdateAccessTokenAsync())
            {
                return null;
            }

            string url = string.Format(LeagueData1v1NAUrl, season, leagueId);
            var result = await GetAsync<LadderTierList>(url);
            if (result == null || result.Tier == null || result.Tier.Count == 0)
            {
                return null;
            }
            foreach (var item in result.Tier)
            {
                list.Add(new LadderTierModel(
                    (LadderLeague)(result.Key?.League_Id ?? 0),
                    (LadderTier)item.Id,
                    item.Min_Rating,
                    item.Max_Rating));
            }
            list.Reverse();
        }
        catch (Exception)
        {
        }
        return list;
    }

    private async Task<T?> GetAsync<T>(string url)
    {
        if (_token == null || _token.IsTokenExpired())
        {
            return default;
        }
        using var httpClient = new HttpClient();
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.Access_Token);
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

    private async Task<bool> UpdateAccessTokenAsync()
    {
        if (_token != null && !_token.IsTokenExpired())
        {
            return true;
        }
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(AccessTokenUrl)
        };
        var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}"));
        auth = string.Format("Basic {0}", auth);
        request.Headers.Add("Authorization", auth);
        HttpContent httpContent = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
        request.Content = httpContent;

        try
        {
            var reqResult = await client.SendAsync(request).ConfigureAwait(false);
            if (reqResult.IsSuccessStatusCode)
            {
                var contentStr = await reqResult.Content.ReadAsStringAsync();
                _token = JsonConvert.DeserializeObject<BattleNetToken>(contentStr);
                if (_token != null)
                {
                    _token.IssuedTime = DateTime.UtcNow;
                    return true;
                }
            }
        }
        catch (Exception)
        {
        }
        return false;
    }
}