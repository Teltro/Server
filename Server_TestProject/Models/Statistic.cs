using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server_TestProject.Models
{
    public class Statistic
    {
        public int TotalMatchesPlayed { get; set; }
        public int MaximumMatchesPerDay { get; set; }
        public float AverageMatchesPerDay { get; set; }
        public int MaximumPopulation { get; set; }
        public float AveragePopulation { get; set; }
        public string[] Top5GameMods { get; set; }
        public string[] Top5Maps { get; set; }

        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public Server Server { get; set; }
        [JsonIgnore]
        public int ServerId { get; set; }
    }
}
