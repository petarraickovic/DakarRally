using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DakarRally.DataAccess.RaceSimulator
{
    public class RaceActions : IRaceActions
    {
        public void CreateRaceAndStartThread(long raceID)
        {
            RaceStarter raceStarter = new(raceID);
            Thread runRaceThread = new(raceStarter.RunRace);
            runRaceThread.Start();
        }



    }
}
