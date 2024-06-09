using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Parking;

namespace Parking
{
    /// <summary>
    /// Wrapper class for the parking patterns.
    /// </summary>
    public class Patterns
    {   
        /// <summary>
        /// The input line indicating the location of the pattern.
        /// </summary>
        public Line LocationLine {  private get; set; }

        private int _patternType;

        /// <summary>
        /// Set the parking pattern type.
        /// 1 for the non interlocking pattern.
        /// 2 for the interlocing pattern.
        /// 3 for the herringbone pattern.
        /// </summary>
        public int PatternType
        {
            private get { return _patternType; }
            set
            {
                if (value < 1 || value > 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(PatternType), "PatternType must be between 1 an 3.");
                }
                _patternType = value;
            }
        }

        /// <summary>
        /// The width of the parking bays.
        /// </summary>
        public float BayWidth { private get; set; }

        /// <summary>
        /// The length of the parking bays.
        /// </summary>
        public float BayLength { private get; set; }

        /// <summary>
        /// The angle of the parking bays.
        /// </summary>
        public float BayAngle { private get; set; }

        /// <summary>
        /// The rotation angle of the pattern.
        /// </summary>
        public float PatternRotation { private get; set; }

        /// <summary>
        /// The width of the non interlocking pattern island.
        /// </summary>
        public float IslandWidth { private get; set; }


        /// <summary>
        /// Creates instances of the parking pattern class.
        /// </summary>
        /// <param name="locationLine">The input line indicating the location of the pattern.</param>
        /// <param name="patternType">Set the parking pattern type.</param>
        /// <param name="bayWidth">the width of the parking bays.</param>
        /// <param name="bayLength">the length of the parking bays.</param>
        /// <param name="bayAngle">the angle of the parking bays.</param>
        /// <param name="patternRotation">The rotation angle of the pattern.</param>
        /// <param name="islandWidth">The width of the non interlocking pattern island.</param>
        public Patterns(
            Line locationLine,
            int patternType = 1,
            float bayWidth = (float)2.5,
            float bayLength = 5,
            float bayAngle = 30,
            float patternRotation = 0,
            float islandWidth = 1) 
        { 
            LocationLine = locationLine;
            PatternType = patternType;
            BayWidth = bayWidth;
            BayLength = bayLength;
            BayAngle = bayAngle;
            PatternRotation = patternRotation;
            IslandWidth = islandWidth;
        }


        /// <summary>
        /// Calculates the number of parking bays to copy along the location line.
        /// </summary>
        /// <returns></returns>
        public int ParkingCopyNumber() 
        {
            // calculate the actual bay width against the pattern location line.
            float actualWidth = (float)BayWidth / (float)DSCore.Math.Cos((float)BayAngle);

            // calculate the number of bays to copy along the location line.
            int copyNumber = (int)DSCore.Math.Ceiling(LocationLine.Length / actualWidth);

            return copyNumber;
        }


        /// <summary>
        /// Creates half of the parking pattern points.
        /// </summary>
        /// <param name="patternOffset">The offset distance of the pattern points from the location line.</param>
        /// <returns name="locationPoints">A list of points to host parking bays.</returns>
        public List<Point> HalfPoints(float patternOffset = 1) 
        {
            // get the location line start coordinate system.
            CoordinateSystem lineCoord = LocationLine.CoordinateSystemAtParameter(0);

            // get the x vector of the coordinate system.
            Vector coordVector = lineCoord.XAxis.Reverse();

            // extend the location line if required to ensure the bays fit accurately.
            float newLineLength = ((float)BayWidth / (float)DSCore.Math.Cos((float)BayAngle)) * ParkingCopyNumber();
            float extensionLength = (float)newLineLength - (float)LocationLine.Length;
            Line extendedLine = LocationLine.ExtendEnd(extensionLength) as Line;

            // move the location line to offset the bays in relation to the location line.
            Line movedLine = extendedLine.Translate(coordVector, patternOffset) as Line;

            // add the parking bay location points to the moved line.
            List<Point> locationPoints = new List<Point>();
            foreach (float number in Common.Math.Range(0, 1, ParkingCopyNumber() + 1)) 
            {
                Point point = movedLine.PointAtParameter(number);
                locationPoints.Add(point);
            }

            return locationPoints;
        }


        /// <summary>
        /// Gets the rotation angle of the location line from the y axis.
        /// </summary>
        /// <returns name="rotationAngle"></returns>
        private float GetLineRotationAngle()
        {
            // get the direction of the location line.
            Vector lineDirection = LocationLine.Direction;

            // compute the rotation angle of the parking bay.
            float rotationAngle = (float)lineDirection.AngleAboutAxis(Vector.YAxis(), Vector.ZAxis());

            return rotationAngle;
        }


        /// <summary>
        /// Creates a mirror plane at the pattern location line.
        /// </summary>
        /// <returns name="mirrorPlane">The location line mirror plane.</returns>
        private Plane LocationLineMirrorPlane() 
        {
            // create the mirror plane to mirror the target points along the location line.
            Point lineStartPoint = LocationLine.StartPoint;
            CoordinateSystem lineCoord = LocationLine.CoordinateSystemAtParameter(0);
            Vector coordVector = lineCoord.XAxis;
            Plane mirrorPlane = Plane.ByOriginNormal(lineStartPoint, coordVector);

            return mirrorPlane;
        }


