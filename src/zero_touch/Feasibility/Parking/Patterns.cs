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
        /// Creates instances of the parking pattern class.
        /// </summary>
        /// <param name="locationLine">The input line indicating the location of the pattern.</param>
        /// <param name="patternType">Set the parking pattern type.</param>
        /// <param name="bayWidth">the width of the parking bays.</param>
        /// <param name="bayLength">the length of the parking bays.</param>
        /// <param name="bayAngle">the angle of the parking bays.</param>
        /// <param name="patternRotation">The rotation angle of the pattern.</param>
        public Patterns(
            Line locationLine,
            int patternType = 1,
            float bayWidth = (float)2.5,
            float bayLength = 5,
            float bayAngle = 30,
            float patternRotation = 0) 
        { 
            LocationLine = locationLine;
            PatternType = patternType;
            BayWidth = bayWidth;
            BayLength = bayLength;
            BayAngle = bayAngle;
            PatternRotation = patternRotation;  
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
        /// <param name="patternOffset"></param>
        /// <returns></returns>
        public List<Point> HalfPoints(float patternOffset = 1) 
        {
            // get the location line start coordinate system.
            CoordinateSystem lineCoord = LocationLine.CoordinateSystemAtParameter(0);

            // get the x vector of the coordinate system.
            Vector coordVector = lineCoord.XAxis.Reverse();

            // move the location line to offset the bays in relation to the location line.
            Line movedLine = LocationLine.Translate(coordVector, patternOffset) as Line;

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
        /// Creates half of the parking pattern.
        /// </summary>
        /// <param name="bayWidth">the width of the parking bays</param>
        /// <param name="bayLength">the length of the parking bays</param>
        /// <param name="bayAngle">the angle of the parking bays</param>
        /// <param name="patternLength">the length of the parking pattern</param>
        /// <param name="islandWidth">the width of the island at the pattern center</param>
        /// <returns name="rectangles">the parking pattern rectangles</returns>
        /// <returns name="centerLine">the centerline of the pattern</returns>
        /// <returns name="planes">planes at the start points of the rectangles</returns>
        /// <returns name="mirrorPlane">plane to mirror the pattern along the center line</returns>
        [MultiReturn(new[] { "rectangles", "centerLine", "planes", "mirrorPlane" })]
        public static Dictionary<string, object> HalfPattern(
            float bayWidth=(float)2.5, 
            float bayLength=5, 
            float bayAngle=30, 
            float patternLength=100,  
            float islandWidth=0) 
        {
            // calculate the bay width against the pattern center line.
            float actualWidth = (float)bayWidth / (float)DSCore.Math.Cos((float)bayAngle);

            // calculate number of bays to copy along center line.
            int copyNumber = (int)DSCore.Math.Ceiling(patternLength / actualWidth);

            // create the line points.
            Point startPoint = Point.ByCoordinates(0, 0);
            Point endPoint = Point.ByCoordinates(0, actualWidth * copyNumber);

            // create the center line.
            Line centerLine = Line.ByStartPointEndPoint(startPoint, endPoint) as Line;

            // get the line start point coordinate system.
            CoordinateSystem lineCoord = centerLine.CoordinateSystemAtParameter(0) as CoordinateSystem;

            // get the x vector of the coordinate system.
            Vector coordVector = lineCoord.XAxis.Reverse() as Vector;

            // create the pattern mirror plane.
            Plane mirrorPlane = Plane.ByOriginNormal(startPoint, coordVector);

            // move center line to offset bays from the island.
            Line movedLine = centerLine.Translate(coordVector, (float)islandWidth / 2) as Line;

            // add the parking bay location points to the moved line.
            List<Point> locationPoints = new List<Point>();
            foreach (float number in Common.Math.Range(0, 1, copyNumber+1))
            { 
                Point point = movedLine.PointAtParameter(number) as Point;
                locationPoints.Add(point);
            }

            // remove the last point from the list.
            // locationPoints.RemoveAt(locationPoints.Count - 1);

            // add planes at the points.
            List<Plane> linePlanes = new List<Plane>();
            foreach (Point point in locationPoints)
            {
                Plane plane = Plane.ByOriginNormal(point, Vector.ZAxis());
                linePlanes.Add(plane);
            }

            // get the plane coordinate systems.
            List<CoordinateSystem> planeCS = new List<CoordinateSystem>();
            foreach (Plane plane in linePlanes)
            { 
                CoordinateSystem coordSys = CoordinateSystem.ByPlane(plane);
                planeCS.Add(coordSys);
            }

            // create the initial parking bay rectangle.
            Rectangle bayRectangle = Rectangle.ByWidthLength(bayLength, bayWidth);

            // create plane to rotate rectangle.
            Plane rotatePlane = Plane.ByOriginNormal(startPoint, Vector.ZAxis());

            // rotate the initial parking rectangle.
            Rectangle rotateRectangle = bayRectangle.Rotate(rotatePlane, bayAngle) as Rectangle;

            // get the plane at the start point of the rotated rectangle.
            Plane bayPlane = Plane.ByOriginNormal(rotateRectangle.StartPoint, Vector.ZAxis());

            // get the bay plane coordinate system.
            CoordinateSystem bayCS = CoordinateSystem.ByPlane(bayPlane);
            
            // copy the bay rectangle to the line points.
            List<Rectangle> copiedBays = new List<Rectangle>();
            foreach (CoordinateSystem coordSys in planeCS)  
            { 
                Rectangle transformedRectangle = rotateRectangle.Transform(bayCS, coordSys) as Rectangle;
                copiedBays.Add(transformedRectangle);

            }

            return new Dictionary<string, object> 
            {
                { "rectangles", copiedBays },
                { "centerLine" , centerLine },
                { "planes", linePlanes },
                { "mirrorPlane", mirrorPlane },
            };
        }


        /// <summary>
        /// Creates the non interlocking parking pattern.
        /// </summary>
        /// <param name="bayWidth">the width of the parking bays</param>
        /// <param name="bayLength">the length of the parking bays</param>
        /// <param name="bayAngle">the angle of the parking bays</param>
        /// <param name="patternLength">the length of the parking pattern</param>
        /// <param name="islandWidth">the width of the island at the pattern center</param>
        /// <param name="patternRotation">the rotation angle of the overall pattern</param>
        /// <returns name="rectangles">the parking pattern rectangles</returns>
        /// <returns name="centerLine">the centerline of the pattern</returns>
        [MultiReturn(new[] { "rectangles", "centerLine" })]
        public static Dictionary<string, object> NonInterlockingPattern(
            float bayWidth=(float)2.5, 
            float bayLength=5, 
            float bayAngle=30, 
            float patternLength=100, 
            float islandWidth=0, 
            float patternRotation=0) 
        {
            // create half of the parking pattern.
            Dictionary<string, object> halfPattern = HalfPattern(bayWidth, bayLength, bayAngle, patternLength, islandWidth);

            // get the half pattern rectangles.
            List<Rectangle> halfBays = halfPattern["rectangles"] as List<Rectangle>;

            // get the mirror plane.
            Plane mirrorPlane = halfPattern["mirrorPlane"] as Plane;

            // get the center line.
            Line centerLine = halfPattern["centerLine"] as Line;

            // mirror the bays along the center line.
            List<Rectangle> mirrorBays = new List<Rectangle>();
            foreach (Rectangle bay in halfBays as List<Rectangle>) 
            {
                Rectangle mirrorBay = bay.Mirror(mirrorPlane) as Rectangle; 
                mirrorBays.Add(mirrorBay);
            }

            // combine the patterns into one list.
            List<List<Rectangle>> combinedBays = new List<List<Rectangle>>();
            combinedBays.Add(halfBays);
            combinedBays.Add(mirrorBays);

            // flatten the combined bays.
            List<Rectangle> flattenedBays = combinedBays.SelectMany(bays => bays).ToList() as List<Rectangle>;

            // rotate the full pattern.
            List<Rectangle> rotatedBays = new List<Rectangle>();
            Point rotationPoint = centerLine.PointAtParameter(0.5);
            Plane rotationPlane = Plane.ByOriginNormal(rotationPoint, Vector.ZAxis());
            foreach (Rectangle bay in flattenedBays)
            {
                Rectangle rotatedBay = bay.Rotate(rotationPlane, patternRotation) as Rectangle;
                rotatedBays.Add(rotatedBay);
            }

            // rotate the full pattern for two-way parking.
            List<Rectangle> rotatedBaysTwoWay = new List<Rectangle>();
            foreach (Rectangle bay in rotatedBays)
            {
                Rectangle rotatedBayTwoWay = bay.Rotate(rotationPlane, 180) as Rectangle;
                rotatedBaysTwoWay.Add(rotatedBayTwoWay);
            }

            return new Dictionary<string, object>
            {
                { "rectangles", rotatedBaysTwoWay },
                { "centerLine" , halfPattern["centerLine"] },
            };
        }
    }
}
