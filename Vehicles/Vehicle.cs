﻿using CloudRP.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Numerics;

namespace CloudRP.Vehicles
{
    public class DbVehicle : BaseEntity
    {
        [Key]
        public int vehicle_id { get; set; }

        [Required]
        public int owner_id { get; set; }

        [Required]
        public string vehicle_name { get; set; }

        public bool vehicle_locked { get; set; }

        [Required]
        public uint vehicle_spawn_hash { get; set; }

        [Required]
        public string numberplate { get; set; }

        [Required]
        public float position_x { get; set; }

        [Required]
        public float position_y { get; set; }

        [Required]
        public float position_z { get; set; }

        [Required]
        public float rotation { get; set;}

        public string vehicle_dimension { get; set; }

        public int vehicle_insurance_id { get; set; }

        public int vehicle_garage_id { get; set; }

        [NotMapped]
        public bool[] vehicle_doors { get; set; }

        [NotMapped]
        public bool[] vehicle_windows { get; set; }

    }

    class VehicleDimensions
    {
        public static readonly string World = "world";
        public static readonly string Insurance = "insurance";
        public static readonly string Garage = "Garage";
        public static readonly Vector3 morsPosition = new Vector3(-862, -265, 40);
        public static readonly int _morsId = 0;
    }
}
