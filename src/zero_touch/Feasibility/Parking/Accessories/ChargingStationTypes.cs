using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Accessories
{
    /// <summary>
    /// Defines enums for charging station types.
    /// Types are based on visual appearance.
    /// </summary>
    public enum ChargingStationTypes
    {
        /// <summary>
        /// Pole mounted charger type.
        /// </summary>
        PoleMounted,

        /// <summary>
        /// Box shaped charger type.
        /// </summary>
        BoxShaped,

        /// <summary>
        /// Wall mounted charger type.
        /// </summary>
        WallMounted,

        /// <summary>
        /// No charger required.
        /// </summary>
        NoCharger,
    }
}
