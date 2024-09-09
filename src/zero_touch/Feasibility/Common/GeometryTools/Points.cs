using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.GeometryTools
{
    /// <summary>
    /// Wrapper class for points.
    /// </summary>
    public class Points
    {
        // Hides the overall class as a node.
        private Points() { }


        /// <summary>
        /// Get the XYZ values of points.
        /// </summary>
        /// <param name="points">The input point list.</param>
        /// <returns name="xyzValues">The point XYZ values.</returns>
        public static List<double[]> CreatePointXYZ(List<Autodesk.DesignScript.Geometry.Point> points) 
        {
            List<double[]> pointsXYZ = new List<double[]> { };
            foreach (Autodesk.DesignScript.Geometry.Point point in points) 
            {
                // List to hold the point XYZ values.
                double[] pointXYZ = new double[] { point.X, point.Y, point.Z };
                pointsXYZ.Add(pointXYZ);
            }

            return pointsXYZ;
        }
    }
}
