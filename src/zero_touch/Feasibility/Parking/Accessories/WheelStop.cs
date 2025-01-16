using Autodesk.DesignScript.Geometry;
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
            float height = 1f,
            float width = 0.5f,
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
        /// <returns></returns>
        public Solid CreateFullLengthWheelStop()
        {
            // Create the hosting point. 
            Autodesk.DesignScript.Geometry.Point hostingPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0);

            return null;
        }
    }
}
