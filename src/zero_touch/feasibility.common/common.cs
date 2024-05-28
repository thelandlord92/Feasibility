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
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns point="Point"></returns>
        public static Autodesk.DesignScript.Geometry.Point CreatePoint(int x, int y, int z)
        {
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, y, z) as Autodesk.DesignScript.Geometry.Point;

            return point;
        }
    }
}
