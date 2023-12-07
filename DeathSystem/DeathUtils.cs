﻿using System;
using System.Collections.Generic;
using System.Text;
using CloudRP.Character;
using GTANetworkAPI;

namespace CloudRP.DeathSystem
{
    class Hospital
    {
        public Vector3 position;
        public string name;
    }

    class Corpse
    {
        public int remoteId;
        public string characterName;
        public CharacterModel model;
        public int? corpseId;
        public Vector3 position;
    }
}
