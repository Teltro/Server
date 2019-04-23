using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Server;
using Server.Attributes;
using Server_TestProject.Models;

namespace Server_TestProject.Controllers
{
    public class StatisticController : Controller
    {
        ServerContext db;

        public StatisticController(ServerContext context)
        {
            db = context;
        }


        [Http("GET")]
        public PlayerStats PlayerStats(string Name)
        {
            return db.PlayersStats.Include(ps => ps.GameModsPlayed).FirstOrDefault(ps => ps.Name == Name);
        }

        [Http("GET")]
        public Match[] RecentMatches(int count = 5)
        {
            Match[] matches = new Match[0];
            try
            {
                if (count > 50)
                    count = 50;
                if (count < 0)
                    return new Match[0];

                matches = db.Matches
                    .Include(m => m.MapDb)
                    .Include(m => m.GameModeDb)
                    .Include(m => m.ScoreBoard)
                    .OrderBy(m => m.TimeStamp)
                    .Take(count)
                    .ToArray();
            }
            catch(Exception exc)
            {
                Console.WriteLine($"{exc.Source}\n{exc.Message}\n");
            }
            return matches;
        }


        [Http("GET")]
        public PlayerStats[] BestPlayers(int count = 5)
        {
            if (count > 50)
                count = 50;
            if (count < 0)
                return new PlayerStats[0];

            return db.PlayersStats
                .OrderByDescending(p => p.KillToDeathRatio)
                .Where(ps => ps.Deaths != 0 /*&& ps.TotalMatchesPlayed >= 10*/)
                .ToArray();
        }


        //edit
        [Http("GET")]
        public Models.Server[] PopularServers(int count = 5)
        {
            if (count > 50)
                count = 50;
            if (count < 0)
                return new Models.Server[0];

            var gameMode = db.GameMods.ToArray();
            return db.Servers.Include(s => s.Info).ThenInclude(inf => inf.InfoGameMods).OrderByDescending(s => s.Matches.Count).Take(count).ToArray();
        }
    }
}
