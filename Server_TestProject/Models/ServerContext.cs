using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Server_TestProject.Models
{
    public class ServerContext : DbContext
    {

        public DbSet<Info> Info { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Map> Maps { get; set; }
        public DbSet<GameMode> GameMods { get; set; }
        public DbSet<InfoGameMode> InfoGameMods { get; set; }
        public DbSet<PlayerStats> PlayersStats { get; set; }
        public DbSet<GameModePlayedCount> GameModePlayedCounts { get; set; }
        public DbSet<ServerPlayerStats> ServerPlayerStats { get; set; }
        public DbSet<PlayerMatchStats> PlayersMatchStats { get; set; }

        public ServerContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<InfoGameMode>()
                .HasKey(t => new { t.InfoId, t.GameModeId });

            modelBuilder.Entity<InfoGameMode>()
                .HasOne(igm => igm.Info)
                .WithMany(igm => igm.InfoGameMods)
                .HasForeignKey(igm => igm.InfoId);

            modelBuilder.Entity<InfoGameMode>()
                .HasOne(igm => igm.GameMode)
                .WithMany(info => info.InfoGameMods)
                .HasForeignKey(igm => igm.GameModeId);


            modelBuilder.Entity<ServerPlayerStats>()
                .HasKey(t => new { t.ServerId, t.PlayerStatsId });

            modelBuilder.Entity<ServerPlayerStats>()
                .HasOne(sps => sps.Server)
                .WithMany(serv => serv.PlayerStats)
                .HasForeignKey(sps => sps.ServerId);

            modelBuilder.Entity<ServerPlayerStats>()
                .HasOne(igm => igm.PlayerStats)
                .WithMany(ps => ps.ServersPlayed)
                .HasForeignKey(igm => igm.PlayerStatsId);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=ServerDb;Trusted_Connection=True;");
        }
    }
}
