using Autodesk.DesignScript.Geometry;
using DSCore;
using Modifiers;
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
        // private ParkingBay() { }

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
        /// <returns name="parkingRectangles">The parking rectangle geometry.</returns>
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

            // create the vertical and horizontal mirror planes.
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
        /// Creates the parking stripe geometry.
        /// </summary>
        /// <param name="stripeThickness">The thickness of the parking stripe.</param>
        /// <param name="stripeOpeningWidth">The opening width of the parking stripe.</param>
        /// <returns name="parkingStripe">The parking stripe curve.</returns>
        public PolyCurve CreateStripe(float stripeThickness=(float)0.1, float stripeOpeningWidth = (float)1.8) 
        {
            // create the parking rectangle. 
            Rectangle parkingRectangle = CreateRectangle();

            // try catch block to ensure internal surface is smaller than the parking surface.
            Surface subtractedSurface;
            try 
            {
                // offset the parking rectangle by the strip thickness.
                Curve[] stripeOffset = parkingRectangle.OffsetMany(-stripeThickness, Vector.ZAxis()) as Curve[];

                // join the offset curves.
                Curve joinedCurves = PolyCurve.ByJoinedCurves(stripeOffset, 0.01, false, 0) as Curve;

                // create the parking spot surface.
                Surface parkingSurface = Surface.ByPatch(parkingRectangle);

                // create the internal surface for subtraction from parking surface.
                List<Surface> internalSurface = new List<Surface> { Surface.ByPatch(joinedCurves) };

                // subtract the internal surface from the parking surface.
                subtractedSurface = parkingSurface.Difference(internalSurface);
            }
            catch 
            {
                // offset the parking rectangle by the strip thickness.
                Curve[] stripeOffset = parkingRectangle.OffsetMany(stripeThickness, Vector.ZAxis()) as Curve[];

                // join the offset curves.
                Curve joinedCurves = PolyCurve.ByJoinedCurves(stripeOffset, 0.01, false, 0) as Curve;

                // create the parking spot surface.
                Surface parkingSurface = Surface.ByPatch(parkingRectangle);

                // create the internal surface for subtraction from parking surface.
                List<Surface> internalSurface = new List<Surface> { Surface.ByPatch(joinedCurves) };

                // subtract the internal surface from the parking surface.
                subtractedSurface = parkingSurface.Difference(internalSurface);
            }

            // get the center of the parking spot.
            Point parkingCenter = parkingRectangle.Center();

            // create a plane at the center point.
            Plane centerPlane = Plane.ByOriginNormal(parkingCenter, Vector.ZAxis());

            // create the parking stripe entry cut rectangle.
            Rectangle entryRectangle = Rectangle.ByWidthLength(centerPlane, stripeOpeningWidth, stripeThickness * 3);

            // rotate the entry cut rectangle.
            Rectangle rotateRectangle = entryRectangle.Rotate(centerPlane, -GetRotationAngle()) as Rectangle;

            // move the entry rectangle to the parking bay entrance.
            Rectangle moveRectangle = rotateRectangle.Translate(GetParkingDirection(), parkingRectangle.Width/2) as Rectangle;

            // create a surface from the entry rectangle for subtraction.
            List<Surface> entrySurface = new List<Surface> { Surface.ByPatch(moveRectangle) };

            // subtract the entry rectangle from the subtracted surface.
            Surface stripeSurface = subtractedSurface.Difference(entrySurface);

            // get the perimeter curve of the stripe surface.
            PolyCurve stripeCurve = PolyCurve.ByJoinedCurves(stripeSurface.PerimeterCurves(), 0.001, false, 0);

            return stripeCurve;
        }


        /// <summary>
        /// Get the vector along the length of the parking spots.
        /// </summary>
        /// <returns name="lengthVector">vector along the length of the parking bay.</returns>
        public Vector GetParkingDirection() 
        {
            // get the parking bay rectangle.
            Rectangle parkingRectangle = CreateRectangle();

            // explode the parking rectangle to get an array of geometry.
            Geometry[] rectangleGeometries = parkingRectangle.Explode();

            // convert the array of Geometry to a list of Line.
            List<Line> rectangleLines = rectangleGeometries.OfType<Line>().ToList();

            // get the vector of a line at the length of the parking bay.
            Vector lengthVector = rectangleLines[1].Direction;

            return lengthVector;
        }


        /// <summary>
        /// Gets the rotation angle of the bays from the y axis.
        /// </summary>
        /// <returns name="rotationAngle"></returns>
        public float GetRotationAngle() 
        {
            // compute the rotation angle of the parking bay.
            float rotationAngle = (float)GetParkingDirection().AngleAboutAxis(Vector.YAxis(), Vector.ZAxis());

            return rotationAngle;
        }


        /// <summary>
        /// Gets the center points of the placed parking bays.
        /// </summary>
        /// <returns name="parkingCenter">the center point of the parking bay.</returns>
        public Point GetCenterPoint()
        {
            // get the parking bay rectangle.
            Rectangle parkingRectangle = CreateRectangle();

            // get the center of the parking bay.
            Point parkingCenter = parkingRectangle.Center();

            return parkingCenter;
        }
    }
}
