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
        public Point TargetPosition {  private get; set; }

        /// <summary>
        /// The center of the pattern for rotation.
        /// </summary>
        public Point PatternCenter { private get; set; }

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
        /// The rotation angle of the pattern.
        /// </summary>
        public float PatternRotation { private get; set; }

        /// <summary>
        /// The parking bay rectangle geometry.
        /// </summary>
        public Rectangle Geometry { get; private set; }

        /// <summary>
        /// Flip the parking bay horizontally.
        /// </summary>
        public Boolean FlipHorizontal { private get; set; }

        /// <summary>
        /// Flip the parking bay vertically.
        /// </summary>
        public Boolean FlipVertical { private get; set; }

        // hides the overall class as a node.
        private ParkingBay() { }

        /// <summary>
        /// Creates a parking bay instance.
        /// </summary>
        /// <param name="targetPosition">the target transform position of the parking bay.</param>
        /// <param name="patternCenter">the center of the pattern.</param>
        /// <param name="bayWidth">the width of the parking bay.</param>
        /// <param name="bayLength">the length of the parking bay.</param>
        /// <param name="bayAngle">the angle of the parking bay.</param>
        /// <param name="patternRotation">the rotation angle of the parking bay around the pattern center point.</param>
        /// <param name="flipHorizontal">flip the parking bay horizontally.</param>
        /// <param name="flipVertical">flip the parking bay vertically</param>
        public ParkingBay(
            Point targetPosition,
            Point patternCenter,
            float bayWidth = (float)2.5,
            float bayLength = 5,
            float bayAngle = 30,
            float patternRotation = 0,
            bool flipHorizontal = false,
            bool flipVertical = false)
        { 
            TargetPosition = targetPosition;
            PatternCenter = patternCenter;
            BayWidth = bayWidth;
            BayLength = bayLength;
            BayAngle = bayAngle;
            PatternRotation = patternRotation;
            Geometry = CreateRectangle();
            FlipHorizontal = flipHorizontal;
            FlipVertical = flipVertical;
        }

        /// <summary>
        /// Creates the parking rectangle geometry.
        /// </summary>
        /// <returns></returns>
        public Rectangle CreateRectangle() 
        {
            // create the base rectangle.
            Rectangle baseRectangle = Rectangle.ByWidthLength(BayLength, BayWidth);

            // get the start point of the base rectangle.
            Point startPoint = baseRectangle.StartPoint as Point;

            // create plane at rectangle start point for rotation.
            Plane rotatePlane = Plane.ByOriginNormal(startPoint, Vector.ZAxis());

            // rotate the rectangle. 
            Rectangle rotatedRectangle = baseRectangle.Rotate(rotatePlane, BayAngle) as Rectangle;

            // get the coordinate system of the rotation plane.
            CoordinateSystem planeCS = CoordinateSystem.ByPlane(rotatePlane);

            // get the x and y axis of the rotation plane coordinate system.
            Vector coordx = planeCS.XAxis;
            Vector coordy = planeCS.YAxis;

            // create the vertical and horiztal mirror planes.
            Plane horizotalMirrorPlane = Plane.ByOriginNormal(startPoint, coordx);
            Plane verticalMirroPlane = Plane.ByOriginNormal(startPoint, coordy);

            // mirror the parking bay horizontally.
            Rectangle bayHorizontalMirror;

            if (FlipHorizontal == true)
            {
                bayHorizontalMirror = rotatedRectangle.Mirror(horizotalMirrorPlane) as Rectangle;
            }
            else 
            {
                bayHorizontalMirror = rotatedRectangle;
            }

            // mirror the parking bay vertically.
            Rectangle bayVerticalMirror;

            if (FlipVertical == true) 
            {
                bayVerticalMirror = bayHorizontalMirror.Mirror(verticalMirroPlane) as Rectangle;
            }
            else
            {
                bayVerticalMirror= bayHorizontalMirror;
            }

            // create a plane at the target position.
            Plane targetPlane = Plane.ByOriginNormal(TargetPosition, Vector.ZAxis());

            // get the coordinate system of the target plane.
            CoordinateSystem targetCS = CoordinateSystem.ByPlane(targetPlane);

            // transform the rectangle to the target plane.
            Rectangle transRectangle = bayVerticalMirror.Transform(planeCS, targetCS) as Rectangle;

            // create a plane at the pattern center point.
            Plane patternCenterPlane = Plane.ByOriginNormal(PatternCenter, Vector.ZAxis());

            // rotate the transformed rectangle around the pattern center point.
            Rectangle patternRotate = transRectangle.Rotate(patternCenterPlane, PatternRotation) as Rectangle;

            return patternRotate;
        }

        /// <summary>
        /// Gets the center points of the placed parking bays.
        /// </summary>
        /// <returns></returns>
        public Point GetCenterPoint() 
        {
            return Point.ByCoordinates(0, 0);
        }

        /// <summary>
        /// Gets the rotation angle of the bays from the y axis.
        /// </summary>
        /// <returns></returns>
        public float GetRotationAngle() 
        {
            return (float)34;
        }
    }
}
