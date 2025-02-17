﻿using CloudRP.GeneralSystems.GeneralCommands;
using CloudRP.PlayerSystems.Character;
using CloudRP.PlayerSystems.DeathSystem;
using CloudRP.PlayerSystems.DMV;
using CloudRP.PlayerSystems.FactionSystems;
using CloudRP.PlayerSystems.PlayerData;
using CloudRP.ServerSystems.Utils;
using CloudRP.VehicleSystems.Vehicles;
using CloudRP.World.MarkersLabels;
using CloudRP.World.TimeWeather;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography;

namespace CloudRP.PlayerSystems.Jobs
{
    public class FreelanceJobSystem : Script
    {
        public delegate void FreelanceJobSystemEventsHandler(Player player, FreeLanceJobData job);

        #region Event Handlers
        public static event FreelanceJobSystemEventsHandler quitJob;
        #endregion

        public static readonly int baseSalaryPay = 50;
        public static readonly string _FreelanceJobDataIdentifier = "FreeLanceJobData";
        public static readonly string _FreelanceJobVehicleDataIdentifier = "FreeLanceJobVehicleData";

        public FreelanceJobSystem()
        {
            TimeSystem.realHourPassed += handlePayBaseSalary;
            Main.playerDisconnect += removeLeavePlayerVehicles;

            DeathEvent.onDeath += (player) =>
            {
                if(player.getFreelanceJobData() != null)
                {
                    deleteFreeLanceVehs(player, true);
                    MarkersAndLabels.removeClientBlip(player);
                }
            };

            Commands.loggingOut += (Player player, DbCharacter character) =>
            {
                if(player.getFreelanceJobData() != null)
                {
                    deleteFreeLanceVehs(player);
                }
            };

            Main.resourceStart += () => ChatUtils.startupPrint($"A total of {Enum.GetNames(typeof(FreelanceJobs)).Length} freelance jobs were loaded.");
        }

        #region Global Methods
        public static void handleVehicleDestroyed(Vehicle vehicle)
        {
            FreeLanceJobVehicleData vehicleData = vehicle.getFreelanceJobData();
            if (vehicleData != null)
            {
                NAPI.Pools.GetAllPlayers().ForEach(p =>
                {
                    DbCharacter playerData = p.getPlayerCharacterData();

                    if(p.getPlayerCharacterData() != null && playerData != null && p.getPlayerCharacterData().character_id == vehicleData.characterOwnerId)
                    {
                        p.SendChatMessage($"{ChatUtils.freelanceJobs} Your job vehicle has been destroyed and you have been fired from your job as a {vehicleData.jobName}.");
                    }
                });

                vehicle.Delete();
            }
        }

        public static void handlePayBaseSalary()
        {
            NAPI.Pools.GetAllPlayers().ForEach(p =>
            {
                DbCharacter character = p.getPlayerCharacterData();

                if (character == null || character != null && character.faction_duty_status != -1) return;

                p.addPlayerSalary(baseSalaryPay, "Base Salary Pay");

                p.setPlayerCharacterData(character, false, true);
                p.SendChatMessage(ChatUtils.salary + $"You have recieved your benefit salary of {ChatUtils.moneyGreen}${baseSalaryPay.ToString("N0")}{ChatUtils.White}.");
            });
        }

        public static bool hasFreeLanceVehicle(Player player)
        {
            DbCharacter characterData = player.getPlayerCharacterData();
            bool hasVeh = false;

            if (characterData != null)
            {
                NAPI.Pools.GetAllVehicles().ForEach(veh =>
                {
                    if(veh.getFreelanceJobData()?.characterOwnerId == characterData.character_id)
                    {
                        hasVeh = true;
                    }
                });
            }

            if(hasVeh)
            {
                CommandUtils.errorSay(player, "You already have a work truck. Continue with your job or use /qjob.");
            }

            return hasVeh;
        }

