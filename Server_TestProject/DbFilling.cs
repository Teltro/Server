using Microsoft.EntityFrameworkCore;
using Server_TestProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_TestProject
{
    public static class DbFilling
    {
        static Random random = new Random();
        static string alphabet = "qwertyuiopasdfghjklzxcvbnmeyuioa";
        public static void DbFill(ServerContext context)
        {
            if (context.Servers.Any())
                return;

            for(int i = 0; i < 6; i++)
            {
                GameMode gameMode = new GameMode
                {
                    Name = RandomText(5).ToUpper()
                };
                context.GameMods.Add(gameMode);
            }
            for(int i = 0; i < 30; i++)
            {
                Map map = new Map {
                    Name = RandomText(12),
                    Matches = new List<Match>()
                };
                context.Maps.Add(map);
            }
            context.SaveChanges();
            
            //---------------
            //for(int i = 0; i < 200 + random.Next(301); i++)
            //{
            //    PlayerStats ps = new PlayerStats();
            //    ps.Name = RandomText(10);
            //    context.PlayersStats.Add(ps);
            //}
            //---------------

            for (int i = 0; i < 5; i++)
            {
                Models.Server server = new Models.Server
                {
                    Endpoint = RandomText(12),
                    Matches = new List<Match>()
                };

                Info info = new Info
                {
                    Name = RandomText(6)
                };

                float chance;
                foreach(var player in context.PlayersStats)
                {
                    chance = 0;
                    if (player.UniqueServers == 5)
                        continue;

                    chance = random.Next(100) + 1;
                    chance /= player.UniqueServers;
                    if (chance <= 50)
                        continue;
                    //else
                    if((chance > 50) || (i == 5 && player.UniqueServers == 0))
                    {
                        ServerPlayerStats sps = new ServerPlayerStats { Server = server, PlayerStats = player };
                        player.ServersPlayed.Add(sps);
                        context.ServerPlayerStats.Add(sps);
                    }

                }


                info.Server = server;
                server.Info = info;
                context.Servers.Add(server);
                context.Info.Add(info);
                context.SaveChanges();

                var igms = new List<InfoGameMode>();
                for (int j = 0; j < random.Next(6) + 1; j++)
                {
                    InfoGameMode igm = new InfoGameMode
                    {
                        Info = context.Info.ToArray()[i],
                        GameMode = context.GameMods.ToArray()[j]
                    };
                    igms.Add(igm);
                    context.InfoGameMods.Add(igm);
                    context.Info.ToArray()[i].InfoGameMods.Add(igm);
                    context.GameMods.ToArray()[j].InfoGameMods.Add(igm);
                }
                context.SaveChanges();

                for (int j = 0; j < random.Next(10) + 1; j++)
                {
                    var serverMatch = context.Servers.Last();
                    var igmsMatch = context
                            .InfoGameMods
                            .Where(igm => igm.Info.Server.Endpoint == serverMatch.Endpoint)
                            .ToArray();
                    var gmMatch = igmsMatch[random.Next(igmsMatch.Length)].GameMode;
                    int timeLimit = random.Next(100);
                    var maps = context.Maps.ToArray();
                    var map = maps[random.Next(maps.Length)];

                    Match match = new Match();
                    match.ScoreBoard = new List<PlayerMatchStats>();
                    match.MapDb = map;
                    match.GameModeDb = gmMatch;

                    DateTime startDate = DateTime.Now - new TimeSpan(3650, 0, 0, 0);
                    TimeSpan timeSpan = new TimeSpan(random.Next(3650), 0 ,0 ,0);
                    match.TimeStamp = startDate + timeSpan;

                    match.FragLimit = random.Next(100);
                    match.TimeLimit = timeLimit;
                    match.TimeElapsed = timeLimit - random.Next(timeLimit);

                    gmMatch.Matches.Add(match);
                    map.Matches.Add(match);
                    serverMatch.Matches.Add(match);
                    match.Server = serverMatch;
                    for(int k = 0; k < random.Next(30); k++)
                    {
                        PlayerMatchStats pms = new PlayerMatchStats();
                        pms.Name = RandomText(10);
                        pms.Kills = random.Next(100);
                        pms.Frags = pms.Kills - random.Next(pms.Kills);
                        pms.Deaths = random.Next(100);
                        pms.Match = match;
                        match.ScoreBoard.Add(pms);
                        context.PlayersMatchStats.Add(pms);
                    }
                    context.Matches.Add(match);
                    context.SaveChanges();
                    
                    //----------------
                    //var serverPlayers = context.PlayersStats
                    //    .Where(ps => ps.ServersPlayed
                    //        .Any(s => s.ServerId == i)
                    //    );
                    //var matchPlayerStats = context.Matches.Last();
                    //List<PlayerMatchStats> pmss = new List<PlayerMatchStats>();
                    //int kills = 0;
                    //int deaths = 0;
                    //for (int k = 0; k < random.Next(50) + 1; k++)
                    //{
                    //    var player = serverPlayers.ToArray()
                    //        [random
                    //            .Next(serverPlayers.Count())
                    //        ];
                    //    if(pmss.Any(pms => pms.Name == player.Name))
                    //    {
                    //        k--;
                    //        continue;
                    //    }
                    //    PlayerMatchStats playerMatchStats = new PlayerMatchStats();
                    //    playerMatchStats.Name = player.Name;
                    //    kills = random.Next(100);
                    //    deaths = random.Next(100);
                    //    playerMatchStats.Kills = kills;
                    //    player.Kills += kills;
                    //    playerMatchStats.Deaths = deaths;
                    //    player.Deaths += deaths;
                    //    playerMatchStats.Frags = kills - random.Next(kills);

                    //    //matchPlayerStats.ScoreBoard.Add(playerMatchStats);
                    //    //playerMatchStats.Match = matchPlayerStats;
                    //    //context.SaveChanges();
                    //}
                    //---------------

                }

                context.SaveChanges();


            }

        }

        public static string RandomText(int maxLength)
        {
            int randomLength = random.Next(maxLength);
            string text = "";
            for (int i = 0; i <= randomLength; i++)
            {
                text += alphabet[random.Next(alphabet.Length)];
            }
            return text;
        }
        

    }
}
