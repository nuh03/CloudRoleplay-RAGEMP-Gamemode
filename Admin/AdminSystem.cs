﻿using CloudRP.AntiCheat;
using CloudRP.Authentication;
using CloudRP.Character;
using CloudRP.Database;
using CloudRP.DeathSystem;
using CloudRP.DiscordSystem;
using CloudRP.GeneralCommands;
using CloudRP.PlayerData;
using CloudRP.Utils;
using CloudRP.Vehicles;
using Discord;
using GTANetworkAPI;
using Integration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CloudRP.Authentication.Account;

namespace CloudRP.Admin
{
    internal class AdminSystem : Script
    {
        public static List<Report> activeReports = new List<Report>();
        public static Dictionary<int, Vector3> adminAdutyPositions = new Dictionary<int, Vector3>();
        public static int _maxReports = 2;

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnect(Player player, DisconnectionType type, string reason)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(userData != null)
            {
                foreach(KeyValuePair<int, Vector3> adminPos in adminAdutyPositions)
                {
                    if(adminPos.Key == userData.accountId)
                    {
                        adminAdutyPositions.Remove(adminPos.Key);
                    }
                }
            }
        }

        public static void saveAdutyPosition(User userData, Vector3 pos)
        {
            if (adminAdutyPositions.ContainsKey(userData.accountId))
            {
                adminAdutyPositions.Remove(userData.accountId);
            }

            adminAdutyPositions.Add(userData.accountId, pos);   
        }

        [ServerEvent(Event.PlayerConnected)]
        public void playerConnected(Player player)
        {
            Ban checkIsBanned = AdminUtils.checkPlayerIsBanned(player);

            if (checkIsBanned != null)
            {
                AdminUtils.setPlayerToBanScreen(player, checkIsBanned);
            }

        }

        [Command("report", "~y~Use: ~w~/report [description]", GreedyArg = true)]
        public async Task onReport(Player player, string desc)
        {
            User userData = PlayersData.getPlayerAccountData(player);
            DbCharacter characterData = PlayersData.getPlayerCharacterData(player);

            if (userData == null || characterData == null) return;

            if(desc.Length > 256)
            {
                CommandUtils.errorSay(player, "Report descriptions must be less than 256 characters.");
                return;
            }

            if(activeReports.Where(rep => rep.playerReporting.Equals(player)).ToList().Count > 0)
            {
                CommandUtils.errorSay(player, "You already have a report active. Use /closereport to close.");
                return;
            }

            Report report = new Report
            {
                playerReporting = player,
                characterData = characterData,
                userData = userData,
                description = desc
            };

            activeReports.Add(report);
            
            int id = activeReports.IndexOf(report);
            
            AdminUtils.sendMessageToAllStaff($"{characterData.character_name} [{player.Id}] has created a new report with ID {id}");
            CommandUtils.successSay(player, $"Created report with id {id}. Staff have been alerted.");
            await DiscordSystems.addAReport(report, id);

            string msgUri = DiscordUtils.getRedirectUri(report.discordRefId);

            EmbedBuilder sendInChannel = new EmbedBuilder
            {
                Title = "Report #" + id + " | **" + desc + "**",
                Color = Discord.Color.DarkerGrey,
                Description = "Created by " + characterData.character_name + " [" + player.Id + "]"
            };

            await DiscordIntegration.SendEmbed(report.discordChannelId, sendInChannel);
        }

