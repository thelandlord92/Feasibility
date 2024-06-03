using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking
{
    /// <summary>
    /// Wrapper class for the parking bay.
    /// </summary>
    public class ParkingBay
    {
        /// <summary>
        /// The target position of the parking bay.
        /// </summary>
        public Point TargetPosition {  get; set; }

        /// <summary>
        /// The width of the parking bay.
        /// </summary>
        public float BayWidth { get; set; }

        /// <summary>
        /// The length of the parking bay.
        /// </summary>
        public float BayLength { get; set; }

        /// <summary>
        /// The angle of the parking bay.
        /// </summary>
        public float BayAngle { get; set; }

        /// <summary>
        /// The parking bay rectangle geometry.
        /// </summary>
        public Rectangle Geometry { get; private set; }

        // hides the overall class as a node.
        private ParkingBay() { }

        /// <summary>
        /// Creates a parking bay instance.
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="bayWidth"></param>
        /// <param name="bayLength"></param>
        /// <param name="bayAngle"></param>
        public ParkingBay(
            Point targetPosition,
            float bayWidth = (float)2.5,
            float bayLength = 5,
            float bayAngle = 30)
        { 
            TargetPosition = targetPosition;
            BayWidth = bayWidth;
            BayLength = bayLength;
            BayAngle = bayAngle;
            Geometry = CreateGeometry();
        }

        /// <summary>
        /// Creates the parking rectangle geometry.
        /// </summary>
        /// <returns></returns>
        public Rectangle CreateGeometry() 
        {
            Rectangle baseRectangle = Rectangle.ByWidthLength(BayWidth, BayLength);
            return baseRectangle;
        }
    }
}
