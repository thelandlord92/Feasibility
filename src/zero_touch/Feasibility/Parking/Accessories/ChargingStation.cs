using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Parking.Accessories
{
    /// <summary>
    /// Wrapper class for the charging stations.
    /// </summary>
    public class ChargingStation
    {
        /// <summary>
        /// The target plane of the charging stations
        /// </summary>
        public Autodesk.DesignScript.Geometry.Plane TargetPlane { private get; set; }

        private float _height;

        /// <summary>
        /// The height of the charging station.
        /// </summary>
        public float Height 
        {
            get { return _height; }
            set 
            {
                if (value <= 0)
                {
                    throw new ArgumentException("The charging station height must be non zero");
                }
                _height = value;
            }
        }

        private float _width;

        /// <summary>
        /// The width of the charging station. 
        /// </summary>
        public float Width 
        {
            get { return _width; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("The charging station width must be non zero");
                }
                _width = value;
            }
        }

        private float _depth;

        /// <summary>
        /// The depth of the charging station. 
        /// </summary>
        public float Depth 
        {
            get { return _depth; }
            set 
            {
                if (value <= 0) 
                {
                    throw new ArgumentException("The charging station depth must be non zero");
                }
                _depth = value;
            }
        }

        /// <summary>
        /// The angle of the charging station.
        /// </summary>
        public float Angle { private get; set; }

        /// <summary>
        /// The charging station type. 
        /// </summary>
        internal ChargingStationTypes ChargingStationType { get; set; }


        /// <summary>
        /// Creates a charging station instance.
        /// </summary>
        /// <param name="targetPlane">The target plane to transform the charging station.</param>
        /// <param name="height">The height of the charging station.</param>
        /// <param name="width">The width of the charging station.</param>
        /// <param name="depth">The depth of the charging station.</param>
        /// <param name="angle">The angle of the charging station.</param>
        public ChargingStation(
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
        /// Creates the pole mounted charging station.
        /// </summary>
        /// <param name="poleDiameter">Diameter of the charging station's pole.</param>
        /// <param name="chargingBoxHeight">The height of the charging unit.</param>
        /// <returns>The solid of the pole mounted charging station.</returns>
        public Solid CreatePoleMountedChargingStation(
            float poleDiameter = 0.1f,
            float chargingBoxHeight = 0.4f)
        {
            // Check if the pole diameter is zero. 
            if (poleDiameter <= 0) 
            {
                throw new ArgumentException("The pole diameter must be non zero");
            }

            // Check if the charging box is zero or greater or equal to the charging station height.
            if (chargingBoxHeight <= 0 || chargingBoxHeight >= Height) 
            {
                throw new ArgumentException("The charging box height cannot be less than zero or equal to the overall height");
            }

            // Create the base point.
            Autodesk.DesignScript.Geometry.Point basePoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0);

            // Create a plane at the base point. 
            Autodesk.DesignScript.Geometry.Plane basePlane = Autodesk.DesignScript.Geometry.Plane.ByOriginNormal(basePoint, Autodesk.DesignScript.Geometry.Vector.ZAxis());

            // Get the coordinate system of the plane. 
            CoordinateSystem planeCoordSys = basePlane.ContextCoordinateSystem;

            // Create the pole.
            Solid poleSolid = Cylinder.ByRadiusHeight(planeCoordSys, poleDiameter/2, Height);

            // Create the box location point. 
            Autodesk.DesignScript.Geometry.Point boxPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, poleDiameter / 2 + Depth / 2, Height - chargingBoxHeight/2);

            // Create the box.
            Cuboid box = Cuboid.ByLengths(boxPoint, Width, Depth, chargingBoxHeight);

            // Join the box and the pole solid.
            Solid mergedSolids = Solid.ByUnion(new List<Solid>() { box, poleSolid });

            // Create a temp target plane if the target plane input is null.
            Autodesk.DesignScript.Geometry.Plane _targetPlane = null;
            if (TargetPlane == null)
            {
                _targetPlane = Autodesk.DesignScript.Geometry.Plane.ByOriginNormal(basePoint, Autodesk.DesignScript.Geometry.Vector.ZAxis());
            }
            else
            {
                _targetPlane = TargetPlane;
            }

            // Add transformations to the charging station.
            List<Geometry> transformedChargingStation = Common.GeometryTools.GeometryUtilities.AddTransformations(
                new List<Geometry>() { mergedSolids as Geometry },
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0),
                _targetPlane,
                Autodesk.DesignScript.Geometry.Vector.ZAxis(),
                Angle,
                0,
                1
            );

            return transformedChargingStation[0] as Solid;
        }
    }
}
