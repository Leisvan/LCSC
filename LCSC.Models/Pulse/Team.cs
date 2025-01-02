using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.Models.Pulse
{
    public record class Team
    {
        public int Rating { get; set; }
        public int Season { get; set; }
        public int QueueType { get; set; }
        public string? Region { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
        public DateTime? LastPlayed { get; set; }
        public int TierType { get; set; }
        public int LeagueType { get; set; }

        public List<Member>? Members { get; set; }
    }
}
