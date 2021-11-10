using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DakarRally.Models
{
    public class VehicleStatistics
    {
        public long Distance { get; set; }
        public string Status { get; set; }
        public DateTime? FinishTime { get; set; }
        public double MalfunctionStatistics { get; set; }
    }
}
