using AirtableApiClient;
using LCSC.Http.Extensions;
using LCSC.Models.Airtable;

namespace LCSC.Http.Services;

public class AirtableHttpService(string? airtableToken, string? baseId)
{
    private const string DiscordBotSettingsTableName = "DiscordBotSettings";
    private const string LadderRegionsTableName = "LadderRegions";
    private const string MembersTableName = "Members";
    private const string ProfilesTableName = "BattleNetProfiles";
    private const string TournamentTableName = "Tournaments";
    private readonly string? _airtableToken = airtableToken;
    private readonly string? _baseId = baseId;

    public async Task<string?> CreateBattleNetProfile(BattleNetProfileRecord record)
    {
        using var airtableBase = new AirtableBase(_airtableToken, _baseId);

        var result = await airtableBase.CreateRecord(ProfilesTableName, record.GetFields());
        return result.Success ? result.Record.Id : null;
    }

    public async Task<IEnumerable<BattleNetProfileRecord>?> GetBattleNetProfilesAsync()
    {
        var records = await GetRecordsAsync(ProfilesTableName);
        if (records == null)
        {
            return null;
        }
        return records.Select(r => r.ToBattleNetProfileRecord());
    }

    public async Task<IEnumerable<DiscordBotSettingsRecord>?> GetDiscordBotSettingsAsync()
    {
        var records = await GetRecordsAsync(DiscordBotSettingsTableName);
        if (records == null)
        {
            return null;
        }
        return records.Select(r => r.ToDiscordBotSettings());
    }

    public async Task<IEnumerable<LadderRegionRecord>?> GetLadderRegionsAsync()
    {
        var records = await GetRecordsAsync(LadderRegionsTableName);
        if (records == null)
        {
            return null;
        }
        return records.Select(r => r.ToLadderRegionRecord());
    }

    public async Task<IEnumerable<MemberRecord>?> GetMemberRecordsAsync()
    {
        var records = await GetRecordsAsync(MembersTableName);
        if (records == null)
        {
            return null;
        }
        return records.Select(r => r.ToMemberRecord());
    }

    public async Task<BattleNetProfileRecord?> GetSingleBattleNetProfileAsync(string profileId)
    {
        var record = await GetSingleRecordAsync(ProfilesTableName, profileId);
        if (record == null)
        {
            return null;
        }
        return record.ToBattleNetProfileRecord();
    }

    public async Task<MemberRecord?> GetSingleMemberAsync(string id)
    {
        var record = await GetSingleRecordAsync(MembersTableName, id);
        if (record == null)
        {
            return null;
        }
        return record.ToMemberRecord();
    }

    public async Task<IEnumerable<TournamentRecord>?> GetTournamentRecordsAsync()
    {
        var records = await GetRecordsAsync(TournamentTableName);
        if (records == null)
        {
            return null;
        }
        return records.Select(r => r.ToTournamentRecord());
    }

    public async Task<string?> UpdateOrCreateRegionAsync(
                    string id,
        int seasonId,
        string? region,
        string race,
        int currentMMR,
        int previousMMR,
        int league,
        int tier,
        int wins,
        int totalMatches,
        string bnetProfileId)
    {
        using var airtableBase = new AirtableBase(_airtableToken, _baseId);
        Fields fields = new Fields();
        fields.AddField(nameof(LadderRegionRecord.SeasonId), seasonId);
        fields.AddField(nameof(LadderRegionRecord.Region), region);
        fields.AddField(nameof(LadderRegionRecord.Race), race);
        fields.AddField(nameof(LadderRegionRecord.CurrentMMR), currentMMR);
        fields.AddField(nameof(LadderRegionRecord.PreviousMMR), previousMMR);
        fields.AddField(nameof(LadderRegionRecord.League), league);
        fields.AddField(nameof(LadderRegionRecord.Tier), tier);
        fields.AddField(nameof(LadderRegionRecord.Wins), wins);
        fields.AddField(nameof(LadderRegionRecord.TotalMatches), totalMatches);
        fields.AddField(nameof(LadderRegionRecord.BattleNetProfiles), new string[] { bnetProfileId });
        if (string.IsNullOrEmpty(id))
        {
            var result = await airtableBase.CreateRecord(LadderRegionsTableName, fields);
            return result.Success ? result.Record.Id : null;
        }
        var result2 = await airtableBase.UpdateRecord(LadderRegionsTableName, fields, id);
        return result2.Success ? result2.Record.Id : null;
    }

    public async Task<bool> UpdateTournamentMatchesData(string tournamentId, string matchesData)
    {
        using var airtableBase = new AirtableBase(_airtableToken, _baseId);
        var fields = new Fields();
        fields.AddField(nameof(TournamentRecord.MatchesData), matchesData);
        var response = await airtableBase.UpdateRecord(TournamentTableName, fields, tournamentId);
        return response.Success;
    }

    private async Task<IEnumerable<AirtableRecord>> GetRecordsAsync(string table)
    {
        using var airtableBase = new AirtableBase(_airtableToken, _baseId);
        string? offset = null;
        string? errorMessage = null;
        var records = new List<AirtableRecord>();
        do
        {
            var response = await airtableBase.ListRecords(table);
            if (response.Success)
            {
                records.AddRange(response.Records);
            }
            else if (response.AirtableApiError is not null)
            {
                errorMessage = response.AirtableApiError.ErrorMessage;
                if (response.AirtableApiError is AirtableInvalidRequestException)
                {
                    errorMessage += "\nDetailed error message: ";
                    errorMessage += response.AirtableApiError.DetailedErrorMessage;
                }
                break;
            }
            else
            {
                errorMessage = "Unknown error";
                break;
            }
        } while (offset != null);
        return records;
    }

    private async Task<AirtableRecord?> GetSingleRecordAsync(string table, string id)
    {
        using var airtableBase = new AirtableBase(_airtableToken, _baseId);
        string? errorMessage = null;

        var response = await airtableBase.RetrieveRecord(table, id);
        if (response.Success)
        {
            return response.Record;
        }
        else if (response.AirtableApiError is not null)
        {
            errorMessage = response.AirtableApiError.ErrorMessage;
            if (response.AirtableApiError is AirtableInvalidRequestException)
            {
                errorMessage += "\nDetailed error message: ";
                errorMessage += response.AirtableApiError.DetailedErrorMessage;
            }
        }
        else
        {
            errorMessage = "Unknown error";
        }
        return null;
    }
}