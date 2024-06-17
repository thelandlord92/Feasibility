using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Common;

namespace Parking
{
    /// <summary>
    /// Wrapper class for the internal parking bays.
    /// </summary>
    public class InternalBays
    {
        private Surface _layoutArea;

        /// <summary>
        /// A horizontal planar surface representing the parking area.
        /// </summary>
        public Surface LayoutArea 
        {
            private get { return _layoutArea; }
            set 
            {
                Point minPoint = value.BoundingBox.MinPoint;
                Point maxPoint = value.BoundingBox.MaxPoint;

                if (maxPoint.Z > minPoint.Z) 
                {
                    throw new ArgumentOutOfRangeException(nameof(_layoutArea), "The layout surface must be horizontal and planar.");
                }
                _layoutArea = value;
            }
        }


        private Surface _exclusionArea;

        /// <summary>
        /// Horizontal planar surfaces representing the exlusion zones.
        /// </summary>
        public Surface ExclusionArea 
        {
            private get { return _exclusionArea; }
            set 
            {
                Point minPoint = value.BoundingBox.MinPoint;
                Point maxPoint = value.BoundingBox.MaxPoint;

                if (maxPoint.Z > minPoint.Z)
                {
                    throw new ArgumentOutOfRangeException(nameof(_exclusionArea), "The exclusion area surface must be horizontal and planar.");
                }
                _exclusionArea = value;
            }
        }


        private Curve _axialRoute;

        /// <summary>
        /// Axial route curves to mark primary access routes around the parking layout.
        /// </summary>
        public Curve AxialRoute 
        {
            private get { return _axialRoute;  }
            set 
            {
                if (value.IsPlanar == false)
                {
                    throw new ArgumentException(nameof(_axialRoute), "The axial route curve must be planar.");
                }
                else if (value.EndPoint.Z > value.StartPoint.Z) 
                {
                    throw new ArgumentException(nameof(_axialRoute), "The axial route curve must be horizontal.");
                }
                _axialRoute = value;
            }
        }


        private int _patternType;

        /// <summary>
        /// Set the parking pattern type.
        /// 1 for the non interlocking pattern.
        /// 2 for the interlocking pattern.
        /// 3 for the herringbone pattern.
        /// </summary>
        public int PatternType
        {
            private get { return _patternType; }
            set
            {
                if (value < 1 || value > 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(PatternType), "PatternType must be between 1 an 3.");
                }
                _patternType = value;
            }
        }


        private int _aisleType;

        /// <summary>
        /// Set the internal parking aisle type.
        /// 1 for one way traffic.
        /// 2 for two way traffic.
        /// </summary>
        public int AisleType 
        {
            private get { return _aisleType; }
            set 
            {
                if (value < 1 || value > 2) 
                { 
                    throw new ArgumentOutOfRangeException(nameof(AisleType), "AisleType must be 1 or 2.");
                }
            }
        }


        /// <summary>
        /// The width of the axial routes.
        /// </summary>
        public float AxialRouteWidth { private get; set; }


        /// <summary>
        /// The width of the aisle around the exclusion zone.
        /// </summary>
        public float ExclusionZoneAisleWidth { private get; set; }


        /// <summary>
        /// The width of the walkway around the exclusion zone.
        /// </summary>
        public float ExclusionZoneWalkwayWidth { private get; set; }


        /// <summary>
        /// The vehicular turning radius at the convex corners of the perimeter aisle.
        /// </summary>
        public float ExternalTurningRadius { private get; set; }


        /// <summary>
        /// The vehicular turning radius at the concave corners of the perimeter aisle.
        /// </summary>
        public float InternalTurningRadius { private get; set; }


        /// <summary>
        /// The radius at the corners of the internal parking islands.
        /// </summary>
        public float IslandCornerRadius { private get; set; }


        /// <summary>
        /// the height of the parking islands above the layout level.
        /// </summary>
        public float IslandHeight { private get; set; }


        /// <summary>
        /// The rotation of the internal parking layout.
        /// </summary>
        public float LayoutRotation { private get; set; }   


