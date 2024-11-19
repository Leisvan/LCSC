using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Jicoteo.Services
{
    public class MessageListener
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
}