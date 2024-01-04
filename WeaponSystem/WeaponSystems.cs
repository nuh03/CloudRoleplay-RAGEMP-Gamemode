﻿using CloudRP.Character;
using CloudRP.PlayerData;
using GTANetworkAPI;

namespace CloudRP.WeaponSystem
{
    class WeaponSystems : Script
    {
        public static string _switchAnimIdentifer = "weaponSwitchAnim";


        [ServerEvent(Event.PlayerWeaponSwitch)]
        public void OnPlayerWeaponSwitch(Player player, WeaponHash oldWeapon, WeaponHash newWeapon)
        {
            DbCharacter characterData = player.getPlayerCharacterData();
            if (characterData != null)
            {
                if (characterData.injured_timer > 0) return;

                player.SetCustomSharedData(_switchAnimIdentifer, true);

                NAPI.Task.Run(() =>
                {
                    player.ResetSharedData(_switchAnimIdentifer);
                }, 3000);
            }
        }
    }
}