        [Command("aesp", "~r~/asep")]
        public void adminEspToggle(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                userData.adminEsp = !userData.adminEsp;

                PlayersData.setPlayerAccountData(player, userData);

                using (DefaultDbContext dbContext = new DefaultDbContext())
                {
                    Account account = dbContext.accounts.Find(userData.accountId);

                    if (account == null) return;

                    account.admin_esp = userData.adminEsp;

                    dbContext.accounts.Update(account);
                    dbContext.SaveChanges();
                }

                AdminUtils.staffSay(player, $"You have {(userData.adminEsp ? "disabled" : "enabled")} admin esp.");

            }
        }

        [Command("twerk", "~y~Use: ~w~/twerk")]
        public void twerkCommand(Player player)
        {
            player.PlayAnimation("mini@strip_club@private_dance@part3", "priv_dance_p3", 1);
            NAPI.Chat.SendChatMessageToPlayer(player, ChatUtils.Success + " You are now twerking!!");
        }       
        
        
        [Command("anim", "~y~Use: ~w~/anim")]
        public void anim(Player player, string dict, string lib)
        {
            player.PlayAnimation(dict, lib, 1);
            NAPI.Chat.SendChatMessageToPlayer(player, ChatUtils.Success + " You are now twerking!!");
        }

        [Command("closereport", "~y~Use: ~w~/closereport")]
        public void closeReport(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);
            DbCharacter characterData = PlayersData.getPlayerCharacterData(player);

            if (userData == null || characterData == null) return;

            Report findReport = activeReports.Where(rep => rep.playerReporting == player).FirstOrDefault(); 

            if(findReport != null)
            {
                CommandUtils.successSay(player, "Your active report was closed.");
                DiscordSystems.closeAReport(findReport);
                return;
            }
            else
            {
                CommandUtils.errorSay(player, "You do not have a active report.");
            }
        }

        [RemoteEvent("server:viewReports")]
        [Command("reports", "~r~/reports")]
        public void viewReports(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(userData.adminLevel > (int)AdminRanks.Admin_None)
            {
                if(activeReports.Count == 0)
                {
                    uiHandling.sendPushNotifError(player, "There are currently no active reports.", 4300);
                    return;
                }

                List<SharedReport> sharedReports = new List<SharedReport>();

                foreach(Report report in activeReports)
                {
                    sharedReports.Add(new SharedReport
                    {
                        description = report.description,
                        playerId = report.playerReporting.Id,
                        reportId = activeReports.IndexOf(report)
                    });
                }

                uiHandling.handleObjectUiMutation(player, MutationKeys.PlayerReportData, sharedReports);

                uiHandling.pushRouterToClient(player, Browsers.ReportsPage);

            } else AdminUtils.sendNoAuth(player);

        }

        [RemoteEvent("server:acceptReport")]
        public void acceptReport(Player player, int reportId)
        {
            User userData = PlayersData.getPlayerAccountData(player);
            DbCharacter characterData = PlayersData.getPlayerCharacterData(player);

            if (userData == null || characterData == null || userData.adminLevel < 1) return;

            if (reportId > activeReports.Count || reportId < 0)
            {
                CommandUtils.errorSay(player, "Invalid report id.");
                return;
            }

            Report findReport = activeReports[reportId];

            if(findReport.adminsHandling.ContainsKey(player))
            {
                CommandUtils.errorSay(player, "You have already accepted this report.");
                return;
            }

            if(findReport != null)
            {
                if(findReport.playerReporting.Equals(player))
                {
                    CommandUtils.errorSay(player, "You cannot accept your own reports");
                    return;
                }

                findReport.adminsHandling.Add(player, userData);

                if(findReport.adminsHandling.Count == 0 && findReport.discordAdminsHandling.Count == 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(findReport.playerReporting, ChatUtils.reports + $"Your report was accepted by {userData.adminName}");
                } else
                {
                    NAPI.Chat.SendChatMessageToPlayer(findReport.playerReporting, ChatUtils.reports + $"Admin {userData.adminName} joined your report.");
                }

                uiHandling.resetRouter(player);
                NAPI.Chat.SendChatMessageToPlayer(player, ChatUtils.reports + $"You accepted report {reportId}.");

                DiscordIntegration.SendMessage(findReport.discordChannelId, $"Admin {userData.adminName} joined your report.");
            }
        }

        [RemoteEvent("server:closeReport")]
        public async Task closeReport(Player player, int reportId)
        {
            User userData = PlayersData.getPlayerAccountData(player);
            DbCharacter characterData = PlayersData.getPlayerCharacterData(player);

            if (userData == null || characterData == null || userData.adminLevel < 1) return;

            Report findReport = activeReports[reportId];

            if (reportId > activeReports.Count || reportId < 0) return;

            if (!findReport.adminsHandling.ContainsKey(player))
            {
                CommandUtils.errorSay(player, "You must a be an admin handling this report to close it.");
                return;
            }

            uiHandling.resetRouter(player);

            await DiscordSystems.closeAReport(findReport, true); 

            activeReports.Remove(findReport);
            NAPI.Chat.SendChatMessageToPlayer(player, ChatUtils.Success + "You closed report with id " + reportId);
            NAPI.Chat.SendChatMessageToPlayer(findReport.playerReporting, ChatUtils.reports + $"Your report was closed");


        }

        [Command("rr", "~r~Use: ~w~/rr [message]", GreedyArg = true, Alias = "reportrespond")]
        public async Task reportResponse(Player player, string message)
        {
            User userData = PlayersData.getPlayerAccountData(player);
            DbCharacter characterData = PlayersData.getPlayerCharacterData(player);

            if (userData == null || characterData == null) return;

            Report report = activeReports
                .Where(rep => rep.playerReporting == player)
                .FirstOrDefault();
            
            Report handlingReport = activeReports
                .Where(rep => rep.adminsHandling
                .ContainsKey(player))
                .FirstOrDefault();

            if(handlingReport != null)
            {
                AdminUtils.sendToAdminsHandlingReport(handlingReport, ChatUtils.reports + ChatUtils.red + "[Admin] " + ChatUtils.White + userData.adminName + ChatUtils.grey + " says:" + ChatUtils.White + message, player);
                NAPI.Chat.SendChatMessageToPlayer(player, ChatUtils.reports + $"You {ChatUtils.grey}say:{ChatUtils.White} " + message);
                NAPI.Chat.SendChatMessageToPlayer(handlingReport.playerReporting, ChatUtils.reports + $"{ChatUtils.red}[Admin] {ChatUtils.White}{userData.adminName} {ChatUtils.grey}says:{ChatUtils.White} " + message);
                await DiscordIntegration.SendMessage(handlingReport.discordChannelId, "``[ADMIN]`` " + userData.adminName + " **says:** " + message, false);
                return;
            }

            if (report != null)
            {
                if(report.discordAdminsHandling.Count == 0 && report.adminsHandling.Count == 0)
                {
                    CommandUtils.errorSay(player, "You must wait until your report is accepted to use this command.");
                    return;
                }

                AdminUtils.sendToAdminsHandlingReport(report, ChatUtils.reports + $"{characterData.character_name} [{player.Id}]" + ChatUtils.grey + " says: " + ChatUtils.White + message, player);
                await DiscordIntegration.SendMessage(report.discordChannelId, characterData.character_name + $" [{player.Id}] " + "**says:** " + message);
                NAPI.Chat.SendChatMessageToPlayer(player, ChatUtils.reports + $"You {ChatUtils.grey}say:{ChatUtils.White} " + message);
                return;
            }
            else
            {
                CommandUtils.errorSay(player, "You do not have any active report to respond to.");
            }
        }

        [Command("goback", "~r~/goback")]
        public static void goAdminBack(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                KeyValuePair<int, Vector3> savedAdminPosition = adminAdutyPositions.Where(savePos => savePos.Key == userData.accountId).FirstOrDefault(); 

                if(savedAdminPosition.Value == null)
                {
                    CommandUtils.errorSay(player, "You have not been on admin duty yet.");
                    return;
                }

                player.Position = savedAdminPosition.Value;

                AdminUtils.staffSay(player, "Returned to admin duty position.");
            }
        }

        [Command("aduty", "~r~/aduty")]
        public void onAduty(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData != null && userData.adminLevel > (int)AdminRanks.Admin_SeniorSupport)
            {
                userData.adminDuty = !userData.adminDuty;
                userData.showAdminPed = userData.adminDuty;

                if (userData.adminDuty)
                {
                    saveAdutyPosition(userData, player.Position);
                    AdminUtils.sendMessageToAllStaff($"{userData.adminName} is on duty");
                }
                else
                {
                    userData.isFlying = false;
                    player.TriggerEvent("admin:endFly");
                    AdminUtils.sendMessageToAllStaff($"{userData.adminName} is off duty");
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

                    NAPI.Chat.SendChatMessageToPlayer(entry.Key, ChatUtils.red + $"[Admin Chat] " + ChatUtils.White + colouredAdminRank + userData.adminName + ChatUtils.red + " says: " + ChatUtils.White + message);
                }
            }
            else AdminUtils.sendNoAuth(player);
        }

        [Command("tpto", "~r~/tpto [nameOrId]", Alias = "goto")]
        public void teleportToPlayer(Player player, string nameOrId)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                Player findPlayer = CommandUtils.getPlayerFromNameOrId(nameOrId);

                if(findPlayer == null)
                {
                    CommandUtils.notFound(player);
                    return;
                }

                if (findPlayer.Equals(player))
                {
                    AdminUtils.staffSay(player, "You cannot tp to yourself.");
                    return;
                }

                if (findPlayer != null)
                {
                    player.Position = findPlayer.Position;

                    AdminUtils.staffSay(player, "Teleported to Player [" + findPlayer.Id + "]");
                }
            }
        }

        [Command("kick", "~r~/kick [nameOrId] [silent]")]
        public void kickPlayer(Player player, string nameOrId, bool isSilent = false)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                Player findPlayer = CommandUtils.getPlayerFromNameOrId(nameOrId);

                if(findPlayer != null)
                {
                    DbCharacter characterData = PlayersData.getPlayerCharacterData(findPlayer);

                    if (findPlayer.Equals(player))
                    {
                        AdminUtils.staffSay(player, "You cannot kick yourself");
                        return;
                    }

                    findPlayer.Kick();

                    if(!isSilent)
                    {
                        AdminUtils.sendMessageToAllStaff(userData.adminName + " kicked " + characterData.character_name);
                    }
                } else
                {
                    CommandUtils.notFound(player);
                }
            }
        }

        [Command("revive", "~r~/revive [nameOrId]")]
        public void reviveCommand(Player player, string nameOrId)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                Player findPlayer = CommandUtils.getPlayerFromNameOrId(nameOrId);

                if(findPlayer == null)
                {
                    CommandUtils.notFound(player);
                    return;
                }

                DbCharacter findCharacter = PlayersData.getPlayerCharacterData(findPlayer);

                if(findCharacter.injured_timer <= 0)
                {
                    AdminUtils.staffSay(player, "This player isn't injured!");
                    return;
                }

                DeathEvent.resetTimer(findPlayer, findCharacter);
                NAPI.Player.SpawnPlayer(findPlayer, findPlayer.Position);
                
                AdminUtils.staffSay(player, "You revived " + findCharacter.character_name);
                AdminUtils.staffSay(findPlayer, "You were revived by Admin " + userData.adminName);
            }

        }

        [Command("bring", "~r~/bring [nameOrId]")]
        public void bringPlayer(Player player, string nameOrId)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (AdminUtils.checkUserData(player, userData))
            {
                Player findPlayer = CommandUtils.getPlayerFromNameOrId(nameOrId);

                if (findPlayer != null)
                {
                    if (findPlayer.Equals(player))
                    {
                        AdminUtils.staffSay(player, "You cannot bring yourself");
                        return;
                    }

                    AntiCheatSystem.sleepClient(findPlayer);
                    findPlayer.Position = player.Position;

                    AdminUtils.staffSay(player, "Teleported Player [" + findPlayer.Id + "] to you.");
                    AdminUtils.staffSay(findPlayer, $"Admin {userData.adminName} teleported you.");
                }
                else
                {
                    CommandUtils.notFound(player);
                    return;
                }


                return;
            }
        }

        [Command("veh", "~r~/veh [vehName]")]
        public void spawnVehicle(Player player, string vehName)
        {
            User userData = PlayersData.getPlayerAccountData(player);
            DbCharacter charData = PlayersData.getPlayerCharacterData(player);

            if (charData == null) return;

            if (userData.adminLevel > (int)AdminRanks.Admin_HeadAdmin || userData.adminLevel > (int)AdminRanks.Admin_SeniorAdmin && userData.adminDuty)
            {
                Vector3 playerPosition = player.Position;
                float playerRotation = player.Rotation.Z;

                Vehicle vehicleBuild = VehicleSystem.buildVehicle(vehName, playerPosition, playerRotation, charData.character_id);

                if (vehicleBuild == null) return;

                player.SetIntoVehicle(vehicleBuild, 0);

                AdminUtils.staffSay(player, $"Spawned in vehicle {vehName}");
            }
            else AdminUtils.sendNoAuth(player);
        }

        [Command("bringv", "~r~/bringv [vehicleIdOrPlate]", Alias = "vbring")]
        public void bringVehicle(Player player, string vehicleIdOrPlate)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData.adminLevel > (int)AdminRanks.Admin_SeniorSupport && userData.adminDuty || userData.adminLevel > (int)AdminRanks.Admin_HeadAdmin)
            {
                Vehicle findVehicle = VehicleSystem.getVehicleByPlate(vehicleIdOrPlate);

                if (findVehicle == null)
                {
                    int? parseId = CommandUtils.tryParse(vehicleIdOrPlate);
                    
                    if(parseId == null)
                    {
                        CommandUtils.errorSay(player, "Vehicle was not found.");
                        return;
                    }

                    findVehicle = VehicleSystem.getVehicleById((int)parseId, player.Position);
                }

                if(findVehicle == null)
                {
                    CommandUtils.errorSay(player, "Vehicle was not found.");
                } else
                {
                    findVehicle.Position = player.Position;
                    AdminUtils.staffSay(player, $"Vehicle was brought to you.");
                    player.SetIntoVehicle(findVehicle, 0);
                }

            }
        }

        [RemoteEvent("admin:fly")]
        [Command("fly", "~r~/fly")]
        public void fly(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (AdminUtils.checkUserData(player, userData))
            {
                userData.isFlying = !userData.isFlying;

                if (userData.isFlying)
                {
                    uiHandling.sendPushNotif(player, "Enabled fly", 6000);
                    player.TriggerEvent("admin:startFly");
                }
                else
                {
                    uiHandling.sendPushNotif(player, "Disabled fly", 6000);
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
                Player getPlayer = CommandUtils.getPlayerFromNameOrId(nameOrId);

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

                    if(getPlayer.IsInVehicle)
                    {
                        getPlayer.WarpOutOfVehicle();
                    }

                    AdminUtils.staffSay(player, $"You {isFrozen} {targetPlayerData.username}");
                    AdminUtils.staffSay(getPlayer, $"You were {isFrozen + "n"} by Admin {userData.adminName}");
                }


            }
        }

        [Command("tpm", "~r~/tpm", Alias = "telm")]
        public void onTeleportToWay(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (AdminUtils.checkUserData(player, userData))
            {
                player.TriggerEvent("admin:events:teleportWay");
            }
        }

        [Command("delv", "~r~/delv [yourVehicle Or vehicleId]")]
        public void delV(Player player, int vehicleId = -1)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData.adminLevel > (int)AdminRanks.Admin_HeadAdmin)
            {
                if(vehicleId == -1 && player.IsInVehicle)
                {
                    DbVehicle vehicleData = VehicleSystem.getVehicleData(player.Vehicle);

                    if (vehicleData == null)
                    {
                        player.Vehicle.Delete();
                        return;
                    }

                    bool deletePlayerVeh = VehicleSystem.deleteVehicleById(vehicleData.vehicle_id);

                    if(deletePlayerVeh)
                    {
                        AdminUtils.staffSay(player, $"Vehicle with id {vehicleData.vehicle_id} was deleted.");
                        return;
                    }

                    AdminUtils.staffSay(player, $"Enter a vehicle ID to delete or enter a vehicle and use this command.");
                    return;
                }

                bool delVehicle = VehicleSystem.deleteVehicleById(vehicleId);

                if (delVehicle)
                {
                    AdminUtils.staffSay(player, $"Vehicle with id {vehicleId} deleted.");
                } else
                {
                    AdminUtils.staffSay(player, $"Vehicle with was not found.");
                }

            }
            else AdminUtils.sendNoAuth(player);
        }

        [Command("gcv", "~r~/gcv")]
        public void getVehicleInfo(Player player)
        {
            User userData = PlayersData.getPlayerAccountData(player);
            
            if(userData.adminLevel > 0)
            {
                Vehicle findVeh = VehicleSystem.getClosestVehicleToPlayer(player);

                if(findVeh != null)
                {
                    DbVehicle currentVehicleData = VehicleSystem.getVehicleData(findVeh);

                    if (currentVehicleData != null)
                    {
                        VehicleSystem.sayInfoAboutVehicle(player, userData, currentVehicleData);
                    }
                }
                else
                {
                    AdminUtils.staffSay(player, "Vehicle couldn't be found.");
                }
            }
        }

        [Command("setdimension", "~r~/setdimension [playerIdOrName] [dimension]", Alias = "setd")]
        public void setDimension(Player player, string playerIdOrName, uint dimension)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                Player getPlayer = CommandUtils.getPlayerFromNameOrId(playerIdOrName);
                DbCharacter characterData = PlayersData.getPlayerCharacterData(getPlayer);

                if (getPlayer == null || characterData == null)
                {
                    AdminUtils.playerNotFound(player);
                    return;
                }

                if(dimension == Auth._startDimension)
                {
                    CommandUtils.errorSay(player, "This dimension is restricted.");
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

        [Command("setaped", "~r~/setaped [pedName]")]
        public void setAdminPed(Player player, string pedName)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(userData.adminLevel > (int)AdminRanks.Admin_SeniorSupport)
            {
                using (DefaultDbContext dbContext = new DefaultDbContext())
                {
                    Account findAccount = dbContext.accounts.Find(userData.accountId);

                    if (findAccount != null)
                    {
                        findAccount.admin_ped = pedName;
                        userData.adminPed = pedName;
                    }

                    dbContext.Update(findAccount);
                    dbContext.SaveChanges();
                    PlayersData.setPlayerAccountData(player, userData);
                    AdminUtils.setPed(player, pedName);

                    AdminUtils.staffSay(player, $"You set your admin ped to {pedName}");
                }
            } else AdminUtils.sendNoAuth(player);
        }

        [Command("ban", "~r~/ban [playerIdOrName] [length minutes(-1 for permanent ban)] [reason]", GreedyArg = true)]
        public static void banPlayer(Player player, string playerNameOrId, int time, string reason)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                Player banPlayer = CommandUtils.getPlayerFromNameOrId(playerNameOrId);
                if(banPlayer == null)
                {
                    CommandUtils.notFound(player);
                    return;
                }

                if(player.Equals(banPlayer))
                {
                    AdminUtils.staffSay(player, "You cannot ban yourself.");
                    return;
                }

                User banPlayerUserData = PlayersData.getPlayerAccountData(banPlayer);
                DbCharacter characterData = PlayersData.getPlayerCharacterData(banPlayer);

                if(banPlayerUserData == null || characterData == null) return;

                if (time < -1)
                {
                    AdminUtils.staffSay(player, "Enter a valid minute time.");
                    return;
                }

                long lift_unix_time = time == -1 ? -1 : CommandUtils.generateUnix() + time * 60;

                AdminUtils.banAPlayer(time, userData, banPlayerUserData, banPlayer, reason);

                string playerAdminRank = AdminUtils.getColouredAdminRank(userData);
                string endOfBanString = lift_unix_time == 1 ? ChatUtils.red + "is permanent" : "expires at " + ChatUtils.orange + CommandUtils.unixTimeStampToDateTime(lift_unix_time);
                
                CommandUtils.sendToAllPlayers($"{AdminUtils.staffPrefix}{playerAdminRank} {userData.adminName} banned {characterData.character_name} with reason {reason} ban {endOfBanString}");
            }
        }

        [Command("unban", "~r~/unban [username]", GreedyArg = true)]
        public void unbanAplayer(Player player, string accountName)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                bool unbanViaUsername = AdminUtils.unbanViaUsername(accountName);

                if(unbanViaUsername)
                {
                    AdminUtils.staffSay(player, $"Unbanned user {accountName}");
                } else
                {
                    AdminUtils.staffSay(player, $"No ban for user {accountName} was found.");
                }
            }
            
        }

        [Command("spw", "~r~/spw [weaponName] [ammo]")]
        public void spawnWeapon(Player player, WeaponHash weaponName, int ammo = 999)
        {
            User userData = PlayersData.getPlayerAccountData(player);
            if (userData == null) return;

            if(userData.adminLevel > (int)AdminRanks.Admin_HeadAdmin)
            {
                NAPI.Player.GivePlayerWeapon(player, weaponName, ammo);

                AdminUtils.staffSay(player, $"You gave yourself a {ChatUtils.yellow}{weaponName}{ChatUtils.White} with {ammo} ammo");
            }
            else AdminUtils.sendNoAuth(player);

        }

        [Command("flip", "~r~/flip [Current Vehicle Or VehicleId]")]
        public void flipVehicle(Player player, int vehicleId = -1)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData == null) return;

            if(userData.adminLevel > 2 && userData.adminDuty || userData.adminLevel > (int)AdminRanks.Admin_SeniorAdmin)
            {
                if(vehicleId == -1 && !player.IsInVehicle)
                {
                    CommandUtils.errorSay(player, "You must be in a vehicle or specify a vehicle ID.");
                }

                if(player.IsInVehicle)
                {
                    player.Vehicle.Rotation = new Vector3(0, 0, 0);
                    AdminUtils.staffSay(player, "Flipped vehicle.");
                    return;
                }

                Vehicle findById = VehicleSystem.findVehicleById(vehicleId); 

                if(findById == null)
                {
                    CommandUtils.errorSay(player, "Vehicle with that ID was not found");
                    return;
                }

                findById.Rotation = new Vector3(0, 0, 0);
                AdminUtils.staffSay(player, "Flipped vehicle");

            } else AdminUtils.sendNoAuth(player);
        }

        [Command("stv", "~r~/stv [seatId]", Alias = "setintovehicle")]
        public void setIntoVehicle(Player player, int seatId = 0)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (AdminUtils.checkUserData(player, userData))
            {
                Vehicle findVeh = VehicleSystem.getClosestVehicleToPlayer(player);

                if(findVeh == null)
                {
                    CommandUtils.errorSay(player, "There are no vehicles within range to enter");
                    return;
                }

                player.SetIntoVehicle(findVeh, seatId);
            }
        }

        [Command("id", "~r~/id [playerIdOrName]")]
        public void idPlayer(Player player, string playerName)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                Player findPlayer = CommandUtils.getPlayerFromNameOrId(playerName);

                if(findPlayer == null)
                {
                    CommandUtils.errorSay(player, "Player was not found.");
                    return;
                }

                DbCharacter findPlayerCharData = PlayersData.getPlayerCharacterData(findPlayer);

                uiHandling.pushRouterToClient(player, Browsers.StatsPage);

                uiHandling.handleObjectUiMutation(player, MutationKeys.PlayerStats, findPlayerCharData);
                uiHandling.handleObjectUiMutation(player, MutationKeys.PlayerData, player);

                AdminUtils.staffSay(player, $"Viewing {findPlayerCharData.character_name}'s stats");
            }
        }

        [Command("sethp", "~r~/sethp [nameOrId] [health]", Alias = "sethealth")]
        public void setHealth(Player player, string nameOrId, int health)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                Player findPlayer = CommandUtils.getPlayerFromNameOrId(nameOrId);
                DbCharacter characterData = PlayersData.getPlayerCharacterData(findPlayer);

                if(findPlayer == null)
                {
                    CommandUtils.notFound(player);
                    return;
                }

                if(PlayersData.getPlayerAccountData(findPlayer).adminDuty)
                {
                    CommandUtils.errorSay(player, "You cannot use this command with on duty admins.");
                    return;
                }

                findPlayer.Health = health;

                AdminUtils.staffSay(player, $"Set {characterData.character_name}'s health to {health}");
                AdminUtils.staffSay(findPlayer, $"Your health was set to {health} by Admin {userData.adminName}");

            }
        }

        [Command("setaname", "~r~/setaname [nameOrId] [adminName]", GreedyArg = true)]
        public void setAdminName(Player player, string nameOrId, string adminName)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(userData.adminLevel > (int)AdminRanks.Admin_Admin && userData.adminDuty || userData.adminLevel > (int)AdminRanks.Admin_HeadAdmin)
            {
                Player findPlayer = CommandUtils.getPlayerFromNameOrId(nameOrId);

                if(findPlayer == null)
                {
                    CommandUtils.notFound(player);
                    return;
                }

                User findPlayerData = PlayersData.getPlayerAccountData(findPlayer);

                findPlayerData.adminName = adminName;

                using(DefaultDbContext dbContext = new DefaultDbContext())
                {
                    Account findAccount = dbContext.accounts.Find(findPlayerData.accountId);

                    findAccount.admin_name = adminName;

                    dbContext.Update(findAccount);
                    dbContext.SaveChanges();
                }

                PlayersData.setPlayerAccountData(findPlayer, findPlayerData);
                AdminUtils.staffSay(player, $"Set Player [{findPlayer.Id}]'s admin name to {adminName}.");
                AdminUtils.staffSay(findPlayer, $"Your admin name was set to {adminName} by {userData.adminName}.");


            } else AdminUtils.sendNoAuth(player);
        }

        [Command("setadmin", "~r~/setadmin [nameOrId] [adminLevel(0-8)]")]
        public void setAdmin(Player player, string nameOrId, int adminRankSet)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            Player findPlayer = CommandUtils.getPlayerFromNameOrId(nameOrId);

            if(userData != null && (userData.adminLevel > (int)AdminRanks.Admin_Admin && userData.adminDuty || userData.adminLevel > (int)AdminRanks.Admin_HeadAdmin))
            {
                if(adminRankSet > RankList.adminRanksList.Length || adminRankSet < 0)
                {
                    AdminUtils.staffSay(player, "Enter a valid admin rank between 0 and " +RankList.adminRanksList.Length);
                    return;
                }

                if (adminRankSet > 3 && userData.adminLevel <= (int)AdminRanks.Admin_SeniorAdmin)
                {
                    AdminUtils.staffSay(player, "You cannot set admin ranks that high. You can set up to moderator and below.");
                    return;
                }
                
                if(adminRankSet > 6 && userData.adminLevel <= (int)AdminRanks.Admin_HeadAdmin)
                {
                    AdminUtils.staffSay(player, "You cannot set admin ranks that high. You can set up to Senior Admin and below.");
                    return;
                }

                User findPlayerData = PlayersData.getPlayerAccountData(findPlayer);
                DbCharacter findPlayerCharData = PlayersData.getPlayerCharacterData(findPlayer);

                if(findPlayerData.adminLevel >= (int)AdminRanks.Admin_SeniorAdmin && userData.adminLevel <= (int)AdminRanks.Admin_SeniorAdmin)
                {
                    AdminUtils.staffSay(player, "You cannot remove admin ranks from a rank that high.");
                    return;
                }


                findPlayerData.adminLevel = adminRankSet;
                findPlayerData.adminDuty = false;

                using (DefaultDbContext dbContext = new DefaultDbContext())
                {
                    Account findAcc = dbContext.accounts.Find(findPlayerData.accountId);

                    if (findAcc == null) return;

                    findAcc.admin_status = adminRankSet;

                    dbContext.accounts.Update(findAcc);
                    dbContext.SaveChanges();
                }

                PlayersData.setPlayerAccountData(findPlayer, findPlayerData);

                string setAdminRank = AdminUtils.getColouredAdminRank(findPlayerData); 
;                AdminUtils.staffSay(player, $"You set {findPlayerCharData.character_name}'s admin level to {setAdminRank}");
;                AdminUtils.staffSay(findPlayer, $"Your admin level was set to {setAdminRank} by Admin {userData.adminName}");

                AdminUtils.sendMessageToAllStaff($"{userData.adminName} set {findPlayerCharData.character_name}'s admin level to {setAdminRank}");
            
            } else AdminUtils.sendNoAuth(player);
        }

        [Command("sendtoinsurance", "~r~/sendtoinsurance [plateOrId]", Alias = "sentov")]
        public void sendToInsurance(Player player, string plateOrId)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                Vehicle findVehicle = VehicleSystem.vehicleIdOrPlate(plateOrId);

                if(findVehicle != null)
                {
                    VehicleSystem.sendVehicleToInsurance(findVehicle);

                    AdminUtils.staffSay(player, "Sent vehicle " + plateOrId + " to insurance.");
                } else
                {
                    AdminUtils.staffSay(player, "Vehicle couldn't be found.");
                    return;
                }
            }
        }

        [Command("gotov", "~r~/gotov [idOrPlate]")]
        public void goToVehicle(Player player, string idOrPlate)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (AdminUtils.checkUserData(player, userData))
            {
                Vehicle findVehicle = VehicleSystem.vehicleIdOrPlate(idOrPlate);

                if(findVehicle != null)
                {
                    player.Position = findVehicle.Position;
                    AdminUtils.staffSay(player, "Teleported to vehicle " + idOrPlate);

                } else
                {
                    CommandUtils.errorSay(player, "Vehicle couldn't be found.");
                }
            }
        }

        [Command("ha", "~r~/headadmin [message]", Alias = "headadmin", GreedyArg = true)]
        public void headAdminChat(Player player, string message)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData.adminLevel > (int)AdminRanks.Admin_SeniorAdmin)
            {
                Dictionary<Player, Dictionary<User, DbCharacter>> adminGroup = AdminUtils.gatherAdminGroupAbove(AdminRanks.Admin_SeniorAdmin);

                string adminRank = AdminUtils.getColouredAdminRank(userData);
                string prefix = ChatUtils.red + "[HA+] " + ChatUtils.White;

                foreach (KeyValuePair<Player, Dictionary<User, DbCharacter>> admin in adminGroup)
                {
                    NAPI.Chat.SendChatMessageToPlayer(admin.Key, prefix + adminRank + userData.adminName + ChatUtils.red + " says: " + ChatUtils.White + message);
                }
            }
            else AdminUtils.sendNoAuth(player);
        }

        [Command("setw", "~r~/setw [weather]")]
        public void setweather(Player player, Weather weather)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData.adminLevel > (int)AdminRanks.Admin_SeniorAdmin)
            {
                NAPI.World.SetWeather(weather);

                AdminUtils.staffSay(player, "Set weather to " + weather);

            }
            else AdminUtils.sendNoAuth(player);
        }

        [Command("arefuel", "~r~/arefuel [currentVehicle|id|plate]")]
        public void adminRefuelVehicle(Player player, string vehicleIdOrPlate = null)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                Vehicle findVehicle = null;

                if (vehicleIdOrPlate == null && player.IsInVehicle)
                {
                    findVehicle = player.Vehicle;
                } else if(!player.IsInVehicle &&  vehicleIdOrPlate == null)
                {
                    CommandUtils.errorSay(player, "Select the correct parameters (/arefuel [currentVehicle|id|plate]) or enter a vehicle to use this command.");
                    return; 
                }

                if (findVehicle == null)
                {
                    findVehicle = VehicleSystem.getVehicleByPlate(vehicleIdOrPlate);

                    if (findVehicle == null)
                    {
                        int? parseId = CommandUtils.tryParse(vehicleIdOrPlate);

                        if (parseId == null)
                        {
                            CommandUtils.errorSay(player, "Vehicle was not found.");
                            return;
                        }

                        findVehicle = VehicleSystem.getVehicleById((int)parseId, null, false);
                    }
                }

                if(findVehicle != null)
                {
                    DbVehicle findVehicleData = VehicleSystem.getVehicleData(findVehicle);

                    if (findVehicleData == null) return;

                    findVehicleData.vehicle_fuel = 100;

                    VehicleSystem.saveVehicleData(findVehicle, findVehicleData, true);

                    AdminUtils.staffSay(player, $"You refilled vehicle with id {findVehicleData.vehicle_id}.");
                }
            }

        }

        [Command("banchar", "~r~/banchar [characterName]", Alias = "bancharacter", GreedyArg = true)]
        public void banCharacter(Player player, string characterName)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                bool bannedCharacter = AdminUtils.banCharacter(characterName);

                if(bannedCharacter)
                {
                    AdminUtils.staffSay(player, "You banned character " + characterName +"!");
                } else
                {
                    CommandUtils.errorSay(player, "Specified character was not found.");
                }
            }
        }        
        
        [Command("unbanchar", "~r~/unbanchar [characterName]", Alias = "unbancharacter", GreedyArg = true)]
        public void unbanCharacter(Player player, string characterName)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if(AdminUtils.checkUserData(player, userData))
            {
                bool hasUnBanCharacter = AdminUtils.unBanCharacter(characterName);

                if(hasUnBanCharacter)
                {
                    AdminUtils.staffSay(player, "You unbanned character " + characterName +"!");
                } else
                {
                    CommandUtils.errorSay(player, "Specified character ban was not found.");
                }

            }
        }

        [Command("setvplate", "~r~/setvplate [plate]")]
        public void setVPlate(Player player, string numberplate)
        {
            User userData = PlayersData.getPlayerAccountData(player);

            if (userData.adminLevel > (int)AdminRanks.Admin_SeniorAdmin)
            {
                if(!player.IsInVehicle)
                {
                    CommandUtils.errorSay(player, "You must be in a vehicle to use this command.");
                    return;
                }

                Vehicle pVeh = player.Vehicle;
                DbVehicle vehData = VehicleSystem.getVehicleData(pVeh);
                string formattedPlate = numberplate.ToUpper();    

                if (vehData != null)
                {
                    using(DefaultDbContext dbContext = new DefaultDbContext())
                    {
                        DbVehicle findVeh = dbContext.vehicles
                            .Where(veh => veh.numberplate == formattedPlate)
                            .FirstOrDefault();

                        if (findVeh != null)
                        {
                            CommandUtils.errorSay(player, "There is already a vehicle with this plate.");
                            return;
                        }

                        vehData.numberplate = formattedPlate;
                        pVeh.NumberPlate = formattedPlate;

                        dbContext.Update(vehData);
                        dbContext.SaveChanges();

                        VehicleSystem.setVehicleData(pVeh, vehData);
                        AdminUtils.staffSay(player, $"You have set vehicle #{vehData.vehicle_id}'s license plate to {formattedPlate}");
                    }


                }

            }
            else AdminUtils.sendNoAuth(player);

        }
    }
}
