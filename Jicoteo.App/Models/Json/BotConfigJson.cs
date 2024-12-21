using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.Bot.Models.Json
{
    public struct BotConfigJson
    {
        [JsonProperty("guildId")]
        public string GuildId { get; private set; }

        [JsonProperty("token")]
        public string Token { get; private set; }
    }
}