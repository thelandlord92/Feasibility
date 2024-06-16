using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using CoreNodeModels;

namespace Common
{
    /// <summary>
    /// Wrapper class for geometry.
    /// Contains common geometrical operations.
    /// </summary>
    public class Geometry
    {
        // this hides the overall class as a node.
        private Geometry() { }


        /// <summary>
        /// Get the perimter polycurve of a surface.
        /// </summary>
        /// <param name="surface">The surface input.</param>
        /// <returns name="polyCurve">The closed perimeter polycurve.</returns>
        private static PolyCurve SurfacePerimeter(Surface surface)
        {
            // get the perimeter curve of the layout surface.
            Curve[] perimeterCurves = surface.PerimeterCurves();
            PolyCurve perimeterCurve = PolyCurve.ByJoinedCurves(perimeterCurves, 0.001, false, 0);

            return perimeterCurve;
        }


        /// <summary>
        /// Check the planarity of an input surface.
        /// </summary>
        /// <param name="surface">The input surface.</param>
        /// <returns name="planarSurface">The planar surface.</returns>
        /// <exception cref="ArgumentException"></exception>
        private static Surface CheckSurfacePlanarity(Surface surface) 
        {
            // get the max and min point of the surface bounding box.
            Point minPoint = surface.BoundingBox.MinPoint;
            Point maxPoint = surface.BoundingBox.MaxPoint;

            // check if the surface is planar and horizontal.
            Surface _surface = null;
            if (maxPoint.Z > minPoint.Z)
            {
                throw new ArgumentException(nameof(surface), "The surface must be horizontal and planar.");
            }
            _surface = surface;

            return _surface;
        }


        /// <summary>
        /// Get the plane of a horizontal planar surface.
        /// Returns an error if the surface is not planar and horizontal.
        /// </summary>
        /// <param name="surface">The surface input.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static Plane SurfacePlane(Surface surface)
        {
            // check if the surface is planar and horizontal.
            Surface _surface = CheckSurfacePlanarity(surface);

            // get the perimeter curve of the surface.
            PolyCurve perimeterCurve = SurfacePerimeter(_surface);

            // get the plane of the perimeter curve.
            Plane curvePlane = perimeterCurve.BasePlane();

            return curvePlane;
        }


        /// <summary>
        /// To project curves onto a planar and horizontal surface.
        /// </summary>
        /// <param name="surface">The input surface.</param>
        /// <param name="curve">The input curve.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static Curve ProjectCurves(Surface surface, Curve curve)
        {
            // get the plane of the perimeter curve.
            Plane curvePlane = SurfacePlane(surface);

            // pull the curve onto the plane.
            Curve pulledCurve = curve.PullOntoPlane(curvePlane);

            return pulledCurve;
        }


        /// <summary>
        /// Offset a planar surface and round the edges if required.
        /// </summary>
        /// <param name="surface">The surface input.</param>
        /// <param name="offsetDistance">The perimeter offset distance.</param>
        /// <param name="concaveFillet">The fillet radius at the concave corner.</param>
        /// <param name="convexFillet">The fillet radius at the convex corner.</param>
        /// <returns>The offset surface.</returns>
        /// <exception cref="Exception"></exception>
        public static Surface OffsetSurface(
            Surface surface, 
            float offsetDistance, 
            float concaveFillet = 0, 
            float convexFillet = 0)
        {
            // Check if the surface is planar and horizontal.
            Surface _surface = CheckSurfacePlanarity(surface);

            // Get the surface perimeter curve.
            PolyCurve perimeterCurve = SurfacePerimeter(_surface);

            // offset the perimeter curve.
            Curve[] offsetCurves = perimeterCurve.OffsetMany(offsetDistance, SurfacePlane(_surface).Normal);

            // Offset the perimeter curve.
            List<PolyCurve> joinedCurves = new List<PolyCurve>();
            try
            {
                // Check if the area of the offset curve is greater than that of the original surface.
                foreach (Curve curve in offsetCurves)
                {
                    if (Surface.ByPatch(curve).Area > _surface.Area)
                    {
                        Curve[] offsetCurvesAlt = perimeterCurve.OffsetMany(-offsetDistance, SurfacePlane(surface).Normal);
                        foreach (Curve curveAlt in offsetCurvesAlt)
                        {
                            joinedCurves.Add(curveAlt as PolyCurve);
                        }
                    }
                    else
                    {
                        joinedCurves.Add(curve as PolyCurve);
                    }
                }
            }
            catch 
            {
                throw new Exception("The surface cannot be offset. Reduce the offset distance.");
            }
            // Round the concave corners of the offset curves.
            List<PolyCurve> concaveRoundedCurves = new List<PolyCurve>();
            foreach (PolyCurve curve in joinedCurves)
            {
                if (concaveFillet > 0)
                {
                    try
                    {
                        concaveRoundedCurves.Add(curve.Fillet(concaveFillet, false));
                    }
                    catch
                    {
                        concaveRoundedCurves.Add(curve);
                    }
                }
                else
                {
                    concaveRoundedCurves.Add(curve);
                }
            }

            // Round the convex corners of the concave rounded curves.
            List<PolyCurve> convexRoundedCurves = new List<PolyCurve>();
            foreach (PolyCurve curve in concaveRoundedCurves)
            {
                if (convexFillet > 0)
                {
                    try
                    {
                        convexRoundedCurves.Add(curve.Fillet(convexFillet, true));
                    }
                    catch
                    {
                        convexRoundedCurves.Add(curve);
                    }
                }
                else
                {
                    convexRoundedCurves.Add(curve);
                }
            }

            // Create surfaces from the curve loops.
            List<Surface> offsetSurfaces = new List<Surface>();
            foreach (PolyCurve curve in convexRoundedCurves)
            {
                offsetSurfaces.Add(Surface.ByPatch(curve));
            }

            // Join thes surfaces into a single surface.
            Surface finalSurface = Surface.ByUnion(offsetSurfaces);

            
            return finalSurface;
        }


        /// <summary>
        /// To create a surface along the perimeter of a surface.
        /// </summary>
        /// <param name="surface">The input surface.</param>
        /// <param name="surfaceWidth">The width of the perimeter surface.</param>
        /// <param name="internalConcaveFillet">The fillet radius at the internal concave corners.</param>
        /// <param name="internalConvexFillet">The fillet radius at the internal convex corners.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Surface PerimeterSurface(
            Surface surface, 
            float surfaceWidth, 
            float internalConcaveFillet = 0,
            float internalConvexFillet = 0)
        {
            // create the internal surface for subtraction.
            List<Surface> internalSurfaces = new List<Surface>();
            try
            {
                internalSurfaces.Add(OffsetSurface(surface, surfaceWidth, internalConcaveFillet, internalConvexFillet));
            }
            catch
            {
                throw new Exception("The surface cannot be created. Reduce the surface width.");
            };

            // subtract the internal surface from the primary surface.
            Surface perimeterSurface = surface.Difference(internalSurfaces);

            return perimeterSurface;
        }
    }
}
