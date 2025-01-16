using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Accessories
{
    /// <summary>
    /// Wrapper class for the wheel stops.
    /// </summary>
    public class WheelStop
    {
        /// <summary>
        /// The target plane of the wheel stop.
        /// </summary>
        public Autodesk.DesignScript.Geometry.Plane TargetPlane { private get; set; }

        private float _height;

        /// <summary>
        /// The height of the wheel stop.
        /// </summary>
        public float Height
        {
            internal get { return _height; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("The wheel stop height must be non zero");
                }
                _height = value;
            }
        }

        private float _width;

        /// <summary>
        /// The width of the wheel stop. 
        /// </summary>
        public float Width
        {
            internal get { return _width; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("The wheel stop width must be non zero");
                }
                _width = value;
            }
        }

        private float _depth;

        /// <summary>
        /// The depth of the wheel stop. 
        /// </summary>
        public float Depth
        {
            internal get { return _depth; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("The wheel stop depth must be non zero");
                }
                _depth = value;
            }
        }

        /// <summary>
        /// The angle of the wheel stop.
        /// </summary>
        public float Angle { private get; set; }

        /// <summary>
        /// The wheel stop type. 
        /// </summary>
        internal WheelStopTypes WheelStopType { get; set; }


        /// <summary>
        /// Creates a wheel stop instance.
        /// </summary>
        /// <param name="targetPlane">The target plane to transform the wheel stop.</param>
        /// <param name="height">The height of the wheel stop.</param>
        /// <param name="width">The width of the wheel stop.</param>
        /// <param name="depth">The depth of the wheel stop.</param>
        /// <param name="angle"></param>
        public WheelStop(
            Autodesk.DesignScript.Geometry.Plane targetPlane = null,
            float height = 0.1f,
            float width = 1f,
            float depth = 0.1f,
            float angle = 0f)
        {
            TargetPlane = targetPlane;
            Height = height;
            Width = width;
            Depth = depth;
            Angle = angle;
        }


        /// <summary>
        /// Creates the full length wheel stop.
        /// </summary>
        /// <returns name="locationPoint">The wheel stop location point.</returns>
        /// <returns name="locationCurve">Location curve along the width of the wheel stop.</returns>
        /// <returns name="wheelStopSolid">The solid of the wheel stop.</returns>
        [MultiReturn(new[] { "locationPoint", "locationCurve", "wheelStopSolid" })]
        public Dictionary<string, object> CreateFullLengthWheelStop()
        {
            // Create the hosting point. 
            Autodesk.DesignScript.Geometry.Point hostingPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0);

            // Create a plane at the hosting point. 
            Autodesk.DesignScript.Geometry.Plane hostPlane = Autodesk.DesignScript.Geometry.Plane.ByOriginNormal(hostingPoint, Autodesk.DesignScript.Geometry.Vector.ZAxis());

            // Create a base rectangle at the plane.
            Rectangle baseRectangle = Rectangle.ByWidthLength(Width, Depth);

            // Extrude the rectangle.
            Solid solid = baseRectangle.ExtrudeAsSolid(Height);

            // Create the location curve. 
            Autodesk.DesignScript.Geometry.Point startPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(-Width/2, 0, 0);
            Autodesk.DesignScript.Geometry.Point endPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(Width / 2, 0, 0);
            Line locationCurve = Line.ByStartPointEndPoint(startPoint, endPoint);

            // Create a temp target plane if the target plane input is null.
            Autodesk.DesignScript.Geometry.Plane _targetPlane = null;
            if (TargetPlane == null)
            {
                _targetPlane = Autodesk.DesignScript.Geometry.Plane.ByOriginNormal(hostingPoint, Autodesk.DesignScript.Geometry.Vector.ZAxis());
            }
            else
            {
                _targetPlane = TargetPlane;
            }

            // Add transformations to the charging station elements.
            List<Geometry> transformedWheelStop = Common.GeometryTools.GeometryUtilities.AddTransformations(
                new List<Geometry>() { hostingPoint, locationCurve as Geometry , solid as Geometry},
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0),
                _targetPlane,
                Autodesk.DesignScript.Geometry.Vector.ZAxis(),
                Angle,
                0,
                1
            );

            // Add the hosting point, location curve and solid to a dictionary.
            Dictionary<string, object> elementDict = new Dictionary<string, object>();
            elementDict["locationPoint"] = transformedWheelStop[0];
            elementDict["locationCurve"] = transformedWheelStop[1];
            elementDict["wheelStopSolid"] = transformedWheelStop[2];

            // Set the wheel stop type.
            WheelStopType = WheelStopTypes.FullLengthWheelStop;

            return elementDict;
        }


        /// <summary>
        /// Creates the segmented wheel stop.
        /// </summary>
        /// <param name="gapWidth">The center gap width of the wheel stop.</param>
        /// <returns name="locationPoint">The wheel stop location point.</returns>
        /// <returns name="locationCurve">Location curve along the width of the wheel stop.</returns>
        /// <returns name="wheelStopSolid">The solid of the wheel stop.</returns>
        [MultiReturn(new[] { "locationPoint", "locationCurve", "wheelStopSolid" })]
        public Dictionary<string, object> CreateSegmentedWheelStop(float gapWidth = 0.2f)
        {
            // Check if the gap is greater than or equal to the length of the wheel stop. 
            if (gapWidth >= Width) 
            {
                throw new ArgumentException("The gap cannot be greater or equal to the wheel stop width");
            }

            // Get the elements of the full length wheel stop.
            Dictionary<string, object> wheelStop = CreateFullLengthWheelStop();

            // Create solid to subtract from full length wheel stop solid.
            Rectangle rect = Rectangle.ByWidthLength(TargetPlane, gapWidth, Depth);
            Solid subSolid = rect.ExtrudeAsSolid(Height);

            // Rotate the solid. 
            Solid rotatedSolid = subSolid.Rotate(TargetPlane, Angle) as Solid;

            // Subtract the solid from the full length solid.
            Solid segmentedSolid = (wheelStop["wheelStopSolid"] as Solid).Difference(rotatedSolid);

            // Replace the full length solid with the segmented solid. 
            wheelStop["wheelStopSolid"] = segmentedSolid;

            // Set the wheel stop type.
            WheelStopType = WheelStopTypes.SegmentedWheelStop;

            return wheelStop;
        }
    }
}
