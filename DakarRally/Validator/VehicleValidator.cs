using DakarRally.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DakarRally.Validator
{
    public class VehicleValidator : AbstractValidator<VehicleDto>
    {
        private List<string> carTypes = new() { "Sport", "Terrain" };
        private List<string> motorcycleTypes = new() { "Sport", "Cross" };
        public VehicleValidator()
        {
            RuleFor(x => x.TeamName).NotEmpty();
            RuleFor(x => x.VehicleModel).NotEmpty();
            RuleFor(x => x.RaceID).NotEmpty();
            When(x => x.VehicleModel == "Motorcycle", () =>
           {
               RuleFor(x => motorcycleTypes.Contains(x.VehicleType)).Equal(true).WithMessage("Bad type");
           });
            When(x => x.VehicleModel == "Car", () =>
            {
                RuleFor(x => carTypes.Contains(x.VehicleType)).Equal(true).WithMessage("Bad type");
            });
            When(x => x.VehicleModel == "Truck", () =>
            {
                RuleFor(x => string.IsNullOrWhiteSpace(x.VehicleType)).Equal(true).WithMessage("Bad type");
            });
        }



    }
}
