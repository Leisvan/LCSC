using Jicoteo.Bot.Models.Json;
using Jicoteo.Manager.Bot;
using Jicoteo.Manager.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

const string BotConfigFileName = "dconfig.json";
const string CONFIGDirectoryName = "Bot\\Config";

var dependencies = new DependencyServiceHelper();
var bot = dependencies.ServiceProvider.GetService<BotManager>();

#region Read Bot config

string json = FileHelper.ReadTextFileFromDirectory(CONFIGDirectoryName, BotConfigFileName);
var configJson = JsonConvert.DeserializeObject<BotConfigJson>(json);
ulong.TryParse(configJson.GuildId, out ulong gid);

#endregion Read Bot config

if (bot != null)
{
    bot.ConfigureBot(dependencies.ServiceProvider, configJson.Token);
    bot.ConnectAsync()
        .GetAwaiter()
        .GetResult();
}