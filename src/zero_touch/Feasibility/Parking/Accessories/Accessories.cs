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
            // Based on the input enum, set the bicycle rack type.
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

            // Based on the input enum, set the charging station type.
            switch (chargingStationType) 
            {
                case ChargingStationTypes.SurfaceMounted:
                    ChargingStation = new ChargingStation();
                    ChargingStation.ChargingStationType = ChargingStationTypes.SurfaceMounted;
                    break;
                case ChargingStationTypes.BoxShaped:
                    ChargingStation = new ChargingStation();
                    ChargingStation.ChargingStationType = ChargingStationTypes.BoxShaped;
                    break;
                case ChargingStationTypes.PoleMounted:
                    ChargingStation = new ChargingStation();
                    ChargingStation.ChargingStationType = ChargingStationTypes.PoleMounted;
                    break;
                case ChargingStationTypes.NoCharger:
                    ChargingStation = new ChargingStation();
                    ChargingStation.ChargingStationType = ChargingStationTypes.NoCharger;
                    break;
            }


            // Based on the input enum, set the wheel stop type.
            switch (wheelStopType) 
            { 
                case WheelStopTypes.FullLengthWheelStop:
                    WheelStop = new WheelStop();
                    WheelStop.WheelStopType = WheelStopTypes.FullLengthWheelStop;
                    break;
                case WheelStopTypes.SegmentedWheelStop:
                    WheelStop = new WheelStop();
                    WheelStop.WheelStopType = WheelStopTypes.SegmentedWheelStop;
                    break;
                case WheelStopTypes.NoWheelStop:
                    WheelStop = new WheelStop();
                    WheelStop.WheelStopType = WheelStopTypes.NoWheelStop;
                    break;
            }
        }
    }
}
