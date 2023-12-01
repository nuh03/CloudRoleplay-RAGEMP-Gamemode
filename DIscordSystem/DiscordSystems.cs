﻿using CloudRP.AntiCheat;
using CloudRP.Authentication;
using CloudRP.Character;
using CloudRP.Database;
using CloudRP.Migrations;
using CloudRP.Utils;
using CloudRP.Vehicles;
using Discord;
using Discord.WebSocket;
using GTANetworkAPI;
using Integration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static System.Collections.Specialized.BitVector32;

namespace CloudRP.DiscordSystem
{
    class DiscordSystems : Script
    {
        public static List<Command> commands = new List<Command>();

        public static Timer updatePlayerCountTimer;
        public static string tokenIdentifier = "discordToken";
        public static string discordPrefix = "!";
        public static ulong staffChannel = 1112793951406137415;
        public static int _updatePlayerCount = 5000;
        public static int _maxPlayers = 200;

        [ServerEvent(Event.ResourceStart)]
        public async Task OnResourceStart()
        {
            string token = Environment.GetEnvironmentVariable(tokenIdentifier);

            if(token == null)
            {
                Console.WriteLine("Discord Token was not found.");
                return;
            }

            DiscordIntegration.SetUpBotInstance(token, "Starting...", ActivityType.Playing, UserStatus.Online);

            NAPI.Task.Run(() =>
            {
                DiscordIntegration.RegisterChannelForListenting(staffChannel);
            }, 5000);


            NAPI.Task.Run(() =>
            {
                updatePlayerCountTimer = new Timer();
                updatePlayerCountTimer.Interval = _updatePlayerCount;
                updatePlayerCountTimer.Elapsed += updatePlayerCount;

                updatePlayerCountTimer.AutoReset = true;
                updatePlayerCountTimer.Enabled = true;
            });
        }

        public static void updatePlayerCount(object source, ElapsedEventArgs e)
        {
            List<Player> onlinePlayers = NAPI.Pools.GetAllPlayers();

            string status = "with " + onlinePlayers.Count + "/" + _maxPlayers + " players.";
            DiscordIntegration.UpdateStatus(status, ActivityType.Playing, UserStatus.Online);

        }

        public static void handleDiscordCommand(string[] args, SocketUser user)
        {
            commands.Clear();

            addCommmand(() => say(args, user), "to say message to all online players", "say");
            addCommmand(() => vinfo(args, user), "to view info about a vehicle", "vinfo");
            addCommmand(() => kickPlayer(args, user), "to kick a player from the server", "kickplayer");
            addCommmand(() => helpCommand(args, user), "to view all available commands", "help");
            
            foreach (Command command in commands)
            {
                if (command.name == args[0])
                {
                    command.action.Invoke();
                }
            }

        }

        private static void addCommmand(Action takeFunction, string desc = "N/A", string name = "N/A")
        {
            Command command = new Command
            {
                action = takeFunction,
                description = "A command " + desc + ".",
                name = name
            };

            commands.Add(command);

        }

        public static string getSplicedArgument(string[] args)
        {
            string message = string.Join(" ", args);
            message = message[(message.Split()[0].Length + 1)..];

            return message;
        }

        public static void kickPlayer(string[] args, SocketUser user)
        {
            if (args.Length < 2)
            {
                missingArgs("kickplayer", "nameOrId");
                return;
            }

            Player player = CommandUtils.getPlayerFromNameOrId(args[1]);

            if(player != null)
            {
                player.Kick();
                successEmbed("Kicked player [" + player.Id + "]");
            } else
            {
                errorEmbed("This player wasn't found online.");
                return;
            }

        }

        public static void getCharacterFromUnix(string[] args)
        {
            if(args.Length < 3)
            {
                missingArgs("getfromunix", "nameOrId, unixTime");
                return;
            }

            using(DefaultDbContext dbContext = new DefaultDbContext())
            {
                //CharacterJoinLog log = new Joinlogs();

            }


        }

