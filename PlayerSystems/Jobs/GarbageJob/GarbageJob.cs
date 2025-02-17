﻿using CloudRP.PlayerSystems.Character;
using CloudRP.PlayerSystems.Jobs.BusDriver;
using CloudRP.PlayerSystems.PlayerData;
using CloudRP.ServerSystems.CustomEvents;
using CloudRP.ServerSystems.Utils;
using CloudRP.VehicleSystems.Vehicles;
using CloudRP.World.MarkersLabels;
using CloudRP.WorldSystems.MarkersLabels;
using CloudRP.WorldSystems.NpcInteractions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Update;  
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace CloudRP.PlayerSystems.Jobs.GarbageJob
{
    public class GarbageJob : Script
    {
        public static readonly Vector3 startJob = new Vector3(-322.2, -1545.8, 31.0);
        public static readonly Vector3 spawnVehiclesAt = new Vector3(-369.4, -1524.8, 27.8);
        public static readonly float spawnVehiclesRot = 177.7f;
        public static readonly int jobId = (int)FreelanceJobs.GarbageJob;
        public static readonly int jobPay = 3000;
        public static readonly string jobName = "Garbage Job";
        public static readonly string spawnName = "trash";
        public static readonly string _garbageJobDataKey = "jobs:garbage:stopsDone";
        public static readonly Vector3 finishJob = new Vector3(-345.7, -1562.7, 25.2);
        public static int npcId = -1;
        public static readonly float npcHeading = -85.0f; 

        public static List<Vector3> stops = new List<Vector3>
        {
            new Vector3(-848.8, 463.7, 87.3),
            new Vector3(-862.9, 519.5, 89.4),
            new Vector3(-1093.8, 587.9, 102.9),
            new Vector3(-1253.5, 484.8, 94.2),
            new Vector3(-1414.4, 467.1, 109.0),
            new Vector3(-1444.4, 532.7, 119.5),
            new Vector3(-1416.4, 557.2, 124.0),
            new Vector3(-1359.2, 557.3, 129.0),
            new Vector3(-1359.3, 601.2, 133.7)
        };

        public GarbageJob()
        {
            KeyPressEvents.keyPress_E += iterateJobState;

            NpcInteractions.buildPed(PedHash.GarbageSMY, startJob, npcHeading, "Darren - Little Pricks", new string[]
            {
                "Start Garbage Job"
            }, (player, rayoption) =>
            {
                uiHandling.sendPrompt(player,
                    "fa-solid fa-briefcase",
                    "Garbage Job Start",
                    $"Are you sure you want to start this job for the pay of ${jobPay.ToString("N0")} with a total of {stops.Count} stops",
                    startGarbageJob);
            });

            NAPI.Blip.CreateBlip(318, startJob, 1f, 81, "Garbage Job", 255, 0, true, 0, 0);

            ColShape finish = NAPI.ColShape.CreateSphereColShape(finishJob, 5f, 0);

            finish.OnEntityEnterColShape += (col, player) =>
            {
                if (col.Equals(finish)) handleJobPay(player);
            };

            Main.resourceStart += () => ChatUtils.startupPrint($"Garbage driver job has loaded.");
        }

        #region Global Methods
        private static void iterateJobState(Player player)
        {
            Vector3 stop = stops.Where(s => player.checkIsWithinCoord(s, 2f))
                .FirstOrDefault();
            List<Vector3> stopsDone = player.GetData<List<Vector3>>(_garbageJobDataKey);

            if (stop == null || stopsDone == null || stopsDone != null && stopsDone.Contains(stop)) return;   

            FreeLanceJobData job = player.getFreelanceJobData();
            if (job == null) return;

            if (job.jobId != jobId || job.jobLevel == -1) return;

            if(job.jobLevel == (stops.Count - 1))
            {
                endJob(player, job);
                return;
            }

            job.jobLevel++;
            stopsDone.Add(stop);

            player.setFreelanceJobData(job);
            player.SetCustomData(_garbageJobDataKey, stopsDone);

            MarkersAndLabels.deleteClientBlip(player, stops.IndexOf(stop));
            uiHandling.sendNotification(player, "~g~Removed ~g~Garbage", false, true, "Removes garbage...");
        }

        private static void endJob(Player player, FreeLanceJobData job)
        {
            job.jobFinished = true;
            player.setFreelanceJobData(job);

            player.SendChatMessage(ChatUtils.freelanceJobs + "You have finished the garbage job. Head back to the HQ to get paid.");
            MarkersAndLabels.flushClientBlips(player);
            MarkersAndLabels.addBlipForClient(player, 1, "Finish Job", finishJob, 2, 255, -1, true, true);
        }

        private static void handleJobPay(Player player)
        {
            FreeLanceJobData job = player.getFreelanceJobData();
            DbCharacter character = player.getPlayerCharacterData();

            if (job == null || job != null && !job.jobFinished || character == null) return;

            if (job.jobId != jobId) return;

            if (!FreelanceJobSystem.checkValidFreelanceVeh(player, FreelanceJobs.GarbageJob)) return;

            MarkersAndLabels.removeClientBlip(player);
            FreelanceJobSystem.deleteFreeLanceVehs(player);

            player.addPlayerSalary(jobPay, "Garbage Job Pay");

            player.setPlayerCharacterData(character, false, true);
            player.SendChatMessage(ChatUtils.freelanceJobs + $"You have finished the garbage job and {ChatUtils.moneyGreen}${jobPay.ToString("N0")}{ChatUtils.White} has been paid into your salary.");
        }
        #endregion

        #region Server Events 
        [ServerEvent(Event.PlayerEnterVehicle)]
        public void handleEnterVehicle(Player player, Vehicle vehicle, sbyte seatId)
        {
            FreeLanceJobData job = player.getFreelanceJobData();

            if(job == null || job != null && job.jobId != jobId || !FreelanceJobSystem.checkValidFreelanceVeh(player, FreelanceJobs.GarbageJob)) return;

            if (job.jobLevel != -1) return;

            job.jobLevel = 0;

            player.setFreelanceJobData(job);

            player.SendChatMessage(ChatUtils.freelanceJobs + $"Head to the blips on the map collecting the garbage.");

            MarkersAndLabels.setClientWaypoint(player, stops.First());

            MarkersAndLabels.loadClientBlips(player, stops, "Garbage Stop", 1, 3, true);

            player.SetCustomData(_garbageJobDataKey, new List<Vector3>());
        }
        #endregion

        #region Remote Events
        public void startGarbageJob(Player player, object prompt)
        {
            if (!player.checkIsWithinCoord(startJob, 2f) || FreelanceJobSystem.hasAJob(player, jobId) || FreelanceJobSystem.hasFreeLanceVehicle(player)) return;

            player.setFreelanceJobData(new FreeLanceJobData
            {
                jobId = jobId,
                jobName = jobName,
                jobLevel = -1,
                jobStartedUnix = CommandUtils.generateUnix()
            });

            FreelanceJobSystem.createFreelanceVehicle(player, spawnName, spawnVehiclesAt, spawnVehiclesRot, jobId, jobName, "GBG");

            uiHandling.sendNotification(player, "Started job.", false);
            player.ResetData(_garbageJobDataKey);
        }
        #endregion
    }
}
