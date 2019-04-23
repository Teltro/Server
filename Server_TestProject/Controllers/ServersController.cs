using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

using Server;
using Server.Attributes;
using Server_TestProject.Models;
using Microsoft.EntityFrameworkCore;

namespace Server_TestProject.Controllers
{

    public class ServersController : Controller
    {
        ServerContext db;

        public ServersController(ServerContext context)
        {
            db = context;
        }


        [Http("GET")]
        public Models.Server[] Info()
        {
            return db.Servers
                .Include(s => s.Info)
                .ThenInclude(i => i.InfoGameMods)
                .ThenInclude(igm => igm.GameMode)
                .ToArray();
        }


        // ------------------Костыль---------------------- //
        [Http("GET")]
        public Models.Server Info(string endpoint)
        {
            return db.Servers
                .Where(s => s.Endpoint == endpoint)
                .Include(s => s.Info)
                .ThenInclude(info => info.InfoGameMods)
                .ThenInclude(igm => igm.GameMode)
                .FirstOrDefault();
        }


        [Http("PUT")]
        public void Info([FromBody]Info info, string endpoint)
        {
            try
            {
                for (int i = 0; i < info.InfoGameMods.Count; i++)
                {
                    GameMode PUT_GameMode = info.InfoGameMods[i].GameMode;
                    GameMode Db_GameMode = db.GameMods.FirstOrDefault(gm => gm.Name == PUT_GameMode.Name);
                    if (Db_GameMode != null)
                        info.InfoGameMods[i].GameMode = Db_GameMode;
                }
                var Db_Server = db.Servers.Include(s => s.Info).FirstOrDefault(s => s.Endpoint == endpoint);
                if (Db_Server != null)
                {
                    db.Info.Remove(Db_Server.Info);
                    Db_Server.Info = info;
                    info.Server = Db_Server;
                    db.Info.Add(info);
                    db.SaveChanges();
                    return;
                }
                Models.Server PUT_Server = new Models.Server
                {
                    Endpoint = endpoint,
                    Info = info,
                    Matches = new List<Match>()
                };
                db.Servers.Add(PUT_Server);
                db.SaveChanges();
            }
            catch (Exception exc)
            {
                Console.WriteLine($"{exc.Source}\n{exc.Message}\n");
            }
        }

        [Http("GET")]
        public Match Match(string endpoint, DateTime dateTime)
        {
            return db.Matches
                .Include(m => m.MapDb)
                .Include(m => m.GameModeDb)
                .Include(m => m.ScoreBoard)
                .Where(m => m.TimeStamp == dateTime)
                .FirstOrDefault(m => m.Server.Endpoint == endpoint);
        }

        [Http("GET")]
        public Match[] Matches(string endpoint)
        {
            return db.Matches
                .Include(m => m.MapDb)
                .Include(m => m.GameModeDb)
                .Include(m => m.ScoreBoard)
                .Where(m => m.Server.Endpoint == endpoint)
                .ToArray();
        }

