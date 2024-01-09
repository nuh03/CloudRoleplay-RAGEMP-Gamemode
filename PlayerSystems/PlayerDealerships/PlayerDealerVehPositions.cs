﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudRP.PlayerSystems.PlayerDealerships
{
    public class PlayerDealerVehPositions
    {
        public static List<DealerVehPos> dealerVehPositions = new List<DealerVehPos>()
        {
            new DealerVehPos
            {
                ownerId = 0,
                vehPos = new Vector3(-40.5, -2101.2, 16.7),
                vehRot = -160.5
            }
        };

    }
}
