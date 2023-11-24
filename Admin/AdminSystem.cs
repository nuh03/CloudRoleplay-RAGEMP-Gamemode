﻿using CloudRP.Authentication;
using CloudRP.Character;
using CloudRP.Database;
using CloudRP.PlayerData;
using CloudRP.Utils;
using CloudRP.Vehicles;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml.Linq;
using static CloudRP.Authentication.Account;

namespace CloudRP.Admin
{
    internal class AdminSystem : Script
    {

        [Command("aduty")]
        public void onAduty(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData != null && userData.adminLevel > (int)AdminRanks.Admin_SeniorSupport)
            {
                userData.adminDuty = !userData.adminDuty;

                if (userData.adminDuty)
                {
                    AdminUtils.sendMessageToAllStaff($"{AdminUtils.staffPrefix} {userData.adminName} is on duty");
                    uint hash = NAPI.Util.GetHashKey(userData.adminPed);
                    NAPI.Player.SetPlayerSkin(player, hash);
                }
                else
                {
                    uint hash = NAPI.Util.GetHashKey("mp_m_freemode_01");
                    NAPI.Player.SetPlayerSkin(player, hash);
                    AdminUtils.sendMessageToAllStaff($"{AdminUtils.staffPrefix} {userData.adminName} is off duty");
                }

                PlayersData.setPlayerAccountData(player, userData);

            }
            else AdminUtils.sendNoAuth(player);
        }

        [Command("staff")]
        public void staff(Player player)
        {
            int index = 0;
            foreach (var item in AdminUtils.gatherStaff())
            {
                index++;
                User user = item.Value;
                string adminRank = AdminUtils.getColouredAdminRank(user);
                string duty = user.adminDuty ? "[!{green}On-Duty!{white}]" : "[!{red}Off-Duty!{white}]";

                
                AdminUtils.staffSay(player, index + $". {user.adminName} {adminRank} {duty}");
            }
        }

