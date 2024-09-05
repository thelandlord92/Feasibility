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
    /// Wrapper class for boundary patterns.
    /// Contains patterns generated within a closed curve.
    /// </summary>
    public class BoundaryPatterns
    {
        // Hides the overall class as a node.
        private BoundaryPatterns() { }


        /// <summary>
        /// To create a diagonal hatch pattern.
        /// </summary>
        /// <param name="curve">The curve within which to define the hatch pattern.</param>
        /// <param name="borderThickness">The thickness of the pattern's border.</param>
        /// <param name="hatchThickness">The thickness of the diagonal hatches.</param>
        /// <param name="hatchSpacing">The spacing between the hatches.</param>
        /// <param name="hatchRotation">The rotaton of the hatch pattern.</param>
        /// <returns name="hatchSurface">The surface of the hatch.</returns>
        /// <returns name="hatchOutlines">The perimeter curves of the hatch surface.</returns>
        /// <exception cref="Exception"></exception>
        [MultiReturn(new[] { "hatchSurface", "hatchOutlines" })]
        public static Dictionary<string, object> DiagonalHatchPattern(
            Curve curve,
            float borderThickness = 1f,
            float hatchThickness = 1f,
            float hatchSpacing = 5f,
            float hatchRotation = 45f)
        {
            // Throw excpetion if inputs less than 0.001.
            if (borderThickness < 0.001 || hatchThickness < 0.001 || hatchSpacing < 0.001)
            {
                throw new ArgumentException("borderThickness, hatchThickness, and hatchSpacing cannot be less than 0.001");
            }

            // Check if the input curve is closed.
            if (curve.IsClosed == false)
            {
                throw new ArgumentException("The input curve must be closed");
            }

            // Check the planarity of the input polycuve.
            Curve curve1 = Common.GeometryTools.Curves.CheckCurvePlanarity(curve);

            // Create a surface from the curve.
            Surface surface = Surface.ByPatch(curve1);

            // Add the perimeter surface.
            Surface perimeterSurface = Common.GeometryTools.Surfaces.PerimeterSurface(surface, borderThickness) as Surface;

            // Create the bounding rectangle.
            Rectangle boundingRectangle = Common.GeometryTools.Curves.CreateBoundingRectangle(curve1 as PolyCurve, hatchRotation);

            // Create the hatch setout lines.
            List<Line> lines = Common.GeometryTools.Curves.SetOutLines(boundingRectangle, hatchSpacing, hatchSpacing);

            // Create the hatch outlines.
            List<PolyCurve> hatchOutlines = new List<PolyCurve>();
            foreach (Line line in lines)
            {
                hatchOutlines.Add(PolyCurve.ByThickeningCurveNormal(line, hatchThickness, Autodesk.DesignScript.Geometry.Vector.ZAxis()));
            }

            // Create surfaces from the hatch outlines.
            List<Surface> hatchSurfaces = new List<Surface>();
            foreach (PolyCurve hatchOutline in hatchOutlines)
            {
                hatchSurfaces.Add(Surface.ByPatch(hatchOutline));
            }

            // Intersect the hatch surfaces with hatch area surface.
            List<Surface> intersectedSurfaces = new List<Surface>();
            foreach (Surface hatchSurface in hatchSurfaces)
            {
                // Cast the overall and hatch surfaces as geometry.
                Autodesk.DesignScript.Geometry.Geometry castSurface = surface as Autodesk.DesignScript.Geometry.Geometry;
                Autodesk.DesignScript.Geometry.Geometry castHatchSurface = hatchSurface as Autodesk.DesignScript.Geometry.Geometry;

                // Intersect the overall and hatch surfaces.
                Autodesk.DesignScript.Geometry.Geometry[] surfaceList = castSurface.Intersect(castHatchSurface);

                foreach (Autodesk.DesignScript.Geometry.Geometry geometry in surfaceList)
                {
                    intersectedSurfaces.Add((Surface)geometry);
                }
            }

            // Join the perimeter surface and the intersected hatch surfaces.
            List<Surface> combinedSurfaces = new List<Surface>();
            combinedSurfaces.AddRange(new List<Surface> { perimeterSurface });
            combinedSurfaces.AddRange(intersectedSurfaces);
            Surface joinedSurface = Surface.ByUnion(combinedSurfaces);

            // Get the perimeter curves of the joined surface.
            Curve[] perimeterCurves = joinedSurface.PerimeterCurves();

            return new Dictionary<string, object>
            {
                { "hatchSurface", combinedSurfaces },
                { "hatchOutlines", perimeterCurves }
            };
        }

    }
}
