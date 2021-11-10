using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DakarRally.Models
{
    public class VehicleDto
    {
        public long ID { get; set; }
        public string TeamName { get; set; }
        public string VehicleModel { get; set; }
        public long RaceID { get; set; }
        public DateTime ManufacturingDate { get; set; }
        public string VehicleType { get; set; }
    }
}
