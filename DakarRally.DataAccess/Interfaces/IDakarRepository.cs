using DakarRally.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DakarRally.DataAccess.Interfaces
{
    public interface IDakarRepository
    {
        void CreateRace(Race race);
        Race GetRaceByID(long raceID);
        void SetupDB();
        bool IsRaceForTheYearDefined(int year);
        VehicleModel GetModelIdByString(string model);
        VehicleType GetTypeIdByString(string type);
        void AddVehicleToDB(Vehicle vehicle);
        Vehicle GetVehicleByID(long vehicleID);
        void UpdateVehicle(Vehicle vehicleToUpdate);
        void DeleteVehicle(Vehicle vehicle);
        int GetNoRacesByStatus(int statusID);
        void Updaterace(Race race);
        Race GetRaceByStatus(int statusID);
        List<Vehicle> GetVehiclesByRaceID(long raceID, int? model = null);
        List<Vehicle> GetVehiclesByParams(string team = null, long? model = null,
            DateTime? manufacturingDate = null, string status = null, long? distance = null);
    }
}
