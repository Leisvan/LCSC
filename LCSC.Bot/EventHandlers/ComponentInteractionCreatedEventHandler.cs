using DSharpPlus;
using DSharpPlus.EventArgs;
using LCSC.Discord.Services;

namespace LCSC.Discord.EventHandlers
{
    public class ComponentInteractionCreatedEventHandler : IEventHandler<ComponentInteractionCreatedEventArgs>
    {
        private readonly DiscordBotService _service;

        public ComponentInteractionCreatedEventHandler(DiscordBotService service)
        {
            _service = service;
        }

        public Task HandleEventAsync(DiscordClient sender, ComponentInteractionCreatedEventArgs eventArgs)
            => _service.RespondToInteractionAsync(eventArgs);
    }
}