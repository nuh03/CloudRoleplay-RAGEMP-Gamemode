﻿using System;
using CloudRP.Admin;
using CloudRP.AntiCheat;
using CloudRP.Authentication;
using CloudRP.Character;
using CloudRP.GeneralCommands;
using CloudRP.HousingSystem;
using CloudRP.InventorySystem;
using CloudRP.VehicleModification;
using CloudRP.Vehicles;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;

namespace CloudRP.Database
{
    public class DefaultDbContext : DbContext
    {
        public static readonly string _databaseConnectionString = $"Server={Main._dbHost};Database={Main._dbDatabase};Uid={Main._dbUser};Pwd={Main._dbPassword}";
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_databaseConnectionString);
        }

        public DbSet<Account> accounts { get; set; }
        public DbSet<DbCharacter> characters { get; set; }
        public DbSet<Ban> bans { get; set; }
        public DbSet<CharacterModel> character_models { get; set; }
        public DbSet<CharacterConnection> server_connections { get; set; }
        public DbSet<Nickname> nicknames { get; set; }
        public DbSet<CharacterClothing> character_clothes { get; set; }
        public DbSet<VehicleMods> vehicle_mods { get; set; }
        public DbSet<VehicleKey> vehicle_keys { get; set; }
        public DbSet<DbVehicle> vehicles { get; set; }
        public DbSet<Tattoo> player_tattoos { get; set; }
        public DbSet<House> houses { get; set; }
        public DbSet<FloatingDo> floating_dos { get; set; }
        public DbSet<AdminMarker> admin_markers { get; set; }
        public DbSet<InventoryItem> inventory_items { get; set; }
    }

    public class BaseEntity
    {
        public string key_uuid = Guid.NewGuid().ToString();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}