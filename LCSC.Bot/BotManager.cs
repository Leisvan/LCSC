using DSharpPlus;
using DSharpPlus.EventArgs;

namespace LCSC.Discord;

internal class BotManager
{
    private readonly ulong _appUserId = 1325549763856306206;

    public BotManager(string token, ulong userId)
    {
        ConfigureBot(token);
    }

    public DiscordClient? Client { get; private set; }

    public async Task ConnectAsync()
    {
        if (Client == null)
        {
            return;
        }
        await Client.ConnectAsync();
        await Task.Delay(-1);
    }

    private void ConfigureBot(string token)
    {
        LogNotifier.Notify("Configuring bot");
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

    private Task OnClientMessageCreated(DiscordClient sender, MessageCreatedEventArgs args)
    {
        if (args.Author.Id != _appUserId)
        {
            //_messageListener?.HandleMessageAsync(sender, args);
        }
        return Task.CompletedTask;
    }
}