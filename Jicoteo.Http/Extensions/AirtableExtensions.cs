using AirtableApiClient;
using LCSC.Http.Models.Airtable;

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
            MainProfile: record.GetField<bool>(nameof(BattleNetProfileRecord.MainProfile)),
            Notes: record.GetField<string>(nameof(BattleNetProfileRecord.Notes)),
            Members: record.GetField<string[], string>(nameof(BattleNetProfileRecord.Members)));

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

    public static Fields GetFields(this BattleNetProfileRecord record)
    {
        var fields = new Fields();
        fields.AddField(nameof(BattleNetProfileRecord.BattleTag), record.BattleTag);
        fields.AddField(nameof(BattleNetProfileRecord.PulseId), record.PulseId);
        fields.AddField(nameof(BattleNetProfileRecord.ProfileRealm), record.ProfileRealm);
        fields.AddField(nameof(BattleNetProfileRecord.ProfileId), record.ProfileId);
        fields.AddField(nameof(BattleNetProfileRecord.MainProfile), record.MainProfile);
        fields.AddField(nameof(BattleNetProfileRecord.Notes), record.Notes);
        fields.AddField(nameof(BattleNetProfileRecord.Members), record.Members);
        return fields;

    }
}