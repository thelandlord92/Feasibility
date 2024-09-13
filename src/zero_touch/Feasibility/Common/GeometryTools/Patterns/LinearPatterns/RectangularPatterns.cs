using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DesignScript.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.GeometryTools.Patterns.LinearPatterns
{
    /// <summary>
    /// Wrapper class for the linear patterns.
    /// </summary>
    public class RectangularPatterns
    {
       // Hides the overall class as a node.
        private RectangularPatterns() { }


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
            float idealWidth;
            if (rectangleRotation <= 0) 
            {
                idealWidth = rectangleWidth;
            }
            else if (rectangleRotation >= 90)
            {
                idealWidth = rectangleWidth;
            }
            else 
            {
                idealWidth = (float)(rectangleWidth / DSCore.Math.Cos(rectangleRotation));
            }

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
            if (rectangleRotation <= 0 || rectangleRotation >= 90) 
            {
                if (chordLength < idealWidth) // Copy number is zero if the chord length of the location curve is less than the ideal width of the pattern retangle.
                {
                    copyNumber = 1;
                }
                else if (chordLength == idealWidth)
                {
                    copyNumber = 1;
                }
                else
                {
                    copyNumber = (int)DSCore.Math.Floor(locationCurve.Length / rectangleWidth);
                }
            }
            else 
            {
                if (chordLength < idealWidth) // Copy number is zero if the chord length of the location curve is less than the ideal width of the pattern retangle.
                {
                    copyNumber = 1;
                }
                else if (chordLength == idealWidth)
                {
                    copyNumber = 1;
                }
                else
                {
                    copyNumber = (int)DSCore.Math.Floor(locationCurve.Length / idealWidth);
                }
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
            if (rectangleRotation <= 0 || rectangleRotation >= 90) 
            {
                if (chordLength <= idealWidth)
                {
                    actualWidth = chordLength;
                }
                else 
                {
                    actualWidth = (float)DSCore.Math.Cos(rectangleRotation) * locationCurveWidth;
                }
            }
            else
            {
                if (chordLength <= idealWidth)
                {
                    actualWidth = (float)DSCore.Math.Cos(rectangleRotation) * chordLength;
                }
                else
                {
                    actualWidth = (float)DSCore.Math.Cos(rectangleRotation) * locationCurveWidth;
                }
            }
            
            return actualWidth;
        }


        /// <summary>
        /// Adjust the location curve to prevent the pattern rectangles from extending beyond the curve endpoints when rotated.
        /// </summary>
        /// <param name="locationCurve">The input curve to place the pattern rectangles along.</param>
        /// <param name="rectangleLength">Length of the rectangles.</param>
        /// <param name="rectangleRotation">Rotation angle of the rectangle.</param>
        /// <returns name="adjustedCurve">Adjusted location curve.</returns>
        public static Curve AdjustLocationCurveLength(Curve locationCurve, float rectangleLength, float rectangleRotation) 
        {
            // Calculate the distance the rectangle extends beyond the location line when rotated.
            float extensionDistance = (float)DSCore.Math.Cos(90 - rectangleRotation) * rectangleLength;

            // Add a point to the location curve from its end pont.
            Autodesk.DesignScript.Geometry.Point point = locationCurve.PointAtChordLength(extensionDistance, 1, false);

            // Split the location curve with the point.
            List<Curve> curves = locationCurve.SplitByPoints(new List<Autodesk.DesignScript.Geometry.Point>() { point }).ToList();

            // Get the curve intersecting with the start point.
            List<Curve> adjustedLocationCurve = new List<Curve>();
            foreach (Curve curve in curves) 
            {
                if (curve.DoesIntersect(locationCurve.StartPoint)) 
                { 
                    adjustedLocationCurve.Add(curve);
                };
            }

            return adjustedLocationCurve[0];
        }


        /// <summary>
        /// Creates the non interlocking pattern.
        /// </summary>
        /// <param name="locationCurve">The input curve to place the pattern rectangles along.</param>
        /// <param name="rectangleWidth">Width of the rectangles.</param>
        /// <param name="rectangleLength">Length of the rectangles.</param>
        /// <param name="rectangleRotation">Rotation angle of the rectangle. The rotation angle cannot be less than 0 or greater than 90.</param>
        /// <param name="patternOffset">The offset distance of the pattern points from the location line.</param>
        /// <param name="patternSideOne">Turn on/off the first pattern side.</param>
        /// <param name="patternSideTwo">Turn on/off the second pattern side.</param>
        /// <returns name="patternRectangles">Pattern rectangles created along the input location curve.</returns>
        /// <returns name="patternPoints">Placement points of the pattern rectangles.</returns>
        /// <returns name="patternRotation">Rotation values of the placed rectangles.</returns>
        /// <returns name="taperedRectangles">Tapered polycurves.</returns>
        [MultiReturn(new[] { "patternRectangles", "patternPoints", "patternRotation", "taperedRectangles" })]
        public static Dictionary<string, object> NonInterlockingRegularPattern(
            [DefaultArgument("Line.ByStartPointEndPoint(Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0), Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 100, 0))")] Curve locationCurve,
            float rectangleWidth = 2.5f,
            float rectangleLength = 5f,
            float rectangleRotation = 0f,
            float patternOffset = 1f,
            bool patternSideOne = true,
            bool patternSideTwo = true)
        {
            // Check the inputs.

            // Throw an exception if the user turns off both pattern sides.
            if (!patternSideOne && !patternSideTwo) 
            {
                throw new ArgumentException("Both pattern sides cannot be off.");
            }

            // Check the rotation angle.
            if (rectangleRotation <= 0f) 
            { 
                rectangleRotation = 0f;
            }
            else if (rectangleRotation >= 90) 
            {
                rectangleRotation = 0f;
            }

            // Create the adjusted location curve to account.
            Curve adjustedLocationCurve = AdjustLocationCurveLength(locationCurve, rectangleLength, rectangleRotation);

            // Get the pattern copy number.
            int copyNumber = PatternLocationCurveCopyNumber(adjustedLocationCurve, rectangleWidth, rectangleRotation);

            // Create the points along the location curve.
            List<Autodesk.DesignScript.Geometry.Point> _curvePoints = (Curves.PointsAtEqualChordLength(adjustedLocationCurve, copyNumber));
            List<Autodesk.DesignScript.Geometry.Point> curvePoints = _curvePoints.GetRange(1, _curvePoints.Count -1);

            // Get the normals at the points.
            List<Autodesk.DesignScript.Geometry.Vector> curveNormals = Curves.CurveNormalsAtPoints(adjustedLocationCurve, curvePoints);

            // Create the first side pattern.
            List<Rectangle> sideOneRectangles = new List<Rectangle>();
            List<float> sideOneRotation = new List<float>();
            List<Autodesk.DesignScript.Geometry.Point> sideOnePoints = new List<Autodesk.DesignScript.Geometry.Point>();
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
                List<Autodesk.DesignScript.Geometry.Point> patternPointList = Curves.PointsAtEqualChordLength(newLocationCurve, newCopyNumber);
                sideOnePoints.AddRange(patternPointList);

                // Get the normals at the new points.
                List<Autodesk.DesignScript.Geometry.Vector> newCurveNormals = Curves.CurveNormalsAtPoints(newLocationCurve, patternPointList);

                // Calculate the required rotation of the rectangles at the placement points.
                List<float> requiredRotation = new List<float>();
                foreach (Autodesk.DesignScript.Geometry.Vector vector in newCurveNormals)
                {
                    // Calcualte the angle of the normal around the Y axis.
                    float normalAnglesAroundY = (float)vector.AngleAboutAxis(Autodesk.DesignScript.Geometry.Vector.YAxis(), Autodesk.DesignScript.Geometry.Vector.ZAxis());

                    // Add the angle to the input rectangle rotation value.
                    requiredRotation.Add(rectangleRotation + (normalAnglesAroundY + 90));
                }
                sideOneRotation.AddRange(requiredRotation);

                // Get the width of the rectangles to be created.
                float actualRectangleWidth = PatternActualWidth(newLocationCurve, rectangleWidth, rectangleRotation);

                // Create the rectangles at the new points.
                for (int i = 0; i < patternPointList.Count; i++) 
                {
                    Rectangle rectangle = BaseRectangle(
                        actualRectangleWidth,
                        rectangleLength,
                        requiredRotation[i],
                        0,
                        false,
                        true,
                        Plane.ByOriginNormal(patternPointList[i], Autodesk.DesignScript.Geometry.Vector.ZAxis())
                    );
                    sideOneRectangles.Add(rectangle);
                }
            }
            else
            {
                sideOneRectangles.Add(null);
            }

            // Create the second side pattern.
            List<Rectangle> sideTwoRectangles = new List<Rectangle>();
            List<float> sideTwoRotation = new List<float>();
            List<Autodesk.DesignScript.Geometry.Point> sideTwoPoints = new List<Autodesk.DesignScript.Geometry.Point>();
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
                List<Autodesk.DesignScript.Geometry.Point> patternPointList = Curves.PointsAtEqualChordLength(newLocationCurve, newCopyNumber);
                sideTwoPoints.AddRange(patternPointList);

                // Get the normals at the new points.
                List<Autodesk.DesignScript.Geometry.Vector> newCurveNormals = Curves.CurveNormalsAtPoints(newLocationCurve, patternPointList);

                // Calculate the required rotation of the rectangles at the placement points.
                List<float> requiredRotation = new List<float>();
                foreach (Autodesk.DesignScript.Geometry.Vector vector in newCurveNormals)
                {
                    // Calcualte the angle of the normal around the Y axis.
                    float normalAnglesAroundY = (float)vector.AngleAboutAxis(Autodesk.DesignScript.Geometry.Vector.YAxis(), Autodesk.DesignScript.Geometry.Vector.ZAxis());

                    // Subtract the angle from the input rectangle rotation value.
                    requiredRotation.Add(rectangleRotation -  (normalAnglesAroundY + 90));
                }
                sideTwoRotation.AddRange(requiredRotation);

                // Get the width of the rectangles to be created.
                float actualRectangleWidth = PatternActualWidth(newLocationCurve, rectangleWidth, rectangleRotation);

                // Create the rectangles at the new points.
                for (int i = 0; i < patternPointList.Count; i++)
                {
                    Rectangle rectangle = BaseRectangle(
                        actualRectangleWidth,
                        rectangleLength,
                        requiredRotation[i],
                        0,
                        true,
                        true,
                        Plane.ByOriginNormal(patternPointList[i], Autodesk.DesignScript.Geometry.Vector.ZAxis())
                    );
                    sideTwoRectangles.Add(rectangle);
                }
            }

            // Logic for the tapered rectangles.

            // Get the pattern copy number.
            int taperedCopyNumber = PatternLocationCurveCopyNumber(locationCurve, rectangleWidth, rectangleRotation);

            // Create the points along the location curve.
            List<Autodesk.DesignScript.Geometry.Point> taperedCurvePoints = (Curves.PointsAtEqualChordLength(locationCurve, taperedCopyNumber));

            // Get the normals at the points.
            List<Autodesk.DesignScript.Geometry.Vector> taperedRectangleNormals = Curves.CurveNormalsAtPoints(locationCurve, taperedCurvePoints);

            List<PolyCurve> sideOneTaperedRectangles = new List<PolyCurve>();
            if (patternSideOne) 
            {
                // Move the points along the normal vector. To be used to create the inner and outer curve offset points.
                List<Autodesk.DesignScript.Geometry.Point> innerMovedPoints = new List<Autodesk.DesignScript.Geometry.Point>();
                List<Autodesk.DesignScript.Geometry.Point> outerMovedPoints = new List<Autodesk.DesignScript.Geometry.Point>();
                for (int i = 0; i < taperedCurvePoints.Count; i++)
                {
                    innerMovedPoints.Add(taperedCurvePoints[i].Translate(taperedRectangleNormals[i], patternOffset) as Autodesk.DesignScript.Geometry.Point);
                    outerMovedPoints.Add(taperedCurvePoints[i].Translate(taperedRectangleNormals[i], patternOffset + rectangleLength) as Autodesk.DesignScript.Geometry.Point);
                }

                // Create the inner and outer curves.
                Curve innerCurve = NurbsCurve.ByPoints(innerMovedPoints);
                Curve outerCurve = NurbsCurve.ByPoints(outerMovedPoints);

                // Split the inner and outer curve using the moved points.
                List<Curve> splitInnerCurves = innerCurve.SplitByPoints(innerMovedPoints).ToList();
                List<Curve> splitOuterCurves = outerCurve.SplitByPoints(outerMovedPoints).ToList();

                // Create surfaces by lofting between the split curves.
                List<Surface> surfaces = new List<Surface>();
                for (int i = 0;i < splitInnerCurves.Count; i++) 
                { 
                    surfaces.Add(Surface.ByLoft(new List<Curve> { splitInnerCurves[i], splitOuterCurves[i] }));
                }

                // Get the perimeter polycurves of the surfaces.
                foreach (Surface surface in surfaces) 
                {
                    sideOneTaperedRectangles.Add(Common.GeometryTools.Surfaces.SurfacePerimeter(surface));
                }
            }

            List<PolyCurve> sideTwoTaperedRectangles = new List<PolyCurve>();
            if (patternSideTwo)
            {
                // Move the points along the normal vector. To be used to create the inner and outer curve offset points.
                List<Autodesk.DesignScript.Geometry.Point> innerMovedPoints = new List<Autodesk.DesignScript.Geometry.Point>();
                List<Autodesk.DesignScript.Geometry.Point> outerMovedPoints = new List<Autodesk.DesignScript.Geometry.Point>();
                for (int i = 0; i < taperedCurvePoints.Count; i++)
                {
                    innerMovedPoints.Add(taperedCurvePoints[i].Translate(taperedRectangleNormals[i].Reverse(), patternOffset) as Autodesk.DesignScript.Geometry.Point);
                    outerMovedPoints.Add(taperedCurvePoints[i].Translate(taperedRectangleNormals[i].Reverse(), patternOffset + rectangleLength) as Autodesk.DesignScript.Geometry.Point);
                }

                // Create the inner and outer curves.
                Curve innerCurve = NurbsCurve.ByPoints(innerMovedPoints);
                Curve outerCurve = NurbsCurve.ByPoints(outerMovedPoints);

                // Split the inner and outer curve using the moved points.
                List<Curve> splitInnerCurves = innerCurve.SplitByPoints(innerMovedPoints).ToList();
                List<Curve> splitOuterCurves = outerCurve.SplitByPoints(outerMovedPoints).ToList();

                // Create surfaces by lofting between the split curves.
                List<Surface> surfaces = new List<Surface>();
                for (int i = 0; i < splitInnerCurves.Count; i++)
                {
                    surfaces.Add(Surface.ByLoft(new List<Curve> { splitInnerCurves[i], splitOuterCurves[i] }));
                }

                // Get the perimeter polycurves of the surfaces.
                foreach (Surface surface in surfaces)
                {
                    sideTwoTaperedRectangles.Add(Common.GeometryTools.Surfaces.SurfacePerimeter(surface));
                }
            }


            // Combine the pattern rectangles in a list.
            List<List<Rectangle>> patternRectangles = new List<List<Rectangle>>();
            patternRectangles.Add(sideOneRectangles);
            patternRectangles.Add(sideTwoRectangles);

            // Combine the pattern points in a list.
            List<List<Autodesk.DesignScript.Geometry.Point>> patternPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            patternPoints.Add(sideOnePoints);
            patternPoints.Add(sideTwoPoints);

            // Combine the pattern rotation values in a list.
            List<List<float>> patternRotation = new List<List<float>>();
            patternRotation.Add(sideOneRotation);
            patternRotation.Add(sideTwoRotation);

            // Combine the tapered rectangles in a list.
            List<List<PolyCurve>> taperedRectangles = new List<List<PolyCurve>>();
            taperedRectangles.Add(sideOneTaperedRectangles);
            taperedRectangles.Add(sideTwoTaperedRectangles);

            return new Dictionary<string, object> 
            {
                { "patternRectangles", patternRectangles },
                { "patternPoints", patternPoints },
                { "patternRotation", patternRotation },
                { "taperedRectangles", taperedRectangles }
            };
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
