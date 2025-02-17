﻿using CloudRP.PlayerSystems.Character;
using CloudRP.PlayerSystems.PlayerData;
using CloudRP.ServerSystems.CustomEvents;
using CloudRP.ServerSystems.Utils;
using CloudRP.World.MarkersLabels;
using CloudRP.World.TimeWeather;
using CloudRP.WorldSystems.RaycastInteractions;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace CloudRP.World.BanksAtms
{
    public class Banks : Script
    {
        public static int bankCloseHour = 22;
        public static int bankOpenHour = 8;
        public static List<Bank> banks = new List<Bank>
        {
            new Bank
            {
                blipPos = new Vector3(278.9, 232.5, 170.9),
                tellers = new List<Vector3>
                {
                    new Vector3(242.0, 224.0, 106.3),
                    new Vector3(247.1, 222.0, 106.3),
                    new Vector3(252.2, 220.0, 106.3)
                }
            },
            new Bank
            {
                blipPos = new Vector3(309.1, -301.2, 76.4),
                tellers = new List<Vector3>
                {
                    new Vector3(313.1, -278.1, 54.2),
                    new Vector3(314.7, -278.7, 54.2)
                }
            },
            new Bank
            {
                blipPos = new Vector3(-108.7, 6473.3, 39.5),
                tellers = new List<Vector3>
                {
                    new Vector3(-113.7, 6469.6, 31.6),
                    new Vector3(-112.7, 6468.6, 31.6),
                    new Vector3(-111.6, 6467.5, 31.6)
                }
            },
            new Bank
            {
                blipPos = new Vector3(149.4, -1040.6, 29.4),
                tellers = new List<Vector3>
                {
                    new Vector3(150.1, -1040.2, 29.4),
                    new Vector3(148.6, -1039.6, 29.4)
                }
            },
            new Bank
            {
                blipPos = new Vector3(-2962.6, 482.1, 15.7),
                tellers = new List<Vector3>
                {
                    new Vector3(-2963.1, 483.0, 15.7),
                    new Vector3(-2963.3, 481.3, 15.7)
                }
            },
            new Bank
            {
                blipPos = new Vector3(-351.4, -48.4, 49.0),
                tellers = new List<Vector3>
                {
                    new Vector3(-350.7, -49.2, 49.0),
                    new Vector3(-352.2, -48.5, 49.0)
                }
            },
            new Bank
            {
                blipPos = new Vector3(-1213.9, -330.2, 37.8),
                tellers = new List<Vector3>
                {
                    new Vector3(-1212.7, -330.4, 37.8),
                    new Vector3(-1214.2, -331.0, 37.8)
                }
            }
        };

        public Banks()
        {
            banks.ForEach(bank =>
            {
                NAPI.Blip.CreateBlip(374, bank.blipPos, 1.0f, 5, "Bank", 255, 1.0f, true, 0, 0);

                bank.tellers.ForEach(teller =>
                {
                    RaycastInteractionSystem.raycastPoints.Add(new RaycastInteraction
                    {
                        menuTitle = "Bank Teller",
                        raycastMenuItems = new List<string> { "Open bank menu" },
                        raycastMenuPosition = teller,
                        targetMethod = openBankEvent
                    });
                });
            });


            Main.resourceStart += () => ChatUtils.startupPrint($"A total of {banks.Count} banks were loaded in.");
        }

        #region Global Methods
        public static bool closeToBankTeller(Player player)
        {
            bool close = false;

            banks.ForEach(bank =>
            {
                Vector3 closeTeller = bank.tellers
                .Where(teller => player.checkIsWithinCoord(teller, 2f))
                .FirstOrDefault();

                if (closeTeller != null) close = true;
            });

            return close;
        }

        public void openBankEvent(Player player, string rayMenuOption)
        {
            if (!closeToBankTeller(player)) return;

            DbCharacter characterData = player.getPlayerCharacterData();

            if (characterData != null)
            {
                if(isBankOpen(player))
                {
                    sendAtmUIData(player, characterData);
                }
            }
        }

        public static void sendAtmUIData(Player player, DbCharacter character)
        {
            uiHandling.handleObjectUiMutation(player, MutationKeys.AtmData, new AtmUiData
            {
                isBank = closeToBankTeller(player),
                balanceMoney = character.money_amount,
                balanceCash = character.cash_amount,
                balanceSalary = character.salary_amount
            });

            uiHandling.pushRouterToClient(player, Browsers.Atm, true);
        }

        public static bool isBankOpen(Player player)
        {
            if (TimeSystem.hour > bankCloseHour - 1 || TimeSystem.hour < bankOpenHour)
            {
                uiHandling.sendPushNotifError(player, $"The bank is currently closed. Come back at {bankOpenHour}{(bankOpenHour > 12 ? "PM" : "AM")}", 5500);
                return false;
            }

            return true;
        }
        #endregion

        #region Remote Events
        [RemoteEvent("server:bankDepositCash")]
        public void bankDepositEvent(Player player, string amount)
        {
            DbCharacter characterData = player.getPlayerCharacterData();

            if (closeToBankTeller(player) && characterData != null && isBankOpen(player))
            {
                try
                {
                    int cashDepo = int.Parse(amount);

                    if (cashDepo < 0 || cashDepo > 200000)
                    {
                        uiHandling.sendPushNotifError(player, "Cash amount must be greater than zero and less than $200,000", 5600, true);
                        return;
                    }

                    if((characterData.cash_amount - cashDepo) < 0)
                    {
                        uiHandling.sendPushNotifError(player, "You don't have enough cash to deposit this amount", 6600, true);
                        return;
                    }

                    player.processCashPayment(cashDepo, "Bank Deposit");
                    uiHandling.sendNotification(player, $"~g~Deposited ${cashDepo.ToString("N0")}.", false, true, "Deposits cash.");

                    uiHandling.setLoadingState(player, false);
                    sendAtmUIData(player, characterData);
                }
                catch
                {
                    uiHandling.sendPushNotifError(player, "Enter a valid money amount", 6600, true);
                }
            }
            else
            {
                uiHandling.sendPushNotifError(player, "You must be in a bank to use this.", 5500, true);
            }
        }

        [RemoteEvent("server:bankTransferSomeone")]
        public void bankTransferEvent(Player player, string data)
        {
            DbCharacter characterData = player.getPlayerCharacterData();
            BankTransfer bankTransfer = JsonConvert.DeserializeObject<BankTransfer>(data);

            if (closeToBankTeller(player) && characterData != null && bankTransfer != null && isBankOpen(player))
            {
                try
                {
                    int transferAmount = int.Parse(bankTransfer.transferAmount);

                    if (transferAmount < 0 || transferAmount > 200000)
                    {
                        uiHandling.sendPushNotifError(player, "Transfer amount must be between $0 and $200,000", 6600, true);
                        return;
                    }

                    bool found = false;

                    if (bankTransfer.recieverName != null)
                    {
                        NAPI.Pools.GetAllPlayers().ForEach(p =>
                        {
                            DbCharacter targetCharData = p.getPlayerCharacterData();

                            if (targetCharData != null && targetCharData.character_name == bankTransfer.recieverName.Replace(" ", "_"))
                            {
                                found = true;

                                if (targetCharData.character_id == characterData.character_id)
                                {
                                    uiHandling.sendPushNotifError(player, "You cannot bank transfer money to yourself.", 6600, true);
                                    return;
                                }

                                if (!player.processPayment(transferAmount, "Transfer Charge"))
                                {
                                    uiHandling.sendPushNotifError(player, "You don't have enough money to send that amount.", 6600, true);
                                    return;
                                }

                                p.addPlayerMoney(transferAmount, "Transfer Transaction");

                                p.SendChatMessage(ChatUtils.info + $"You have just been bank transferred ${transferAmount.ToString("N0")}.");
                                CommandUtils.successSay(player, $"You successfully bank transferred {targetCharData.character_name} ${transferAmount.ToString("N0")}");
                                uiHandling.setLoadingState(player, false);
                                sendAtmUIData(player, characterData);
                            }
                        });
                    }

                    if (!found)
                    {
                        uiHandling.sendPushNotifError(player, "Player wasn't found.", 6600, true);
                    }
                }
                catch
                {
                    uiHandling.sendPushNotifError(player, "Enter a valid transfer amount.", 6600, true);
                }
            }
        }

        [RemoteEvent("server:bank:retrieveSalary")]
        public void retrievePlayerSalary(Player player)
        {
            if (!closeToBankTeller(player) || !isBankOpen(player)) return;

            DbCharacter character = player.getPlayerCharacterData();

            if (character == null || character != null && character.salary_amount == 0) return;

            player.addPlayerMoney((int)character.salary_amount, "Salary Retrieval");

            CommandUtils.successSay(player, $"You have successfully deposited {ChatUtils.moneyGreen}${character.salary_amount.ToString("N0")}{ChatUtils.White} from your salary into your bank account.");

            character.salary_amount = 0;
            player.setPlayerCharacterData(character, false, true);

            uiHandling.setLoadingState(player, false);
            sendAtmUIData(player, character);
        }
        #endregion
    }
}
