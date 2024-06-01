using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking
{
    /// <summary>
    /// Wrapper class for the parking patterns.
    /// </summary>
    public class Patterns
    {
        // this hides the overall class as a node.
        private Patterns() { }

        /// <summary>
        /// Creates a point.
        /// </summary>
        /// <param name="x">the x coordinate value</param>
        /// <param name="y">the y coordinate value</param>
        /// <returns name="point">the created point</returns>
        /// <returns name="number">the random number</returns>
        public static Tuple<Point, int> CreatePoint(int x, int y)
        {
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, y);

            var number = 25;


            return Tuple.Create(point, number);
        }
    }
}
