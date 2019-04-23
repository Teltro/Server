using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server_TestProject.Models
{
    public class Map
    {
        public Map()
        {
            Matches = new List<Match>();
        }
        public string Name { get; set; }

        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public List<Match> Matches { get; set; }
    }
}
