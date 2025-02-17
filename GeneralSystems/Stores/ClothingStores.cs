﻿using CloudRP.PlayerSystems.ChatSystem;
using CloudRP.PlayerSystems.Character;
using CloudRP.PlayerSystems.PlayerData;
using CloudRP.ServerSystems.Database;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudRP.ServerSystems.Utils;
using CloudRP.World.MarkersLabels;
using CloudRP.ServerSystems.CustomEvents;

namespace CloudRP.GeneralSystems.Stores
{
    class ClothingStores : Script
    {
        public static List<ClothingStore> clothingStores = new List<ClothingStore>();
        public static string _clothingStoreIdentifier = "clothingStoreData";

        public ClothingStores()
        {
            KeyPressEvents.keyPress_Y += checkForMaskStore;

            clothingStores.Add(new ClothingStore
            {
                displayName = "Binco Clothing",
                name = "Binco LSPD",
                position = new Vector3(425.5, -806.3, 29.5)
            });

            clothingStores.Add(new ClothingStore
            {
                displayName = "Discount Clothing",
                name = "Discount Strawberry",
                position = new Vector3(75.5, -1392.8, 29.4)
            });

            clothingStores.Add(new ClothingStore
            {
                displayName = "Discount Clothing",
                name = "Discount Paleto",
                position = new Vector3(4.6, 6512.3, 31.9)
            });

            clothingStores.Add(new ClothingStore
            {
                displayName = "Ponsonbys Clothing",
                name = "Ponsonbys Rockford Plaza",
                position = new Vector3(-163.4, -302.8, 39.7)
            });

            clothingStores.Add(new ClothingStore
            {
                displayName = "Surburban Clothing",
                name = "Surburban Hawick Ave",
                position = new Vector3(125.4, -224.7, 54.6)
            });

            clothingStores.Add(new ClothingStore
            {
                displayName = "Discount Clothing",
                name = "Discount Grapeseed",
                position = new Vector3(1693.9, 4822.6, 42.1)
            });
            
            clothingStores.Add(new ClothingStore
            {
                displayName = "Discount Clothing",
                name = "Discount Vespucci",
                position = new Vector3(-822.1, -1073.9, 11.3)
            });
            
            clothingStores.Add(new ClothingStore
            {
                displayName = "Surburban Clothing",
                name = "Surburban Harmony",
                position = new Vector3(614.7, 2768.4, 42.1)
            });
            
            clothingStores.Add(new ClothingStore
            {
                displayName = "Discount Clothing",
                name = "Discount Zancudo",
                position = new Vector3(-1101.1, 2710.4, 19.1)
            });
            
            clothingStores.Add(new ClothingStore
            {
                displayName = "Surburban Clothing",
                name = "Surburban Del Perro",
                position = new Vector3(-1188.7, -765.7, 17.3)
            });

            clothingStores.Add(new ClothingStore
            {
                displayName = "Vespucci Mask Store",
                name = "Vespucci Mask Store",
                position = new Vector3(-1339.0, -1280.5, 4.8),
                maskStore = true
            });

            for (int i = 0; i < clothingStores.Count; i++)
            {
                clothingStores[i].id = i;
                Vector3 pos = clothingStores[i].position;
                string name = clothingStores[i].name;
                string dispName = clothingStores[i].displayName;

                ColShape clothingColShape = NAPI.ColShape.CreateCylinderColShape(pos, 1.0f, 1.0f);
                NAPI.TextLabel.CreateTextLabel($"{dispName}\nUse ~y~Y~w~ to interact", pos, 10f, 1.0f, 4, new Color(255, 255, 255, 255), true);
                MarkersAndLabels.setPlaceMarker(pos);

                if (clothingStores[i].maskStore)
                {
                    NAPI.Blip.CreateBlip(362, pos, 1.0f, 4, name, 255, 1.0f, true, 0, 0);
                }
                else
                {
                    NAPI.Blip.CreateBlip(73, pos, 1.0f, 63, name, 255, 1.0f, true, 0, 0);
                }

                setColShapeData(clothingColShape, clothingStores[i]);
            }
        }

