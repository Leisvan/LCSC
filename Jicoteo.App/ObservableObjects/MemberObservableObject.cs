using CommunityToolkit.Mvvm.ComponentModel;
using LCSC.App.Models;
using LCSC.Common.Models;
using LCSC.Common.Models.Airtable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LCSC.App.ObservableObjects
{
    public partial class MemberObservableObject(MemberRecord record, List<BattleNetProfileRecord> profiles)
        : ObservableObject, IComparable<MemberObservableObject>
    {
        public bool IsBanned => Record.Banned;

        public bool IsVerified => Record.Verified;

        public string? Nick => Record.Nick;

        public string? PictureUrl => Record.PictureUrl;

        public ObservableCollection<BattleNetProfileRecord> Profiles { get; }
            = new ObservableCollection<BattleNetProfileRecord>(profiles);

        public Race Race
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Record.KnownRace))
                {
                    return Race.Unknown;
                }
                return Enum.Parse<Race>(Record.KnownRace, true);
            }
        }

        public string? RealName => Record.RealName;

        public MemberRecord Record { get; } = record;

        public int CompareTo(MemberObservableObject? other)
        {
            if (other == null)
            {
                return 1;
            }
            return Nick?.CompareTo(other.Nick) ?? -1;
        }
    }
}