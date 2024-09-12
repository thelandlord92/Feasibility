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
    public class RectangularPatterns
    {
        /// <summary>
        /// The input curve to place the pattern rectangles along.
        /// </summary>
        internal Curve LocationCurve { get; set; }

        
        private float _rectangleWidth;

        /// <summary>
        /// Width of the pattern rectangles.
        /// </summary>
        internal float RectangleWidth 
        { 
            get { return _rectangleWidth; }
            set 
            { 
                if (value <= 0) 
                {
                    throw new ArgumentException("The rectangle width must be greater than zero");
                }
                _rectangleWidth = value;
            }
        }


        private float _rectangleLength;

        /// <summary>
        /// Length of the pattern rectangles.
        /// </summary>
        internal float RectangleLength
        {
            get { return _rectangleLength; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("The rectangle length must be greater than zero");
                }
                _rectangleLength = value;
            }
        }


        private float _rectangleRotation;

        /// <summary>
        /// Rotation angle of the pattern rectangles.
        /// The rotation angle cannot be less than 0 or greater than 90 degrees.
        /// </summary>
        internal float RectangleRotation
        {
            get { return _rectangleRotation; }
            set
            {
                if (value <= 0)
                {
                    _rectangleRotation = 0;
                }
                if (value >= 90)
                {
                    _rectangleRotation = 90;
                }
                _rectangleRotation = value;
            }
        }


        private bool _patternSideOne;
        private bool _patternSideTwo;


        /// <summary>
        /// Turn on/off the first side pattern.
        /// </summary>
        internal bool PatternSideOne
        {
            get { return _patternSideOne; }
            set
            {
                if (!value && !_patternSideTwo)
                {
                    throw new InvalidOperationException("Both pattern sides cannot be off.");
                }
                _patternSideOne = value;
            }
        }


        /// <summary>
        /// Turn on/off the second side pattern.
        /// </summary>
        internal bool PatternSideTwo
        {
            get { return _patternSideTwo; }
            set
            {
                if (!value && !_patternSideOne)
                {
                    throw new InvalidOperationException("Both pattern sides cannot be off.");
                }
                _patternSideTwo = value;
            }
        }


        /// <summary>
        /// Create instances of the rectangular patterns.
        /// </summary>
        /// <param name="locationCurve">The input curve to place the pattern rectangles along</param>
        /// <param name="rectangleWidth">Width of the pattern rectangles</param>
        /// <param name="rectangleLength">Length of the pattern rectangles</param>
        /// <param name="rectangleRotation">Rotation angle of the pattern rectangles</param>
        /// <param name="patternSideOne">Turn on/off the first side pattern.</param>
        /// <param name="patternSideTwo">Turn on/off the second side pattern.</param>
        public RectangularPatterns(
            Curve locationCurve,
            float rectangleWidth = 2.5f,
            float rectangleLength = 5f,
            float rectangleRotation = 0f,
            bool patternSideOne = true,
            bool patternSideTwo = true) 
        { 
            LocationCurve = locationCurve;
            RectangleWidth = rectangleWidth;
            RectangleLength = rectangleLength;
            PatternSideOne = patternSideOne;
            PatternSideTwo = patternSideTwo;
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
        /// <returns name="patternRectangles">Pattern rectangles created along the input location curve.</returns>
        /// <returns name="patternPoints">Placement points of the pattern rectangles.</returns>
        /// <returns name="patternRotation">Rotation values of the placed rectangles.</returns>
        /// <returns name="taperedPolyCurves">Tapered polycurves.</returns>
        [MultiReturn(new[] { "patternRectangles", "patternPoints", "patternRotation", "taperedPolyCurves" })]
        public static Dictionary<string, object> NonInterlockingRegularPattern(
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
            List<Rectangle> sideOneRectangles = new List<Rectangle>();
            List<float> sideOneRotation = new List<float>();
            List<Autodesk.DesignScript.Geometry.Point> sideOnePoints = new List<Autodesk.DesignScript.Geometry.Point>();
            List<PolyCurve> sideOneTaperedPolyCurves = new List<PolyCurve>();
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
            List<PolyCurve> sideTwoTaperedPolyCurves = new List<PolyCurve>();
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
            List<List<PolyCurve>> taperedPolyCurves = new List<List<PolyCurve>>();
            taperedPolyCurves.Add(sideOneTaperedPolyCurves);
            taperedPolyCurves.Add(sideTwoTaperedPolyCurves);

            return new Dictionary<string, object> 
            {
                { "patternRectangles", patternRectangles },
                { "patternPoints", patternPoints },
                { "patternRotation", patternRotation },
                { "taperedPolyCurves", taperedPolyCurves }
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
