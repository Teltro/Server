using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server_TestProject.Models
{
    public class PlayerMatchStats
    {
        public string Name { get; set; }
        public int Frags { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }

        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public Match Match { get; set; }
        [JsonIgnore]
        public int MatchId { get; set; }
    }
}
