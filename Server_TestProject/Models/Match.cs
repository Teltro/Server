using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Server_TestProject.Models
{
    public class Match
    {
        public Match()
        {
            ScoreBoard = new List<PlayerMatchStats>();
        }

        [NotMapped]
        public string Map
        {
            get { return MapDb.Name; }
            set
            {
                try
                {
                    MapDb = new Map();
                    MapDb.Name = value;
                    MapDb.Matches.Add(this);
                }
                catch(Exception exc)
                {
                    Console.WriteLine($"{exc.Source}\n{exc.Message}\n");
                }
            }
        }

        [NotMapped]
        public string GameMode
        {
            get { return GameModeDb.Name; }
            set
            {
                try
                {
                    GameModeDb = new GameMode();
                    GameModeDb.Name = value;
                    GameModeDb.Matches.Add(this);
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{exc.Source}\n{exc.Message}\n");
                }
            }
        }

        public DateTime TimeStamp { get; set; }
        public int FragLimit { get; set; }
        public int TimeLimit { get; set; }
        public float TimeElapsed { get; set; }
        public List<PlayerMatchStats> ScoreBoard { get; set; }
        //public PlayerMatchStats[] pmss { get; set; }
            
        [JsonIgnore]
        public Map MapDb { get; set; }
        [JsonIgnore]
        public int MapDbId { get; set; }

        [JsonIgnore]
        public GameMode GameModeDb { get; set; }
        [JsonIgnore]
        public int GameModeDbId { get; set; }

        [JsonIgnore]
        public Server Server { get; set; }
        [JsonIgnore]
        public int ServerId { get; set; }

        [JsonIgnore]
        public int Id { get; set; }
    }
}
