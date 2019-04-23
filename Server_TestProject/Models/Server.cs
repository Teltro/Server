using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Server_TestProject.Models;

namespace Server_TestProject.Models 
{
    public class Server
    {
        public Server()
        {
            Matches = new List<Match>();
        }
        public string Endpoint { get; set; }
        public Info Info { get; set; }

        [JsonIgnore]
        public List<Match> Matches { get; set; }

        [JsonIgnore]
        public List<ServerPlayerStats> PlayerStats { get; set; }

        [JsonIgnore]
        public int Id { get; set; }

    }
}
