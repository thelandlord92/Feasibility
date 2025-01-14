using Autodesk.DesignScript.Geometry;
using Parking.Accessories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Accessories
{
    /// <summary>
    /// Wrapper class for the parking accessories.
    /// Compiles all the required accessories for the parking bay.
    /// </summary>
    public class Accessories
    {
        /// <summary>
        /// The bicycle rack accessory input.
        /// </summary>
        internal BicycleRack BicycleRack { get; private set; }

        /// <summary>
        /// The wheelstop accessory input.
        /// </summary>
        internal WheelStop WheelStop { get; private set; }

        /// <summary>
        /// The charging station accessory input.
        /// </summary>
        internal ChargingStation ChargingStation { get; private set; }

        /// <summary>
        /// To group accessories to be assigned to the parking bay type.
        /// </summary>
        /// <param name="bicycleRackType">The bicycle rack type.</param>
        /// /// <param name="chargingStationType">The charging station type.</param>
        /// <param name="wheelStopType">The wheel stop type.</param>
        public Accessories(
            BicycleRackTypes bicycleRackType = BicycleRackTypes.NoRack,
            ChargingStationTypes chargingStationType = ChargingStationTypes.NoCharger,
            WheelStopTypes wheelStopType = WheelStopTypes.NoWheelStop)
        {
            // Based on the input enum, set the correct accessory type.
            switch (bicycleRackType) 
            {
                case BicycleRackTypes.InvertedURack:
                    BicycleRack = new BicycleRack();
                    BicycleRack.BicycleRackType = BicycleRackTypes.InvertedURack;
                    break;
                case BicycleRackTypes.WaveRack:
                    BicycleRack = new BicycleRack();
                    BicycleRack.BicycleRackType = BicycleRackTypes.WaveRack;
                    break;
                case BicycleRackTypes.PostandRingRack:
                    BicycleRack = new BicycleRack();
                    BicycleRack.BicycleRackType = BicycleRackTypes.PostandRingRack;
                    break;
                case BicycleRackTypes.NoRack:
                    BicycleRack = new BicycleRack();
                    BicycleRack.BicycleRackType = BicycleRackTypes.NoRack;
                    break;
            }

     
            
            
        }
    }
}
