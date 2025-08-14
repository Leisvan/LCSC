using AirtableApiClient;
using LCSC.Http.Extensions;
using LCSC.Models;
using LCSC.Models.Airtable;

namespace LCSC.Http.Services;

public class AirtableHttpService(string? airtableToken, string? baseId)
{
    private const string DiscordBotGuildSettingsTableName = "DiscordBotGuildSettings";
    private const string LadderRegionsTableName = "LadderRegions";
    private const string MembersTableName = "Members";
    private const string ProfilesTableName = "BattleNetProfiles";
    private const int RecordsChunkSize = 10;
    private const string TournamentTableName = "Tournaments";
    private readonly string? _airtableToken = airtableToken;
    private readonly string? _baseId = baseId;

    private bool IsConfigured
        => !string.IsNullOrEmpty(_airtableToken) && !string.IsNullOrEmpty(_baseId);

    public async Task<string?> CreateBattleNetProfile(BattleNetProfileRecord record)
    {
        if (!IsConfigured)
        {
            return default;
        }
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

    public async Task<IEnumerable<DiscordBotGuildSettingsRecord>?> GetDiscordBotGuildsSettingsAsync()
    {
        var records = await GetRecordsAsync(DiscordBotGuildSettingsTableName);
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

    public async Task<bool> UpdateOrCreateRegionsAsync(IEnumerable<LadderRegionModel> regions)
    {
        if (!IsConfigured)
        {
            return default;
        }
        List<Fields> fieldsToCreate = [];
        List<IdFields> fieldsToUpdate = [];
        foreach (var region in regions)
        {
            IdFields idFields = new(region.Id);
            idFields.AddField(nameof(LadderRegionRecord.SeasonId), region.SeasonId);
            idFields.AddField(nameof(LadderRegionRecord.Region), region.Region);
            idFields.AddField(nameof(LadderRegionRecord.Race), region.Race);
            idFields.AddField(nameof(LadderRegionRecord.CurrentMMR), region.CurrentMMR);
            idFields.AddField(nameof(LadderRegionRecord.PreviousMMR), region.PreviousMMR);
            idFields.AddField(nameof(LadderRegionRecord.League), region.League);
            idFields.AddField(nameof(LadderRegionRecord.Tier), region.Tier);
            idFields.AddField(nameof(LadderRegionRecord.Wins), region.Wins);
            idFields.AddField(nameof(LadderRegionRecord.TotalMatches), region.TotalMatches);
            idFields.AddField(nameof(LadderRegionRecord.BattleNetProfiles), new string[] { region.BattleNetProfileId });
            if (string.IsNullOrEmpty(region.Id))
            {
                fieldsToCreate.Add(idFields);
            }
            else
            {
                fieldsToUpdate.Add(idFields);
            }
        }
        using var airtableBase = new AirtableBase(_airtableToken, _baseId);
        AirtableCreateUpdateReplaceMultipleRecordsResponse? createResponse = null;
        AirtableCreateUpdateReplaceMultipleRecordsResponse? updateResponse = null;
        if (fieldsToCreate.Count > 0)
        {
            var chunks = fieldsToCreate.Chunk(RecordsChunkSize);
            foreach (var chunk in chunks)
            {
                try
                {
                    createResponse = await airtableBase.CreateMultipleRecords(LadderRegionsTableName, chunk.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        if (fieldsToUpdate.Count > 0)
        {
            var chunks = fieldsToUpdate.Chunk(RecordsChunkSize);
            foreach (var chunk in chunks)
            {
                try
                {
                    updateResponse = await airtableBase.UpdateMultipleRecords(LadderRegionsTableName, chunk.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        return (createResponse == null && updateResponse == null) || (createResponse?.Success == true || updateResponse?.Success == true);
    }

    public async Task<string?> UpdateOrCreateSingleRegionAsync(LadderRegionModel region)
    {
        if (!IsConfigured)
        {
            return default;
        }
        using var airtableBase = new AirtableBase(_airtableToken, _baseId);
        Fields fields = new();
        fields.AddField(nameof(LadderRegionRecord.SeasonId), region.SeasonId);
        fields.AddField(nameof(LadderRegionRecord.Region), region.Region);
        fields.AddField(nameof(LadderRegionRecord.Race), region.Race);
        fields.AddField(nameof(LadderRegionRecord.CurrentMMR), region.CurrentMMR);
        fields.AddField(nameof(LadderRegionRecord.PreviousMMR), region.PreviousMMR);
        fields.AddField(nameof(LadderRegionRecord.League), region.League);
        fields.AddField(nameof(LadderRegionRecord.Tier), region.Tier);
        fields.AddField(nameof(LadderRegionRecord.Wins), region.Wins);
        fields.AddField(nameof(LadderRegionRecord.TotalMatches), region.TotalMatches);
        fields.AddField(nameof(LadderRegionRecord.BattleNetProfiles), new string[] { region.BattleNetProfileId });
        if (string.IsNullOrEmpty(region.Id))
        {
            var result = await airtableBase.CreateRecord(LadderRegionsTableName, fields);
            return result.Success ? result.Record.Id : null;
        }
        var result2 = await airtableBase.UpdateRecord(LadderRegionsTableName, fields, region.Id);
        return result2.Success ? result2.Record.Id : null;
    }

    public async Task<bool> UpdateTournamentMatchesData(string tournamentId, string matchesData)
    {
        if (!IsConfigured)
        {
            return default;
        }
        using var airtableBase = new AirtableBase(_airtableToken, _baseId);
        var fields = new Fields();
        fields.AddField(nameof(TournamentRecord.MatchesData), matchesData);
        var response = await airtableBase.UpdateRecord(TournamentTableName, fields, tournamentId);
        return response.Success;
    }

    private async Task<IEnumerable<AirtableRecord>> GetRecordsAsync(string table)
    {
        if (!IsConfigured)
        {
            return [];
        }
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
        if (!IsConfigured)
        {
            return default;
        }
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