        public void checkForMaskStore(Player player)
        {
            ClothingStore store = clothingStores.Where(store => player.checkIsWithinCoord(store.position, 3) && store.maskStore)
                .FirstOrDefault();

            if(store != null)
            {
                uiHandling.handleObjectUiMutation(player, MutationKeys.MaskStoreState, true);
            }

        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public void enterClothingColShape(ColShape colshape, Player player)
        {
            ClothingStore clothingStoreData = colshape.GetData<ClothingStore>(_clothingStoreIdentifier);

            if (clothingStoreData != null)
            {
                setClothingStoreData(player, clothingStoreData);
                uiHandling.sendPushNotif(player, "To interact with this clothing store use Y.", 5500, false, false);
            }
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public void onExitClothingColShape(ColShape colShape, Player player)
        {
            if (colShape.GetData<ClothingStore>(_clothingStoreIdentifier) != null)
            {
                flushClothingStoreData(player);

                DbCharacter charData = player.getPlayerCharacterData();
                if (charData != null)
                {
                    uiHandling.handleObjectUiMutation(player, MutationKeys.MaskStoreState, false);
                    player.setCharacterClothes(charData.characterClothing);
                }
            }
        }

        [RemoteEvent("server:handleClothesPurchase")]
        public void clothesPurchase(Player player, string clothes)
        {
            DbCharacter characterData = player.getPlayerCharacterData();
            CharacterClothing clothingData = JsonConvert.DeserializeObject<CharacterClothing>(clothes);

            NAPI.Task.Run(() =>
            {
                uiHandling.setLoadingState(player, false);
            }, 1500);

            if (characterData != null && clothingData != null)
            {
                if(characterData.faction_duty_status != -1)
                {
                    uiHandling.sendPushNotifError(player, "You can't purchase clothing whilst on faction duty.", 6600);
                    return;
                }

                if (characterData.characterClothing.Equals(clothingData))
                {
                    uiHandling.sendPushNotifError(player, "You haven't purchased anything.", 6600);
                    return;
                }

                if (!player.processPayment(300, "Clothing Purchase"))
                {
                    uiHandling.sendPushNotifError(player, "You do not have enough money to cover this purchase.", 5500, true);
                    return;
                }

                using (DefaultDbContext dbContext = new DefaultDbContext())
                {
                    CharacterClothing findCharClothes = dbContext.character_clothes
                        .Where(charFindClothes => charFindClothes.character_id == characterData.character_id)
                        .FirstOrDefault();

                    if (findCharClothes != null)
                    {
                        characterData.characterClothing = clothingData;
                        characterData.cachedClothes = clothingData;

                        dbContext.character_clothes.Remove(findCharClothes);
                        dbContext.SaveChanges();

                        clothingData.character_id = characterData.character_id;
                        dbContext.character_clothes.Add(clothingData);
                        dbContext.SaveChanges();
                        uiHandling.sendPushNotif(player, "You successfully purchase a new item of clothing.", 6600, false, false);

                        player.setPlayerCharacterData(characterData, true, true);
                        CommandUtils.successSay(player, $"You purchased a new clothing item for {ChatUtils.moneyGreen}$300");
                    }
                }
            }
        }

        public static void flushClothingStoreData(Player player)
        {
            player.ResetData(_clothingStoreIdentifier);
            player.ResetSharedData(_clothingStoreIdentifier);
        }

        public static void setClothingStoreData(Player player, ClothingStore store)
        {
            player.SetCustomData(_clothingStoreIdentifier, store);
            player.SetCustomSharedData(_clothingStoreIdentifier, store);
        }

        public static void setColShapeData(ColShape colShape, ClothingStore store)
        {
            colShape.SetData(_clothingStoreIdentifier, store);
            colShape.SetSharedData(_clothingStoreIdentifier, store);
        }

    }
}
