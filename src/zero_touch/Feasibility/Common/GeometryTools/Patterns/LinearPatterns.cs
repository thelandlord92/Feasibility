using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DesignScript.Builtin;
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
        /// <param name="mirrorHorizontal">Mirror the rectangle horizontally at the host plane center.</param>
        /// <param name="mirrorVertical">Mirror the rectangle vertically at the host plane center.</param>
        /// <param name="hostPlane">Host plane to create the rectangle.</param>
        /// <returns name="rectangle">The created rectangle.</returns>
        public static Rectangle BaseRectangle(
            float rectangleWidth = 2.5f, 
            float rectangleLength = 5f,
            float rectangleRotation = 0f,
            float rectanglePlaneOffset = 0f,
            bool mirrorHorizontal = false,
            bool mirrorVertical = false,
            [DefaultArgument("Plane.XY()")] Plane hostPlane = null)
        {
            // Create the base rectangle at the origin.
            List<Geometry> rectangle = new List<Geometry> { Rectangle.ByWidthLength(rectangleLength, rectangleWidth) };

            // Get the start point of the rectangle. 
            Autodesk.DesignScript.Geometry.Point startPoint = (rectangle[0] as Rectangle).StartPoint;

            // Add the required transformations to the rectangle.
            List<Geometry> transformedRectangle = GeometryUtilities.AddTransformations(
                rectangle,
                startPoint,
                hostPlane,
                Vector.ZAxis(),
                rectangleRotation,
                rectanglePlaneOffset,
                1,
                mirrorHorizontal,
                mirrorVertical
            );

            return transformedRectangle[0] as Rectangle;
        }


        /// <summary>
        /// Calculate the ideal width of the pattern rectangles against the location curve.
        /// </summary>
        /// <param name="rectangleWidth">Width of the rectangle.</param>
        /// <param name="rectangleRotation">Rotation angle of the rectangle.</param>
        /// <returns name="idealWidth">Ideal width of the pattern rectangles against the location curve.</returns>
        public static float PatternIdealWidth(
            float rectangleWidth = 2.5f,
            float rectangleRotation = 0f)
        {
            // Calculate the ideal width of the pattern rectangles against location curve.
            float idealWidth = (float)(rectangleWidth / DSCore.Math.Cos(rectangleRotation));

            return idealWidth;
        }


        /// <summary>
        /// Calculate the chord length of the location curve using its end points.
        /// The chord length of a straight line will be equal to the line's length.
        /// </summary>
        /// <param name="locationCurve">The location curve.</param>
        /// <returns name="chordLength">Calculated chord length.</returns>
        public static float LocationCurveChordLength(
            [DefaultArgument("Line.ByStartPointEndPoint(Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0), Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 100, 0))")] Curve locationCurve) 
        {
            // Calculate the chord length of the full location curve using the end points.
            Autodesk.DesignScript.Geometry.Point startPoint = locationCurve.StartPoint;
            Autodesk.DesignScript.Geometry.Point endPoint = locationCurve.EndPoint;
            float chordLength = (float)startPoint.DistanceTo(endPoint);

            return chordLength;
        }


        /// <summary>
        /// Calculate the number of times to copy the pattern rectangles against the location curve.
        /// </summary>
        /// <param name="locationCurve">The input curve to place the pattern rectangles along.</param>
        /// <param name="rectangleWidth">Width of the rectangle.</param>
        /// <param name="rectangleRotation">Rotation angle of the rectangle.</param>
        /// <returns name="copyNumber">Pattern rectangle copy number along the location curve.</returns>
        public static int PatternLocationCurveCopyNumber(
            [DefaultArgument("Line.ByStartPointEndPoint(Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0), Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 100, 0))")] Curve locationCurve,
            float rectangleWidth = 2.5f,
            float rectangleRotation = 0f) 
        {
            // Get the ideal width of the pattern rectangles against the location curve.
            float idealWidth = PatternIdealWidth(rectangleWidth, rectangleRotation);

            // Get the chord length of the full location curve..
            float chordLength = LocationCurveChordLength(locationCurve);

            // Calculate the number of bays to copy along the location curve.
            int copyNumber;
            if (chordLength < idealWidth) // Copy number is zero if the chord length of the location curve is less than the ideal width of the pattern retangle.
            {
                copyNumber = 0;
            }
            else if (chordLength == idealWidth)
            {
                copyNumber = 1;
            }
            else 
            {
                copyNumber = (int)DSCore.Math.Floor(locationCurve.Length / idealWidth);
            }

            return copyNumber;
        }


        /// <summary>
        /// Calculate the width of the pattern rectangles against the location curve.
        /// </summary>
        /// <param name="locationCurve">The input curve to place the pattern rectangles along.</param>
        /// <param name="rectangleWidth">Width of the rectangle.</param>
        /// <param name="rectangleRotation">Rotation angle of the rectangle.</param>
        /// <returns name="locationCurveWidth">Width of the pattern against the location curve.</returns>
        public static float PatternActualLocationCurveWidth(
            [DefaultArgument("Line.ByStartPointEndPoint(Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0), Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 100, 0))")] Curve locationCurve,
            float rectangleWidth = 2.5f,
            float rectangleRotation = 0f)
        {
            // Get the ideal width of the pattern rectangles against the location curve.
            float idealWidth = PatternIdealWidth(rectangleWidth, rectangleRotation);

            // Get the chord length of the full location curve..
            float chordLength = LocationCurveChordLength(locationCurve);

            // Calculate the actual width of the pattern rectangles against the location curve.
            float actualWidth;
            if (chordLength <= idealWidth) 
            {
                actualWidth = chordLength;
            }
            else 
            {
                // Get the pattern copy number.
                int copyNumber = PatternLocationCurveCopyNumber(
                    locationCurve,
                    rectangleWidth,
                    rectangleRotation
                );

                // Add points along the location curve to create a polycurve.
                List<Autodesk.DesignScript.Geometry.Point> points = locationCurve.PointsAtEqualChordLength(copyNumber).ToList();

                // Get the distance between the first and second point in the points list.
                actualWidth = (float)points[0].DistanceTo(points[1]);
            }

            return actualWidth;
        }


        /// <summary>
        /// Calculate the actual width of the rectangles to be created.
        /// </summary>
        /// <param name="locationCurve">The input curve to place the pattern rectangles along.</param>
        /// <param name="rectangleWidth">Width of the rectangle.</param>
        /// <param name="rectangleRotation">Rotation angle of the rectangle.</param>
        /// <returns></returns>
        public static float PatternActualWidth(
            [DefaultArgument("Line.ByStartPointEndPoint(Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0), Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 100, 0))")] Curve locationCurve,
            float rectangleWidth = 2.5f,
            float rectangleRotation = 0f)
        {
            // Get the ideal width of the pattern rectangles against the location curve.
            float idealWidth = PatternIdealWidth(rectangleWidth, rectangleRotation);

            // Get the chord length of the full location curve.
            float chordLength = LocationCurveChordLength(locationCurve);

            // Get the actual width of the rectangles against the location curve.
            float locationCurveWidth = PatternActualLocationCurveWidth(locationCurve, rectangleWidth, rectangleRotation);

            // Calculate the actual width of the rectangles.
            float actualWidth;
            if (chordLength <= idealWidth) 
            {
                actualWidth = (float)DSCore.Math.Cos(rectangleRotation) * chordLength;
            }
            else
            {
                actualWidth = (float)DSCore.Math.Cos(rectangleRotation) * locationCurveWidth;
            }

            return actualWidth;
        }


        /// <summary>
        /// Creates the non interlocking pattern.
        /// </summary>
        /// <param name="locationCurve">The input curve to place the pattern rectangles along.</param>
        /// <param name="rectangleWidth">Width of the rectangles.</param>
        /// <param name="rectangleLength">Length of the rectangles.</param>
        /// <param name="rectangleRotation">Rotation angle of the rectangle.</param>
        /// <param name="patternOffset">The offset distance of the pattern points from the location line.</param>
        /// <param name="patternSideOne"></param>
        /// <param name="patternSideTwo"></param>
        /// <returns name="points">Points to host the first half of the pattern.</returns>
        public static object NonInterlockingRegularPattern(
            [DefaultArgument("Line.ByStartPointEndPoint(Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0), Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 100, 0))")] Curve locationCurve,
            float rectangleWidth = 2.5f,
            float rectangleLength = 5f,
            float rectangleRotation = 0f,
            float patternOffset = 1f,
            bool patternSideOne = true,
            bool patternSideTwo = true)
        {
            // Throw an exception if the user turns off both pattern sides.
            if (!patternSideOne && !patternSideTwo) 
            {
                throw new ArgumentException("Both pattern sides cannot be off.");
            }

            // Get the pattern copy number.
            int copyNumber = PatternLocationCurveCopyNumber(locationCurve, rectangleWidth, rectangleRotation);

            // Create the points along the location curve.
            List<Autodesk.DesignScript.Geometry.Point> curvePoints = Curves.PointsAtEqualChordLength(locationCurve, copyNumber);

            // Get the normals at the points.
            List<Autodesk.DesignScript.Geometry.Vector> curveNormals = Curves.CurveNormalsAtPoints(locationCurve, curvePoints);

            // Create the first side pattern.
            List<object> sideOneRectangles = new List<object>();
            if (patternSideOne) 
            {
                // Move the points along the normal vector. To be used to create a new curve.
                List<Autodesk.DesignScript.Geometry.Point> movedPoints = new List<Autodesk.DesignScript.Geometry.Point>();
                for (int i = 0; i < curvePoints.Count; i++)
                {
                    movedPoints.Add(curvePoints[i].Translate(curveNormals[i], patternOffset) as Autodesk.DesignScript.Geometry.Point);
                }

                // Create a new curve from the moved points. This is to accommodate for the lengthening or shortening of the location curve after offset.
                NurbsCurve newLocationCurve = NurbsCurve.ByPoints(movedPoints);

                // Calculate a new copy number based on the new location curve.
                int newCopyNumber = PatternLocationCurveCopyNumber(newLocationCurve, rectangleWidth, rectangleRotation);

                // Add the pattern points to the new curve.
                List<Autodesk.DesignScript.Geometry.Point> patternPoints = Curves.PointsAtEqualChordLength(newLocationCurve, copyNumber);

                // Get the normals at the new points.
                List<Autodesk.DesignScript.Geometry.Vector> newCurveNormals = Curves.CurveNormalsAtPoints(newLocationCurve, patternPoints);

                // Get the angle of the normals around the Y axis.
                List<float> normalAnglesAroundY = new List<float>();
                foreach(Autodesk.DesignScript.Geometry.Vector vector in newCurveNormals) 
                {
                    normalAnglesAroundY.Add((float)vector.AngleAboutAxis(Autodesk.DesignScript.Geometry.Vector.YAxis(), Autodesk.DesignScript.Geometry.Vector.ZAxis()) + 90);
                }

                // Get the width of the rectangles to be created.
                float actualRectangleWidth = PatternActualWidth(newLocationCurve, rectangleWidth, rectangleRotation);

                // Create the rectangles at the new points.
                for (int i = 0; i < patternPoints.Count; i++) 
                {
                    Rectangle rectangle = BaseRectangle(
                        actualRectangleWidth,
                        rectangleLength,
                        rectangleRotation + normalAnglesAroundY[i],
                        0,
                        false,
                        true,
                        Plane.ByOriginNormal(patternPoints[i], Autodesk.DesignScript.Geometry.Vector.ZAxis())
                    );
                    sideOneRectangles.Add(rectangle);
                }
            }
            else
            {
                sideOneRectangles.Add(null);
            }

            // Create the second side pattern.
            List<object> sideTwoRectangles = new List<object>();
            if (patternSideTwo) 
            {
                // Move the points along the normal vector. To be used to create a new curve.
                List<Autodesk.DesignScript.Geometry.Point> movedPoints = new List<Autodesk.DesignScript.Geometry.Point>();
                for (int i = 0; i < curvePoints.Count; i++)
                {
                    movedPoints.Add(curvePoints[i].Translate(curveNormals[i].Reverse(), patternOffset) as Autodesk.DesignScript.Geometry.Point);
                }

                // Create a new curve from the moved points. This is to accommodate for the lengthening or shortening of the location curve after offset.
                NurbsCurve newLocationCurve = NurbsCurve.ByPoints(movedPoints);

                // Calculate a new copy number based on the new location curve.
                int newCopyNumber = PatternLocationCurveCopyNumber(newLocationCurve, rectangleWidth, rectangleRotation);

                // Add the pattern points to the new curve.
                List<Autodesk.DesignScript.Geometry.Point> patternPoints = Curves.PointsAtEqualChordLength(newLocationCurve, copyNumber);
                sideTwoRectangles.Add(patternPoints);

                // Get the normals at the new points.
                List<Autodesk.DesignScript.Geometry.Vector> newCurveNormals = Curves.CurveNormalsAtPoints(newLocationCurve, patternPoints);

                // Get the angle of the normals around the Y axis.
                List<float> normalAnglesAroundY = new List<float>();
                foreach (Autodesk.DesignScript.Geometry.Vector vector in newCurveNormals)
                {
                    normalAnglesAroundY.Add((float)vector.AngleAboutAxis(Autodesk.DesignScript.Geometry.Vector.YAxis(), Autodesk.DesignScript.Geometry.Vector.ZAxis()) + 90);
                }

                // Get the width of the rectangles to be created.
                float actualRectangleWidth = PatternActualWidth(newLocationCurve, rectangleWidth, rectangleRotation);

                // Create the rectangles at the new points.
                for (int i = 0; i < patternPoints.Count; i++)
                {
                    Rectangle rectangle = BaseRectangle(
                        actualRectangleWidth,
                        rectangleLength,
                        rectangleRotation - normalAnglesAroundY[i],
                        0,
                        true,
                        true,
                        Plane.ByOriginNormal(patternPoints[i], Autodesk.DesignScript.Geometry.Vector.ZAxis())
                    );
                    sideTwoRectangles.Add(rectangle);
                }

            }



            // Combine the pattern rectangles in a list.
            List<List<object>> patternRectangles = new List<List<object>>();
            patternRectangles.Add(sideOneRectangles);
            patternRectangles.Add(sideTwoRectangles);




            return patternRectangles;
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
