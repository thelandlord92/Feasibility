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
        /// <returns name="Point">the created point</returns>
        /// <returns name="Number">the random number</returns>
        public static Dictionary<string, object> CreatePoint(int x, int y)
        {
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, y);

            var intNumber = 25;

            // Create a dictionary to hold the outputs.
            var outputs = new Dictionary<string, object>();

            // Add the various outputs to the dictionary.
            outputs["Point"] = point as Autodesk.DesignScript.Geometry.Point;
            outputs["Number"] = intNumber;

            return outputs;
        }
    }
}
