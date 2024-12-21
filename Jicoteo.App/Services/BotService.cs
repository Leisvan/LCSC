using LCSC.App.Bot;
using LCSC.Bot.Models.Json;
using LCSC.Manager.Helpers;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LCSC.App.Services
{
    public class BotService
    {
        private const string BotConfigFileName = "dconfig.json";
        private const string CONFIGDirectoryName = "Bot\\Config";

        private BotManager _bot;

        public BotService()
        {
            _bot = new BotManager();
            InitializeBot();
        }

        public Task ConnectAsync()
            => _bot.ConnectAsync();

        private void InitializeBot()
        {
            Console.WriteLine("Configuring bot");
            string json = FileHelper.ReadTextFileFromDirectory(CONFIGDirectoryName, BotConfigFileName);
            var configJson = JsonConvert.DeserializeObject<BotConfigJson>(json);
            ulong.TryParse(configJson.GuildId, out ulong gid);
            _bot.ConfigureBot(configJson.Token);
        }
    }
}