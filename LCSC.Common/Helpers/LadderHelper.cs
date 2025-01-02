using LCSC.Models;
using LCSC.Models.Pulse;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.Core.Helpers
{
    public static class LadderHelper
    {
        public static Race GetRaceFromTeamResult(Team team)
        {
            var member = team.Members?.FirstOrDefault();
            if (member == null)
            {
                return Race.Unknown;
            }
            if (member.TerranGamesPlayed > 0)
            {
                return Race.Terran;
            }
            if (member.ZergGamesPlayed > 0)
            {
                return Race.Zerg;
            }
            if (member.ProtossGamesPlayed > 0)
            {
                return Race.Protoss;
            }
            if (member.RandomGamesPlayed > 0)
            {
                return Race.Random;
            }
            return Race.Unknown;
        }
    }
}
