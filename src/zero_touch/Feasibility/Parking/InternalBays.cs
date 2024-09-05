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
                // Check if the layout surface is horizontal and planar.
                Point minPoint = value.BoundingBox.MinPoint;
                Point maxPoint = value.BoundingBox.MaxPoint;

                if (maxPoint.Z > minPoint.Z)
                {
                    throw new ArgumentException(nameof(_layoutArea), "The layout surface must be horizontal and planar.");
                }

                // Check if the perimeter curves of the surfaces contain unallowed curve types.
                PolyCurve surfacePerimeter = Common.GeometryTools.Surfaces.SurfacePerimeter(value); // Get the perimeter of the surface.
                List<string> edgeTypes = Common.GeometryTools.Curves.PolyCurveEdgeTypes(surfacePerimeter);

                if (edgeTypes.Contains("Arc") || edgeTypes.Contains("NurbsCurve") || edgeTypes.Contains("Circle")) 
                {
                    throw new ArgumentException("The surface edge must contain only straight lines.");
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
            get { return _patternType; }
            private set
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
        /// The parking patterns.
        /// Add a list of parking patterns into this input.
        /// The first pattern is treated as the primary pattern.
        /// </summary>
        public List<Patterns> Patterns { private get; set; }


        /// <summary>
        /// Creates internal parking layout instances.
        /// </summary>
        /// <param name="layoutArea"></param>
        /// <param name="layoutPlane">The host plane of the parking layout.</param>
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
        /// <param name="patterns"></param>
        public InternalBays(
            Surface layoutArea,
            // Surface exclusionArea,
            // Curve axialRoute,
            Plane layoutPlane,
            //int patternType = 1,
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
            float perimeterWalkwayWidth = 1,
            List<Patterns> patterns = null)
        {
            LayoutArea = layoutArea;
            LayoutPlane = layoutPlane;
            // ExclusionArea = exclusionArea;
            // AxialRoute = axialRoute;
            //PatternType = patternType;
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
            Patterns = patterns;
        }


        /// <summary>
        /// Creates the parking layout surface.
        /// </summary>
        /// <returns name="layoutSurface">The parking layout surface.</returns>
        public Surface CreateLayoutSurface()
        {
            // check the planarity of the input surface.
            Surface surface = Common.GeometryTools.Surfaces.CheckSurfacePlanarity(LayoutArea);

            // pull the surface onto the input plane.
            Surface layoutSurface = Common.GeometryTools.Surfaces.PullSurfaceToPlane(surface, LayoutPlane);

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
                internalSurface = Common.GeometryTools.Surfaces.OffsetSurface(
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
                roadWaySurface = Common.GeometryTools.Surfaces.PerimeterSurface(
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
                curves.Add(Common.GeometryTools.Surfaces.SurfacePerimeter(surface));
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
                rectangles.Add(Common.GeometryTools.Curves.CreateBoundingRectangle(curves[i], -rotationValues[i]));
            }

            return rectangles;       
        }


        /// <summary>
        /// Order and add the correct configuration to the input patterns.
        /// </summary>
        /// <returns></returns>
        public List<Patterns> ConfigurePatternInputs() 
        {
            // create a new list to hold the input parking patterns.
            List<Patterns> inputPatterns = new List<Patterns>(Patterns);

            // assign the same location line to the pattern inputs.
            //Line tempLocationLine = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0), Point.ByCoordinates(0, 50));
            //PatternPrimary.LocationLine = tempLocationLine;
            //PatternSecondary.LocationLine = tempLocationLine;
            //PatternTertiary.LocationLine = tempLocationLine;

            // assign the same pattern type to the pattern inputs.
            foreach (Patterns pattern in inputPatterns) 
            { 
                pattern.PatternType = Patterns[0].PatternType;
            }

            // assign the same bay angle to the pattern inputs.
            foreach (Patterns pattern in inputPatterns) 
            { 
                pattern.BayAngle = Patterns[0].BayAngle;
            }

            // sort the patterns by their pattern width.???? Keep user defined order?
            //List<Patterns> sortedPatterns = inputPatterns.OrderBy(p => p.PatternWidth).ToList();

            return inputPatterns;
        }


        /// <summary>
        /// Creates the setout curves for the internal parking layout.
        /// </summary>
        /// <returns></returns>
        public List<List<Line>> CreatePatternSetOutLines() 
        {
            // Add the user provided patterns to a list.
            List<Patterns> patterns = ConfigurePatternInputs();
           
            // Add the pattern width to a variable.
            float patternWidth = patterns[0].PatternWidth;

            // sort the patterns by their pattern width.???? Keep user defined order?
            //List<Patterns> sortedPatterns = patterns.OrderBy(p => p.PatternWidth).ToList();

            // create the bounding rectangles.
            List<Rectangle> boundingRectangles = CreateBoundingRectangles();

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

            // get the first curve of each bounding rectangle.
            List<Curve> curves = new List<Curve>();
            foreach (Rectangle rect in boundingRectangles) 
            {
                // extend the curve to ensure the pattern fully covers the layout.
                Curve initialCurve = rect.Curves()[0];
                int patternRowNum = (int)DSCore.Math.Ceiling(((initialCurve.Length - patternWidth / 2) / (patternWidth + aisleWidth)));
                float newCurveLength = (patternWidth / 2) + ((patternWidth + aisleWidth) * patternRowNum);
                float extensionLength = newCurveLength - (float)initialCurve.Length;
                curves.Add(initialCurve.ExtendEnd(extensionLength));
            }

            // add the first setout point of the pattern to the curves.
            List<Point> points = new List<Point>();
            foreach (Curve curve in curves) 
            {
                points.Add(curve.PointAtChordLength(patternWidth / 2));
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
                // extend the curve to ensure the pattern fully covers the layout.
                Curve initialCurve = rect.Curves()[2];
                int patternRowNum = (int)DSCore.Math.Ceiling(((initialCurve.Length - patternWidth / 2) / (patternWidth + aisleWidth)));
                float newCurveLength = (patternWidth / 2) + ((patternWidth + aisleWidth) * patternRowNum);
                float extensionLength = newCurveLength - (float)initialCurve.Length;
                oppositeCurves.Add(initialCurve.ExtendStart(extensionLength));
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


        /// <summary>
        /// Create the internal parking bays.
        /// </summary>
        /// <returns></returns>
        public List<List<Patterns>> CreateInternalBays() 
        {
            // Add the user provided patterns to a list.
            List<Patterns> patterns = ConfigurePatternInputs();

            // create the setout lines.
            List<List<Line>> setoutLines = CreatePatternSetOutLines();

            // count the number of list is the setout lines input.
            int listNumber = setoutLines.Count;

            // reverse the direction of the setout lines if necessary to allow for one or two way traffic.
            List<List<Line>> reversedLines = new List<List<Line>>();

            // keep as is if aisle is one way and pattern type is non interlocking.
            if (AisleType == 1 && PatternType == 1)
            {
                reversedLines = setoutLines;
            }

            // replace every second row as required depending on the aisle type (one or two way) selected.
            else if (
                (AisleType == 2 && patterns[0].PatternType == 1) || 
                (AisleType == 1 && patterns[0].PatternType == 2) || 
                (AisleType == 2 && patterns[0].PatternType == 3)
            ) 
            {
                for (int i = 0; i < listNumber; i++) 
                {
                    List<Line> lineList = new List<Line> ();
                    int lineNumber = setoutLines[i].Count;
                    for (int j = 0; j < lineNumber; j++) 
                    {
                        if (j % 2 == 1) 
                        {
                            lineList.Add(setoutLines[i][j].Reverse() as Line);
                        }
                        else 
                        { 
                            lineList.Add (setoutLines[i][j]);
                        }
                    }
                    reversedLines.Add(lineList);
                }
            }
            else 
            {
                reversedLines = setoutLines;
            }

            // add the patterns to the setout lines.
            List<List<Patterns>> layoutPatterns = new List<List<Patterns>>();
            for (int i = 0; i < listNumber; i++) // indices to loop through the reversed line lists.
            {
                List<Patterns> patternList = new List<Patterns>();
                int lineNumber = reversedLines[i].Count;
                for (int j = 0; j < lineNumber; j++) // indices to loop through each reversed line in the lists.
                {
                    patternList.Add(new Parking.Patterns(
                        reversedLines[i][j],
                        patterns[0].PatternType,
                        patterns[0].BayWidth,
                        true,
                        patterns[0].BayLength,
                        patterns[0].BayAngle,
                        patterns[0].IslandWidth,
                        patterns[0].ParkingType)
                    );
                }
                layoutPatterns.Add( patternList );
            }

            // create the internal layout surface(s).
            Surface internalSurface = CreateInternalSurface();

            // Get the parking bays that intersect with the internal layout surface.
            List<List<List<ParkingBay>>> internalBays = new List<List<List<ParkingBay>>>(); // cannot return pattern objects since bays must be removed.

            return layoutPatterns;
        }
    }
}
