using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DakarRally.DataAccess.Entities
{
    public class VehicleModel
    {
        public int ID { get; set; }
        public string Model { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; }

    }
}
