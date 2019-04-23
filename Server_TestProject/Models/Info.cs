using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Server_TestProject.Models
{
    public class Info
    {
        public Info()
        {
            InfoGameMods = new List<InfoGameMode>();
        }
        public string Name { get; set; }
        [JsonIgnore]
        public List<InfoGameMode> InfoGameMods { get; set; }
        [NotMapped]
        public string[] GameMods
        {
            get
            {
                string[] gm = new string[InfoGameMods.Count];
                for (int i = 0; i < gm.Length; i++)
                    gm[i] = InfoGameMods[i].GameMode.Name;

                return gm;
            }
            set
            {
                foreach(var gm in value)
                {
                    InfoGameMode igm = new InfoGameMode
                    {
                        GameMode = new GameMode { Name = gm },
                        Info = this
                    };
                    InfoGameMods.Add(igm);
                }
                
            }
        }

    [JsonIgnore]
    public int Id { get; set; }
    [JsonIgnore]
    public Server Server { get; set; }
    [JsonIgnore]
    public int ServerId { get; set; }
}
}
