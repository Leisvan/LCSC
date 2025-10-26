using DSharpPlus.Entities;

namespace LCSC.Discord.Helpers;

internal static class InteractionsHelper
{
    public const string CancelRegionUpdateButtonId = "button_cancelupdaterank";

    public static DiscordButtonComponent GetCancelUpdateRankButton(bool disabled = false)
        => new(DiscordButtonStyle.Primary, CancelRegionUpdateButtonId, "Cancelar", disabled);
}