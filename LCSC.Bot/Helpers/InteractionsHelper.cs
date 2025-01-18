using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.Discord.Helpers
{
    internal static class InteractionsHelper
    {
        public const string CancelRegionUpdateButtonId = "button_cancelupdaterank";

        public static DiscordButtonComponent GetCancelUpdateRankButton(bool disabled = false)
            => new(DiscordButtonStyle.Primary, CancelRegionUpdateButtonId, "Cancelar", disabled);
    }
}