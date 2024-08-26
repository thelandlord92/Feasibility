using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Common
{
    /// <summary>
    /// Wrapper class for text.
    /// </summary>
    public static class Text
    {
        /// <summary>
        /// To create text in the Dynamo work space. 
        /// </summary>
        /// <param name="text">The text to be created.</param>
        /// <param name="origin">The host point.</param>
        /// <param name="scale">The scale of the text.</param>
        /// <param name="fontType">The font type to write the text in. The node will revert to Arial if type non existent.</param>
        /// <returns></returns>
        public static IEnumerable<Curve> OutlinesFromStringOriginAndScale(
            string text,
            [DefaultArgument("Point.ByCoordinates(0, 0)")] Point origin, 
            double scale = 5,
            string fontType = "Arial")
        {
            var crvs = new List<Curve>();

            // Create and run the WPF-related logic on an STA thread
            var thread = new System.Threading.Thread(() =>
            {
                var font = new System.Windows.Media.FontFamily(fontType);
                var fontStyle = FontStyles.Normal;
                var fontWeight = FontWeights.Medium;

                // Use the PixelsPerDip overload
                var formattedText = new FormattedText(
                    text,
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface(
                        font,
                        fontStyle,
                        fontWeight,
                        FontStretches.Normal),
                    1,
                    System.Windows.Media.Brushes.Black,  // This brush does not matter since we use the geometry of the text. 
                    96.0 // Assuming standard DPI; adjust this as needed
                );

                // Build the geometry object that represents the text.
                var textGeometry = formattedText.BuildGeometry(new System.Windows.Point(0, 0));
                foreach (var figure in textGeometry.GetFlattenedPathGeometry().Figures)
                {
                    var a = figure.StartPoint;
                    System.Windows.Point b;
                    foreach (var segment in figure.GetFlattenedPathFigure().Segments)
                    {
                        if (segment is LineSegment lineSeg)
                        {
                            b = lineSeg.Point;
                            var crv = LineBetweenPoints(origin, scale, a, b);
                            a = b;
                            crvs.Add(crv);
                        }
                        else if (segment is PolyLineSegment plineSeg)
                        {
                            foreach (var segPt in plineSeg.Points)
                            {
                                var crv = LineBetweenPoints(origin, scale, a, segPt);
                                a = segPt;
                                crvs.Add(crv);
                            }
                        }
                    }
                }
            });

            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();

            //

            return crvs;
        }

        private static Line LineBetweenPoints(Point origin, double scale, System.Windows.Point a, System.Windows.Point b)
        {
            var pt1 = Point.ByCoordinates((a.X * scale) + origin.X, ((-a.Y + 1) * scale) + origin.Y, origin.Z);
            var pt2 = Point.ByCoordinates((b.X * scale) + origin.X, ((-b.Y + 1) * scale) + origin.Y, origin.Z);
            var crv = Line.ByStartPointEndPoint(pt1, pt2);
            return crv;
        }
    }
}
