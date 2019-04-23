using System;
using System.Collections.Generic;
using System.Text;

namespace Server_TestProject.Models
{
    public class InfoGameMode
    {
        public int InfoId { get; set; }
        public Info Info { get; set; }

        public int GameModeId { get; set; }
        public GameMode GameMode { get; set; }
    }
}
