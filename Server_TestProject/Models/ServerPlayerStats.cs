using System;
using System.Collections.Generic;
using System.Text;

namespace Server_TestProject.Models
{
    public class ServerPlayerStats
    {
        public Models.Server Server { get; set; }
        public int ServerId { get; set; }

        public PlayerStats PlayerStats { get; set; }
        public int PlayerStatsId { get; set; }
    }
}