        /// <summary>
        /// Creates the non interlocking parking pattern.
        /// </summary>
        /// <returns name="parkingBays">The parking bay instances.</returns>
        public List<List<ParkingBay>> NonInterlockingPattern() 
        {
            // create the first half of the parking bay target points.
            List<Point> locationPoints = HalfPoints(IslandWidth / 2);

            // create the mirror plane to mirror the target points along the location line.
            Plane mirrorPlane = LocationLineMirrorPlane();

            // create the second half of the parking bay target points.
            List<Point> secondLocationPoints = new List<Point>();
            foreach (Point point in locationPoints) 
            { 
                Point mirrorPoint = point.Mirror(mirrorPlane) as Point;
                secondLocationPoints.Add(mirrorPoint);
            }

            // get the center of the location line.
            Point locationCenter = LocationLine.PointAtParameter(0.5);

            // add the parking bay instances to the first half target points.
            List<ParkingBay> firstParkingBays = new List<ParkingBay>();  
            foreach (Point point in locationPoints) 
            {
                ParkingBay bay = new ParkingBay(
                    point, 
                    locationCenter, 
                    BayWidth, 
                    BayLength, 
                    BayAngle + GetLineRotationAngle(), 
                    PatternRotation,
                    false,
                    true);
                firstParkingBays.Add(bay);
            }

            // add the parking bay instances to the second half target points.
            List<ParkingBay> secondParkingBays = new List<ParkingBay>();
            foreach (Point point in secondLocationPoints)
            {
                ParkingBay bay = new ParkingBay(
                    point,
                    locationCenter,
                    BayWidth,
                    BayLength,
                    BayAngle + GetLineRotationAngle(),
                    PatternRotation,
                    true,
                    true);
                secondParkingBays.Add(bay);
            }

            // add the lists of parking bays to a single list.
            List<List<ParkingBay>> parkingBays = new List<List<ParkingBay>>();
            parkingBays.Add(firstParkingBays);
            parkingBays.Add(secondParkingBays);

            return parkingBays;
        }


        /// <summary>
        /// Creates the extended non interlocing bay rectangles for cutting the island surface.
        /// </summary>
        /// <returns name="extendedRectangles">The extended parking bay rectangles.</returns>
        public List<Rectangle> ElongatedNonInterlockingRectangles() 
        {
            // add the parking bays to a list.
            List<List<ParkingBay>> parkingBays = NonInterlockingPattern();

            // calculate the additional length to extend the parking bay.
            float additionalLength = (float)(BayWidth * DSCore.Math.Tan(BayAngle));

            // extend the parking bays.
            List<Rectangle> extendedRectangles =  new List<Rectangle>();
            foreach (List<ParkingBay> bayList in parkingBays) 
            {
                foreach (ParkingBay bay in bayList) 
                { 
                    bay.BayLength = additionalLength + BayLength;
                    extendedRectangles.Add(bay.CreateRectangle());
                }
            }

            return extendedRectangles;
        }


        /// <summary>
        /// Calculates the width of the non interlocking pattern.
        /// </summary>
        /// <returns name="patternWidth">The width of the non interlocking pattern.</returns>
        private float NonInterlockingPatternWidth
        {
            get
            {
                // calculate the overall pattern width.
                float width1 = (float)(BayWidth * DSCore.Math.Sin(BayAngle)); // closest triangle width to the center island.
                float width2 = (float)(DSCore.Math.Cos(BayAngle) * BayLength); // furthermost trinagle wifth from the center island.
                float patternWidth = (float)((width1 + width2) * 2 + IslandWidth);

                return patternWidth;
            }
        }


        /// <summary>
        /// Creates a rectangle covering the width of the pattern and length of the location line.
        /// </summary>
        /// <returns name="patternSurface">The pattern surface.</returns>
        public Surface NonInterlockingPatternSurface() 
        {
            // create the surface rectangle.
            Plane centerPlane = Plane.ByOriginNormal(LocationLine.PointAtParameter(0.5), Vector.ZAxis());
            Rectangle surfaceRectangle = Rectangle.ByWidthLength(centerPlane, NonInterlockingPatternWidth, LocationLine.Length);

            // rotate the rectangle.
            Rectangle rotatedRectangle = surfaceRectangle.Rotate(centerPlane, PatternRotation) as Rectangle;

            // create the surface.
            Surface patternSurface = Surface.ByPatch(rotatedRectangle);

            return patternSurface;
        }


        /// <summary>
        /// Creates the interlocking pattern.
        /// </summary>
        /// <returns name="parkingBays">The parking bay instances.</returns>
        public List<ParkingBay> InterlockingPattern() 
        {
            // create the first half of the parking bay target points.
            List<Point> locationPoints = HalfPoints(-(float)(BayWidth * DSCore.Math.Sin(BayAngle)) / 2);

            // create the mirror plane to mirror the target points along the location line.
            Plane mirrorPlane = LocationLineMirrorPlane();

            // create the second half of the parking bay target points.
            List<Point> secondLocationPoints = new List<Point>();
            foreach (Point point in locationPoints)
            {
                Point mirrorPoint = point.Mirror(mirrorPlane) as Point;
                secondLocationPoints.Add(mirrorPoint);
            }

            // get the center of the location line.
            Point locationCenter = LocationLine.PointAtParameter(0.5);

            // add the parking bay instances to the first half target points.
            List<ParkingBay> firstParkingBays = new List<ParkingBay>();
            foreach (Point point in locationPoints)
            {
                ParkingBay bay = new ParkingBay(
                    point,
                    locationCenter,
                    BayWidth,
                    BayLength,
                    BayAngle + GetLineRotationAngle(),
                    PatternRotation,
                    false,
                    true);
                firstParkingBays.Add(bay);
            }

            return firstParkingBays;
        }
    }
}
