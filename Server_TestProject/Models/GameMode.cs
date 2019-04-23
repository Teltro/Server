using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server_TestProject.Models
{
    public class GameMode
    {
        public GameMode()
        {
            InfoGameMods = new List<InfoGameMode>();
            Matches = new List<Match>();
        }
        public string Name { get; set; }

        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public List<InfoGameMode> InfoGameMods { get; set; }
        [JsonIgnore]
        public List<Match> Matches { get; set; }
    }
}
