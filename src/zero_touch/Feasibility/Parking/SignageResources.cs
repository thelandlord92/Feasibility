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
        public static readonly Dictionary<SignageType, string> ResourceMap = new Dictionary<SignageType, string>
        {
            { SignageType.EV, "Feasibility.Parking.Symbols.evParkingSymbol.json" },
            { SignageType.PWD, "Feasibility.Parking.Symbols.pwdParkingSymbol.json" },
            { SignageType.Standard, "Feasibility.Parking.Symbols.standardParkingSymbol.json" },
            { SignageType.Car, "Feasibility.Parking.Symbols.carParkingSymbol.json" },
            { SignageType.Bicycle, "Feasibility.Parking.Symbols.bicycleParkingSymbol.json" },
            { SignageType.Motorbike, "Feasibility.Parking.Symbols.motorbikeParkingSymbol.json" },
            { SignageType.NoParking, "Feasibility.Parking.Symbols.noParkingSymbol.json" }
        };
    }
}