        [Http("PUT")]
        public void Match([FromBody]Match match, string endpoint)
        {
            try
            {
                var server = db.Servers
                    .Include(s => s.PlayerStats)
                    .FirstOrDefault(serv => serv.Endpoint == endpoint);
                var gameMode = db.InfoGameMods
                     .Include(igm => igm.GameMode)
                     .FirstOrDefault(igm => igm.Info.Server.Endpoint == endpoint
                                        && igm.GameMode.Name == match.GameModeDb.Name)
                     .GameMode;   
                if (gameMode != null && server != null)
                {
                    gameMode.Matches.Add(match);
                    match.GameModeDb = gameMode;
                    match.Server = server;
                    match.ScoreBoard = match.ScoreBoard.OrderByDescending(pms => pms.Kills).ToList();
                    db.Matches.Add(match);
                    PlayerStatsUpdate(match, server);
                    db.SaveChanges();
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine($"{exc.Source}\n{exc.Message}\n");
            }
        }

        //"Map": "pool_day",
        //"GameMode": "DM",
        //"TimeStamp": "2019-04-08T11:04:05.5657142",
        //"FragLimit": 20,
        //"TimeLimit": 5,
        //"TimeElapsed": 4,
        //"ScoreBoard": [
        //    {
        //        "Name": "nonche",
        //        "Frags": 20,
        //        "Kill": 20,
        //        "Deaths": 0
        //    },
        //    {
        //        "Name": "teltro",
        //        "Frags": 0,
        //        "Kill": 0,
        //        "Deaths": 20
        //    }
        //]

        [Http("GET")]
        public Statistic Stats(string endpoint)
        {
            Statistic stats = new Statistic();
            var server = db.Servers.Include(s => s.Info).ThenInclude(inf => inf.InfoGameMods).FirstOrDefault(s => s.Endpoint == endpoint);
            var allMatches = db.Matches.Where(m => m.Server.Endpoint == endpoint);
            var lifeTime = allMatches.Max(m => m.TimeStamp.Date) - allMatches.Min(m => m.TimeStamp.Date);

            stats.AveragePopulation = (float)db.ServerPlayerStats.Where(sps => sps.Server.Endpoint == endpoint).Count() / (float)lifeTime.Days;
            stats.AverageMatchesPerDay = (float)allMatches.Count() / (float)lifeTime.Days;

            var matches = db.Matches
                .Include(m => m.ScoreBoard)  //  ??
                .Include(m => m.MapDb)
                .Where(m => m.Server.Endpoint == endpoint); 
            stats.MaximumMatchesPerDay = MaximunMatchesPerDay(matches);
            stats.MaximumPopulation = MaximumPopulation(matches);
            stats.TotalMatchesPlayed = matches.Count();

            var infoGameMods = server.Info.InfoGameMods;
            var gameMode = db.GameMods.ToArray();
            //var gameMods = db.GameMods
            //   .Where(gm => gm.InfoGameMods
            //       .Any(igm => igm.Info.Server.Endpoint == endpoint))
            //   .ToArray();  //  ??
            stats.Top5GameMods = Top5GameMods(matches, infoGameMods);
            stats.Top5Maps = Top5Maps(matches);
            stats.TotalMatchesPlayed = matches.Count();

            return stats;
        }

        private int MaximunMatchesPerDay(IEnumerable<Match> matches)
        {
            matches = matches.OrderBy(m => m.TimeStamp.Date);
            int matchCount = 0;
            int matchMaxCount = 0;
            DateTime date = matches.FirstOrDefault().TimeStamp.Date;
            foreach (var match in matches)
            {
                if (match.TimeStamp.Date == date)
                {
                    matchCount++;
                    if (matchMaxCount < matchCount)
                        matchMaxCount = matchCount;
                }
                else
                {
                    matchCount = 0;
                    date = match.TimeStamp.Date;
                }
            }
            return matchMaxCount;
        }

        private int MaximumPopulation(IEnumerable<Match> matches)
        {
            matches = matches.OrderBy(m => m.TimeStamp);
            int playersCount = 0;
            int playersMaxCount = 0;
            DateTime date = matches.FirstOrDefault().TimeStamp.Date;
            foreach (var match in matches)
            {
                if (match.TimeStamp.Date == date)
                {
                    playersCount += match.ScoreBoard.Count();
                    if (playersCount > playersMaxCount)
                        playersMaxCount = playersCount;
                }
                else
                {
                    date = match.TimeStamp.Date;
                    //if (playersCount > playersMaxCount)
                    //    playersMaxCount = playersCount;
                    playersCount = 0;
                }
            }
            return playersMaxCount;
        }

        private string[] Top5GameMods(IEnumerable<Match> matches, IEnumerable<InfoGameMode> infoGameMods)
        {
            //string[] top5GameMods = new string[matches.Count() > 5 ? 5 : matches.Count()];
            List<string> top5GameMods = new List<string>();
            int gameModePlayedCount;
            int gameModePlayedMaxCount;
            infoGameMods = infoGameMods.OrderBy(igm => igm.GameModeId);
            string topGameMode = infoGameMods.First().GameMode.Name;

            for (int i = 0; i < 5; i++)
            {
                gameModePlayedCount = 0;
                gameModePlayedMaxCount = 0;
                foreach (var match in matches)
                {
                    if (top5GameMods.Any(gm => gm == match.GameModeDb.Name))
                        continue;
                    if (match.GameModeDb.Name == topGameMode)
                    {
                        gameModePlayedCount++;
                        if (gameModePlayedCount > gameModePlayedMaxCount)
                            gameModePlayedMaxCount = gameModePlayedCount;
                    }
                    else
                    {
                        gameModePlayedCount = 1;
                        topGameMode = match.GameModeDb.Name;
                    }
                }
                if(!top5GameMods.Any(gm => gm == topGameMode))
                    top5GameMods.Add(topGameMode);
            }
            return top5GameMods.ToArray();
        }

        private string[] Top5Maps(IEnumerable<Match> matches) 
        {
            List<string> top5Maps = new List<string>();
            int mapPlayedCount;
            int mapPlayedMaxCount;
            matches = matches.OrderBy(m => m.MapDbId);
            string topMap = matches.First().MapDb.Name;

            for (int i = 0; i < 5; i++)
            {
                mapPlayedCount = 0;
                mapPlayedMaxCount = 0;
                foreach (var match in matches)
                {
                    if (top5Maps.Any(m => m == match.MapDb.Name))
                        continue;
                    if (match.MapDb.Name == topMap)
                    {
                        mapPlayedCount++;
                        if (mapPlayedCount > mapPlayedMaxCount)
                            mapPlayedMaxCount = mapPlayedCount;
                    }
                    else
                    {
                        mapPlayedCount = 1;
                        topMap = match.MapDb.Name;
                    }
                }
                if(!top5Maps.Any(m => m == topMap))
                    top5Maps.Add(topMap);
            }
            return top5Maps.ToArray();
        }
        
        private void PlayerStatsUpdate(Match match, Models.Server server)
        {
            foreach (var pms in match.ScoreBoard)
            {
                var ps = db.PlayersStats.FirstOrDefault(playerStats => playerStats.Name == pms.Name);

                if (ps != null)
                {
                    var gameModePlayedCounts = db.GameModePlayedCounts.ToArray(); //  ??

                    ps.Kills += pms.Kills;
                    ps.Deaths += pms.Deaths;
                    ps.TotalMatchesPlayed++;
                    ps.TotalMatchesWon += (match.ScoreBoard.First().Name == pms.Name) ? 1 : 0;
                    ps.MatchesPerThisDay += (match.TimeStamp.Day == ps.LastMatchPlayed.Day) ? 1 : 0;
                    ps.TotalDaysPlayed += (match.TimeStamp.Day != ps.LastMatchPlayed.Day) ? 1 : 0;
                    ps.LastMatchPlayed = match.TimeStamp;

                    if (ps.MatchesPerThisDay > ps.MaximumMatchesPerDay)
                        ps.MaximumMatchesPerDay = ps.MatchesPerThisDay;

                    ps.AverageScoreBoardPercent = 
                        (ps.AverageScoreBoardPercent + 
                        (match.ScoreBoard.Count() - match.ScoreBoard.IndexOf(pms) - 1) * 100 / (match.ScoreBoard.Count() - 1))
                        /2;

                    var serverPlayed = ps.ServersPlayed.FirstOrDefault(sp => sp.Server.Endpoint == match.Server.Endpoint);
                    if(serverPlayed == null)
                        ps.ServersPlayed.Add(serverPlayed);

                    var gameModePlayed = ps.GameModsPlayed.FirstOrDefault(gmp => gmp.GameModeName == match.GameModeDb.Name);
                    if (gameModePlayed != null)
                        gameModePlayed.PlayedCount++;
                    else
                        ps.GameModsPlayed.Add(new GameModePlayedCount(match.GameModeDb.Name, 1));

                    db.SaveChanges();
                    continue;
                }

                ps = new PlayerStats();
                ps.Name = pms.Name;
                ps.Kills = pms.Kills;
                ps.Deaths = pms.Deaths;
                ps.LastMatchPlayed = match.TimeStamp;
                ps.MatchesPerThisDay = 1;
                ps.MaximumMatchesPerDay = 1;
                ps.TotalMatchesPlayed = 1;
                ps.TotalDaysPlayed = 1;
                ps.TotalMatchesWon = (match.ScoreBoard.First().Name == pms.Name) ? 1 : 0;

                var sps = new ServerPlayerStats { Server = server, PlayerStats = ps };
                ps.ServersPlayed.Add(sps);

                GameModePlayedCount gmpc = new GameModePlayedCount
                {
                    GameModeName = match.GameModeDb.Name,
                    PlayedCount = 1,
                    PlayerStats = ps
                };
                ps.GameModsPlayed.Add(gmpc);

                ps.AverageScoreBoardPercent =
                    (match.ScoreBoard.Count() - match.ScoreBoard.IndexOf(pms) - 1) * 100 / (match.ScoreBoard.Count() - 1);

                db.PlayersStats.Add(ps);
                db.SaveChanges();
            }
        }

    }
}

