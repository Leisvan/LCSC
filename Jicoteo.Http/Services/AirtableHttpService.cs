using AirtableApiClient;
using LCSC.Http.Extensions;
using LCSC.Http.Models;

namespace LCSC.Http.Services;

public class AirtableHttpService(string? airtableToken, string? baseId)
{
    private const string MembersTableName = "Members";
    private const string ProfilesTableName = "BattleNetProfiles";
    private const string TournamentTableName = "Tournaments";
    private readonly string? _airtableToken = airtableToken;
    private readonly string? _baseId = baseId;

    public async Task<IEnumerable<MemberRecord>?> GetMemberRecordsAsync()
    {
        var records = await GetRecordsAsync(MembersTableName);
        if (records == null)
        {
            return null;
        }
        return records.Select(r => r.ToMemberRecord());
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

    public async Task<BattleNetProfileRecord?> GetSingleProfileAsync(string profileId)
    {
        var record = await GetSingleRecordAsync(ProfilesTableName, profileId);
        if (record == null)
        {
            return null;
        }
        return record.ToBattleNetProfileRecord();
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
        else if (response.AirtableApiError is AirtableApiException)
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