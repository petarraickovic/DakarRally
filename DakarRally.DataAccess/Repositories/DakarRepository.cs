using DakarRally.DataAccess.Context;
using DakarRally.DataAccess.Entities;
using DakarRally.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceStatusEnum = DakarRally.Domain.Enums.RaceStatus;
using VehicleModelEnum = DakarRally.Domain.Enums.VehicleModel;
using VehicleTypeEnum = DakarRally.Domain.Enums.VehicleType;

namespace DakarRally.DataAccess.Repositories
{
    public class DakarRepository : IDakarRepository
    {
        private DakarContext _dakarContext;
        public DakarRepository(DakarContext dakarContext)
        {
            _dakarContext = dakarContext;
        }

        public void CreateRace(Race race)
        {
            _dakarContext.Races.Add(race);
            _dakarContext.SaveChanges();


        }


        public void Updaterace(Race race)
        {
            _dakarContext.Races.Update(race);
            _dakarContext.SaveChanges();


        }
        public Race GetRaceByID(long raceID)
        {
            Race race = _dakarContext.Races.Where(x => x.ID == raceID)
                .Include(x => x.RaceStatus) 
                .Include(x => x.Vehicles)
                .ThenInclude(x => x.VehicleModel)
                .FirstOrDefault();
            return race;


        }

        public bool IsRaceForTheYearDefined(int year)
        {
            int raceNumber = _dakarContext.Races.Where(x => x.Year == year).Count();
            if(raceNumber > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public VehicleModel GetModelIdByString(string model)
        {
            VehicleModel result = _dakarContext.VehicleModels.Where(x => x.Model == model).FirstOrDefault();
            return result;
        }
        public VehicleType GetTypeIdByString(string type)
        {
            VehicleType result = _dakarContext.VehicleTypes.Where(x => x.Type == type).FirstOrDefault();
            return result;
        }


        public Vehicle GetVehicleByID(long vehicleID)
        {
            Vehicle result = _dakarContext.Vehicles.Where(x => x.ID == vehicleID).FirstOrDefault();
            return result;
        }

        public void UpdateVehicle(Vehicle vehicleToUpdate)
        {
            _dakarContext.Vehicles.Update(vehicleToUpdate);
            _dakarContext.SaveChanges();
        }

        public void DeleteVehicle(Vehicle vehicle)
        {
            _dakarContext.Vehicles.Remove(vehicle);
            _dakarContext.SaveChanges();
        }

        public void AddVehicleToDB(Vehicle vehicle)
        {
            _dakarContext.Vehicles.Add(vehicle);
            _dakarContext.SaveChanges();
        }


        public int GetNoRacesByStatus(int statusID)
        {
            int racesNmbr = _dakarContext.Races.Where(x => x.Status == statusID).Count();
            return racesNmbr;
        }

        public Race GetRaceByStatus(int statusID)
        {
            Race race = _dakarContext.Races.Where(x => x.Status == statusID).FirstOrDefault();
            return race;
        }

        public List<Vehicle> GetVehiclesByRaceID(long raceID, int? model = null)
        {
            var query = _dakarContext.Vehicles.Where(x => x.RaceID == raceID).Include(x => x.VehicleModel)
                .Include(x => x.VehicleType).AsQueryable();

            if(model != null)
            {
                query = query.Where(x => x.Model == model);
            }

            List<Vehicle> vehicles = query.ToList();
            return vehicles;
        }

        public List<Vehicle> GetVehiclesByParams(string team = null, long? model = null, 
            DateTime? manufacturingDate = null, string status = null, long? distance = null)
        {
            var query = _dakarContext.Vehicles
                .Include(x => x.VehicleModel) 
                .Include(x => x.VehicleType)
                .AsQueryable();
            if(team != null)
            {
                query = query.Where(x => x.TeamName == team);
            }
            if (model != null)
            {
                query = query.Where(x => x.Model == model);
            }
            if (manufacturingDate != null)
            {
                query = query.Where(x => x.ManufacturingDate == manufacturingDate);
            }
            if (status != null)
            {
                query = query.Where(x => x.Status == status);
            }
            if (distance != null)
            {
                query = query.Where(x => x.Distance == distance);
            }
            List<Vehicle> vehicles = query.ToList();
            return vehicles;
        }

        public void SetupDB()
        {
            _dakarContext.RaceStatuses.Add(new DataAccess.Entities.RaceStatus { ID = (int)RaceStatusEnum.Pending ,Status = "Pending" });
            _dakarContext.RaceStatuses.Add(new DataAccess.Entities.RaceStatus { ID = (int)RaceStatusEnum.Running, Status = "Running" });
            _dakarContext.RaceStatuses.Add(new DataAccess.Entities.RaceStatus { ID = (int)RaceStatusEnum.Finished, Status = "Finished" });
            _dakarContext.SaveChanges();
            _dakarContext.VehicleModels.Add(new DataAccess.Entities.VehicleModel { ID = (int)VehicleModelEnum.Truck, Model = "Truck" });
            _dakarContext.VehicleModels.Add(new DataAccess.Entities.VehicleModel { ID = (int)VehicleModelEnum.Car, Model = "Car" });
            _dakarContext.VehicleModels.Add(new DataAccess.Entities.VehicleModel { ID = (int)VehicleModelEnum.Motorcycle, Model = "Motorcycle" });
            _dakarContext.SaveChanges();

            _dakarContext.VehicleTypes.Add(new VehicleType { ID = (int)VehicleTypeEnum.Truck, Type = "Truck" });
            _dakarContext.VehicleTypes.Add(new VehicleType { ID = (int)VehicleTypeEnum.Sport, Type = "Sport" });
            _dakarContext.VehicleTypes.Add(new VehicleType { ID = (int)VehicleTypeEnum.Terrain, Type = "Terrain" });
            _dakarContext.VehicleTypes.Add(new VehicleType { ID = (int)VehicleTypeEnum.Cross, Type = "Cross" });
            _dakarContext.SaveChanges();
        }


    }
}
