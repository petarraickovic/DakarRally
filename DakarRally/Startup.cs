using AutoMapper;
using DakarRally.DataAccess.Context;
using DakarRally.DataAccess.Entities;
using DakarRally.DataAccess.Interfaces;
using DakarRally.DataAccess.RaceSimulator;
using DakarRally.DataAccess.Repositories;
using DakarRally.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleModelEnum = DakarRally.Domain.Enums.VehicleModel;
using VehicleStatusEnum = DakarRally.Domain.Enums.VehicleStatus;

namespace DakarRally
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DakarRally", Version = "v1" });
            });

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Race, RaceDto>()
                .ForMember(x => x.Status, opt => opt.MapFrom(y => y.RaceStatus.Status));
                cfg.CreateMap<Vehicle, VehicleDto>()
                .ForMember(x => x.VehicleModel, opt => opt.MapFrom(y => y.VehicleModel.Model))
                .ForMember(x => x.VehicleType, opt => opt.MapFrom(y => y.Model != (long)VehicleModelEnum.Truck ? y.VehicleType.Type : string.Empty))
                .ReverseMap()
                .ForMember(x => x.VehicleModel, opt => opt.Ignore())
                .ForMember(x => x.VehicleType, opt => opt.Ignore())
                .ForMember(x => x.Model, opt => opt.Ignore())
                .ForMember(x => x.Type, opt => opt.Ignore())
                .ForMember(x => x.MaxSpeed, opt => opt.Ignore())
                .ForMember(x => x.LightMalfunctionProbablity, opt => opt.Ignore())
                .ForMember(x => x.LightMalfunctionWaitTime, opt => opt.Ignore())
                .ForMember(x => x.HeavyMalfunctionProbablity, opt => opt.Ignore())
                .ForMember(x => x.Distance, opt => opt.Ignore())
                .ForMember(x => x.FinishTime, opt => opt.Ignore())
                .ForMember(x => x.Status, opt => opt.Ignore())
                .ForMember(x => x.MalfunctionStatistics, opt => opt.Ignore())
                .AfterMap((dto, entity, context) =>
                {
                    var result = CalculateMaxSpeedAndPropabilities(dto);
                    entity.MaxSpeed = result.Item1;
                    entity.LightMalfunctionProbablity = result.Item2;
                    entity.LightMalfunctionWaitTime = result.Item4;
                    entity.HeavyMalfunctionProbablity = result.Item3;

                });
                cfg.CreateMap<Vehicle, VehicleDtoWithDetails>()
                .ForMember(x => x.VehicleModel, opt => opt.MapFrom(y => y.VehicleModel.Model))
                .ForMember(x => x.Status, opt => opt.MapFrom(y => y.VehicleStatus.Status))
                .ForMember(x => x.VehicleType, opt => opt.MapFrom(y => y.Model != (long)VehicleModelEnum.Truck ? y.VehicleType.Type : string.Empty));
                cfg.CreateMap<Vehicle, VehicleStatistics>();
                cfg.CreateMap<Race, RaceStatusResponse>()
                .ForMember(x => x.RaceStatus, opt => opt.MapFrom(y => y.RaceStatus.Status))
                .AfterMap((entity, dto, context) =>
                {
                    PopulateVehicleModelObjs(dto, entity, "Truck", (int)VehicleModelEnum.Truck);
                    PopulateVehicleModelObjs(dto, entity, "Car", (int)VehicleModelEnum.Car);
                    PopulateVehicleModelObjs(dto, entity, "Motorcycle", (int)VehicleModelEnum.Motorcycle);
                    PopulateVehicleStatusObjs(dto, entity, "Pending", (int)VehicleStatusEnum.Pending);
                    PopulateVehicleStatusObjs(dto, entity, "Running", (int)VehicleStatusEnum.Running);
                    PopulateVehicleStatusObjs(dto, entity, "LightMalfunction", (int)VehicleStatusEnum.LightMulfunction);
                    PopulateVehicleStatusObjs(dto, entity, "HeavyMalfunction", (int)VehicleStatusEnum.HeavyMulfunction);
                    PopulateVehicleStatusObjs(dto, entity, "FinishedRace", (int)VehicleStatusEnum.FinishedRace);
                });
            });
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            
            services.AddTransient<IDakarRepository, DakarRepository>();
            services.AddTransient<IRaceActions, RaceActions>();
            services.AddDbContext<DakarContext>(
                options => options.UseInMemoryDatabase(databaseName: "DakarDB")
                );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DakarContext dakarContext )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DakarRally v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //Setting up some DB data, needed only because InMemory DB is used
            new DakarRepository(dakarContext).SetupDB();
        }

        private Tuple<int,int, int, int> CalculateMaxSpeedAndPropabilities(VehicleDto vehicle)
        {
            if(vehicle.VehicleModel == "Car" && vehicle.VehicleType == "Sport")
            {
                return new Tuple<int, int, int, int>(140,12,2, 5);
            }
            else if(vehicle.VehicleModel == "Car" && vehicle.VehicleType == "Terrain")
            {
                return new Tuple<int, int, int, int>(100, 3, 1, 5);
            }
            else if (vehicle.VehicleModel == "Truck")
            {
                return new Tuple<int, int, int, int>(80, 6, 4, 7);
            }
            else if (vehicle.VehicleModel == "Motorcycle" && vehicle.VehicleType == "Cross")
            {
                return new Tuple<int, int, int, int>(85, 3, 2, 3);
            }
            else
            {
                return new Tuple<int, int, int, int>(130, 18, 10, 3);
            }
        }

        private void PopulateVehicleModelObjs(RaceStatusResponse raceStatus, Race race, string model, int modelID)
        {
            VehicleModelObject vmObj = new()
            {
                VehicleModel = model,
                NumberOfVehicles = race.Vehicles.Where(x => x.Model == modelID).Count()
            };
            if (vmObj.NumberOfVehicles != 0)
            {
                raceStatus.VehicleModels.Add(vmObj);
            }
        }
        private void PopulateVehicleStatusObjs(RaceStatusResponse raceStatus, Race race, string status, int statusID)
        {
            VehicleStatusObject vmObj = new()
            {
                VehicleStatus = status,
                NumberOfVehicles = race.Vehicles.Where(x => x.Status == statusID).Count()
            };
            if (vmObj.NumberOfVehicles != 0)
            {
                raceStatus.VehicleStatuses.Add(vmObj);
            }
        }


    }
}
