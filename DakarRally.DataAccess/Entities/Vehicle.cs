using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DakarRally.DataAccess.Entities
{
    public class Vehicle
    {
        public long ID { get; set; }
        public string TeamName { get; set; }

        public int Model { get; set; }
        public long RaceID { get; set; }

        public DateTime ManufacturingDate { get; set; }

        public int Type { get; set; }
        public long MaxSpeed { get; set; }
        public int LightMalfunctionProbablity { get; set; }
        public int HeavyMalfunctionProbablity { get; set; }
        public int LightMalfunctionWaitTime { get; set; }
        public long Distance { get; set; }
        public string Status { get; set; }
        public DateTime? FinishTime { get; set; }
        public double MalfunctionStatistics { get; set; }


        public VehicleModel VehicleModel { get; set; }
        public Race Race { get; set; }
        public VehicleType VehicleType { get; set; }





    }
}
