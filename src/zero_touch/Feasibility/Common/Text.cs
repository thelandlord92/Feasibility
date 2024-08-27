using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using NUnit.Framework.Constraints;
using ProtoCore.AST.ImperativeAST;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Common
{
    /// <summary>
    /// Wrapper class for text.
    /// </summary>
    public static class Text
    {
        /// <summary>
        /// To create horizontal text hosted at a point. 
        /// </summary>
        /// <param name="text">The text to be created.</param>
        /// <param name="thickness">The thickness of the extruded text solids.</param>
        /// <param name="origin">The host point.</param>
        /// <param name="scale">The scale of the text.</param>
        /// <param name="fontType">The font type to write the text in. The node will revert to Arial if type non existent.</param>
        /// <param name="fontStyle">The font style. Enter Normal, Italic, or Oblique.</param>
        /// <param name="fontWeight">The weight of the font. Enter Thin, Normal, or Bold </param>
        /// <returns name="textSurfaces">The surfaces of the text.</returns>
        /// <returns name="textPolyCurves">The polycurves of the text.</returns>
        /// <returns name="textSolids">The solids of the text.</returns>
        [MultiReturn(new[] { "textSurfaces", "textPolyCurves", "textSolids" })]
        public static Dictionary<string, object> ByStringOriginAndScale(
            string text,
            float thickness,
            [DefaultArgument("Point.ByCoordinates(0, 0)")] Point origin,
            double scale = 5,
            string fontType = "Arial",
            string fontStyle = "Normal",
            string fontWeight = "Normal")
        {
            var crvs = new List<Curve>();

            // Create and run the WPF-related logic on an STA thread
            var thread = new System.Threading.Thread(() =>
            {
                // Configure the text.
                var font = new System.Windows.Media.FontFamily(fontType);

                // Assign the appropriate styling to the text.
                FontStyle _fontStyle;
                if (fontStyle == "Normal")
                {
                    _fontStyle = FontStyles.Normal;
                }
                else if (fontStyle == "Italic")
                {
                    _fontStyle = FontStyles.Italic;
                }
                else if (fontStyle == "Oblique")
                {
                    _fontStyle = FontStyles.Oblique;
                }
                else
                {
                    // Default to Normal if an unrecognized style is provided.
                    _fontStyle = FontStyles.Normal;
                }

                // Assign the text weight.
                FontWeight _fontWeight;
                if (fontWeight == "Normal")
                {
                    _fontWeight = FontWeights.Normal;
                }
                else if(fontWeight == "Thin") 
                { 
                    _fontWeight = FontWeights.Thin;
                }
                else if (fontStyle == "Bold")
                {
                    _fontWeight = FontWeights.Bold;
                }
                else
                {
                    // Default to Normal if an unrecognized weight is provided.
                    _fontWeight = FontWeights.Normal;
                }

                // Use the PixelsPerDip overload
                var formattedText = new FormattedText(
                    text,
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface(
                        font,
                        _fontStyle,
                        _fontWeight,
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

            // Steps below to center the text and produce its polycurve outlines and surfaces.

            // Sort the curve loops into lists.
            List<List<Curve>> lineLists = new List<List<Curve>>();
            List<Curve> crvList = new List<Curve>();

            for (int num = 0; num < crvs.Count - 1; num++)
            {
                if (crvs[num].DoesIntersect(crvs[num + 1]))
                {
                    // Add the current curve to the current group
                    crvList.Add(crvs[num]);
                }
                else
                {
                    // Add the current curve to the current group and then store the group
                    crvList.Add(crvs[num]);
                    lineLists.Add(crvList);
                    crvList = new List<Curve>();  // Reset for the next group
                }
            }

            // Handle the last curve or any remaining group
            if (crvList.Any())
            {
                crvList.Add(crvs.Last());
                lineLists.Add(crvList);
            }

            // Create polycurves from the line lists
            List<PolyCurve> polycurves = new List<PolyCurve>();
            foreach (var lineList in lineLists)
            {
                polycurves.Add(PolyCurve.ByJoinedCurves(lineList, 0.001, false, 0));
            }

            // Create surfaces from the polycurves
            List<Surface> surfaces = new List<Surface>();
            foreach (var polycurve in polycurves)
            {
                surfaces.Add(Surface.ByPatch(polycurve));
            }

            // Group surfaces based on intersections
            List<object> mainList = new List<object>();
            while (surfaces.Any())
            {
                Surface currentSurface = surfaces[0];
                surfaces.RemoveAt(0);

                List<Surface> group = new List<Surface> { currentSurface };
                List<Surface> intersectingSurfaces = new List<Surface>();

                foreach (var otherSurface in surfaces.ToList())
                {
                    if (currentSurface.DoesIntersect(otherSurface))
                    {
                        intersectingSurfaces.Add(otherSurface);
                        surfaces.Remove(otherSurface);
                    }
                }

                // Check for further intersections within the intersecting surfaces.
                foreach (var surface in intersectingSurfaces.ToList())
                {
                    foreach (var otherSurface in surfaces.ToList())
                    {
                        if (surface.DoesIntersect(otherSurface))
                        {
                            intersectingSurfaces.Add(otherSurface);
                            surfaces.Remove(otherSurface);
                        }
                    }
                }

                // Determine the larger surface and group smaller surfaces.
                if (intersectingSurfaces.Any())
                {
                    group.AddRange(intersectingSurfaces);
                    Surface largestSurface = group.OrderByDescending(s => s.Area).First();
                    group.Remove(largestSurface);
                    mainList.Add(new List<object> { largestSurface, group });
                }
                else
                {
                    mainList.Add(currentSurface);
                }
            }

            // Subtract the smaller surfaces from the larger surface
            List<Surface> newSurfaces = new List<Surface>();
            foreach (var surfaceGroup in mainList)
            {
                if (surfaceGroup is Surface singleSurface)
                {
                    newSurfaces.Add(singleSurface);
                }
                else if (surfaceGroup is List<object> surfaceList && surfaceList[0] is Surface largeSurface)
                {
                    List<Surface> smallSurfaces = surfaceList[1] as List<Surface>;
                    Surface subtractedSurface = largeSurface.Difference(smallSurfaces);
                    newSurfaces.Add(subtractedSurface);
                }
            }

            // Create a singular polysurface from the polysurfaces.
            PolySurface polySurface = PolySurface.ByJoinedSurfaces(newSurfaces);

            // Create a bouding box from the polysurface and get the min and max points.
            BoundingBox boundingBox = polySurface.BoundingBox;
            Point minPoint = boundingBox.MinPoint;
            Point maxPoint = boundingBox.MaxPoint;

            // Create a line between the min and max points and get ts center.
            Line line = Line.ByStartPointEndPoint(minPoint, maxPoint);
            Point centerPoint = line.PointAtParameter(0.5);

            // Create a vector between the center point and the input origin.
            Autodesk.DesignScript.Geometry.Vector vector = Autodesk.DesignScript.Geometry.Vector.ByTwoPoints(
               centerPoint, origin);

            // Move the letter surfaces to the origin.
            List<Surface> movedSurfaces = new List<Surface>();
            foreach (Surface surface1 in newSurfaces) 
            { 
                movedSurfaces.Add(surface1.Translate(vector) as Surface);
            }

            // Move the letter polycurves to the origin.
            List<PolyCurve> movedPolyCurves = new List<PolyCurve>();
            foreach (PolyCurve polycurve1 in polycurves)
            {
                movedPolyCurves.Add(polycurve1.Translate(vector) as PolyCurve);
            }


            // Create the text solids.
            List<Solid> solids = new List<Solid>();
            foreach (Surface surface2 in movedSurfaces) 
            { 
                solids.Add(surface2.Thicken(thickness, false));
            }

            return new Dictionary<string, object> 
            {
                { "textSurfaces", movedSurfaces },
                { "textPolyCurves", movedPolyCurves },
                { "textSolids", solids }
            };
        }


        /// <summary>
        /// To create text at a host plane. 
        /// </summary>
        /// <param name="text">The text to be created.</param>
        /// <param name="thickness">The thickness of the extruded text solids.</param>
        /// <param name="plane">The host plane.</param>
        /// <param name="rotation">The rotation of the text around the host plane normal.</param>
        /// <param name="planeOffset">The offset of the text from the host plane.</param>
        /// <param name="scale">The scale of the text.</param>
        /// <param name="fontType">The font type to write the text in. The node will revert to Arial if type non existent.</param>
        /// <param name="fontStyle">The font style. Enter Normal, Italic, or Oblique.</param>
        /// <param name="fontWeight">The weight of the font. Enter Thin, Normal, or Bold </param>
        /// <returns name="textSurfaces">The surfaces of the text.</returns>
        /// <returns name="textPolyCurves">The polycurves of the text.</returns>
        /// <returns name="textSolids">The solids of the text.</returns>
        [MultiReturn(new[] { "textSurfaces", "textPolyCurves", "textSolids" })]
        public static Dictionary<string, object> ByStringPlaneAndScale(
            string text,
            float thickness,
            [DefaultArgument("Plane.XY()")] Plane plane,
            float rotation = 0,
            float planeOffset = 0,
            double scale = 5,
            string fontType = "Arial",
            string fontStyle = "Normal",
            string fontWeight = "Normal")
        {
            // Create the text geometry.
            Dictionary<string, object> textGeometry = ByStringOriginAndScale(
                text,
                thickness,
                Point.ByCoordinates(0, 0),
                scale,
                fontType,
                fontStyle,
                fontWeight
            );

            // Get the text surfaces.
            List<Surface> textSurfaces = textGeometry["textSurfaces"] as List<Surface>;

            // Get the text polycurves.
            List<PolyCurve> textPolyCurves = textGeometry["textPolyCurves"] as List <PolyCurve>;

            // Get the text solids.
            List<Solid> textSolids = textGeometry["textSolids"] as List<Solid>;

            // Cast the text surfaces as geometry.
            List<Autodesk.DesignScript.Geometry.Geometry> castTextSurfaces = new List<Autodesk.DesignScript.Geometry.Geometry>();
            foreach ( var surface in textSurfaces ) 
            {
                castTextSurfaces.Add(surface as Autodesk.DesignScript.Geometry.Geometry);
            }

            // Cast the text polycurves as geometry.
            List<Autodesk.DesignScript.Geometry.Geometry> castTextPolyCurves = new List<Autodesk.DesignScript.Geometry.Geometry>();
            foreach (var polyCurve in textPolyCurves)
            {
                castTextPolyCurves.Add(polyCurve as Autodesk.DesignScript.Geometry.Geometry);
            }

            // Cast the text solids as geometry.
            List<Autodesk.DesignScript.Geometry.Geometry> castTextSolids = new List<Autodesk.DesignScript.Geometry.Geometry>();
            foreach (var solid in textSolids)
            {
                castTextSolids.Add(solid as Autodesk.DesignScript.Geometry.Geometry);
            }

            // Transform the text surfaces.
            List<Autodesk.DesignScript.Geometry.Geometry> transTextSurfaces = Common.GeometryTools.AddTransformations(
                castTextSurfaces,
                Point.ByCoordinates(0, 0),
                plane,
                Autodesk.DesignScript.Geometry.Vector.ZAxis(),
                rotation,
                planeOffset,
                1
            );

            // Transform the text polycurves.
            List<Autodesk.DesignScript.Geometry.Geometry> transTextPolyCurves = Common.GeometryTools.AddTransformations(
                castTextPolyCurves,
                Point.ByCoordinates(0, 0),
                plane,
                Autodesk.DesignScript.Geometry.Vector.ZAxis(),
                rotation,
                planeOffset,
                1
            );

            // Transform the text solids.
            List<Autodesk.DesignScript.Geometry.Geometry> transTextSolids = Common.GeometryTools.AddTransformations(
                castTextSolids,
                Point.ByCoordinates(0, 0),
                plane,
                Autodesk.DesignScript.Geometry.Vector.ZAxis(),
                rotation,
                planeOffset,
                1
            );

            return new Dictionary<string, object> 
            {
                { "textSurfaces", transTextSurfaces },
                { "textPolyCurves", transTextPolyCurves },
                { "textSolids", transTextSolids }
            };

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
