using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking
{
    /// <summary>
    /// Wrapper class for bounding rectangle.
    /// </summary>
    public class BoundingRectangle
    {
        // this hides the overall class as a node.
        private BoundingRectangle() { }


        /// <summary>
        /// Creates a bounding rectangle around a polycurve. Thanks to Jacob Small for the logic.
        /// </summary>
        /// <param name="curve">The polycurve to create the bounding rectangle around.</param>
        /// <param name="rotation">The rotation value of the created bounding rectangle.</param>
        /// <returns name="boundingRectangle">The bounding rectangle.</returns>
        public static Rectangle CreateBoundingRectangle(PolyCurve curve, float rotation) 
        {
            // rotate the polycurve.
            PolyCurve rotatedCurve = curve.Rotate(Plane.XY(), -rotation) as PolyCurve;

            // get the bounding box of the curve.
            BoundingBox boundingBox = rotatedCurve.BoundingBox;

            // create a diagonal vector using the min and max points of the bounding box.
            Point minPoint = boundingBox.MinPoint;  
            Point maxPoint = boundingBox.MaxPoint;  
            Vector diagonalVector = Vector.ByTwoPoints(minPoint, maxPoint);

            // get the center point along the vector.
            Point centerPoint = minPoint.Translate(diagonalVector.Scale(0.5)) as Point;

            // create a rectangle at the xy plane using the x and y components of the diagonal vector.
            Rectangle rectangle = Rectangle.ByWidthLength(diagonalVector.X, diagonalVector.Y);

            // Move the rectangle to the diagonal vector center point.
            Rectangle movedRectangle = rectangle.Translate(centerPoint.AsVector()) as Rectangle;

            // Rotate the rectangle back to the polycurve around the xy plane.
            Rectangle rotatedRectangle = movedRectangle.Rotate(Plane.XY(), rotation) as Rectangle;

            return rotatedRectangle;
        }
    }
}
