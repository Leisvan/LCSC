using AirtableApiClient;
using LCSC.Http.Models;

namespace LCSC.Http.Extensions;

public static class AirtableExtensions
{
    public static BattleNetProfileRecord ToBattleNetProfileRecord(this AirtableRecord record)
        => new(record.Id,
            Number: record.GetField<int>(nameof(BattleNetProfileRecord.Number)),
            BattleTag: record.GetField<string>(nameof(BattleNetProfileRecord.BattleTag)),
            PulseId: record.GetField<string>(nameof(BattleNetProfileRecord.PulseId)),
            ProfileRealm: record.GetField<string>(nameof(BattleNetProfileRecord.ProfileRealm)),
            ProfileId: record.GetField<string>(nameof(BattleNetProfileRecord.ProfileId)),
            AccountId: record.GetField<string>(nameof(BattleNetProfileRecord.AccountId)),
            MainProfile: record.GetField<bool>(nameof(BattleNetProfileRecord.MainProfile)),
            Path: record.GetField<string>(nameof(BattleNetProfileRecord.Path)),
            Notes: record.GetField<string>(nameof(BattleNetProfileRecord.Notes)));

    public static MemberRecord ToMemberRecord(this AirtableRecord record)
        => new(
            record.Id,
            Number: record.GetField<int>(nameof(MemberRecord.Number)),
            Nick: record.GetField<string>(nameof(MemberRecord.Nick)),
            RealName: record.GetField<string>(nameof(MemberRecord.RealName)),
            KnownRace: record.GetField<string>(nameof(MemberRecord.KnownRace)),
            CountryTag: record.GetField<string>(nameof(MemberRecord.CountryTag)),
            DiscordId: record.GetField<string>(nameof(MemberRecord.DiscordId)),
            Verified: record.GetField<bool>(nameof(MemberRecord.Verified)),
            Banned: record.GetField<bool>(nameof(MemberRecord.Banned)),
            PictureUrl: record.GetField<string>(nameof(MemberRecord.PictureUrl)),
            ContactEmail: record.GetField<string>(nameof(MemberRecord.ContactEmail)),
            ChallongeEmail: record.GetField<string>(nameof(MemberRecord.ChallongeEmail)),
            record.GetField<string>(nameof(MemberRecord.Phone)),
            Tournaments: record.GetField<string[], string>(nameof(MemberRecord.Tournaments)),
            BattleNetProfiles: record.GetField<string[], string>(nameof(MemberRecord.BattleNetProfiles)));

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