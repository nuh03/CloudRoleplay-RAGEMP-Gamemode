﻿using CloudRP.PlayerSystems.Character;
using CloudRP.PlayerSystems.FactionSystems;
using CloudRP.PlayerSystems.FactionSystems.PoliceSystems;
using CloudRP.PlayerSystems.PlayerData;
using CloudRP.ServerSystems.Utils;
using CloudRP.VehicleSystems.Vehicles;
using CloudRP.WorldSystems.RaycastInteractions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudRP.GeneralSystems.SpeedCameras
{
    public class SpeedCameras : Script
    {
        public delegate void OnVehiclePassByCamEvent(Player player, Vehicle vehicle, DbVehicle vehicleData, string camName, int vehicleSpeed);

        #region Event Handlers
        public static event OnVehiclePassByCamEvent onVehiclePassByCamera;
        #endregion

        public static VehicleClasses[] exemptClasses = new VehicleClasses[] {
            VehicleClasses.Helicopters, VehicleClasses.Boats, VehicleClasses.Cycles, VehicleClasses.Planes,
            VehicleClasses.Trains
        };

        public string _speedCameraDataIdentifier = "speedCameraColshapeData";
        public static readonly int speedCameraSabotageTime_seconds = 60;
        public static List<SpeedCamera> cameras = new List<SpeedCamera>
        {
            new SpeedCamera { position = new Vector3(429.7, -543.6, 28.7), camName = "Strawberry Ave Highway Exit", camPropPos = new Vector3(399.9, -561.0, 27.1), camFlashPos = new Vector3(400.1, -560.8, 32.7), camRot = 155, range = 25, speedLimit = 80 },
            new SpeedCamera { position = new Vector3(-2006.4, -388.6, 11.4), camName = "Bay City Incline", camPropPos = new Vector3(-2007.4, -395.9, 9.9), camFlashPos = new Vector3(-2007.3, -395.0, 14.4), camRot = 180, range = 10, speedLimit = 80 },
            new SpeedCamera { position = new Vector3(-76.7, 259.1, 101.4), camName = "Eclipse Blvd", camPropPos = new Vector3(-74.8, 272.1, 99.9), camFlashPos = new Vector3(-75.0, 271.8, 103.9), camRot = 100, range = 15, speedLimit = 80 },
            new SpeedCamera { position = new Vector3(616.7, 42.3, 89.8), camName = "Vinewood PD", camPropPos = new Vector3(630.7, 57.3, 87.7), camFlashPos = new Vector3(631.0, 57.3, 92.4), camRot = 30, range = 15, speedLimit = 80 },
            new SpeedCamera { position = new Vector3(170.8, -818.6, 31.2), camName = "Pillbox Hill Junction",camPropPos = new Vector3(142.7, -823, 29.9), camFlashPos = new Vector3(142.7, -823.8, 35.2), camRot = 180, range = 25, speedLimit = 80 },
            new SpeedCamera { position = new Vector3(399.7, -989.6, 29.5), camName = "Mission Row PD",camPropPos = new Vector3(391.5, -1003.7, 27.8), camFlashPos = new Vector3(391.5, -1003.7, 32.6), camRot = 170, range = 15, speedLimit = 80 },
            new SpeedCamera { position = new Vector3(-1032.5, 263.4, 64.8), camName = "Rockford Hills", camPropPos = new Vector3(-1042.7, 279.5, 62.5), camFlashPos = new Vector3(-1042.7, 279.5, 66.9), camRot = 50, range = 25, speedLimit = 80 },
            new SpeedCamera { position = new Vector3(-92.5, 6440.4, 31.5), camName = "Paleto Bank", camPropPos = new Vector3(-100.5, 6447.5, 30.6), camFlashPos = new Vector3(-100.5, 6447.5, 34.6), camRot = 178.2, range = 15, speedLimit = 80  }
        };

        List<SpeedFine> speedFines = new List<SpeedFine>
        {
            new SpeedFine { finePrice = 400, speed = 82, chargeId = 43 },
            new SpeedFine { finePrice = 800, speed = 120, chargeId = 44 },
            new SpeedFine { finePrice = 2500, speed = 200, chargeId = 45 }
        };

        public SpeedCameras()
        {
            cameras.ForEach(cam =>
            {
                NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_cctv_pole_04"), cam.camPropPos, new Vector3(0, 0, cam.camRot));
                ColShape speedCamCol = NAPI.ColShape.CreateSphereColShape(cam.position, cam.range, 0);
                speedCamCol.SetData(_speedCameraDataIdentifier, cam);

                speedCamCol.OnEntityEnterColShape += setSpeedCamData;
                speedCamCol.OnEntityExitColShape += removeSpeedCamData;

                RaycastInteractionSystem.raycastPoints.Add(new RaycastInteraction
                {
                    menuTitle = "Speedcamera - Sabotage",
                    raycastMenuItems = new List<string> { $"Sabotage Speedcamera for {speedCameraSabotageTime_seconds} seconds." },
                    raycastMenuPosition = new Vector3(cam.camPropPos.X, cam.camPropPos.Y, cam.camPropPos.Z + 1.5),
                    hasPlaceMarker = false,
                    targetMethod = (player, rayOption) => {
                        sabotageSpeedCamera(player, cameras.IndexOf(cam));
                    }
                });
            });

            Main.resourceStart += () => ChatUtils.startupPrint($"A total of {cameras.Count} speed cameras were loaded with {speedFines.Count} speed fines.");
        }

        public void sabotageSpeedCamera(Player player, int cameraIndex)
        {
            SpeedCamera camera = cameras.ElementAt(cameraIndex);

            if (camera == null) return;

            if(camera.sabotaged)
            {
                uiHandling.sendPushNotifError(player, "This camera is already sabotaged.", 6600);
                return;
            }

            camera.sabotaged = true;

            uiHandling.sendNotification(player, $"You sabotaged this speed camera for {speedCameraSabotageTime_seconds} seconds.", false, true, "Sabotages speedcamera");

            NAPI.Task.Run(() =>
            {
                camera.sabotaged = false;
            }, speedCameraSabotageTime_seconds * 1000);
        }

        public void setSpeedCamData(ColShape colshape, Player player)
        {
            SpeedCamera camData = colshape.GetData<SpeedCamera>(_speedCameraDataIdentifier);

            if (camData != null && player.IsInVehicle)
            {
                player.TriggerEvent("client:speedCameraTrigger");
                player.SetCustomData(_speedCameraDataIdentifier, camData);
            }
        }

        public void removeSpeedCamData(ColShape colshape, Player player)
        {
            SpeedCamera camData = colshape.GetData<SpeedCamera>(_speedCameraDataIdentifier);

            if (camData != null)
            {
                player.ResetData(_speedCameraDataIdentifier);
            }
        }

        [RemoteEvent("server:handleSpeedCamera")]
        public void handleSpeedCamera(Player player, int vehicleSpeed)
        {
            SpeedCamera cameraData = player.GetData<SpeedCamera>(_speedCameraDataIdentifier);
            DbCharacter characterData = player.getPlayerCharacterData();

            if (cameraData != null && characterData != null && player.IsInVehicle)
            {
                if(cameraData.sabotaged) return;

                double speed = vehicleSpeed * 3.6;

                DbVehicle vehicleData = player.Vehicle.getData();

                if (vehicleData == null) return;

                if (exemptClasses.Contains((VehicleClasses)vehicleData.vehicle_class_id)) return;

                onVehiclePassByCamera(player, player.Vehicle, vehicleData, cameraData.camName, vehicleSpeed);

                if (FactionSystem.emergencyFactions.Contains((Factions)vehicleData.faction_owner_id)) return;

                if (speed > speedFines[0].speed && speed > cameraData.speedLimit && player.VehicleSeat == 0)
                {
                    SpeedFine closest = speedFines.OrderBy(item => Math.Abs(speed - item.speed)).First();

                    if (closest == null) return;

                    NAPI.Pools.GetAllPlayers().ForEach(p =>
                    {
                        if (Vector3.Distance(p.Position, player.Position) < 120)
                        {
                            p.TriggerEvent("client:handleCameraFlash", player.Vehicle.Id, cameraData.camFlashPos.X, cameraData.camFlashPos.Y, cameraData.camFlashPos.Z);
                        }
                    });

                    CriminalChargeSystem.addPlayerCharge(characterData.character_id, new int[]
                    {
                        closest.chargeId
                    }, 0, closest.finePrice);

                    player.SendChatMessage(ChatUtils.info + $"You have been fined in excess of ${closest.finePrice.ToString("N0")} for speeding ({speed.ToString("N0")}KMH in a {cameraData.speedLimit}KMH Zone). " +
                        $"Please go to a police station and pay your fine or it will end in further legal action being taken.");
                }
            }

        }

    }
}
