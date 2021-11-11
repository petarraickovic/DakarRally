using AutoMapper;
using DakarRally.DataAccess.Entities;
using DakarRally.DataAccess.Interfaces;
using DakarRally.DataAccess.RaceSimulator;
using DakarRally.DataAccess.Repositories;
using DakarRally.Models;
using DakarRally.Models.ErrorHandler;
using DakarRally.Validator;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RaceStatusEnum = DakarRally.Domain.Enums.RaceStatus;
using VehicleStatusEnum = DakarRally.Domain.Enums.VehicleStatus;

namespace DakarRally.Controllers
{
    [ApiController]
    [Route("apis")]
    public class APIs : ControllerBase
    {

        private readonly IDakarRepository _dakarRepo;
        private readonly IMapper _mapper;
        private readonly VehicleValidator _vehicleValidator;
        private readonly IRaceActions _raceActions;

        public APIs(IDakarRepository dakarRepo, IMapper mapper, IRaceActions raceActions)
        {
            _dakarRepo = dakarRepo;
            _mapper = mapper;
            _vehicleValidator = new VehicleValidator();
            _raceActions = raceActions;

        }



        [HttpPost]
        [Route("createrace/{year}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(long))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        public IActionResult CreateRace([FromRoute] int year)
        {
            try
            {
                int yearNmbr = ValidateYearAndConvert(year);
                Race race = new()
                {
                    Year = yearNmbr,
                    Status = (int)RaceStatusEnum.Pending
                };
                _dakarRepo.CreateRace(race);
                return new OkObjectResult(race.ID);
            }
            catch (Exception ee)
            {
                return new Error().ProcessExceptionAndReturnResponse(ee);
            }
            
        }






        [HttpPost]
        [Route("addvehicletorace")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(long))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        public IActionResult AddVehicleToRace([FromBody] VehicleDto vehicleDto)
        {
            try
            {
                ValidationResult results = _vehicleValidator.Validate(vehicleDto);
                if (!results.IsValid)
                {
                    return new Error().CreateBadRequestError("Bad vehicle definition").ReturnHttpResponse();
                }
                Vehicle vehicle = GenerateVehicleObject(vehicleDto);
                vehicle.ID = 0;
                vehicle.Status = (int)VehicleStatusEnum.Pending;
                _dakarRepo.AddVehicleToDB(vehicle);
                return new OkObjectResult(vehicle.ID);
            }
            catch (Exception ee)
            {
                return new Error().ProcessExceptionAndReturnResponse(ee);
            }

        }

        [HttpPatch]
        [Route("updatevehicleinfo")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        public IActionResult UpdatevehicleInfo([FromBody] VehicleDto vehicleDto)
        {
            try
            {
                ValidationResult results = _vehicleValidator.Validate(vehicleDto);
                if (!results.IsValid)
                {
                    return new Error().CreateBadRequestError("Bad vehicle definition").ReturnHttpResponse();
                }
                Vehicle vehicleToUpdate = GetVehicleFromDB(vehicleDto.ID);
                Vehicle vehicleWithNewValues = GenerateVehicleObject(vehicleDto);
                UpdateVehicleWithValues(vehicleToUpdate, vehicleWithNewValues);
                _dakarRepo.UpdateVehicle(vehicleToUpdate); 
                return new OkObjectResult("Sucessfully updated");
            }
            catch (Exception ee)
            {
                return new Error().ProcessExceptionAndReturnResponse(ee);
            }

        }


        [HttpDelete]
        [Route("removevehiclefromrace/{vehicleID}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        public IActionResult RemoveVehicleFromRace([FromRoute] long vehicleID)
        {
            try
            {
                Vehicle vehicleToRemove = GetVehicleFromDB(vehicleID);
                _dakarRepo.DeleteVehicle(vehicleToRemove);
                return new OkObjectResult("Sucessfully deleted");
            }
            catch (Exception ee)
            {
                return new Error().ProcessExceptionAndReturnResponse(ee);
            }

        }



        [HttpGet]
        [Route("startrace/{raceID}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        public IActionResult StartRace([FromRoute] long raceID)
        {
            try
            {
                Race race = ValidateActionAndReturnRace(raceID);
                race.Status = (int)RaceStatusEnum.Running;
                _dakarRepo.Updaterace(race);
                _raceActions.CreateRaceAndStartThread(raceID);
                return new OkObjectResult("Sucessfully started");
            }
            catch (Exception ee)
            {
                return new Error().ProcessExceptionAndReturnResponse(ee);
            }
        }


        [HttpGet]
        [Route("getallvehiclesleatherboard")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(List<VehicleDtoWithDetails>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        public IActionResult GetVehiclesLeaderboard()
        {
            try
            {
                return GetVehiclesForRaceThatIsRuningSortedDescByDistance();
            }
            catch (Exception ee)
            {
                return new Error().ProcessExceptionAndReturnResponse(ee);
            }

        }


        [HttpGet]
        [Route("getallvehiclesleatherboardbytype/{model}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(List<VehicleDtoWithDetails>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        public IActionResult GetVehiclesLeaderboardByType([FromRoute] string model)
        {
            try
            {
                return GetVehiclesForRaceThatIsRuningSortedDescByDistance(model);
            }
            catch (Exception ee)
            {
                return new Error().ProcessExceptionAndReturnResponse(ee);
            }

        }

        [HttpGet]
        [Route("getvehiclestatistics/{vehicleID}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(VehicleStatistics))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        public IActionResult GetVehicleStatistics([FromRoute] long vehicleID)
        {
            try
            {
                Vehicle vehicle = GetVehicleFromDB(vehicleID);
                VehicleStatistics vehicleStatistics = _mapper.Map<VehicleStatistics>(vehicle);
                return new OkObjectResult(vehicleStatistics);
            }
            catch (Exception ee)
            {
                return new Error().ProcessExceptionAndReturnResponse(ee);
            }

        }

        [HttpGet]
        [Route("findvehicles")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(List<VehicleDtoWithDetails>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        public IActionResult FindVehicles([FromQuery] string team, [FromQuery] string model, 
            [FromQuery] DateTime? manufacturingDate, [FromQuery] string status, [FromQuery] long? distance, [FromQuery] string sortOrder)
        {
            try
            {
                int? vehicleModel = null;
                string localTeam = null;
                DateTime? localMD = null;
                int? localStatus = null;
                long? localDistance = null;
                if (!string.IsNullOrWhiteSpace(model))
                {
                    VehicleModel vm = _dakarRepo.GetModelIdByString(model);
                    if(vm == null)
                    {
                        return new Error().CreateBadRequestError("Model parameter is invalid").ReturnHttpResponse();
                    }
                    vehicleModel = vm.ID;
                }
                if(!string.IsNullOrWhiteSpace(team))
                {
                    localTeam = team;
                }
               
                if(manufacturingDate != null)
                {
                    localMD = manufacturingDate;
                }
                if (!string.IsNullOrWhiteSpace(status))
                {
                    VehicleStatus vehicleStatus = _dakarRepo.GetVehicleStatusByString(status);
                    if(vehicleStatus == null)
                    {
                        return new Error().CreateBadRequestError("Status parameter is invalid").ReturnHttpResponse();
                    }
                    localStatus = vehicleStatus.ID;
                }
                if(distance != null)
                {
                    if(string.IsNullOrWhiteSpace(sortOrder))
                    {
                        return new Error().CreateBadRequestError("Sort order must be populated when distance is sent").ReturnHttpResponse();
                    }
                    localDistance = distance;
                }
                if(distance == null && sortOrder != null)
                {
                    return new Error().CreateBadRequestError("Sort order mustn't be populated when distance isn't sent").ReturnHttpResponse();
                }
                List<Vehicle> vehicles = _dakarRepo.GetVehiclesByParams(localTeam, vehicleModel, localMD, localStatus, localDistance);
                if(sortOrder != null && sortOrder.ToLowerInvariant() == "asc")
                {
                    vehicles = vehicles.OrderBy(x => x.Distance).ToList();
                }
                else if(sortOrder != null && sortOrder.ToLowerInvariant() == "desc")
                {
                    vehicles = vehicles.OrderByDescending(x => x.Distance).ToList();
                }
                else if(sortOrder != null)
                {
                    return new Error().CreateBadRequestError("Sort order is not valid").ReturnHttpResponse();
                }
                List<VehicleDtoWithDetails> vehicleDtos = _mapper.Map<List<VehicleDtoWithDetails>>(vehicles);
                return new OkObjectResult(vehicleDtos);
            }
            catch (Exception ee)
            {
                return new Error().ProcessExceptionAndReturnResponse(ee);
            }

        }



        [HttpGet]
        [Route("getracestatus/{raceID}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(RaceStatusResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        public IActionResult GetRaceStatus([FromRoute] long raceID)
        {

            try
            {
                Race race = _dakarRepo.GetRaceByID(raceID);
                if(race == null)
                {
                    return new Error().CreateBadRequestError("There is no race with the given ID").ReturnHttpResponse();
                }
                RaceStatusResponse response = _mapper.Map<RaceStatusResponse>(race);
                return new OkObjectResult(response);
            }
            catch(Exception ee)
            {
                return new Error().ProcessExceptionAndReturnResponse(ee);
            }
        }







        #region Private methods

        private OkObjectResult GetVehiclesForRaceThatIsRuningSortedDescByDistance(string model = null)
        {
            Race race = _dakarRepo.GetRaceByStatus((int)RaceStatusEnum.Running);
            if (race == null)
            {
               new Error().CreateBadRequestError("There is no race that is running in the moment").ThrowException();
            }
            List<Vehicle> vehicles;
            if (!string.IsNullOrWhiteSpace(model))
            {
                var vehicleModel = _dakarRepo.GetModelIdByString(model);
                if(vehicleModel == null)
                {
                    new Error().CreateBadRequestError("The type parameter is wrong").ThrowException();
                }
                vehicles = _dakarRepo.GetVehiclesByRaceID(race.ID, vehicleModel.ID);
            }
            else
            {
                vehicles = _dakarRepo.GetVehiclesByRaceID(race.ID);
            }
            List<VehicleDtoWithDetails> vehicleDtos = _mapper.Map<List<VehicleDtoWithDetails>>(vehicles);
            return new OkObjectResult(vehicleDtos.OrderByDescending(x => x.Distance));
        }


        private Race ValidateActionAndReturnRace(long raceID)
        {
            int numberOfRacesRunning = _dakarRepo.GetNoRacesByStatus((int)RaceStatusEnum.Running);
            if (numberOfRacesRunning > 0)
            {
                new Error().CreateBadRequestError("There is already a race running").ThrowException();
            }
            Race race = _dakarRepo.GetRaceByID(raceID);
            if (race == null || race.Status != (int)RaceStatusEnum.Pending)
            {
                new Error().CreateBadRequestError("There is no race that can be started").ThrowException();
            }
            if(race.Vehicles.Count == 0)
            {
                new Error().CreateBadRequestError("Selected race doesn't have any vehicles").ThrowException();
            }
            return race;
        }

        private Vehicle GenerateVehicleObject(VehicleDto vehicleDto)
        {
            ValidationResult results = _vehicleValidator.Validate(vehicleDto);
            if (!results.IsValid)
            {
                new Error().CreateBadRequestError("Bad vehicle definition").ThrowException();
            }
            Race race = _dakarRepo.GetRaceByID(vehicleDto.RaceID);
            if(race == null || race.Status != (int)RaceStatusEnum.Pending)
            {
                new Error().CreateBadRequestError("Specified race isn't valid").ThrowException();
            }
            Vehicle result = _mapper.Map<Vehicle>(vehicleDto);
            VehicleModel vm = _dakarRepo.GetModelIdByString(vehicleDto.VehicleModel);
            if(vm == null)
            {
                new Error().CreateBadRequestError("Specified vehicle model doesn't exist").ThrowException();
            }
            result.Model = vm.ID;
            VehicleType vt;
            if (vehicleDto.VehicleModel != "Truck")
            {
               vt = _dakarRepo.GetTypeIdByString(vehicleDto.VehicleType);
            }
            else
            {
                vt = _dakarRepo.GetTypeIdByString("Truck");
            }
            if(vt == null)
            {
                new Error().CreateBadRequestError("Specified vehicle type doesn't exist").ThrowException();
            }
            result.Type = vt.ID;
            return result;
        }

       

        private Vehicle GetVehicleFromDB(long vehicleID)
        {
            Vehicle result = _dakarRepo.GetVehicleByID(vehicleID);
            if(result == null)
            {
                new Error().CreateBadRequestError("Specified vehicle doesn't exist").ThrowException();
            }
            return result;
        }

        private void UpdateVehicleWithValues(Vehicle vehicleToUpdate, Vehicle vehicleWithNewValues)
        {
            vehicleToUpdate.ManufacturingDate = vehicleWithNewValues.ManufacturingDate;
            vehicleToUpdate.TeamName = vehicleWithNewValues.TeamName;
            vehicleToUpdate.Model = vehicleWithNewValues.Model;
            vehicleToUpdate.Type = vehicleWithNewValues.Type;
        }

        private int ValidateYearAndConvert(int year)
        {
            if (year < 1000 || year > 3000)
            {
                new Error().CreateBadRequestError("Year parameter is not in a good form.").ThrowException();
            }
            if (_dakarRepo.IsRaceForTheYearDefined(year))
            {
                new Error().CreateBadRequestError("Race for the selected year is already defined.").ThrowException();
            }
            
            return year;
        }


        #endregion

    }
}
