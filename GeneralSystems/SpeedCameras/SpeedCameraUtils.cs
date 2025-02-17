﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudRP.GeneralSystems.SpeedCameras
{
    public class SpeedCamera
    {
        public Vector3 position { get; set; }
        public Vector3 camPropPos { get; set; }
        public Vector3 camFlashPos { get; set; }
        public float range { get; set; }
        public double camRot { get; set; }
        public int speedLimit { get; set; }
        public bool sabotaged { get; set; }
        public string camName { get; set; }
    }

    class SpeedFine
    {
        public int speed { get; set; }
        public int finePrice { get; set; }
        public int chargeId { get; set; }
    }
}
