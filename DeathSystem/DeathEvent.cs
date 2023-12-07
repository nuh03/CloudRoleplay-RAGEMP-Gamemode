﻿using CloudRP.AntiCheat;
using CloudRP.Character;
using CloudRP.Database;
using CloudRP.PlayerData;
using CloudRP.Utils;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CloudRP.DeathSystem
{
    class DeathEvent : Script
    {
        public static List<Hospital> hospitalList = new List<Hospital>();
        public static List<Corpse> corpses = new List<Corpse>();
        public static int _respawnTimeout_seconds = 3;
        public const int _deathTimer_seconds = 600;
        public static string corpseSharedKey = "corpsePed";
        public static int _pedTimeout_seconds= 1800;

        [ServerEvent(Event.ResourceStart)]
        public void initEvents()
        {
            hospitalList.Add(new Hospital { name = "South LS Hospital", position = new Vector3(342.3, -1397.5, 32.5) });
            hospitalList.Add(new Hospital { name = "Pillbox Hill Hospital Back", position = new Vector3(360.8, -585.3, 28.8) });
            hospitalList.Add(new Hospital { name = "Pillbox Hill Hospital Front", position = new Vector3(297.7, -583.6, 43.3) });
            hospitalList.Add(new Hospital { name = "Mount Zonah", position = new Vector3(-497.3, -336.3, 34.5) });
            hospitalList.Add(new Hospital { name = "Sandy Hospital", position = new Vector3(1821.3, 3684.9, 34.3) });
            hospitalList.Add(new Hospital { name = "Paleto Hospital", position = new Vector3(-381.1, 6119.7, 31.5) });

            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);

            ChatUtils.formatConsolePrint(ChatUtils._c_Hospital + "Disabled auto respawn and loaded hospitals", ConsoleColor.DarkCyan);
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {

            DbCharacter characterData = PlayersData.getPlayerCharacterData(player);

            if (characterData != null)
            {
                if (characterData.injured_timer > 0)
                {
                    resetTimer(player, characterData);
                    respawnAtHospital(player);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, ChatUtils.info + "You have been injured.");
                    updateAndSetInjuredState(player, characterData);
                }
            }
        }

        public static void respawnAtHospital(Player player)
        {
            Dictionary<float, Hospital> pDist = new Dictionary<float, Hospital>();

            foreach (Hospital hospital in hospitalList)
            {
                float dist = Vector3.Distance(hospital.position, player.Position);
                
                pDist.Add(dist, hospital);
            }

            List<float> distList = new List<float>(pDist.Keys);

            handleCorpseSet(player);

            distList.Sort();

            AntiCheatSystem.sleepClient(player);

            Hospital closestHospital = pDist.GetValueOrDefault(distList[0]);

            NAPI.Player.SpawnPlayer(player, closestHospital.position);

            NAPI.Chat.SendChatMessageToPlayer(player, ChatUtils.hospital + "You recieved medial treatment at " + closestHospital.name);
        }

        public static void updateAndSetInjuredState(Player player, DbCharacter characterData, int time = _deathTimer_seconds)
        {
            NAPI.Player.SpawnPlayer(player, player.Position);

            characterData.injured_timer = time;
            characterData.character_health = 100;

            using(DefaultDbContext dbContext = new DefaultDbContext())
            {
                dbContext.characters.Update(characterData);
                dbContext.SaveChanges();
            }

            PlayersData.setPlayerCharacterData(player, characterData, false);

            player.TriggerEvent("injured:startInterval", time);
        }

        [RemoteEvent("server:saveInjuredTime")]
        public static void saveInjuredTime(Player player)
        {
            DbCharacter characterData = PlayersData.getPlayerCharacterData(player);

            if (characterData == null) return;

            if(characterData.injured_timer - 10 <= 0)
            {
                removeInjuredStatus(player, characterData);
                return;
            }

            characterData.injured_timer -= 10;

            using(DefaultDbContext dbContext = new DefaultDbContext())
            {
                dbContext.characters.Update(characterData);
                dbContext.SaveChanges();
            }

            PlayersData.setPlayerCharacterData(player, characterData, false);
        }

        public static void removeInjuredStatus(Player player, DbCharacter character, bool respawn = true)
        {
            if (character == null) return;

            resetTimer(player, character);
            player.TriggerEvent("injured:removeStatus");

            if(respawn)
            {
                respawnAtHospital(player);
            }

        }

        public static void resetTimer(Player player, DbCharacter character)
        {
            character.injured_timer = 0;

            using (DefaultDbContext dbContext = new DefaultDbContext())
            {
                dbContext.Update(character);
                dbContext.SaveChanges();
            }

            PlayersData.setPlayerCharacterData(player, character);
            player.TriggerEvent("injured:removeStatus");
        }

        [ServerEvent(Event.PlayerConnected)]
        public static void addCorpsesForPlayer(Player player)
        {
            initCorpses(player);
        }

        public static void removeCorpse(Corpse corpse)
        {
            corpses.Remove(corpse);

            List<Player> onlinePlayers = NAPI.Pools.GetAllPlayers();

            foreach(Player player in onlinePlayers.ToList())
            {
                player.TriggerEvent("corpse:removeCorpse", corpse);
            }

        }

        public static void addCorpseToClients(Corpse corpse)
        {
            List<Player> onlinePlayers = NAPI.Pools.GetAllPlayers();

            foreach (Player onlinePlayer in onlinePlayers)
            {
                onlinePlayer.TriggerEvent("corpse:add", corpse);
            }
        }

        public static void initCorpses(Player player)
        {
            player.TriggerEvent("corpse:setCorpses", corpses);
        }

        [RemoteEvent("sync:corpseValidation")]
        public static void validatePed(Player player, string corpseFromClient)
        {
            if (corpseFromClient == null) return;

            Corpse corpse = JsonConvert.DeserializeObject<Corpse>(corpseFromClient);

            Console.WriteLine(CommandUtils.generateUnix() + " " + corpse.unixCreated + " " + _pedTimeout_seconds);

            if((CommandUtils.generateUnix() - corpse.unixCreated) > _pedTimeout_seconds)
            {
                removeCorpse(corpse);
            }
        }

        public static void handleCorpseSet(Player player)
        {
            DbCharacter characterData = PlayersData.getPlayerCharacterData(player);

            if (characterData == null) return;

            Corpse playerCorpse = new Corpse
            {
                characterName = characterData.character_name,
                model = characterData.characterModel,
                characterId = characterData.character_id,
                position = player.Position,
                unixCreated = CommandUtils.generateUnix()
            };

            corpses.Add(playerCorpse);

            playerCorpse.corpseId = corpses.IndexOf(playerCorpse);

            addCorpseToClients(playerCorpse);
        }

    }
}
