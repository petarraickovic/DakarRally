namespace DakarRally.Domain.Enums
{

    public enum RaceStatus : int
    {

        Pending = 1,
        Running = 2,
        Finished = 3
    }

    public enum VehicleModel : int
    {

        Truck = 1,
        Car = 2,
        Motorcycle = 3
    }

    public enum VehicleType : int
    {

        Truck = 1,
        Sport = 2,
        Terrain = 3,
        Cross = 4
    }


    public enum VehicleStatus : int
    {

        Pending = 1,
        Running = 2,
        LightMulfunction = 3,
        HeavyMulfunction = 4,
        FinishedRace = 5
    }
}
