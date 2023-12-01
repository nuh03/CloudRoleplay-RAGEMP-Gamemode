﻿using CloudRP.Authentication;
using CloudRP.Database;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using static CloudRP.Authentication.Account;

namespace CloudRP
{
    public class Main : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void Start()
        {
            Console.WriteLine("Gamemode started");


            Environment.SetEnvironmentVariable(Auth._emailUserEnv, Env._gmailSmtpUser);
            Environment.SetEnvironmentVariable(Auth._emailPassEnv, Env._gmailSmtpPass);
            Environment.SetEnvironmentVariable(DiscordSystem.DiscordSystems.tokenIdentifier, Env._discordToken);
            Environment.SetEnvironmentVariable(DefaultDbContext.connectionStringKey, Env._databaseConnectionString);
            Environment.SetEnvironmentVariable(DiscordSystem.DiscordSystems.staffChannelIdentifer, Env._discordStaffChannel);
        }

    }
}
