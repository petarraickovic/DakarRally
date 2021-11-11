using DakarRally.DataAccess.Context;
using DakarRally.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RaceStatusEnum = DakarRally.Domain.Enums.RaceStatus;
using VehicleStatusEnum = DakarRally.Domain.Enums.VehicleStatus;

namespace DakarRally.DataAccess.RaceSimulator
{
    public class RaceStarter
    {
        public long RaceID { get; set; }
        public DakarContext _dakarContext { get; set; }

        public long RaceLength = 10000;


        public RaceStarter(long ID)
        {
            RaceID = ID;
            _dakarContext = CreateDakarContext();
        }



        public void RunRace()
        {
            try
            {
                List<Vehicle> vehiclesInRace = _dakarContext.Vehicles.Where(x => x.RaceID == RaceID).ToList();
                List<Task> threads = new();
                foreach (var vehicle in vehiclesInRace)
                {

                    threads.Add(Task.Run(() => SimulateRaceForVehicle(vehicle)));
                }
                Task.WaitAll(threads.ToArray());
                Race raceToFinish = _dakarContext.Races.Where(x => x.ID == RaceID).FirstOrDefault();
                raceToFinish.Status = (int)RaceStatusEnum.Finished;
                _dakarContext.Races.Update(raceToFinish);
                _dakarContext.SaveChanges();
            }
            catch
            {
                FinishRaceIncaseOfAnException();
            }
            
        }



        private async Task SimulateRaceForVehicle(Vehicle vehicle)
        {
            DakarContext newInstanceOfContext = CreateDakarContext();
            DateTime raceStartTime = DateTime.UtcNow;
            long RaceFinishTime = (long)Math.Ceiling(RaceLength * 1.0 / vehicle.MaxSpeed);
            long i = 0;
            double numberOfLightMulfunctions = 0.0;
            long lightMalfunctionTimer = vehicle.LightMalfunctionWaitTime;
            vehicle.Status = (int)VehicleStatusEnum.Running;
            UpdateVehicle(vehicle, newInstanceOfContext);
            while (i < RaceFinishTime)
            {
                if (vehicle.Status != (int)VehicleStatusEnum.LightMulfunction)
                {
                    if (RaceLength - vehicle.Distance < vehicle.MaxSpeed)//vehicle.MaxSpeed*1h
                    {
                        vehicle.Distance = RaceLength;
                    }
                    else
                    {
                        vehicle.Distance += vehicle.MaxSpeed; // *1h
                    }
                    bool EasyMalfunction = CalculateMalfanction(vehicle.LightMalfunctionProbablity);
                    bool heavyMulfunction = CalculateMalfanction(vehicle.HeavyMalfunctionProbablity);
                    if (heavyMulfunction)
                    {
                        vehicle.FinishTime = raceStartTime.AddHours(i);
                        HandleHeavyMalfunction(vehicle, newInstanceOfContext, RaceFinishTime);
                        break;
                    }
                    if (EasyMalfunction)
                    {
                        numberOfLightMulfunctions += 1;
                        RaceFinishTime += vehicle.LightMalfunctionWaitTime;
                        vehicle.Status = (int)VehicleStatusEnum.LightMulfunction;
                    }
                    UpdateVehicle(vehicle, newInstanceOfContext);
                }
                else
                {
                    lightMalfunctionTimer -= 1;
                    if (lightMalfunctionTimer == 0)
                    {
                        vehicle.Status = (int)VehicleStatusEnum.Running;
                        UpdateVehicle(vehicle, newInstanceOfContext);
                        lightMalfunctionTimer = vehicle.LightMalfunctionWaitTime;
                    }
                }
                Thread.Sleep(1000);
                i++;
            }
            if (vehicle.Status != (int)VehicleStatusEnum.HeavyMulfunction)
            {
                vehicle.Status = (int)VehicleStatusEnum.FinishedRace;
                double timeToAdd = CalculateLengthOfRace(i, vehicle);
                vehicle.FinishTime = raceStartTime.AddHours(timeToAdd);
                var stat = vehicle.LightMalfunctionWaitTime * numberOfLightMulfunctions / timeToAdd;
                vehicle.MalfunctionStatistics = Math.Round(stat, 2);
                UpdateVehicle(vehicle, newInstanceOfContext);
            }
        }

        private bool CalculateMalfanction(double desiredProbability)
        {
            Random rnd = new();
            Double dbl = rnd.NextDouble();
            if (dbl* 100 < desiredProbability)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private DakarContext CreateDakarContext()
        {
            var contextOptions = new DbContextOptionsBuilder<DakarContext>()
                .UseInMemoryDatabase(databaseName: "DakarDB")
                .Options;
            DakarContext _dakarContext = new DakarContext(contextOptions);
            return _dakarContext;
        }

        private void FinishRaceIncaseOfAnException()
        {
            List<Vehicle> vehiclesInRace = _dakarContext.Vehicles.Where(x => x.RaceID == RaceID).ToList();
            vehiclesInRace.ForEach(x => x.Status = (int)VehicleStatusEnum.FinishedRace);
            _dakarContext.Vehicles.UpdateRange(vehiclesInRace);
            Race raceToFinish = _dakarContext.Races.Where(x => x.ID == RaceID).FirstOrDefault();
            raceToFinish.Status = (int)RaceStatusEnum.Finished;
            _dakarContext.Races.Update(raceToFinish);
            _dakarContext.SaveChanges();
        }


        private static void UpdateVehicle(Vehicle vehicle, DakarContext context)
        {
            context.Vehicles.Update(vehicle);
            context.SaveChanges();
        }

        private double CalculateLengthOfRace(long hourNmbr, Vehicle vehicle )
        {
            double notIntegerPart = RaceLength * 1.0 / vehicle.MaxSpeed - Math.Truncate(RaceLength * 1.0 / vehicle.MaxSpeed);
            if(notIntegerPart == 0.0)
            {
                return hourNmbr;
            }
            else
            {
                return hourNmbr - 1 + notIntegerPart;
            }
        }

        private void HandleHeavyMalfunction(Vehicle vehicle, DakarContext context, long RaceFinishTime)
        {
            vehicle.Status = (int)VehicleStatusEnum.HeavyMulfunction;
            var stat = (RaceFinishTime - vehicle.Distance / vehicle.MaxSpeed) * 1.0 / RaceFinishTime;
            vehicle.MalfunctionStatistics = Math.Round(stat, 2);
            UpdateVehicle(vehicle, context);
        }
    }
}
