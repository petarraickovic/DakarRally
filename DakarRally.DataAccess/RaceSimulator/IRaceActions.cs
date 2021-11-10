using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DakarRally.DataAccess.RaceSimulator
{
    public interface IRaceActions
    {
        void CreateRaceAndStartThread(long raceID);
    }
}
