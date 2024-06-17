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


        /// <summary>
        /// The host plane of the parking layout.
        /// </summary>
        public Plane LayoutPlane { private get; set; }


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
            private get { return _axialRoute; }
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
                _aisleType = value;
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
        /// The width of the internal one way driving aisle.
        /// </summary>
        public float InternalOneWayAisleWidth { private get; set; }


        /// <summary>
        /// The width of the internal two way driving aisle.
        /// </summary>
        public float InternalTwoWayAisleWidth { private get; set; }


        /// <summary>
        /// The radius at the corners of the internal parking islands.
        /// </summary>
        public float IslandCornerRadius { private get; set; }


        /// <summary>
        /// the height of the parking islands above the layout level.
        /// </summary>
        public float IslandHeight { private get; set; }


        /// <summary>
        /// The rotation values of the internal layouts.
        /// </summary>
        public List<float> LayoutRotation { private get; set; }

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
        /// <param name="layoutPlane">The host plane of the parking layout.</param>
        /// <param name="patternType"></param>
        /// <param name="aisleType"></param>
        /// <param name="externalTurningRadius"></param>
        /// <param name="internalTurningRadius"></param>
        /// <param name="islandCornerRadius"></param>
        /// <param name="internalOneWayAisleWidth"></param>
        /// <param name="internalTwoWayAisleWidth"></param>
        /// <param name="islandHeight"></param>
        /// <param name="layoutRotation">The rotation value(s) of the internal layout.</param>
        /// <param name="perimeterAisleWidth"></param>
        /// <param name="perimeterBayDepth"></param>
        /// <param name="perimeterWalkwayWidth"></param>
        public InternalBays(
            Surface layoutArea,
            // Surface exclusionArea,
            // Curve axialRoute,
            Plane layoutPlane,
            int patternType = 1,
            int aisleType = 1,
            // float axialRouteWidth = 7,
            // float exclusionZoneAisleWidth = 7,
            // float exclusionZoneWalkwayWidth = 1.5f,
            float externalTurningRadius = 5,
            float internalTurningRadius = 3.5f,
            float internalOneWayAisleWidth = 4.5f,
            float internalTwoWayAisleWidth = 5.5f,
            float islandCornerRadius = 1,
            float islandHeight = 1,
            List<float> layoutRotation = null,
            float perimeterAisleWidth = 7,
            float perimeterBayDepth = 5,
            float perimeterWalkwayWidth = 1)
        {
            LayoutArea = layoutArea;
            LayoutPlane = layoutPlane;
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
            InternalOneWayAisleWidth = internalOneWayAisleWidth;
            InternalTwoWayAisleWidth = internalTwoWayAisleWidth;
            IslandHeight = islandHeight;
            LayoutRotation = layoutRotation;
            PerimeterAisleWidth = perimeterAisleWidth;
            PerimeterBayDepth = perimeterBayDepth;
            PerimeterWalkWayWidth = perimeterWalkwayWidth;
        }


        /// <summary>
        /// Creates the parking layout surface.
        /// </summary>
        /// <returns name="layoutSurface">The parking layout surface.</returns>
        public Surface CreateLayoutSurface()
        {
            // check the planarity of the input surface.
            Surface surface = Common.Geometry.CheckSurfacePlanarity(LayoutArea);

            // pull the surface onto the input plane.
            Surface layoutSurface = Common.Geometry.PullSurfaceToPlane(surface, LayoutPlane);

            return layoutSurface;
        }


        /// <summary>
        /// Creates the internal parking area surface.
        /// </summary>
        /// <returns name="internalSurface">The offset internal surface.</returns>
        public Surface CreateInternalSurface()
        {
            // create the layout surface.
            Surface layoutSurface = CreateLayoutSurface();

            Surface internalSurface;
            try
            {
                // create the internal parking surface.
                internalSurface = Common.Geometry.OffsetSurface(
                    layoutSurface,
                    (PerimeterAisleWidth + PerimeterBayDepth + PerimeterWalkWayWidth),
                    InternalTurningRadius,
                    ExternalTurningRadius
                );
            }
            catch
            {
                // assign null value to the internal surface if not created.
                internalSurface = null;
            }

            return internalSurface;
        }


        /// <summary>
        /// Creates the perimter road surface.
        /// </summary>
        /// <returns></returns>
        public Surface CreatePerimeterRoadSurface()
        {
            // create the layout surface.
            Surface layoutSurface = CreateLayoutSurface();

            Surface roadWaySurface;
            try 
            {
                // create the perimter roadway surface.
                roadWaySurface = Common.Geometry.PerimeterSurface(
                    layoutSurface,
                    (PerimeterAisleWidth + PerimeterBayDepth + PerimeterWalkWayWidth),
                    InternalTurningRadius,
                    ExternalTurningRadius
                );
            }
            catch
            {
                // assign null value to the roadway surface if not created.
                roadWaySurface = null;
            }

            return roadWaySurface;
        }


        /// <summary>
        /// Creates the bounding rectangles for the internal parking surfaces.
        /// </summary>
        /// <returns name="boundingRectangle">The bounding rectangles.</returns>
        public List<Rectangle> CreateBoundingRectangles() 
        {
            // get the surfaces of the internal surface.
            Surface internalSurface = CreateInternalSurface();
            Face[] faces = internalSurface.Faces;

            List<Surface> surfaces = new List<Surface>();
            foreach (Face face in faces) 
            {
                surfaces.Add(face.SurfaceGeometry());
            }

            // get the perimeter curve of the surfaces.
            List<PolyCurve> curves = new List<PolyCurve>();
            foreach (Surface surface in surfaces) 
            { 
                curves.Add(Common.Geometry.SurfacePerimeter(surface));
            }

            // create a list to hold the rotation values.
            List<float> rotationValues = new List<float>(LayoutRotation);

            // ensure the number of rotation values is equal to the number of curves.
            // check if the number of items in the rotation list is less than the number of curves.
            if (rotationValues.Count < curves.Count)  
            {
                int itemsToAdd = curves.Count - rotationValues.Count;
                for (int i = 0; i < itemsToAdd; i++) 
                {
                    rotationValues.Add(LayoutRotation[0]);
                }
            }
            // check if the number of items in the rotation list is greater than the number of curves.
            else if (rotationValues.Count > curves.Count) 
            { 
                int removeNum = LayoutRotation.Count - curves.Count;
                rotationValues.RemoveRange(rotationValues.Count - removeNum, removeNum);
            }

            // create the bounding rectangle for each of the surfaces.
            List<Rectangle> rectangles = new List<Rectangle>();
            for (int i = 0; i < curves.Count; i++)
            {
                rectangles.Add(Parking.BoundingRectangle.CreateBoundingRectangle(curves[i], -rotationValues[i]));
            }

            return rectangles;       
        }


        /// <summary>
        /// Creates the setout curves for the internal parking layout.
        /// </summary>
        /// <param name="patternWidth">The width of the parking pattern.</param>
        /// <returns></returns>
        public List<List<Line>> CreatePatternSetOutLines(float patternWidth = 5) 
        { 
            // create the bounding rectangles.
            List<Rectangle> boundingRectangles = CreateBoundingRectangles();

            // get the first curve of each bounding rectangle.
            List<Curve> curves = new List<Curve>();
            foreach (Rectangle rect in boundingRectangles) 
            {
                curves.Add(rect.Curves()[0]);
            }

            // add the first setout point of the pattern to the curves.
            List<Point> points = new List<Point>();
            foreach (Curve curve in curves) 
            {
                points.Add(curve.PointAtChordLength(patternWidth / 2));
            }

            // add the aisle width to a parameter.
            float aisleWidth = 0;
            if (AisleType == (int)1)
            {
                aisleWidth = InternalOneWayAisleWidth;
            }
            else if (AisleType == (int)2) 
            {
                aisleWidth = InternalTwoWayAisleWidth;
            }

            // add the pattern location points to the curves.
            List<Point[]> locationPoints = new List<Point[]>();
            int curveNumber = curves.Count;
            for (int i = 0; i < curveNumber; i++) 
            {
                locationPoints.Add(curves[i].PointsAtChordLengthFromPoint(points[i], patternWidth + aisleWidth) as Point[]);
            }

            // get the third curve of each bounding rectangle.
            List<Curve> oppositeCurves = new List<Curve>();
            foreach (Rectangle rect in boundingRectangles)
            {
                oppositeCurves.Add(rect.Curves()[2]);
            }

            // add the setout points to the opposite setout curve.
            List<Point[]> projectedPoints = new List<Point[]>();
            for (int i = 0; i < curveNumber; i++) // indices to loop over the curve and location points lists.
            {
                List<Point> pointList = new List<Point>();
                int pointNumber = locationPoints[i].Length; 
                for (int j = 0; j < pointNumber; j++) // indices to select points within the location points sublists.
                {
                    pointList.Add(oppositeCurves[i].ClosestPointTo(locationPoints[i][j]) as Point);
                }
                projectedPoints.Add(pointList.ToArray());  
            }

            // create a line between the points.
            List<List<Line>> setoutLines = new List<List<Line>>();
            for (int i = 0; i < curveNumber; i++) 
            {
                List<Line> lineList = new List<Line>();
                int pointNumber = locationPoints[i].Length;
                for (int j = 0; j < pointNumber; j++) 
                {
                    lineList.Add(Line.ByStartPointEndPoint(locationPoints[i][j], projectedPoints[i][j]));
                }
                setoutLines.Add(lineList);
                    
            }

                return setoutLines;
        }
    }
}
