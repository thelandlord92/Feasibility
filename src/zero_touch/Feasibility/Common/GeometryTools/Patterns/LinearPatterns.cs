using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.GeometryTools.Patterns
{
    /// <summary>
    /// Wrapper class for the linear patterns.
    /// </summary>
    public class LinearPatterns
    {
        // Hides the overall class as a node.
        private LinearPatterns() { }


        /// <summary>
        /// Creates a dashed pattern along a curve.
        /// </summary>
        /// <param name="curve">The input curve to create the dashes along.</param>
        /// <param name="dashLength">The length of the dashes.</param>
        /// <param name="dashGap">The length of the gap between the dashes.</param>
        /// <param name="dashThickness">The thickness of the dashed line.</param>
        /// <returns name="dashCenterCurves">Polycurves representing the center of the dashes.</returns>
        /// <returns name="dashOutlines">Polycurves representing the outline of the dashes.</returns>
        /// <returns name="dashSurfaces">The dash surfaces.</returns>
        /// <exception cref="Exception"></exception>
        [MultiReturn(new[] { "dashCenterCurves", "dashOutlines", "dashSurfaces" })]
        public static Dictionary<string, object> DashedPattern(
            [DefaultArgument("Line.ByStartPointEndPoint(Point.ByCoordinates(0, 100), Point.ByCoordinates(0, 0))")] Curve curve,
            float dashLength = 5,
            float dashGap = 2,
            float dashThickness = 2)
        {
            // Throw excpetion if inputs less than 0.001.
            if (dashLength < 0.001 || dashGap < 0.001 || dashThickness < 0.001)
            {
                throw new ArgumentException("dash length, gap, and thickness cannot be less than 0.001");
            }

            // Get the start and end points.
            Autodesk.DesignScript.Geometry.Point startPoint = curve.StartPoint;
            Autodesk.DesignScript.Geometry.Point endPoint = curve.EndPoint;

            // Create the first setout points.
            Autodesk.DesignScript.Geometry.Point setoutStartPoint = curve.PointAtSegmentLength(dashLength);
            List<Autodesk.DesignScript.Geometry.Point> firstSetoutPoints = curve.PointsAtSegmentLengthFromPoint(setoutStartPoint, (dashLength + dashGap)).ToList();

            // Create the second setout points.
            List<Autodesk.DesignScript.Geometry.Point> secondSetoutPoints = curve.PointsAtSegmentLengthFromPoint(startPoint, (dashLength + dashGap)).ToList();

            // Transpose the setout points to create point pairs.
            List<List<Autodesk.DesignScript.Geometry.Point>> zippedPoints = firstSetoutPoints
                .Zip(secondSetoutPoints, (first, second) => new List<Autodesk.DesignScript.Geometry.Point> { first, second })
                .ToList();

            // Combine all the points.
            List<Autodesk.DesignScript.Geometry.Point> combinedPoints = new List<Autodesk.DesignScript.Geometry.Point>();
            combinedPoints.Add(startPoint);
            foreach (var pair in zippedPoints)
            {
                combinedPoints.AddRange(pair);
            }
            combinedPoints.Add(endPoint);

            // Clean the combined points list to remove any null values.
            combinedPoints = combinedPoints.Where(p => p != null).ToList();

            // Remove any duplicate points from the cleaned point list.
            combinedPoints = Autodesk.DesignScript.Geometry.Point.PruneDuplicates(combinedPoints, 0.001).ToList();

            // Chop the pruned point list into segments of 2.
            List<List<Autodesk.DesignScript.Geometry.Point>> choppedPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            for (int i = 0; i < combinedPoints.Count; i += 2)
            {
                if (i + 1 < combinedPoints.Count)
                {
                    choppedPoints.Add(new List<Autodesk.DesignScript.Geometry.Point> { combinedPoints[i], combinedPoints[i + 1] });
                }
            }

            // Remove points list containing only one point.
            List<List<Autodesk.DesignScript.Geometry.Point>> filteredPointList = choppedPoints.Where(p => p.Count != 1).ToList();

            // Create polycurves from the point lists. 
            List<PolyCurve> dashCenterCurves = new List<PolyCurve>();
            foreach (List<Autodesk.DesignScript.Geometry.Point> pointList in filteredPointList)
            {
                dashCenterCurves.Add(PolyCurve.ByPoints(pointList));
            }

            // Create polycurves representing the outline of the dashes.
            List<PolyCurve> dashOutlines = new List<PolyCurve>();
            foreach (PolyCurve polyCurve in dashCenterCurves)
            {
                dashOutlines.Add(PolyCurve.ByThickeningCurveNormal(polyCurve, dashThickness, Autodesk.DesignScript.Geometry.Vector.ZAxis()));
            }

            // Create surfaces representing the dashes.
            List<Surface> dashSurfaces = new List<Surface>();
            foreach (PolyCurve polyCurve1 in dashOutlines)
            {
                dashSurfaces.Add(Surface.ByPatch(polyCurve1));
            }

            return new Dictionary<string, object>
            {
                { "dashCenterCurves", dashCenterCurves },
                { "dashOutlines", dashOutlines },
                { "dashSurfaces", dashSurfaces }
            };
        }


        /// <summary>
        /// Base rectangle for use in pattern creation.
        /// </summary>
        /// <param name="rectangleWidth">Width of the rectangle.</param>
        /// <param name="rectangleLength">Length of the rectangle.</param>
        /// <param name="rectangleRotation">Rotation angle of the rectangle.</param>
        /// <param name="rectanglePlaneOffset">Offset of the rectangle from the host plane.</param>
        /// <param name="hostPlane">Host plane to create the rectangle.</param>
        /// <returns name="rectangle">The created rectangle.</returns>
        public static Rectangle BaseRectangle(
            float rectangleWidth = 2.5f, 
            float rectangleLength = 5f,
            float rectangleRotation = 0f,
            float rectanglePlaneOffset = 0f,
            [DefaultArgument("Plane.XY()")] Plane hostPlane = null)
        {
            // Create the base rectangle at the origin.
            List<Geometry> rectangle = new List<Geometry> { Rectangle.ByWidthLength(rectangleLength, rectangleWidth) };

            // Add the required transformations to the rectangle.
            List<Geometry> transformedRectangle = GeometryUtilities.AddTransformations(
                rectangle,
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0),
                hostPlane,
                Vector.ZAxis(),
                rectangleRotation,
                rectanglePlaneOffset,
                1
            );

            return transformedRectangle[0] as Rectangle;
        }


        /// <summary>
        /// Calculate the actual width of the pattern rectangle against the pattern location curve.
        /// </summary>
        /// <param name="rectangleWidth"></param>
        /// <param name="rectangleLength"></param>
        /// <param name="rectangleRotation"></param>
        /// <returns></returns>
        public static float PatternLocationCurveWidth(
            float rectangleWidth = 2.5f,
            float rectangleLength = 5f,
            float rectangleRotation = 0f) 
        {
            // Calculate the actual width of the pattern rectangle against location curve.
            float actualWidth = (float)(rectangleWidth / DSCore.Math.Cos(rectangleRotation));

            return actualWidth;
        }

        public static object NonInterlockingRegular()
        {
            return null;
        }


        public static object NonInterlockingBookended()
        {
            return null;
        }


        public static object NonInterlockingSegmented()
        {
            return null;
        }


        public static object InterlockingRegular()
        {
            return null;
        }


        public static object InterlockingBookended()
        {
            return null;
        }


        public static object InterlockingSegmented()
        {
            return null;
        }


        public static object HerringboneRegular()
        {
            return null;
        }


        public static object HerringboneBookended()
        {
            return null;
        }


        public static object HerringboneSegmented()
        {
            return null;
        }
    }
}
