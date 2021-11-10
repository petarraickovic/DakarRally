using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DakarRally.DataAccess.Entities
{
    public class Race
    {
        public long ID { get; set; }
        public long Year { get; set; }
        public int Status { get; set; }

        public RaceStatus RaceStatus { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; }


    }
}
