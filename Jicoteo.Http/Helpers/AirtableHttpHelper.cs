using AirtableApiClient;
using LCSC.Http.Extensions;
using LCSC.Http.Models;
using LCSC.Http.Models.Json;
using System.Text.Json;

namespace LCSC.Http.Helpers
{
    public static class AirtableHttpHelper
    {
        private const string AirtableToken = "patSlwRfzMHUuwyiv.e32d45a3daf6b5a0e04de06ae55f3f9d10ba899dddc6bdb5935190657e0ac897";
        private const string LCSCBaseId = "appW9fDctxfHb1WMI";
        private const string MembersTableName = "Members";
        private const string TournamentTableName = "Tournaments";

        public static async Task<IEnumerable<MemberRecord>?> GetMemberRecordsAsync()
        {
            var records = await GetRecordsAsync(MembersTableName);
            if (records == null)
            {
                return null;
            }
            return records.Select(r => r.ToMemberRecord());
        }

        public static async Task<MemberRecord?> GetSingleMemberAsync(string id)
        {
            var record = await GetSingleRecordAsync(MembersTableName, id);
            if (record == null)
            {
                return null;
            }
            return record.ToMemberRecord();
        }

        public static async Task<IEnumerable<TournamentRecord>?> GetTournamentRecordsAsync()
        {
            var records = await GetRecordsAsync(TournamentTableName);
            if (records == null)
            {
                return null;
            }
            return records.Select(r => r.ToTournamentRecord());
        }

        public static async Task<bool> UpdateTournamentMatchesData(string tournamentId, string matchesData)
        {
            using var airtableBase = new AirtableBase(AirtableToken, LCSCBaseId);
            var fields = new Fields();
            fields.AddField(nameof(TournamentRecord.MatchesData), matchesData);
            var response = await airtableBase.UpdateRecord(TournamentTableName, fields, tournamentId);
            return response.Success;
        }

        private static async Task<IEnumerable<AirtableRecord>> GetRecordsAsync(string table)
        {
            using var airtableBase = new AirtableBase(AirtableToken, LCSCBaseId);
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

        private static async Task<AirtableRecord?> GetSingleRecordAsync(string table, string id)
        {
            using var airtableBase = new AirtableBase(AirtableToken, LCSCBaseId);
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
}