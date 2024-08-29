using System.Collections.Generic;

namespace Parking
{
    /// <summary>
    /// Class to define a dictionary of the signage resources.
    /// </summary>
    public static class SignageResources
    {
        /// <summary>
        /// The signage resources dictionary.
        /// </summary>
        public static readonly Dictionary<ParkingType, string> ResourceMap = new Dictionary<ParkingType, string>
        {
            { ParkingType.EV, "Feasibility.Parking.Symbols.evParkingSymbol.json" },
            { ParkingType.PWD, "Feasibility.Parking.Symbols.pwdParkingSymbol.json" },
            { ParkingType.Standard, "Feasibility.Parking.Symbols.standardParkingSymbol.json" },
            { ParkingType.Car, "Feasibility.Parking.Symbols.carParkingSymbol.json" },
            { ParkingType.Bicycle, "Feasibility.Parking.Symbols.bicycleParkingSymbol.json" },
            { ParkingType.Motorbike, "Feasibility.Parking.Symbols.motorbikeParkingSymbol.json" },
            { ParkingType.NoParking, "Feasibility.Parking.Symbols.noParkingSymbol.json" }
        };
    }
}
