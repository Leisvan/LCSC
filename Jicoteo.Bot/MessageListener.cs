using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Jicoteo.Bot
{
    public class MessageListener
    {
        public async void HandleMessageAsync(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (args.Message.Content == "hola")
            {
                await args.Message.RespondAsync("Hola patrás");
            }
        }
    }
}