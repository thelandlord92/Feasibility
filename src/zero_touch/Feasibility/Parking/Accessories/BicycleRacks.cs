using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Parking.Accessories
{
    /// <summary>
    /// Wrapper class for the bicycle racks.
    /// </summary>
    public class CreateBicycleRacks
    {
        /// <summary>
        /// The target position of the bicycle rack.
        /// </summary>
        internal Point TargetPosition { private get; set; }

        /// <summary>
        /// The diameter of the bicycle rack's tube.
        /// </summary>
        public float RackDiameter { get; set; }

        /// <summary>
        /// The height of the bicycle rack.
        /// </summary>
        public float RackHeight { get; set; }

        /// <summary>
        /// The length of the bicycle rack.
        /// </summary>
        public float RackLength { get; set; }

        /// <summary>
        /// The angle of the bicycle rack.
        /// </summary>
        public float RackAngle { private get; set; }

        /// <summary>
        /// The offset of the bicycle rack from the side of the parking bay.
        /// </summary>
        public float RackOffset { private get; set; }


        /// <summary>
        /// Creates a bicycle rack instance.
        /// </summary>
        /// <param name="rackDiameter"></param>
        /// <param name="rackHeight"></param>
        /// <param name="rackLength"></param>
        /// <param name="rackAngle"></param>
        /// <param name="rackOffset"></param>
        public CreateBicycleRacks(
            float rackDiameter, 
            float rackHeight, 
            float rackLength, 
            float rackAngle, 
            float rackOffset)
        {
            RackDiameter = rackDiameter;
            RackHeight = rackHeight;
            RackLength = rackLength;
            RackAngle = rackAngle;
            RackOffset = rackOffset;
        }
    }
}
