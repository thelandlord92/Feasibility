using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

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
        /// <returns name="Point">the output point</returns>
        /// <returns name="Number">the output numbers</returns>
        [MultiReturn(new[] { "Point", "Numbers" })]
        public static Dictionary<string, object> CreatePoint(int x, int y)
        {
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, y);

            var numbers = Common.Math.Range(2, 20, 3);

            return new Dictionary<string, object> 
            { 
                { "Point", point },
                { "Numbers", numbers },
            };
        }
    }
}
