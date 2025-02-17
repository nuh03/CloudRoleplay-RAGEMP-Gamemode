﻿using CloudRP.PlayerSystems.Character;
using CloudRP.PlayerSystems.PlayerData;
using CloudRP.ServerSystems.Admin;
using CloudRP.ServerSystems.AntiCheat;
using CloudRP.ServerSystems.CustomEvents;
using CloudRP.ServerSystems.Logging;
using CloudRP.ServerSystems.Utils;
using CloudRP.VehicleSystems.VehicleParking;
using CloudRP.VehicleSystems.Vehicles;
using CloudRP.World.MarkersLabels;
using CloudRP.WorldSystems.RaycastInteractions;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudRP.VehicleSystems.VehicleDealerships
{
    public class VehicleDealershipSystem : Script
    {
        public static string _dealershipIdentifer = "vehicleDealership";
        public static string _dealerActiveIdentifier = "vehicleDealershipIsActive";
        public static readonly Vector3 garageIplPosition = new Vector3(230.7, -997.9, -98.2);

        public static List<DealerShip> dealerships = new List<DealerShip>
        {
            new DealerShip
            {
                dealerShipId = 0,
                dealershipName = "PDM",
                position = new Vector3(-40.5, -1095.5, 35.2),
                spawnPosition = new Vector3(-44.3, -1097.0, 26.4),
                vehicles = new List<DealerVehicle>{
                    new DealerVehicle { spawnName = "kamacho", price = 40000 },
                    new DealerVehicle { spawnName = "sultan3", price = 1235 },
                    new DealerVehicle { spawnName = "blista", price = 8546345 },
                    new DealerVehicle { spawnName = "panto", price = 6000 },
                    new DealerVehicle { spawnName = "hakuchou2", price = 600000 },
                    new DealerVehicle { spawnName = "issi2", price = 4300 },
                    new DealerVehicle { spawnName = "baller8", price = 89000 },
                    new DealerVehicle { spawnName = "baller3", price = 45000 },
                    new DealerVehicle { spawnName = "dominator3", price = 56000 }
                },
                viewPosition = new Vector3(-30.0, -1104.8, 26.4),
                viewRange = 1f
            },
            new DealerShip
            {
                dealerShipId = 1,
                dealershipName = "Vapid",
                position = new Vector3(-202.1, -1158.6, 23.8),
                spawnPosition = new Vector3(-223.6, -1162.2, 23.0),
                vehicles = new List<DealerVehicle>{
                    new DealerVehicle { spawnName = "kamacho", price = 40000 },
                    new DealerVehicle { spawnName = "sultan3", price = 1235 },
                    new DealerVehicle { spawnName = "blista", price = 8546345 },
                    new DealerVehicle { spawnName = "panto", price = 6000 },
                    new DealerVehicle { spawnName = "hakuchou2", price = 600000 },
                    new DealerVehicle { spawnName = "issi2", price = 4300 },
                    new DealerVehicle { spawnName = "baller8", price = 89000 },
                    new DealerVehicle { spawnName = "baller3", price = 45000 },
                    new DealerVehicle { spawnName = "dominator3", price = 56000 }
                },
                viewPosition = new Vector3(-202.1, -1158.6, 23.8),
                viewRange = 1f
            },
            new DealerShip
            {
                dealerShipId = 2,
                dealershipName = "Benefactor",
                position = new Vector3(-1261.0, -348.8, 36.8),
                spawnPosition = new Vector3(-1265.0, -341.3, 36.7),
                vehicles = new List<DealerVehicle>{
                    new DealerVehicle { spawnName = "schafter", price = 40000 },
                    new DealerVehicle { spawnName = "schafter2", price = 1235 },
                    new DealerVehicle { spawnName = "schafter3", price = 8546345 },
                    new DealerVehicle { spawnName = "schafter4", price = 6000 },
                    new DealerVehicle { spawnName = "schwarzer", price = 600000 },
                    new DealerVehicle { spawnName = "feltzer", price = 4300 },
                    new DealerVehicle { spawnName = "xls", price = 89000 },
                    new DealerVehicle { spawnName = "xls2", price = 45000 },
                    new DealerVehicle { spawnName = "surano", price = 56000 },
                    new DealerVehicle { spawnName = "dubsta", price = 56000 },
                    new DealerVehicle { spawnName = "dubsta3", price = 56000 },
                    new DealerVehicle { spawnName = "panto", price = 56000 }
                },
                viewPosition = new Vector3(-1261.0, -348.8, 36.8),
                viewRange = 1f
            },
            new DealerShip
            {
                dealerShipId = 3,
                dealershipName = "Offroad Dealer",
                position = new Vector3(-223.2, 6243.1, 31.5),
                spawnPosition = new Vector3(-232.9, 6251.9, 31.5),
                vehicles = new List<DealerVehicle>{
                      new DealerVehicle { spawnName = "bfinjection", price = 1235 },
                      new DealerVehicle { spawnName = "bifta", price = 1235 },
                      new DealerVehicle { spawnName = "blazer", price = 1235 },
                      new DealerVehicle { spawnName = "blazer5", price = 1235 },
                      new DealerVehicle { spawnName = "blazer2", price = 1235 },
                      new DealerVehicle { spawnName = "blazer3", price = 1235 },
                      new DealerVehicle { spawnName = "bodhi2", price = 1235 },
                      new DealerVehicle { spawnName = "brawler", price = 1235 },
                      new DealerVehicle { spawnName = "dune", price = 1235 },
                      new DealerVehicle { spawnName = "dune2", price = 1235 },
                      new DealerVehicle { spawnName = "dubsta3", price = 1235 },
                      new DealerVehicle { spawnName = "dloader", price = 1235 },
                      new DealerVehicle { spawnName = "everon", price = 1235 },
                      new DealerVehicle { spawnName = "freecrawler", price = 1235 },
                      new DealerVehicle { spawnName = "guardian", price = 1235 },
                      new DealerVehicle { spawnName = "insurgent", price = 1235 },
                      new DealerVehicle { spawnName = "insurgent2", price = 1235 },
                      new DealerVehicle { spawnName = "insurgent3", price = 1235 },
                      new DealerVehicle { spawnName = "kamacho", price = 1235 },
                      new DealerVehicle { spawnName = "marshall", price = 1235 },
                      new DealerVehicle { spawnName = "menacer", price = 1235 },
                      new DealerVehicle { spawnName = "mesa", price = 1235 },
                      new DealerVehicle { spawnName = "mesa3", price = 1235 },
                      new DealerVehicle { spawnName = "monster", price = 1235 },
                      new DealerVehicle { spawnName = "nightshark", price = 1235 },
                      new DealerVehicle { spawnName = "outlaw", price = 1235 },
                      new DealerVehicle { spawnName = "patriot", price = 1235 },
                      new DealerVehicle { spawnName = "patriot2", price = 1235 },
                      new DealerVehicle { spawnName = "rampbuggy", price = 1235 },
                      new DealerVehicle { spawnName = "rcbandito", price = 1235 },
                      new DealerVehicle { spawnName = "riata", price = 1235 },
                      new DealerVehicle { spawnName = "rebel", price = 1235 },
                      new DealerVehicle { spawnName = "sandking", price = 1235 }
                },
                viewPosition = new Vector3(-223.3, 6243.1, 31.5),
                viewRange = 1f
            },
            new DealerShip
            {
                dealerShipId = 4,
                dealershipName = "Strawberry Wheelin' deals",
                position = new Vector3(-43.6, -1661.8, 29.5),
                spawnPosition = new Vector3(-47.8, -1682.9, 29.5),
                viewPosition = new Vector3(-43.6, -1661.8, 29.5),
                vehicles = new List<DealerVehicle> {
                    new DealerVehicle { spawnName = "blista", price = 1235 },
                    new DealerVehicle { spawnName = "dilett", price = 1235 },
                    new DealerVehicle { spawnName = "issi", price = 1235 },
                    new DealerVehicle { spawnName = "panto", price = 1235 },
                    new DealerVehicle { spawnName = "prairie", price = 1235 },
                    new DealerVehicle { spawnName = "rhapsody", price = 1235 },
                    new DealerVehicle { spawnName = "bjxl", price = 1235 },
                    new DealerVehicle { spawnName = "bison", price = 1235 },
                    new DealerVehicle { spawnName = "bobcatxl", price = 1235 },
                    new DealerVehicle { spawnName = "bodhi", price = 1235 },
                    new DealerVehicle { spawnName = "dubsta2", price = 1235 },
                    new DealerVehicle { spawnName = "dune", price = 1235 },
                    new DealerVehicle { spawnName = "emptr", price = 1235 },
                    new DealerVehicle { spawnName = "emptr2", price = 1235 },
                    new DealerVehicle { spawnName = "faggio", price = 1235 },
                    new DealerVehicle { spawnName = "faggio2", price = 1235 },
                    new DealerVehicle { spawnName = "fugitive", price = 1235 },
                    new DealerVehicle { spawnName = "granger", price = 1235 },
                    new DealerVehicle { spawnName = "ingot", price = 1235 },
                    new DealerVehicle { spawnName = "intruder", price = 1235 }
                },
                viewRange = 1f
            },
            new DealerShip
            {
                dealerShipId = 5,
                dealershipName = "Sanders Motorcycles",
                position = new Vector3(268.7, -1155.5, 29.3),
                spawnPosition = new Vector3(256.1, -1150.3, 29.3),
                viewPosition = new Vector3(268.7, -1155.5, 29.3),
                vehicles = new List<DealerVehicle>
                {
                    new DealerVehicle { spawnName = "bagger", price = 1235 },
                    new DealerVehicle { spawnName = "bati", price = 1235 },
                    new DealerVehicle { spawnName = "bati2", price = 1235 },
                    new DealerVehicle { spawnName = "daemon", price = 1235 },
                    new DealerVehicle { spawnName = "double", price = 1235 },
                    new DealerVehicle { spawnName = "faggio", price = 1235 },
                    new DealerVehicle { spawnName = "faggio2", price = 1235 },
                    new DealerVehicle { spawnName = "hexer", price = 1235 },
                    new DealerVehicle { spawnName = "lectro", price = 1235 },
                    new DealerVehicle { spawnName = "nemesis", price = 1235 },
                    new DealerVehicle { spawnName = "pcj", price = 1235 },
                    new DealerVehicle { spawnName = "ruffian", price = 1235 },
                    new DealerVehicle { spawnName = "sovereign", price = 1235 },
                    new DealerVehicle { spawnName = "vader", price = 1235 }
                },
                viewRange = 1f
            }
        };

        class RaycastMenuOptions
        {
            public const string viewDealerVehicles = "View vehicle dealership vehicles";
            public const string viewDealerVehiclesAmount = "View stock quantity";
        }

        public VehicleDealershipSystem()
        {
            foreach (DealerShip dealerShip in dealerships)
            {
                ColShape viewingCol = NAPI.ColShape.CreateSphereColShape(dealerShip.viewPosition, dealerShip.viewRange, 0);

                RaycastInteractionSystem.raycastPoints.Add(new RaycastInteraction
                {
                    menuTitle = "Vehicle Dealership",
                    raycastMenuItems = new List<string> { RaycastMenuOptions.viewDealerVehicles, RaycastMenuOptions.viewDealerVehiclesAmount },
                    raycastMenuPosition = dealerShip.viewPosition,
                    targetMethod = serverViewDealerVehicles
                });

                viewingCol.SetData(_dealershipIdentifer, dealerShip);
                NAPI.Blip.CreateBlip(595, dealerShip.position, 1.0f, 7, dealerShip.dealershipName, 255, 1.0f, true, 0, 0);

                viewingCol.OnEntityEnterColShape += (col, player) =>
                {
                    if(col.Equals(viewingCol))
                    {
                        player.SetCustomSharedData(_dealershipIdentifer, dealerShip);
                        player.SetCustomData(_dealershipIdentifer, dealerShip);
                    }
                };

                viewingCol.OnEntityExitColShape += (col, player) =>
                {
                    bool dealerActive = player.GetData<bool>(_dealerActiveIdentifier);

                    if (col.Equals(viewingCol) && !dealerActive)
                    {
                        player.ResetData(_dealershipIdentifer);
                        player.ResetSharedData(_dealershipIdentifer);
                    }
                };
            }

            Main.resourceStart += () => ChatUtils.startupPrint($"A total of {dealerships.Count} vehicle dealerships were loaded.");
        }

        public void serverViewDealerVehicles(Player player, string rayOption)
        {
            DealerShip dealerData = player.GetData<DealerShip>(_dealershipIdentifer);
            bool dealerActive = player.GetData<bool>(_dealerActiveIdentifier);

            if (dealerData == null || dealerActive) return;

            switch (rayOption)
            {
                case RaycastMenuOptions.viewDealerVehicles:
                    {
                        player.sleepClientAc();
                        player.Position = garageIplPosition; 
                        player.SetCustomData(_dealerActiveIdentifier, true);
                        player.TriggerEvent("dealers:initDealership");
                        player.safeSetDimension((uint)player.Id + 1);
                        break;
                    }
                case RaycastMenuOptions.viewDealerVehiclesAmount:
                    {
                        int vehCount = dealerData.vehicles.Count;
                        uiHandling.sendNotification(player, $"~w~The dealership ~c~{dealerData.dealershipName}~w~ currently has {vehCount} vehicles in stock.", false, true, "Views dealership stock quantity.");
                        break;
                    }

            }

        }

        [RemoteEvent("server:closeDealership")]
        public void serverCloseDealership(Player player)
        {
            DealerShip playerDealerData = player.GetData<DealerShip>(_dealershipIdentifer);
            bool dealerActive = player.GetData<bool>(_dealerActiveIdentifier);

            if (playerDealerData != null && dealerActive)
            {
                player.sleepClientAc();
                player.SetCustomData(_dealerActiveIdentifier, false);
                player.Position = playerDealerData.viewPosition;
                player.safeSetDimension(0);
            }
        }

        [RemoteEvent("server:dealerShip:purchaseVehicle")]
        public void handlePlayerVehiclePurchase(Player player, string data)
        {
            DealerShip playerDealerData = player.GetData<DealerShip>(_dealershipIdentifer);
            bool dealerActive = player.GetData<bool>(_dealerActiveIdentifier);
            DbCharacter charData = player.getPlayerCharacterData();
            DealerUiData uiData = JsonConvert.DeserializeObject<DealerUiData>(data);

            if (uiData == null) return;

            if (playerDealerData != null && dealerActive && charData != null)
            {
                DealerVehicle findDealerVeh = playerDealerData.vehicles.Where(veh => veh.spawnName == uiData.vehName).FirstOrDefault();
                
                if (findDealerVeh == null)
                {
                    uiHandling.sendPushNotifError(player, "Vehicle wasn't found.", 6600, true);
                    return;
                }

                if (!player.processPayment(findDealerVeh.price, "Vehicle Dealership - Purchase"))
                {
                    uiHandling.sendPushNotifError(player, $"You don't have enough money to purchase this vehicle!", 6600, true);
                    return;
                }

                if (VehicleSystem.checkVehInSpot(playerDealerData.spawnPosition, 5) != null)
                {  
                    uiHandling.sendPushNotifError(player, "There is a vehicle blocking the spawn point!", 6600, true);
                    return;
                }

                (Vehicle buildVeh, DbVehicle vehicleData) = VehicleSystem.buildVehicle(uiData.vehName, playerDealerData.spawnPosition, 0, charData.character_id, uiData.vehColour, uiData.vehColour, charData.character_name);

                if (buildVeh == null || vehicleData == null) return;

                player.TriggerEvent("dealers:closeDealership");
                CommandUtils.successSay(player, $"You purchased a new {vehicleData.vehicle_display_name} for {ChatUtils.moneyGreen}${findDealerVeh.price.ToString("N0")}{ChatUtils.White}. Your vehicle ~y~has been marked on the map~w~.");
                player.SendChatMessage(ChatUtils.info + $"Make sure to {ChatUtils.red}insure{ChatUtils.White} your vehicle. Or you will have to pay 2x in costs to retrieve it from insurance.");

                ServerLogging.addNewLog(charData.character_id, "Vehicle Dealership Purchase", $"{charData.character_name} purchased a {vehicleData.vehicle_display_name} for ${findDealerVeh.price.ToString("N0")}", LogTypes.AssetPurchase);

                MarkersAndLabels.addBlipForClient(player, 523, $"Your new vehicle [{vehicleData.numberplate}]", playerDealerData.spawnPosition, 70, 255);
                ChatUtils.formatConsolePrint($"{charData.character_name} purchased a new {vehicleData.vehicle_display_name} with id #{vehicleData.vehicle_id}", ConsoleColor.Blue);

                uiHandling.setLoadingState(player, false);
                uiHandling.pushRouterToClient(player, Browsers.None);
            }

            uiHandling.setLoadingState(player, false);
        }

    }
}
