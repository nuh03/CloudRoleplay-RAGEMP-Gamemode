﻿using CloudRP.Authentication;
using CloudRP.Character;
using CloudRP.PlayerData;
using CloudRP.Utils;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudRP.ChatSystem
{
    internal class Chat : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            Console.WriteLine("Default chat disabled.");
            NAPI.Server.SetGlobalServerChat(false);
        }

        [ServerEvent(Event.ChatMessage)]
        public void onChatMessage(Player player, string message)
        {
            DbCharacter characterData = PlayersData.getPlayerCharacterData(player);
            User userData = PlayersData.getPlayerAccountData(player);

            if (characterData == null || userData == null) return;

            string prefix = "";
            string suffix = " !{grey}says:!{white} ";

            if(userData.adminDuty)
            {
                string adminRank = AdminUtils.getColouredAdminRank(userData);
                prefix += adminRank + "!{red}" + $"{userData.adminName}" + "!{white} ";
            } else
            {
                prefix += $"{characterData.character_name.Replace("_", " ")}";
            }

            NAPI.Chat.SendChatMessageToAll(prefix + suffix + message);

        }
    }
}
