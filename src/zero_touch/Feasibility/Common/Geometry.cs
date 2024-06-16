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
        /// Get the plane of a surface.
        /// </summary>
        /// <param name="surface">The surface input.</param>
        /// <returns></returns>
        private static Plane SurfacePlane(Surface surface)
        {
            // get the max and min point of the surface bounding box.
            Point minPoint = surface.BoundingBox.MinPoint;
            Point maxPoint = surface.BoundingBox.MaxPoint;

            // check if the surface is planar and horizontal.
            Surface _surface = null;
            if (maxPoint.Z > minPoint.Z)
            {
                throw new ArgumentOutOfRangeException(nameof(surface), "The surface must be horizontal and planar.");
            }
            _surface = surface;

            // get the perimeter curve of the layout surface.
            PolyCurve perimeterCurve = SurfacePerimeter(_surface);

            // get the plane of the perimeter curve.
            Plane curvePlane = perimeterCurve.BasePlane();

            return curvePlane;
        }


        /// <summary>
        /// To project curves onto a surface.
        /// </summary>
        /// <param name="surface">The input surface.</param>
        /// <param name="curve">The input curve.</param>
        /// <returns></returns>
        private static Curve ProjectCurves(Surface surface, Curve curve)
        {
            // get the plane of the perimeter curve.
            Plane curvePlane = SurfacePlane(surface);

            // pull the curve onto the plane.
            Curve pulledCurve = curve.PullOntoPlane(curvePlane);

            return pulledCurve;
        }
    }
}
