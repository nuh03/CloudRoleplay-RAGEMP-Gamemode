﻿using CloudRP.ServerSystems.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CloudRP.VehicleSystems.Vehicles
{
    public class VehicleKey : BaseEntity
    {
        [Key]
        public int vehicle_key_id { get; set; }

        [Required]
        public int vehicle_id { get; set; }
        public string nickname { get; set; }
        public int target_character_id { get; set; }
        public string vehicle_name { get; set; }
    }
}
