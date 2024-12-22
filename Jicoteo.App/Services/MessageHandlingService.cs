using DSharpPlus;
using DSharpPlus.EventArgs;

namespace LCSC.Manager.Services;

public class MessageHandlingService
{
    public async void HandleMessageAsync(DiscordClient sender, MessageCreatedEventArgs args)
    {
        if (args.Message.Content == "hola")
        {
            var emoji = await sender.GetApplicationEmojiAsync(1306029536323960882u);
            await args.Message.CreateReactionAsync(emoji);
        }
    }
}