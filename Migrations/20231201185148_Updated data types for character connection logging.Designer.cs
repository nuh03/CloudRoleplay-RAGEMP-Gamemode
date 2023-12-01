﻿// <auto-generated />
using System;
using CloudRP.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CloudRP.Migrations
{
    [DbContext(typeof(DefaultDbContext))]
    [Migration("20231201185148_Updated data types for character connection logging")]
    partial class Updateddatatypesforcharacterconnectionlogging
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("CloudRP.Admin.Ban", b =>
                {
                    b.Property<int>("ban_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("account_id")
                        .HasColumnType("int");

                    b.Property<string>("admin")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ban_reason")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("client_serial")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ip_address")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<long>("issue_unix_date")
                        .HasColumnType("bigint");

                    b.Property<long>("lift_unix_time")
                        .HasColumnType("bigint");

                    b.Property<ulong>("social_club_id")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("social_club_name")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("ban_id");

                    b.ToTable("bans");
                });

            modelBuilder.Entity("CloudRP.AntiCheat.CharacterConnection", b =>
                {
                    b.Property<int>("join_log_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("character_id")
                        .HasColumnType("int");

                    b.Property<string>("character_name")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("connection_type")
                        .HasColumnType("int");

                    b.Property<int>("player_id")
                        .HasColumnType("int");

                    b.Property<long>("unix")
                        .HasColumnType("bigint");

                    b.HasKey("join_log_id");

                    b.ToTable("server_connections");
                });

            modelBuilder.Entity("CloudRP.Authentication.Account", b =>
                {
                    b.Property<int>("account_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("account_uuid")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("admin_name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("admin_ped")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("admin_status")
                        .HasColumnType("int");

                    b.Property<int>("auto_login")
                        .HasColumnType("int");

                    b.Property<string>("auto_login_key")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("ban_status")
                        .HasColumnType("int");

                    b.Property<string>("client_serial")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("email_address")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("max_characters")
                        .HasColumnType("int");

                    b.Property<string>("password")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<ulong>("social_club_id")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("user_ip")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("vip_status")
                        .HasColumnType("int");

                    b.HasKey("account_id");

                    b.ToTable("accounts");
                });

            modelBuilder.Entity("CloudRP.Character.CharacterModel", b =>
                {
                    b.Property<int>("character_model_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("ageing")
                        .HasColumnType("int");

                    b.Property<int>("blemishes")
                        .HasColumnType("int");

                    b.Property<int>("blushStyle")
                        .HasColumnType("int");

                    b.Property<int>("browHeight")
                        .HasColumnType("int");

                    b.Property<int>("browWidth")
                        .HasColumnType("int");

                    b.Property<int>("cheekBoneHeight")
                        .HasColumnType("int");

                    b.Property<int>("cheekBoneWidth")
                        .HasColumnType("int");

                    b.Property<int>("cheeksWidth")
                        .HasColumnType("int");

                    b.Property<int>("chestHairStyle")
                        .HasColumnType("int");

                    b.Property<int>("chinLength")
                        .HasColumnType("int");

                    b.Property<int>("chinPosition")
                        .HasColumnType("int");

                    b.Property<int>("chinShape")
                        .HasColumnType("int");

                    b.Property<int>("chinWidth")
                        .HasColumnType("int");

                    b.Property<int>("complexion")
                        .HasColumnType("int");

                    b.Property<int>("eyeColour")
                        .HasColumnType("int");

                    b.Property<int>("eyebrowsColour")
                        .HasColumnType("int");

                    b.Property<int>("eyebrowsStyle")
                        .HasColumnType("int");

                    b.Property<int>("eyes")
                        .HasColumnType("int");

                    b.Property<int>("facialHairColour")
                        .HasColumnType("int");

                    b.Property<int>("facialHairStyle")
                        .HasColumnType("int");

                    b.Property<int>("firstHeadShape")
                        .HasColumnType("int");

                    b.Property<int>("firstSkinTone")
                        .HasColumnType("int");

                    b.Property<int>("hairColour")
                        .HasColumnType("int");

                    b.Property<int>("hairHighlights")
                        .HasColumnType("int");

                    b.Property<int>("hairStyle")
                        .HasColumnType("int");

                    b.Property<int>("headMix")
                        .HasColumnType("int");

                    b.Property<int>("jawHeight")
                        .HasColumnType("int");

                    b.Property<int>("jawWidth")
                        .HasColumnType("int");

                    b.Property<int>("lips")
                        .HasColumnType("int");

                    b.Property<int>("lipstick")
                        .HasColumnType("int");

                    b.Property<int>("makeup")
                        .HasColumnType("int");

                    b.Property<int>("molesFreckles")
                        .HasColumnType("int");

                    b.Property<int>("neckWidth")
                        .HasColumnType("int");

                    b.Property<int>("noseBridge")
                        .HasColumnType("int");

                    b.Property<int>("noseBridgeShift")
                        .HasColumnType("int");

                    b.Property<int>("noseHeight")
                        .HasColumnType("int");

                    b.Property<int>("noseLength")
                        .HasColumnType("int");

                    b.Property<int>("noseTip")
                        .HasColumnType("int");

                    b.Property<int>("noseWidth")
                        .HasColumnType("int");

                    b.Property<int>("owner_id")
                        .HasColumnType("int");

                    b.Property<int>("rotation")
                        .HasColumnType("int");

                    b.Property<int>("secondHeadShape")
                        .HasColumnType("int");

                    b.Property<int>("secondSkinTone")
                        .HasColumnType("int");

                    b.Property<bool>("sex")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("skinMix")
                        .HasColumnType("int");

                    b.Property<int>("sunDamage")
                        .HasColumnType("int");

                    b.HasKey("character_model_id");

                    b.ToTable("character_models");
                });

            modelBuilder.Entity("CloudRP.Character.DbCharacter", b =>
                {
                    b.Property<int>("character_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("character_health")
                        .HasColumnType("int");

                    b.Property<int>("character_isbanned")
                        .HasColumnType("int");

                    b.Property<string>("character_name")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("last_login")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("money_amount")
                        .HasColumnType("int");

                    b.Property<int>("owner_id")
                        .HasColumnType("int");

                    b.Property<ulong>("play_time_seconds")
                        .HasColumnType("bigint unsigned");

                    b.Property<uint>("player_dimension")
                        .HasColumnType("int unsigned");

                    b.Property<ulong>("player_exp")
                        .HasColumnType("bigint unsigned");

                    b.Property<float>("position_x")
                        .HasColumnType("float");

                    b.Property<float>("position_y")
                        .HasColumnType("float");

                    b.Property<float>("position_z")
                        .HasColumnType("float");

                    b.HasKey("character_id");

                    b.ToTable("characters");
                });

            modelBuilder.Entity("CloudRP.Vehicles.DbVehicle", b =>
                {
                    b.Property<int>("vehicle_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("numberplate")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("owner_id")
                        .HasColumnType("int");

                    b.Property<float>("position_x")
                        .HasColumnType("float");

                    b.Property<float>("position_y")
                        .HasColumnType("float");

                    b.Property<float>("position_z")
                        .HasColumnType("float");

                    b.Property<float>("rotation")
                        .HasColumnType("float");

                    b.Property<string>("vehicle_dimension")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("vehicle_garage_id")
                        .HasColumnType("int");

                    b.Property<int>("vehicle_insurance_id")
                        .HasColumnType("int");

                    b.Property<bool>("vehicle_locked")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("vehicle_name")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<uint>("vehicle_spawn_hash")
                        .HasColumnType("int unsigned");

                    b.HasKey("vehicle_id");

                    b.ToTable("vehicles");
                });
#pragma warning restore 612, 618
        }
    }
}