        /// <summary>
        /// The width of the perimeter aisle.
        /// </summary>
        public float PerimeterAisleWidth { private get; set; }


        /// <summary>
        /// The depth of the perimeter parking if required.
        /// </summary>
        public float PerimeterBayDepth { private get; set; }


        /// <summary>
        /// The width of the perimeter walkway.
        /// </summary>
        public float PerimeterWalkWayWidth { private get; set; }


        /// <summary>
        /// Creates internal parking layout instances.
        /// </summary>
        /// <param name="layoutArea"></param>
        /// <param name="patternType"></param>
        /// <param name="aisleType"></param>
        /// <param name="externalTurningRadius"></param>
        /// <param name="internalTurningRadius"></param>
        /// <param name="islandCornerRadius"></param>
        /// <param name="islandHeight"></param>
        /// <param name="layoutRotation"></param>
        /// <param name="perimeterAisleWidth"></param>
        /// <param name="perimeterBayDepth"></param>
        /// <param name="perimeterWalkwayWidth"></param>
        public InternalBays(
            Surface layoutArea,
            // Surface exclusionArea,
            // Curve axialRoute,
            int patternType = 1,
            int aisleType = 1,
            // float axialRouteWidth = 7,
            // float exclusionZoneAisleWidth = 7,
            // float exclusionZoneWalkwayWidth = 1.5f,
            float externalTurningRadius = 5,
            float internalTurningRadius = 3.5f,
            float islandCornerRadius = 1,
            float islandHeight = 1,
            float layoutRotation = 45,
            float perimeterAisleWidth = 7,
            float perimeterBayDepth = 5,
            float perimeterWalkwayWidth = 1)
        { 
            LayoutArea = layoutArea;
            // ExclusionArea = exclusionArea;
            // AxialRoute = axialRoute;
            PatternType = patternType;
            AisleType = aisleType;
            // AxialRouteWidth = axialRouteWidth;
            // ExclusionZoneAisleWidth = exclusionZoneAisleWidth;
            // ExclusionZoneWalkwayWidth = exclusionZoneWalkwayWidth;
            ExternalTurningRadius = externalTurningRadius;
            InternalTurningRadius = internalTurningRadius;
            IslandCornerRadius = islandCornerRadius;
            IslandHeight = islandHeight;
            LayoutRotation = layoutRotation;
            PerimeterAisleWidth = perimeterAisleWidth;
            PerimeterBayDepth = perimeterBayDepth;
            PerimeterWalkWayWidth = perimeterWalkwayWidth;
        } 


        /// <summary>
        /// Creates the parking layout surface.
        /// </summary>
        /// <param name="layoutPlane">The plane to project the parking surface onto.</param>
        /// <returns name="layoutSurface">The parking layout surface.</returns>
        public Surface CreateLayoutSurface([DefaultArgument("Plane.XY()")] Plane layoutPlane) 
        {
            // check the planarity of the input surface.
            Surface surface = Common.Geometry.CheckSurfacePlanarity(LayoutArea);

            // pull the surface onto the input plane.
            Surface layoutSurface = Common.Geometry.PullSurfaceToPlane(surface, layoutPlane);

            return layoutSurface;
        }


        /// <summary>
        /// Creates the internal parking area surface.
        /// </summary>
        /// <param name="layoutPlane">The plane to project the parking surface onto.</param>
        /// <param name="concaveFillet">The fillet radius at the concave corners.</param>
        /// <param name="convexFillet">The fillet radius at the convex corners.</param>
        /// <returns></returns>
        public Surface CreateInternalSurface(
            [DefaultArgument("Plane.XY()")] Plane layoutPlane, 
            float concaveFillet = 0, 
            float convexFillet = 0) 
        {
            // create the layout surface.
            Surface layoutSurface = CreateLayoutSurface(layoutPlane);

            // create the internal parking surface.
            Surface internalSurface = Common.Geometry.OffsetSurface(
                LayoutArea,
                (PerimeterAisleWidth + PerimeterBayDepth + PerimeterWalkWayWidth),
                concaveFillet,
                convexFillet
            );

            return layoutSurface;
        }
    }
}
