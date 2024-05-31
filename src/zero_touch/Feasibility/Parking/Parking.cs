using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feasibility.Parking
{
    public class Parking
    {
        // this hides the overall class as a node.
        private Parking() { }

        public static Autodesk.DesignScript.Geometry.Point CreatePoint(int x, int y)
        {
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, y);

            return point;
        }
    }
}
