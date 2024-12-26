using AirtableApiClient;
using LCSC.Http.Models;

namespace LCSC.Http.Extensions;

public static class AirtableExtensions
{
    public static MemberRecord ToMemberRecord(this AirtableRecord record)
    => new(
        record.Id,
        Number: record.GetField<int>(nameof(MemberRecord.Number)),
        Nick: record.GetField<string>(nameof(MemberRecord.Nick)),
        RealName: record.GetField<string>(nameof(MemberRecord.RealName)),
        KnownRace: record.GetField<string>(nameof(MemberRecord.KnownRace)),
        CountryTag: record.GetField<string>(nameof(MemberRecord.CountryTag)),
        PulseId: record.GetField<string>(nameof(MemberRecord.PulseId)),
        DiscordId: record.GetField<string>(nameof(MemberRecord.DiscordId)),
        BattleTag: record.GetField<string>(nameof(MemberRecord.BattleTag)),
        BattleNetPath: record.GetField<string>(nameof(MemberRecord.BattleNetPath)),
        BattleNetRealm: record.GetField<string>(nameof(MemberRecord.BattleNetRealm)),
        BattleNetId: record.GetField<string>(nameof(MemberRecord.BattleNetId)),
        BlizzardAccountId: record.GetField<string>(nameof(MemberRecord.BattleNetId)),
        Verified: record.GetField<bool>(nameof(MemberRecord.Verified)),
        Banned: record.GetField<bool>(nameof(MemberRecord.Banned)),
        PictureUrl: record.GetField<string>(nameof(MemberRecord.PictureUrl)),
        ContactEmail: record.GetField<string>(nameof(MemberRecord.ContactEmail)),
        ChallongeEmail: record.GetField<string>(nameof(MemberRecord.ChallongeEmail)),
        record.GetField<string>(nameof(MemberRecord.Phone)),
        Tournaments: record.GetField<string[], string>(nameof(MemberRecord.Tournaments)));

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