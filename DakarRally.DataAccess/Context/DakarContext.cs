using DakarRally.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DakarRally.DataAccess.Context
{
    public class DakarContext : DbContext
    {

        public DakarContext(DbContextOptions<DakarContext> options) : base(options)
        {

        }
        public DbSet<Race> Races { get; set; }
        public DbSet<RaceStatus> RaceStatuses { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<VehicleStatus> VehicleStatuses { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Race>()
                .HasOne(x => x.RaceStatus)
                .WithMany(x => x.Races)
                .HasForeignKey(x => x.Status);

            modelBuilder.Entity<Vehicle>()
                .HasOne(x => x.VehicleModel)
                .WithMany(x => x.Vehicles)
                .HasForeignKey(x => x.Model);

            modelBuilder.Entity<Vehicle>()
                .HasOne(x => x.Race)
                .WithMany(x => x.Vehicles)
                .HasForeignKey(x => x.RaceID);

            modelBuilder.Entity<Vehicle>()
                .HasOne(x => x.VehicleType)
                .WithMany(x => x.Vehicles)
                .HasForeignKey(x => x.Type);

            modelBuilder.Entity<Vehicle>()
               .HasOne(x => x.VehicleStatus)
               .WithMany(x => x.Vehicles)
               .HasForeignKey(x => x.Status);

        }
    }
}
