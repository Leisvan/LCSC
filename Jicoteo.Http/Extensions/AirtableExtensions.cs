using AirtableApiClient;
using LCSC.Http.Models;

namespace LCSC.Http.Extensions;

public static class AirtableExtensions
{
    public static MemberRecord ToMemberRecord(this AirtableRecord record)
    => new(
        record.Id,
        record.GetField<int>(nameof(MemberRecord.Number)),
        record.GetField<string>(nameof(MemberRecord.Nick)),
        record.GetField<string>(nameof(MemberRecord.RealName)),
        record.GetField<string>(nameof(MemberRecord.KnownRace)),
        record.GetField<string>(nameof(MemberRecord.PulseId)),
        record.GetField<string>(nameof(MemberRecord.CountryTag)),
        record.GetField<string>(nameof(MemberRecord.DiscordId)),
        record.GetField<string>(nameof(MemberRecord.BattleTag)),
        record.GetField<bool>(nameof(MemberRecord.Verified)),
        record.GetField<bool>(nameof(MemberRecord.Banned)),
        record.GetField<string>(nameof(MemberRecord.PictureUrl)),
        record.GetField<string>(nameof(MemberRecord.ContactEmail)),
        record.GetField<string>(nameof(MemberRecord.ChallongeEmail)),
        record.GetField<string>(nameof(MemberRecord.Phone)),
        record.GetField<string[], string>(nameof(MemberRecord.Tournaments)));

    public static TournamentRecord ToTournamentRecord(this AirtableRecord record)
        => new(
            record.Id,
            record.GetField<int>(nameof(TournamentRecord.Number)),
            record.GetField<string>(nameof(TournamentRecord.SeriesName)),
            record.GetField<string>(nameof(TournamentRecord.SeriesNumber)),
            record.GetField<DateTime>(nameof(TournamentRecord.Date)),
            record.GetField<string>(nameof(TournamentRecord.BracketsUrl)),
            record.GetField<string>(nameof(TournamentRecord.Description)),
            record.GetField<string>(nameof(TournamentRecord.DetailsUrl)),
            record.GetField<string>(nameof(TournamentRecord.LogoUrl)),
            record.GetField<string>(nameof(TournamentRecord.SpecialUrl)),
            record.GetField<string>(nameof(TournamentRecord.SpecialUrlDescription)),
            record.GetField<string[], string>(nameof(TournamentRecord.Place1)),
            record.GetField<string[], string>(nameof(TournamentRecord.Place2)),
            record.GetField<string[], string>(nameof(TournamentRecord.Place3)),
            record.GetField<string[], string>(nameof(TournamentRecord.Place4)),
            record.GetField<string[], string>(nameof(TournamentRecord.Participants)),
            record.GetField<string>(nameof(TournamentRecord.MatchesData)));
}