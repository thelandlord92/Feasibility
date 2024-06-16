using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <returns></returns>
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
            // check if the surface is planar and horizontal.
            Surface _surface = CheckSurfacePlanarity(surface);

            // get the surface perimeter curves.
            PolyCurve perimeterCurve = SurfacePerimeter(_surface);

            // offset the perimeter curve.
            PolyCurve offsetCurve;
            try
            {
                Curve[] offsetCurves = perimeterCurve.OffsetMany(offsetDistance, SurfacePlane(_surface).Normal);
                offsetCurve = PolyCurve.ByJoinedCurves(offsetCurves, 0.001, false, 0);
            }
            catch 
            {
                throw new Exception("The surface cannot be offset. Reduce the offset distance.");
            };

            // round the concave edges of the offset curve.
            PolyCurve concaveRoundedCurve;
            if (concaveFillet <= 0) 
            {
                concaveRoundedCurve = offsetCurve;
            }
            else if (concaveFillet > 0)
            {
                try
                {
                    concaveRoundedCurve = offsetCurve.Fillet(concaveFillet, false);
                }
                catch 
                {
                    concaveRoundedCurve = offsetCurve;
                }
            }
            
            // create the offset surface.
            Surface offsetSurface = Surface.ByPatch(offsetCurve);  

            return offsetSurface;
        }


        /// <summary>
        /// To create a surface along the perimeter of a surface.
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="surfaceWidth"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Surface PerimeterSurface(Surface surface, float surfaceWidth)
        {
            // create the internal surface for subtraction.
            List<Surface> internalSurfaces = new List<Surface>();
            try
            {
                internalSurfaces.Add(OffsetSurface(surface, surfaceWidth));
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
