using Autodesk.DesignScript.Geometry;
using ProtoCore.DSASM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feasibility.common
{
    /// <summary>
    /// Wrapper class for the common elements.
    /// </summary>
    public class common
    {
        private common() { }

        /// <summary>
        /// Create a point.
        /// </summary>
        /// <param name="x">the x coordinate</param>
        /// <param name="y">the y coordinate</param>
        /// <param name="z">the z coordinate</param>
        /// <returns name="point">the output point</returns>
        private static Autodesk.DesignScript.Geometry.Point CreatePoint(int x, int y, int z)
        {
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, y, z) as Autodesk.DesignScript.Geometry.Point;

            return point;
        }

        /// <summary>
        /// Creates a plane.
        /// </summary>
        /// <param name="x">the x coordinate</param>
        /// <param name="y">the y coordinate</param>
        /// <param name="z">the z coordinate</param>
        /// <param name="normal">vector indicating the normal direction of the plane</param>
        /// <returns name="plane">the output plane</returns>
        public static Autodesk.DesignScript.Geometry.Plane CreatePlane(int x, int y, int z, Autodesk.DesignScript.Geometry.Vector normal) 
        {
            var point = CreatePoint(x, y, z) as Autodesk.DesignScript.Geometry.Point;

            var plane = Autodesk.DesignScript.Geometry.Plane.ByOriginNormal(point, normal);
            
            return plane;
        }

        /// <summary>
        /// Creates a rectangle.
        /// </summary>
        /// <param name="x">the x coordinate</param>
        /// <param name="y">the y coordinate</param>
        /// <param name="z">the z coordinate</param>
        /// <param name="normal">vector indicating the normal direction of the plane</param>
        /// <param name="width">the width of the rectangle</param>
        /// <param name="length">the length of the rectangle</param>
        /// <returns>the output rectangle</returns>
        public static Autodesk.DesignScript.Geometry.Rectangle CreateRectangle(int x, int y, int z, Autodesk.DesignScript.Geometry.Vector normal, float width, float length) 
        { 
            var rectangle = Autodesk.DesignScript.Geometry.Rectangle.ByWidthLength(CreatePlane(x, y, z, Vector.ZAxis()), width, length);

            return rectangle;
        }
    }
}