        public static void vinfo(string[] args, SocketUser user)
        {
            if(args.Length < 2)
            {
                missingArgs("vinfo", "vehicleId");
                return;
            }

            int? vehicleId = CommandUtils.tryParse(args[1]);

            using(DefaultDbContext dbContext = new DefaultDbContext())
            {
                DbVehicle vehicle = dbContext.vehicles.Find(vehicleId);

                if(vehicle != null)
                {
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Title = MentionUtils.MentionUser(user.Id) + "Vehicle Info",
                        Color = Discord.Color.DarkerGrey,
                        Description = "Vehicle info for vehicle #"+vehicle.vehicle_id
                    };

                    builder.AddField(x =>
                    {
                        x.Name = "Vehicle Dimension";
                        x.Value = vehicle.vehicle_dimension;
                        x.IsInline = false;
                    });
                    
                    builder.AddField(x =>
                    {
                        x.Name = "Last updated";
                        x.Value = vehicle.UpdatedDate;
                        x.IsInline = false;
                    });
                    
                    builder.AddField(x =>
                    {
                        x.Name = "Vehicle Name";
                        x.Value = vehicle.vehicle_name;
                        x.IsInline = false;
                    });

                    DbCharacter characterData = dbContext.characters.Find(vehicle.owner_id);

                    if(characterData != null)
                    {
                        builder.AddField(x =>
                        {
                            x.Name = "Owner";
                            x.Value = characterData.character_name;
                            x.IsInline = false;
                        });
                        
                        builder.AddField(x =>
                        {
                            x.Name = "Owner is banned";
                            x.Value = characterData.character_isbanned == 1 ? "Yes" : "No";
                            x.IsInline = false;
                        });
                    }

                    DiscordIntegration.SendEmbed(staffChannel, builder);

                } else
                {
                    errorEmbed("The specified vehicle couldn't be found.");
                }
            }
        }

        public static void banUser(string[] args, SocketUser user)
        {
            if(args.Length < 2)
            {
                missingArgs("banuser", "username");
                return;
            }

        }

        public static void helpCommand(string[] args, SocketUser user)
        {
            EmbedBuilder builder = new EmbedBuilder
            {
                Title = "Help Command",
                Color = Discord.Color.DarkerGrey,
                Description = "All commands"
            };

            foreach(Command command in commands)
            {
                builder.AddField(field =>
                {
                    field.Name = discordPrefix+command.name;
                    field.Value = command.description;
                    field.IsInline = false;
                });
            }

            DiscordIntegration.SendEmbed(staffChannel, builder);
        }

        public static void say(string[] args, SocketUser user)
        {
            if(args.Length < 2)
            {
                missingArgs("say", "message");
                return;
            }

            string message = ChatUtils.red + "[Discord] " + ChatUtils.White + user.Username + ChatUtils.red + " says: " + ChatUtils.White + getSplicedArgument(args);


            DiscordIntegration.SendMessage(staffChannel, MentionUtils.MentionUser(user.Id) + " sent message in game!", false);
            NAPI.Chat.SendChatMessageToAll(message);
        }

        public static async Task errorEmbed(string desc)
        {
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = Discord.Color.Red,
                Description = desc,
                Title = $"An error occured :("
            };

            await DiscordIntegration.SendEmbed(staffChannel, builder);
        }

        public static async Task successEmbed(string success)
        {
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = Discord.Color.Green,
                Description = success,
                Title = $"Success"
            };

            DiscordIntegration.SendEmbed(staffChannel, builder);
        }

        public static async Task missingArgs(string commandName, string missingArgs)
        {
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = Discord.Color.Red,
                Description = "Missing arguments " + "[" + missingArgs + "]",
                Title = $" Missing potential arguments in command {commandName} :("
            };

            await DiscordIntegration.SendEmbed(staffChannel, builder);
        }
    }

}