        [Command("a", "~r~/adminchat [message]", GreedyArg = true, Alias = "adminchat")]
        public void adminChat(Player player, string message)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData.adminLevel > (int)AdminRanks.Admin_None)
            {
                Dictionary<Player, User> onlineAdmins = AdminUtils.gatherStaff();

                string colouredAdminRank = AdminUtils.getColouredAdminRank(userData);

                foreach (KeyValuePair<Player, User> entry in onlineAdmins)
                {
                    User staff = entry.Value;

                    NAPI.Chat.SendChatMessageToPlayer(entry.Key, "!{red}" + $"[AdminChat] " + "!{white}" + colouredAdminRank + staff.adminName + " !{grey}says:!{white} " + message);

                    userData.isFlying = false;

                    PlayersData.setPlayerAccountData(player, userData);
                    player.TriggerEvent("admin:endFly");
                }
            }

        }

        [Command("veh", "~r~/veh [vehName]")]
        public void spawnVehicle(Player player, string vehName)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData.adminLevel > (int)AdminRanks.Admin_HeadAdmin || userData.adminLevel > (int)AdminRanks.Admin_SeniorAdmin && userData.adminDuty)
            {
                Vector3 playerPosition = player.Position;
                float playerRotation = player.Rotation.Z;

                Vehicle vehicleBuild = VehicleSystem.buildVehicle(vehName, playerPosition, playerRotation, userData.accountId);

                if (vehicleBuild == null) return;

                player.SetIntoVehicle(vehicleBuild, 0);

                AdminUtils.staffSay(player, $"Spawned in vehicle {vehName}");
            }
            else AdminUtils.sendNoAuth(player);
        }

        [Command("bringv", "~r~/bringv [vehicleId]", Alias = "vbring")]
        public void bringVehicle(Player player, int vehicleId)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData.adminLevel > (int)AdminRanks.Admin_SeniorSupport && userData.adminDuty || userData.adminLevel > (int)AdminRanks.Admin_HeadAdmin)
            {
                Vehicle findVehicleById = VehicleSystem.findVehicleById(vehicleId);

                if (findVehicleById == null)
                {
                    AdminUtils.staffSay(player, $"No vehicle with ID {vehicleId} was found.");
                    return;
                }

                VehicleSystem.bringVehicleToPlayer(player, findVehicleById, true);

                AdminUtils.staffSay(player, $"Brought vehicle with ID {vehicleId}");
            }
        }

        [Command("fly", "~r~/fly")]
        public void fly(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (AdminUtils.checkUserData(player, userData))
            {
                userData.isFlying = !userData.isFlying;

                if (userData.isFlying)
                {
                    AdminUtils.staffSay(player, $"Enabled fly");
                    player.TriggerEvent("admin:startFly");
                }
                else
                {
                    AdminUtils.staffSay(player, $"Disabled fly");
                    player.TriggerEvent("admin:endFly");
                }

                PlayersData.setPlayerAccountData(player, userData);
            }
        }

        [Command("freeze", "~r~/freeze [nameOrId]")]
        public void freezePlayer(Player player, string nameOrId)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (AdminUtils.checkUserData(player, userData))
            {
                Player getPlayer = CommandUtils.getPlayerFromNameOrId(player, nameOrId);

                if (getPlayer != null)
                {
                    User targetPlayerData = PlayersData.getPlayerAccountData(getPlayer);

                    if (targetPlayerData.isFlying || targetPlayerData.adminDuty)
                    {
                        CommandUtils.errorSay(player, "You cannot freeze a player that is flying or in admin duty.");
                        return;
                    }

                    targetPlayerData.isFrozen = targetPlayerData.isFrozen = !targetPlayerData.isFrozen;
                    string isFrozen = targetPlayerData.isFrozen ? "froze" : "unfroze";

                    PlayersData.setPlayerAccountData(getPlayer, targetPlayerData);

                    if (!targetPlayerData.isFrozen)
                    {
                        getPlayer.TriggerEvent("admin:events:stopFly");
                    }

                    AdminUtils.staffSay(player, $"You {isFrozen} {targetPlayerData.username}");
                    AdminUtils.staffSay(getPlayer, $"You were {isFrozen + "n"} by Admin {userData.adminName}");
                }


            }
        }

        [Command("tpm", "~r~/tpm")]
        public void onTeleportToWay(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (AdminUtils.checkUserData(player, userData))
            {
                player.TriggerEvent("admin:events:teleportWay");
                AdminUtils.staffSay(player, "Teleported to waypoint");
            }
        }

        [Command("delv", "~r~/delv [vehicleId]")]
        public void delV(Player player, int vehicleId)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData.adminLevel > (int)AdminRanks.Admin_HeadAdmin)
            {
                bool delVehicle = VehicleSystem.deleteVehicleById(vehicleId);

                if (delVehicle)
                {
                    AdminUtils.staffSay(player, $"Vehicle with id {vehicleId} deleted .");
                } else
                {
                    AdminUtils.staffSay(player, $"Vehicle with id {vehicleId} was not found.");
                }

            }
            else AdminUtils.sendNoAuth(player);
        }

        [Command("gcv")]
        public void getVehicleInfo(Player player, int vehicleId)
        {
            User userData = PlayersData.getPlayerAccountData(player);
            if(userData.adminLevel > 0)
            {
                DbVehicle foundVehicleData = VehicleSystem.getVehicleDataById(vehicleId);

                if(foundVehicleData != null)
                {
                    AdminUtils.staffSay(player, Chat.yellow+"-----------------------------------------------------------");
                    AdminUtils.staffSay(player, "Vehicle id: " + Chat.red + foundVehicleData.vehicle_id + Chat.White + " VehName: " + Chat.red + foundVehicleData.vehicle_name);
                    AdminUtils.staffSay(player, "Owner id: " + Chat.red + foundVehicleData.owner_id+ Chat.White + " Numberplate: " + Chat.red + foundVehicleData.numberplate);
                    AdminUtils.staffSay(player, Chat.yellow + "-----------------------------------------------------------");
                }

            }
        }

        [Command("setdimension", "~r~/setdimension [playerIdOrName] [dimension]", Alias = "setd")]
        public void setDimension(Player player, string playerIdOrName, uint dimension)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                Player getPlayer = CommandUtils.getPlayerFromNameOrId(player, playerIdOrName);
                DbCharacter characterData = PlayersData.getPlayerCharacterData(getPlayer);

                if (getPlayer == null || characterData == null)
                {
                    AdminUtils.playerNotFound(player);
                    return;
                }

                characterData.player_dimension = dimension;
                getPlayer.Dimension = dimension;

                using(DefaultDbContext dbContext = new DefaultDbContext())
                {
                    dbContext.characters.Update(characterData);
                    dbContext.SaveChanges();
                }

                AdminUtils.staffSay(player, $"Set {characterData.character_name}'s dimension to {dimension}");
                AdminUtils.staffSay(getPlayer, $"Your dimension was set to {dimension} by Admin {userData.adminName}");
            }
        }

        [Command("fix")]
        public void onFixVehicle(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                if(player.IsInVehicle)
                {
                    NAPI.Vehicle.RepairVehicle(player.Vehicle);
                    AdminUtils.staffSay(player, " Repaired vehicle.");
                } else
                {
                    AdminUtils.staffSay(player, " You must be in a vehicle to repair it.");
                }
            }

        }

        [Command("router")]
        public void pushRouter(Player player, string route)
        {
            uiHandling.pushRouterToClient(player, route);
        }
    }
}
