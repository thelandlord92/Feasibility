using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Parking
{
    /// <summary>
    /// Wrapper class for the parking patterns.
    /// </summary>
    public class Patterns
    {
        // this hides the overall class as a node.
        private Patterns() { }

        /// <summary>
        /// Creates a point.
        /// </summary>
        /// <param name="x">the x coordinate value</param>
        /// <param name="y">the y coordinate value</param>
        /// <returns name="Point">the output point</returns>
        /// <returns name="Number">the output numbers</returns>
        [MultiReturn(new[] { "Point", "Numbers" })]
        public static Dictionary<string, object> CreatePoint(int x, int y)
        {
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, y);

            var numbers = Common.Math.Range(2, 20, 3);

            return new Dictionary<string, object> 
            { 
                { "Point", point },
                { "Numbers", numbers },
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
        /// <returns name="Points">the non interlocking parking pattern rectangles</returns>
        /// <returns name="Line"></returns>
        /// /// <returns name="Planes"></returns>
        [MultiReturn(new[] { "Points", "Line", "Planes" })]
        public static Dictionary<string, object> NonInterlockedPattern(float bayWidth, float bayLength, float bayAngle, float patternLength,  float islandWidth) 
        {
            // calculate the bay width against the pattern center line.
            float actualWidth = (float)bayWidth / (float)DSCore.Math.Cos((float)bayAngle);

            // calculate number of bays to copy along center line.
            int copyNumber = (int)DSCore.Math.Ceiling(patternLength / actualWidth);

            // create the line points.
            Point startPoint = Point.ByCoordinates(0, 0);
            Point endPoint = Point.ByCoordinates(patternLength, 0);

            // create the center line.
            Line centerLine = Line.ByStartPointEndPoint(startPoint, endPoint) as Line;

            // get the line start point coordinate system.
            CoordinateSystem lineCoord = centerLine.CoordinateSystemAtParameter(0) as CoordinateSystem;

            // get the x vector of the coordinate system.
            Vector coordVector = lineCoord.XAxis as Vector;

            // move center line to offset bays from the island.
            Line movedLine = centerLine.Translate(coordVector, (float)islandWidth / 2) as Line;

            // add the parking bay location points to the moved line.
            List<Point> locationPoints = new List<Point>();
            foreach (float number in Common.Math.Range(0, 1, copyNumber))
            { 
                Point point = movedLine.PointAtParameter(number) as Point;
                locationPoints.Add(point);
            }

            // remove the last point from the list.
            locationPoints.RemoveAt(locationPoints.Count - 1);

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

            return new Dictionary<string, object> 
            {
                { "Points", locationPoints },
                { "Line" , movedLine },
                { "Planes", planeCS },
            };
        }
    }
}
