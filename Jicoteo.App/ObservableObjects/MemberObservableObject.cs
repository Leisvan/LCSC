using CommunityToolkit.Mvvm.ComponentModel;
using LSCC.App.Models;
using LSCC.Http.Models;
using System;

namespace LSCC.App.ObservableObjects
{
    public partial class MemberObservableObject : ObservableObject
    {
        private readonly MemberRecord _record;

        public MemberObservableObject(MemberRecord record)
        {
            _record = record;
        }

        public bool IsBanned => _record.Banned;

        public string? Nick => _record.Nick;

        public string? PictureUrl => _record.PictureUrl;

        public Race Race
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_record.KnownRace))
                {
                    return Race.Unknown;
                }
                return Enum.Parse<Race>(_record.KnownRace, true);
            }
        }

        public string? RealName => _record.RealName;
    }
}