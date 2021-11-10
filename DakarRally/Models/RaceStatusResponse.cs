using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DakarRally.Models
{
    public class RaceStatusResponse
    {
        public RaceStatusResponse()
        {
            VehicleStatuses = new List<VehicleStatusObject>();
            VehicleModels = new List<VehicleModelObject>();
        }
        public string RaceStatus { get; set; }
        public List<VehicleStatusObject> VehicleStatuses { get; set; }
        public List<VehicleModelObject> VehicleModels { get; set; }
    }
}
