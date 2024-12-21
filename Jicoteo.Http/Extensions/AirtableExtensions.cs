using AirtableApiClient;
using LCSC.Http.Models;

namespace LCSC.Http.Extensions;

public static class AirtableExtensions
{
    public static MemberRecord ToMemberRecord(this AirtableRecord record)
    => new(
        record.Id,
        record.GetField<string>(nameof(MemberRecord.Nick)),
        record.GetField<string>(nameof(MemberRecord.RealName)),
        record.GetField<string>(nameof(MemberRecord.KnownRace)),
        record.GetField<string>(nameof(MemberRecord.PulseId)),
        record.GetField<string>(nameof(MemberRecord.CountryTag)),
        record.GetField<string>(nameof(MemberRecord.DiscordId)),
        record.GetField<string>(nameof(MemberRecord.BattleTag)),
        record.GetField<bool>(nameof(MemberRecord.Banned)),
        record.GetField<string>(nameof(MemberRecord.PictureUrl)),
        record.GetField<string>(nameof(MemberRecord.ContactEmail)),
        record.GetField<string>(nameof(MemberRecord.ChallongeEmail)),
        record.GetField<string>(nameof(MemberRecord.Phone)),
        record.GetField<string[], string>(nameof(MemberRecord.Tournaments)));
}