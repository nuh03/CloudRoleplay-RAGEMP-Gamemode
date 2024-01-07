﻿using CloudRP.Admin;
using CloudRP.PlayerData;
using CloudRP.Utils;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudRP.Authentication
{
    public static class PlayerExtensions
    {
        public static void setPlayerToLoginScreen(this Player player)
        {
            User userData = player.getPlayerAccountData();

            if (userData != null && userData.adminDuty)
            {
                player.setAdminDuty(false);
            }

            player.Position = PlayersData.defaultLoginPosition;

            player.Dimension = Auth._startDimension;
            player.TriggerEvent("client:loginCameraStart");
            uiHandling.pushRouterToClient(player, Browsers.LoginPage);

            player.flushUserAndCharacterData(new string[]{
                PlayersData._sharedAccountDataIdentifier
            });
        }

    }
}
