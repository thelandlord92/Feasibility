using Autodesk.DesignScript.Geometry;
using Parking.Accessories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Accessories
{
    internal delegate BicycleRack BicycleRackMethod();
    internal delegate ChargingStation ChargingStationMethod();
    internal delegate WheelStop WheelStopMethod();

    internal class Accessories
    {
        private readonly BicycleRackMethod bicycleRackMethod;
    }
}
