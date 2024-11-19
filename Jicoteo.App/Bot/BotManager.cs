using DSharpPlus;
using DSharpPlus.EventArgs;
using Jicoteo.Manager.Services;
using System;
using System.Threading.Tasks;

namespace Jicoteo.App.Bot;

public class BotManager
{
    private const ulong DiscordBotUserId = 1303150451893600276;
    private MessageListener? _messageListener;

    public BotManager(MessageListener listener)
    {
        _messageListener = listener;
    }

    public DiscordClient? Client { get; private set; }

    public void ConfigureBot(IServiceProvider services, string token)
    {
        if (services == null)
        {
            return;
        }

        var builder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents);
        builder.ConfigureEventHandlers
        (
            b => b
            .HandleMessageCreated(OnClientMessageCreated)
            .HandleGuildMemberAdded((s, e) =>
            {
                // non-asynchronous code here
                return Task.CompletedTask;
            })
        );

        Client = builder.Build();
    }

    public async Task ConnectAsync()
    {
        if (Client == null)
        {
            return;
        }
        await Client.ConnectAsync();
        await Task.Delay(-1);
    }


    private Task OnClientMessageCreated(DiscordClient sender, MessageCreatedEventArgs args)
    {
        if (args.Author.Id != DiscordBotUserId)
        {
            _messageListener?.HandleMessageAsync(sender, args);
        }
        return Task.CompletedTask;
    }
}