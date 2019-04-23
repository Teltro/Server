using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Server_TestProject.Models
{
    public class PlayerStats
    {
        public PlayerStats()
        {
            ServersPlayed = new List<ServerPlayerStats>();
            GameModsPlayed = new List<GameModePlayedCount>();
        }
        public string Name { get; set; }

        public int TotalMatchesPlayed { get; set; }

        [JsonIgnore]
        public int TotalDaysPlayed { get; set; }

        public int TotalMatchesWon { get; set; }

        [NotMapped]
        public int UniqueServers
        {
            get { return ServersPlayed.Count; }
        }
        [JsonIgnore]
        public List<ServerPlayerStats> ServersPlayed { get; set; }//??

        [NotMapped]
        public string FavoriteGameMode
        {
            get
            {
                string gameMode = "";
                int max = 0;
                foreach(var gmd in GameModsPlayed)
                {
                    if (gmd.PlayedCount > max)
                    {
                        gameMode = gmd.GameModeName;
                    }
                }
                return gameMode;
            }
        }

        [JsonIgnore]
        public List<GameModePlayedCount> GameModsPlayed { get; set; }

        public float AverageScoreBoardPercent { get; set; }

        public int MaximumMatchesPerDay { get; set; }

        [JsonIgnore]
        public int MatchesPerThisDay { get; set; }

        [NotMapped]
        public float AverageMatchesPerDay
        {
            get { return TotalMatchesPlayed / TotalDaysPlayed; }
        }
        public DateTime LastMatchPlayed { get; set; }

        [NotMapped]
        public float KillToDeathRatio
        {
            get
            {
                if (Deaths == 0)
                    return -1;
                return (float)Kills / (float)Deaths;
            }
        }
        [JsonIgnore]
        public int Kills { get; set; }
        [JsonIgnore]
        public int Deaths { get; set; }

        [JsonIgnore]
        public int Id { get; set; }

       
    }
    
    public class GameModePlayedCount
    {
        public string GameModeName { get; set; }
        public int PlayedCount { get; set; }

        [JsonIgnore]
        public PlayerStats PlayerStats { get; set; }
        [JsonIgnore]
        public int PlayerStatsId { get; set; }

        [JsonIgnore]
        public int Id { get; set; }

        public GameModePlayedCount() { }
        public GameModePlayedCount(string gmn, int count)
        {
            GameModeName = gmn;
            PlayedCount = count;
        }

    }
}