        public static bool checkValidFreelanceVeh(Player player, FreelanceJobs job)
        {
            bool isValid = false;
            if(player.IsInVehicle)
            {
                FreeLanceJobVehicleData freelanceVehData = player.Vehicle.getFreelanceJobData();
                FreeLanceJobData playerJobData = player.getFreelanceJobData();
                DbCharacter characterData = player.getPlayerCharacterData();

                if(freelanceVehData != null && characterData != null && playerJobData != null && freelanceVehData.jobId == (int)job && freelanceVehData.characterOwnerId == characterData.character_id)
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        public static bool hasAJob(Player player, int compareJobId)
        {
            bool hasAJob = false;

            if(player.getFreelanceJobData() != null && player.getFreelanceJobData().jobId != compareJobId)
            {
                player.SendChatMessage(ChatUtils.error + "You already have a freelance job. Use /qjob to quit it.");
                hasAJob = true;
            }

            if(player.GetData<DmvLicensePlayer>(DmvSystem._PlayerDmvDataKey) != null)
            {
                hasAJob = true;
                player.SendChatMessage(ChatUtils.error + "You have a DMV Course pending.");
            }

            return hasAJob;
        }

        public static void deleteFreeLanceVehs(Player player, bool sendMsg = false)
        {
            DbCharacter characterData = player.getPlayerCharacterData();
            bool foundVeh = false;

            if(characterData != null)
            {
                NAPI.Pools.GetAllVehicles().ForEach(veh =>
                {
                    if (veh.getFreelanceJobData() != null && veh.getFreelanceJobData().characterOwnerId == characterData.character_id)
                    {
                        foundVeh = true;
                        veh.Delete();
                    }
                });

                if(sendMsg && foundVeh)
                {
                    player.SendChatMessage(ChatUtils.freelanceJobs + "Your truck has been returned to your employer.");
                }

            }
        }

        public static void createFreelanceVehicle(Player player, string spawnName, Vector3 spawnCoords, float rot, int jobId, string jobName, string plateIdent)
        {
            if (!hasFreeLanceVehicle(player))
            {
                DbCharacter character = player.getPlayerCharacterData();

                Vehicle workVehicle = VehicleSystem.buildVolatileVehicle(player, spawnName, spawnCoords, rot, plateIdent + character.character_id, 1, 1);

                if (workVehicle == null) return;

                workVehicle.setFreelanceJobData(new FreeLanceJobVehicleData
                {
                    characterOwnerId = character.character_id,
                    destroyOnLeave = true,
                    jobId = jobId,
                    jobName = jobName
                });

                MarkersAndLabels.addBlipForClient(player, 67, "Work Truck", spawnCoords, 50, 255, 20);

                player.SendChatMessage(ChatUtils.freelanceJobs + $"Enter your freelance work vehicle marked on the map.");
            }
        }
        #endregion

        #region Commands
        [Command("quitjob", "~y~Use: ~w~/quitjob", Alias = "qjob")]
        public void quitFreeLanceJob(Player player)
        {
            DbCharacter characterData = player.getPlayerCharacterData();
            FreeLanceJobData jobData = player.getFreelanceJobData();

            if(jobData != null && characterData != null)
            {
                string jobName = jobData.jobName;
                quitJob(player, jobData);
                uiHandling.sendPrompt(player, "fa-solid fa-briefcase", "Quit job", $"Are you sure you want to quit your freelance job as a {jobName}?", remoteEventQuitJob);
            }
            else
            {
                CommandUtils.errorSay(player, "You don't have any freelance jobs to quit.");
            }
        }
        #endregion

        #region Remote Events
        public void remoteEventQuitJob(Player player, object prompt)
        {
            DbCharacter characterData = player.getPlayerCharacterData();
            FreeLanceJobData jobData = player.getFreelanceJobData();

            if (jobData != null && characterData != null)
            {
                string jobName = jobData.jobName;
                quitJob(player, jobData);
                player.resetFreeLanceJobData();
                MarkersAndLabels.removeClientBlip(player);
                MarkersAndLabels.flushClientBlips(player);

                player.SendChatMessage(ChatUtils.freelanceJobs + "You have quit your freelance job as a " + jobName + ".");
            }
        }
        #endregion

        #region Server Events
        public void removeLeavePlayerVehicles(Player player)
        {
            if(player.getFreelanceJobData() != null)
            {
                deleteFreeLanceVehs(player);
            }
        }
        #endregion
    }

    public enum FreelanceJobs
    {
        BusJob,
        TruckerJob,
        PostalJob,
        GruppeSix,
        GarbageJob,
        LawnMower
    }

    public class FreeLanceJobData
    {
        public int jobId {  get; set; }
        public bool jobFinished { get; set; }
        public int jobLevel { get; set; }
        public string jobName { get; set; }
        public long jobStartedUnix { get; set; }
    }

    public class FreeLanceJobVehicleData
    {
        public int characterOwnerId { get; set; }
        public int jobId { get; set; }
        public string jobName { get; set; }
        public bool destroyOnLeave { get; set; } = true;
    }
}
