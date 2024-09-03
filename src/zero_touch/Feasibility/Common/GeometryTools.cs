using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using CoreNodeModels;
using Autodesk.DesignScript.Runtime;
using Parking;
using System.Windows.Media;
using System.Windows;

namespace Common
{
    /// <summary>
    /// Wrapper class for geometry.
    /// Contains common geometrical operations.
    /// </summary>
    public class GeometryTools
    {
        // this hides the overall class as a node.
        private GeometryTools() { }


        /// <summary>
        /// To add transformations to input geometry.
        /// </summary>
        /// <param name="geometry">The geometry to be transformed.</param>
        /// <param name="geometryPlaneNormal">The normal direction of the geometry's plane.</param>
        /// <param name="geometryLocation">The point from which the geometry is to be transformed.</param>
        /// <param name="hostPlane">The target host plane.</param>
        /// <param name="rotation">The rotation of the geometry around the center of the host plane.</param>
        /// <param name="planeOffset">The offset of the geometry along host plane's normal.</param>
        /// <param name="scaleFactor">The scale of the geometry at the host plane.</param>
        /// <returns></returns>
        public static List<Autodesk.DesignScript.Geometry.Geometry> AddTransformations(
            List<Autodesk.DesignScript.Geometry.Geometry> geometry,
            Autodesk.DesignScript.Geometry.Point geometryLocation,
            Plane hostPlane,
            [DefaultArgument("Vector.ZAxis()")] Autodesk.DesignScript.Geometry.Vector geometryPlaneNormal,
            float rotation = 0,
            float planeOffset = 0,
            float scaleFactor = 1)
        {
            // add a plane at the location point of the geometry.
            Plane geometryPlane = Plane.ByOriginNormal(geometryLocation, geometryPlaneNormal);

            // transform the geometry to the host plane.
            List<Autodesk.DesignScript.Geometry.Geometry> transGeometry = new List<Autodesk.DesignScript.Geometry.Geometry>();
            foreach (Autodesk.DesignScript.Geometry.Geometry geom in geometry) 
            { 
                if (geom != null) 
                {
                    transGeometry.Add(geom.Transform(CoordinateSystem.ByPlane(geometryPlane), CoordinateSystem.ByPlane(hostPlane)));
                }
            }

            // rotate the geometry at the host plane.
            List<Autodesk.DesignScript.Geometry.Geometry> rotatedGeometry = new List<Autodesk.DesignScript.Geometry.Geometry>();  
            foreach (Autodesk.DesignScript.Geometry.Geometry geom in transGeometry) 
            { 
                if (geom != null) 
                {
                    rotatedGeometry.Add(geom.Rotate(hostPlane, rotation));
                }
            }

            // scale the geometry at the host plane.
            List<Autodesk.DesignScript.Geometry.Geometry> scaledGeometry = new List<Autodesk.DesignScript.Geometry.Geometry>();
            foreach (Autodesk.DesignScript.Geometry.Geometry geom in rotatedGeometry) 
            {
                if (geom != null) 
                { 
                    scaledGeometry.Add(geom.Scale(hostPlane, scaleFactor, scaleFactor, scaleFactor));
                }
            }

            // move the geometry along the host plane normal.
            List<Autodesk.DesignScript.Geometry.Geometry> movedGeometry = new List<Autodesk.DesignScript.Geometry.Geometry>();
            foreach (Autodesk.DesignScript.Geometry.Geometry geom in scaledGeometry) 
            { 
                if (geom != null) 
                { 
                    movedGeometry.Add(geom.Translate(hostPlane.Normal, planeOffset));
                }
            }

            return movedGeometry;
        }


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
            Autodesk.DesignScript.Geometry.Point minPoint = boundingBox.MinPoint;
            Autodesk.DesignScript.Geometry.Point maxPoint = boundingBox.MaxPoint;
            Autodesk.DesignScript.Geometry.Vector diagonalVector = Autodesk.DesignScript.Geometry.Vector.ByTwoPoints(minPoint, maxPoint);

            // get the center point along the vector.
            Autodesk.DesignScript.Geometry.Point centerPoint = minPoint.Translate(diagonalVector.Scale(0.5)) as Autodesk.DesignScript.Geometry.Point;

            // create a rectangle at the xy plane using the x and y components of the diagonal vector.
            Autodesk.DesignScript.Geometry.Rectangle rectangle = Rectangle.ByWidthLength(diagonalVector.X, diagonalVector.Y);

            // Move the rectangle to the diagonal vector center point.
            Autodesk.DesignScript.Geometry.Rectangle movedRectangle = rectangle.Translate(centerPoint.AsVector()) as Rectangle;

            // Rotate the rectangle back to the polycurve around the xy plane.
            Rectangle rotatedRectangle = movedRectangle.Rotate(Plane.XY(), rotation) as Rectangle;

            return rotatedRectangle;
        }


        /// <summary>
        /// Creates setout curves using a rectangle as reference.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="firstlineOffset">The offset distance of the first line.</param>
        /// <param name="restLineOffset">The offset distance of the remaining lines.</param>
        /// <returns name="setOutLines">The setout lines.</returns>
        public static List<Line> SetOutLines(
            [DefaultArgument("Rectangle.ByWidthLength(25, 50)")] Rectangle rectangle,
            float firstlineOffset = 2,
            float restLineOffset = 5)
        {
            // get the first curve of the rectangle.
            Curve initialCurve = rectangle.Curves()[0];

            // add the first setout point to the curve.
            Autodesk.DesignScript.Geometry.Point initialPoint = initialCurve.PointAtChordLength(firstlineOffset);

            // add the line location points to the curve.
            Autodesk.DesignScript.Geometry.Point[] locationPoints = initialCurve.PointsAtChordLengthFromPoint(initialPoint, restLineOffset) as Autodesk.DesignScript.Geometry.Point[];

            // get the third curve of the rectangle.
            Curve oppositeCurve = rectangle.Curves()[2];

            // add the setout points to the opposite setout curve.
            List<Autodesk.DesignScript.Geometry.Point> projectedPoints = new List<Autodesk.DesignScript.Geometry.Point>();
            int pointNumber = locationPoints.Length;
            for (int j = 0; j < pointNumber; j++) // indices to select points within the location points list.
            {
                projectedPoints.Add(oppositeCurve.ClosestPointTo(locationPoints[j]) as Autodesk.DesignScript.Geometry.Point);
            }

            // create a line between the points.
            List<Line> setoutLines = new List<Line>();
            for (int k = 0; k < pointNumber; k++)
            {
                setoutLines.Add(Line.ByStartPointEndPoint(locationPoints[k], projectedPoints[k]));
            }

            return setoutLines;
            
        }


        /// <summary>
        /// Get the perimter polycurve of a surface.
        /// </summary>
        /// <param name="surface">The surface input.</param>
        /// <returns name="polyCurve">The closed perimeter polycurve.</returns>
        public static PolyCurve SurfacePerimeter(Surface surface)
        {
            // get the perimeter curve of the surface.
            Curve[] perimeterCurves = surface.PerimeterCurves();
            PolyCurve perimeterCurve = PolyCurve.ByJoinedCurves(perimeterCurves, 0.001, false, 0);

            return perimeterCurve;
        }


        /// <summary>
        /// Check the planarity of an input surface. Returns an error if the surface is not planar.
        /// If planar it returns the surface.
        /// </summary>
        /// <param name="surface">The input surface.</param>
        /// <returns name="planarSurface">The planar surface.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Surface CheckSurfacePlanarity(Surface surface) 
        {
            // get the max and min point of the surface bounding box.
            Autodesk.DesignScript.Geometry.Point minPoint = surface.BoundingBox.MinPoint;
            Autodesk.DesignScript.Geometry.Point maxPoint = surface.BoundingBox.MaxPoint;

            // check if the surface is planar and horizontal.
            Surface _surface = null;
            if (maxPoint.Z > minPoint.Z)
            {
                throw new ArgumentException(nameof(surface), "The surface must be horizontal and planar.");
            }
            _surface = surface;

            return _surface;
        }


        /// <summary>
        /// Check the planarity of an input curve. Returns an error if the curve.
        /// If planar it returns the curve.
        /// </summary>
        /// <param name="curve">The input curve.</param>
        /// <returns name="planarCurve">The planar curve.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Curve CheckCurvePlanarity(Curve curve)
        {
            // get the max and min point of the curve bounding box.
            Autodesk.DesignScript.Geometry.Point minPoint = curve.BoundingBox.MinPoint;
            Autodesk.DesignScript.Geometry.Point maxPoint = curve.BoundingBox.MaxPoint;

            // check if the surface is planar and horizontal.
            Curve _curve = null;
            if (maxPoint.Z > minPoint.Z)
            {
                throw new ArgumentException("The curve must be horizontal and planar.");
            }
            _curve = curve;

            return _curve;
        }


        /// <summary>
        /// Pull a surface onto a plane, in the plane's normal direction.
        /// </summary>
        /// <param name="surface">The input surface.</param>
        /// <param name="plane">The target plane.</param>
        /// <returns name="pulledSurface">The new surface pulled onto the plane.</returns>
        public static Surface PullSurfaceToPlane(Surface surface, [DefaultArgument("Plane.XY()")]Plane plane)
        {
            Surface newSurface;
            try 
            {
                // get the perimeter polycurve of the surface.
                PolyCurve curve = SurfacePerimeter(surface);

                // pull the polycurve onto the plane.
                PolyCurve pulledCurve = curve.PullOntoPlane(plane) as PolyCurve;

                // create the new surface.
                newSurface = Surface.ByPatch(pulledCurve);
            }
            catch 
            {
                throw new Exception("The surface cannot be projected likely because its projected edges " +
                    "overlap or are perpendicular to the input plane.");
            }
            

            return newSurface;
        }


        /// <summary>
        /// Get the plane of a horizontal planar surface.
        /// Returns an error if the surface is not planar and horizontal.
        /// </summary>
        /// <param name="surface">The surface input.</param>
        /// <returns name="surfacePlane">The surface plane.</returns>
        /// <exception cref="ArgumentException"></exception>
        private static Plane SurfacePlane(Surface surface)
        {
            // check if the surface is planar and horizontal.
            Surface _surface = CheckSurfacePlanarity(surface);

            // get the perimeter curve of the surface.
            PolyCurve perimeterCurve = SurfacePerimeter(_surface);

            // get the plane of the perimeter curve.
            Plane curvePlane = perimeterCurve.BasePlane();

            return curvePlane;
        }


        /// <summary>
        /// To project curves onto a planar and horizontal surface.
        /// </summary>
        /// <param name="surface">The input surface.</param>
        /// <param name="curve">The input curve.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static Curve ProjectCurves(Surface surface, Curve curve)
        {
            // get the plane of the perimeter curve.
            Plane curvePlane = SurfacePlane(surface);

            // pull the curve onto the plane.
            Curve pulledCurve = curve.PullOntoPlane(curvePlane);

            return pulledCurve;
        }


        /// <summary>
        /// Create planar offset polycurve edges from a surface and fillet the corners if required.
        /// </summary>
        /// <param name="surface">The surface input.</param>
        /// <param name="offsetDistance">The perimeter offset distance.</param>
        /// <param name="concaveFillet">The fillet radius at the concave corner.</param>
        /// <param name="convexFillet">The fillet radius at the convex corner.</param>
        /// <returns>The offset surface polycurves.</returns>
        /// <exception cref="Exception"></exception>
        public static List<PolyCurve> CreateOffsetSurfaceEdges(
            Surface surface, 
            float offsetDistance, 
            float concaveFillet = 0, 
            float convexFillet = 0)
        {
            // Check if the surface is planar and horizontal.
            Surface _surface = CheckSurfacePlanarity(surface);

            // Get the surface perimeter curve.
            PolyCurve perimeterCurve = SurfacePerimeter(_surface);

            // offset the perimeter curve.
            Curve[] offsetCurves = perimeterCurve.OffsetMany(offsetDistance, SurfacePlane(_surface).Normal);

            // Offset the perimeter curve.
            List<PolyCurve> joinedCurves = new List<PolyCurve>();
            try
            {
                // Check if the area of the offset curve is greater than that of the original surface.
                foreach (Curve curve in offsetCurves)
                {
                    if (Surface.ByPatch(curve).Area > _surface.Area)
                    {
                        Curve[] offsetCurvesAlt = perimeterCurve.OffsetMany(-offsetDistance, SurfacePlane(surface).Normal);
                        foreach (Curve curveAlt in offsetCurvesAlt)
                        {
                            joinedCurves.Add(curveAlt as PolyCurve);
                        }
                    }
                    else
                    {
                        joinedCurves.Add(curve as PolyCurve);
                    }
                }
            }
            catch 
            {
                throw new Exception("The surface cannot be offset. Reduce the offset distance.");
            }
            // Round the concave corners of the offset curves.
            List<PolyCurve> concaveRoundedCurves = new List<PolyCurve>();
            foreach (PolyCurve curve in joinedCurves)
            {
                if (concaveFillet > 0)
                {
                    try
                    {
                        concaveRoundedCurves.Add(curve.Fillet(concaveFillet, false));
                    }
                    catch
                    {
                        concaveRoundedCurves.Add(curve);
                    }
                }
                else
                {
                    concaveRoundedCurves.Add(curve);
                }
            }

            // Round the convex corners of the concave rounded curves.
            List<PolyCurve> convexRoundedCurves = new List<PolyCurve>();
            foreach (PolyCurve curve in concaveRoundedCurves)
            {
                if (convexFillet > 0)
                {
                    try
                    {
                        convexRoundedCurves.Add(curve.Fillet(convexFillet, true));
                    }
                    catch
                    {
                        convexRoundedCurves.Add(curve);
                    }
                }
                else
                {
                    convexRoundedCurves.Add(curve);
                }
            }

            return convexRoundedCurves;
        }


        /// <summary>
        /// Create an offset surface from the surface's offset edges..
        /// </summary>
        /// <param name="surface">The surface input.</param>
        /// <param name="offsetDistance">The perimeter offset distance.</param>
        /// <param name="concaveFillet">The fillet radius at the concave corner.</param>
        /// <param name="convexFillet">The fillet radius at the convex corner.</param>
        /// <returns name="offsetSurface">The offset surface.</returns>
        /// <exception cref="Exception"></exception>
        public static Surface OffsetSurface(
            Surface surface,
            float offsetDistance,
            float concaveFillet = 0,
            float convexFillet = 0)
        {
            // create the offset edges.
            List<PolyCurve> offsetEdges = CreateOffsetSurfaceEdges(surface, offsetDistance, concaveFillet, convexFillet);

            // create surfaces from the curve loops.
            List<Surface> offsetSurfaces = new List<Surface>();
            foreach (PolyCurve curve in offsetEdges)
            {
                offsetSurfaces.Add(Surface.ByPatch(curve));
            }

            // join thes surfaces into a single surface.
            Surface finalSurface = Surface.ByUnion(offsetSurfaces);

            return finalSurface;
        }


        /// <summary>
        /// To create a surface along the perimeter of a surface.
        /// </summary>
        /// <param name="surface">The input surface.</param>
        /// <param name="surfaceWidth">The width of the perimeter surface.</param>
        /// <param name="internalConcaveFillet">The fillet radius at the internal concave corners.</param>
        /// <param name="internalConvexFillet">The fillet radius at the internal convex corners.</param>
        /// <returns name="perimeterSurface">The perimeter surface.</returns>
        /// <exception cref="Exception"></exception>
        public static Surface PerimeterSurface(
            Surface surface, 
            float surfaceWidth, 
            float internalConcaveFillet = 0,
            float internalConvexFillet = 0)
        {
            // create the internal surface for subtraction.
            List<Surface> internalSurfaces = new List<Surface>();
            try
            {
                internalSurfaces.Add(OffsetSurface(surface, surfaceWidth, internalConcaveFillet, internalConvexFillet));
            }
            catch
            {
                throw new Exception("The surface cannot be created. Reduce the surface width.");
            };

            // subtract the internal surface from the primary surface.
            Surface perimeterSurface = surface.Difference(internalSurfaces);

            return perimeterSurface;
        }


        /// <summary>
        /// Creates curve pairs per corner of the input polycurve. 
        /// Note that the curve pair directions have been reversed towards the corners.
        /// </summary>
        /// <param name="curve">The input polycurve.</param>
        /// <returns name="curvePairs">A list of the paired curves.</returns>
        public static List<List<Curve>> PolyCurveCornerCurvePairs(PolyCurve curve) 
        {
            // explode the polycurve.
            List<Curve> explodedCurves = curve.Curves().ToList();

            // create the reversed corner curve pairs.
            List<Curve> reversedCurves = new List<Curve>();
            if (explodedCurves.Count > 1) 
            {
                // shift the curve list elements to the right by 1.
                List<Curve> shiftedCurves = new List<Curve>();
                shiftedCurves.Add(explodedCurves[explodedCurves.Count - 1]); // add the last element to the beginning
                shiftedCurves.AddRange(explodedCurves.GetRange(0, explodedCurves.Count - 1)); // add the rest

                // reverse the direction of the shifted curves.
                foreach (Curve c in shiftedCurves)
                {
                    reversedCurves.Add(c.Reverse());
                }
            }
            else 
            {
                throw new ArgumentException("The polycurve must have more than one side.");
            }

            // pair the curves into lists. 
            List<List<Curve>> zippedCurves = explodedCurves
                .Zip(reversedCurves, (first, second) => new List<Curve> { first, second }).ToList();

            return zippedCurves;
        }


        /// <summary>
        /// Creates vector pairs per corner of the input polycurve. 
        /// Note that the vector pair directions have been reversed towards the corners.
        /// </summary>
        /// <param name="curve">The input polycurve.</param>
        /// <returns name="vectors">A list of the paired vectors.</returns>
        public static List<List<Autodesk.DesignScript.Geometry.Vector>> PolyCurveCornerVectorPairs(PolyCurve curve) 
        {
            // get the corner curve pairs.
            List<List<Curve>> curvePairs = PolyCurveCornerCurvePairs(curve);

            // get the curve vectors.
            List<List<Autodesk.DesignScript.Geometry.Vector>> vectorPairs = new List<List<Autodesk.DesignScript.Geometry.Vector>>();
            foreach (List<Curve> curvePair in curvePairs) 
            { 
                List<Autodesk.DesignScript.Geometry.Vector> vectorPair = new List<Autodesk.DesignScript.Geometry.Vector>();
                vectorPair.Add(curvePair[0].TangentAtParameter(0));
                vectorPair.Add(curvePair[1].TangentAtParameter(0));

                vectorPairs.Add(vectorPair);
            }

            return vectorPairs;
        }


        /// <summary>
        /// Calculates the corner angles for polycurves of any shape and edge conditions.
        /// The calculation is done using the polycurve's end tangents.
        /// </summary>
        /// <param name="curve">The input polycurve.</param>
        /// <returns name="cornerAngles">The polycurve's corner angles.</returns>
        public static List<float> PolyCurveCornerAngles(PolyCurve curve) 
        {
            // get the corner vector pairs.
            List<List<Autodesk.DesignScript.Geometry.Vector>> vectorPairs = PolyCurveCornerVectorPairs (curve);

            // calculate the angle between the tangent vectors.
            List<float> cornerAngles = new List<float>();
            foreach (List<Autodesk.DesignScript.Geometry.Vector> vectorPair in vectorPairs) 
            { 
               float angle = (float)(vectorPair[0].AngleAboutAxis(vectorPair[1], Autodesk.DesignScript.Geometry.Vector.ZAxis()));
               cornerAngles.Add(angle);    
            }

            return cornerAngles;
        }


        /// <summary>
        /// Return a string list of the curve types that make up a polycurve.
        /// </summary>
        /// <param name="curve">The input polycurve.</param>
        /// <returns name="curveTypes">The curve types.</returns>
        public static List<string> PolyCurveEdgeTypes(PolyCurve curve) 
        {
            // explode the polycurve.
            List<Curve> explodedCurves = curve.Curves().ToList();

            // get the curve types.
            List<string> curveTypes = new List<string>();
            foreach (Curve c in explodedCurves)
            {
                string curveType = c.GetType().Name;
                curveTypes.Add(curveType);
            }

     

            return curveTypes;
        }


        /// <summary>
        /// Creates a dashed pattern along a curve.
        /// </summary>
        /// <param name="curve">The input curve to create the dashes along.</param>
        /// <param name="dashLength">The length of the dashes.</param>
        /// <param name="dashGap">The length of the gap between the dashes.</param>
        /// <param name="dashThickness">The thickness of the dashed line.</param>
        /// <returns name="dashCenterCurves">Polycurves representing the center of the dashes.</returns>
        /// <returns name="dashOutlines">Polycurves representing the outline of the dashes.</returns>
        /// <returns name="dashSurfaces">The dash surfaces.</returns>
        /// <exception cref="Exception"></exception>
        [MultiReturn(new[] { "dashCenterCurves", "dashOutlines", "dashSurfaces"})]
        public static Dictionary<string, object> DashedPattern(
            [DefaultArgument("Line.ByStartPointEndPoint(Point.ByCoordinates(0, 100), Point.ByCoordinates(0, 0))")] Curve curve,
            float dashLength = 5,
            float dashGap = 2,
            float dashThickness = 2) 
        {
            // Throw excpetion if inputs less than 0.001.
            if (dashLength < 0.001 || dashGap < 0.001 || dashThickness < 0.001) 
            {
                throw new ArgumentException("dash length, gap, and thickness cannot be less than 0.001");
            }

            // Get the start and end points.
            Autodesk.DesignScript.Geometry.Point startPoint = curve.StartPoint;
            Autodesk.DesignScript.Geometry.Point endPoint = curve.EndPoint;

            // Create the first setout points.
            Autodesk.DesignScript.Geometry.Point setoutStartPoint = curve.PointAtSegmentLength(dashLength);
            List<Autodesk.DesignScript.Geometry.Point> firstSetoutPoints = curve.PointsAtSegmentLengthFromPoint(setoutStartPoint, (dashLength + dashGap)).ToList();

            // Create the second setout points.
            List<Autodesk.DesignScript.Geometry.Point> secondSetoutPoints = curve.PointsAtSegmentLengthFromPoint(startPoint, (dashLength + dashGap)).ToList();

            // Transpose the setout points to create point pairs.
            List<List<Autodesk.DesignScript.Geometry.Point>> zippedPoints = firstSetoutPoints
                .Zip(secondSetoutPoints, (first, second) => new List<Autodesk.DesignScript.Geometry.Point> { first, second })
                .ToList();

            // Combine all the points.
            List<Autodesk.DesignScript.Geometry.Point> combinedPoints = new List<Autodesk.DesignScript.Geometry.Point>();
            combinedPoints.Add(startPoint);
            foreach (var pair in zippedPoints)
            {
                combinedPoints.AddRange(pair);
            }
            combinedPoints.Add(endPoint);

            // Clean the combined points list to remove any null values.
            combinedPoints = combinedPoints.Where(p => p != null).ToList();

            // Remove any duplicate points from the cleaned point list.
            combinedPoints = Autodesk.DesignScript.Geometry.Point.PruneDuplicates(combinedPoints, 0.001).ToList();

            // Chop the pruned point list into segments of 2.
            List<List<Autodesk.DesignScript.Geometry.Point>> choppedPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            for (int i = 0; i < combinedPoints.Count; i += 2)
            {
                if (i + 1 < combinedPoints.Count)
                {
                    choppedPoints.Add(new List<Autodesk.DesignScript.Geometry.Point> { combinedPoints[i], combinedPoints[i + 1] });
                }
            }

            // Remove points list containing only one point.
            List<List<Autodesk.DesignScript.Geometry.Point>> filteredPointList = choppedPoints.Where(p => p.Count != 1).ToList();

            // Create polycurves from the point lists. 
            List<PolyCurve> dashCenterCurves = new List<PolyCurve>();
            foreach (List<Autodesk.DesignScript.Geometry.Point> pointList in filteredPointList) 
            {
                dashCenterCurves.Add(PolyCurve.ByPoints(pointList));
            }

            // Create polycurves representing the outline of the dashes.
            List<PolyCurve> dashOutlines = new List<PolyCurve>();
            foreach (PolyCurve polyCurve in dashCenterCurves)
            {
                dashOutlines.Add(PolyCurve.ByThickeningCurveNormal(polyCurve, dashThickness, Autodesk.DesignScript.Geometry.Vector.ZAxis()));
            }

            // Create surfaces representing the dashes.
            List<Surface> dashSurfaces = new List<Surface>();
            foreach (PolyCurve polyCurve1 in dashOutlines)
            {
                dashSurfaces.Add(Surface.ByPatch(polyCurve1));
            }

            return new Dictionary<string, object> 
            {
                { "dashCenterCurves", dashCenterCurves },
                { "dashOutlines", dashOutlines },
                { "dashSurfaces", dashSurfaces }
            };
        }


        /// <summary>
        /// To create a diagonal hatch pattern.
        /// </summary>
        /// <param name="curve">The curve within which to define the hatch pattern.</param>
        /// <param name="borderThickness">The thickness of the pattern's border.</param>
        /// <param name="hatchThickness">The thickness of the diagonal hatches.</param>
        /// <param name="hatchSpacing">The spacing between the hatches.</param>
        /// <param name="hatchRotation">The rotaton of the hatch pattern.</param>
        /// <returns name="hatchSurface">The surface of the hatch.</returns>
        /// <returns name="hatchOutlines">The perimeter curves of the hatch surface.</returns>
        /// <exception cref="Exception"></exception>
        [MultiReturn(new[] { "hatchSurface", "hatchOutlines"})]
        public static Dictionary<string, object> DiagonalHatchPattern(
            Curve curve,
            float borderThickness = 1f,
            float hatchThickness = 1f,
            float hatchSpacing = 5f,
            float hatchRotation = 45f) 
        {
            // Throw excpetion if inputs less than 0.001.
            if (borderThickness < 0.001 || hatchThickness < 0.001 || hatchSpacing < 0.001)
            {
                throw new ArgumentException("borderThickness, hatchThickness, and hatchSpacing cannot be less than 0.001");
            }

            // Check if the input curve is closed.
            if (curve.IsClosed == false) 
            {
                throw new ArgumentException("The input curve must be closed");
            }

            // Check the planarity of the input polycuve.
            Curve curve1 = CheckCurvePlanarity(curve);

            // Create a surface from the curve.
            Surface surface = Surface.ByPatch(curve1);

            // Add the perimeter surface.
            Surface perimeterSurface = PerimeterSurface(surface, borderThickness) as Surface;

            // Create the bounding rectangle.
            Rectangle boundingRectangle = CreateBoundingRectangle(curve1 as PolyCurve, hatchRotation);

            // Create the hatch setout lines.
            List<Line> lines = SetOutLines(boundingRectangle, hatchSpacing, hatchSpacing);

            // Create the hatch outlines.
            List<PolyCurve> hatchOutlines = new List<PolyCurve>();
            foreach (Line line in lines)
            {
                hatchOutlines.Add(PolyCurve.ByThickeningCurveNormal(line, hatchThickness, Autodesk.DesignScript.Geometry.Vector.ZAxis()));
            }

            // Create surfaces from the hatch outlines.
            List<Surface> hatchSurfaces = new List<Surface>();
            foreach (PolyCurve hatchOutline in hatchOutlines)
            { 
                hatchSurfaces.Add(Surface.ByPatch(hatchOutline));
            }

            // Intersect the hatch surfaces with hatch area surface.
            List<Surface> intersectedSurfaces = new List<Surface>();
            foreach (Surface hatchSurface in hatchSurfaces) 
            {
                // Cast the overall and hatch surfaces as geometry.
                Autodesk.DesignScript.Geometry.Geometry castSurface = surface as Autodesk.DesignScript.Geometry.Geometry;
                Autodesk.DesignScript.Geometry.Geometry castHatchSurface = hatchSurface as Autodesk.DesignScript.Geometry.Geometry;

                // Intersect the overall and hatch surfaces.
                Autodesk.DesignScript.Geometry.Geometry[] surfaceList = castSurface.Intersect(castHatchSurface);

                foreach (Autodesk.DesignScript.Geometry.Geometry geometry in surfaceList) 
                { 
                    intersectedSurfaces.Add((Surface)geometry);
                }
            }

            // Join the perimeter surface and the intersected hatch surfaces.
            List<Surface> combinedSurfaces = new List<Surface>();
            combinedSurfaces.AddRange(new List<Surface> { perimeterSurface });
            combinedSurfaces.AddRange(intersectedSurfaces);
            Surface joinedSurface = Surface.ByUnion(combinedSurfaces);

            // Get the perimeter curves of the joined surface.
            Curve[] perimeterCurves = joinedSurface.PerimeterCurves();

            return new Dictionary<string, object>
            {
                { "hatchSurface", combinedSurfaces },
                { "hatchOutlines", perimeterCurves }
            };
        }


        /// <summary>
        /// To group intersecting geometry.
        /// </summary>
        /// <param name="geometry">A list containg geometry to be sorted based on intersections.</param>
        /// <returns name="geometryList">A list containing the grouped geometry.</returns>
        public static List<List<Autodesk.DesignScript.Geometry.Geometry>> SortIntersectingGeometry(List<Autodesk.DesignScript.Geometry.Geometry> geometry)
        {
            List<List<Autodesk.DesignScript.Geometry.Geometry>> geoLists = new List<List<Autodesk.DesignScript.Geometry.Geometry>>();

            while (geometry.Any()) // Continue until all geometries are sorted.
            {
                // Start with the first geometry and create a new group.
                Autodesk.DesignScript.Geometry.Geometry currentGeometry = geometry[0];
                geometry.RemoveAt(0);

                List<Autodesk.DesignScript.Geometry.Geometry> geometryGroup = new List<Autodesk.DesignScript.Geometry.Geometry> { currentGeometry };
                bool geometryAdded;

                do
                {
                    geometryAdded = false;

                    // Iterate over a copy of the remaining geometries.
                    foreach (Autodesk.DesignScript.Geometry.Geometry otherGeometry in geometry.ToList())
                    {
                        // If it intersects with any geometry in the group, add it to the group.
                        if (geometryGroup.Any(g => g.DoesIntersect(otherGeometry)))
                        {
                            geometryGroup.Add(otherGeometry);
                            geometry.Remove(otherGeometry);
                            geometryAdded = true; // Mark that we added a geometry.
                        }
                    }
                }
                while (geometryAdded); // Continue checking until no more geometries are added to the group.

                // Add the completed group to the list of grouped geometries.
                geoLists.Add(geometryGroup);
            }

            return geoLists;
        }


        /// <summary>
        /// Split a polycuve with a list of points and return new polycurves representing the splits.
        /// </summary>
        /// <param name="polyCurve">The input polycurve.</param>
        /// <param name="points">The list of points.</param>
        /// <param name="splitWidth">The width of the split gap at the points.</param>
        /// <returns name="polyCurves">The new sorted polycurves.</returns>
        public static List<PolyCurve> SplitPolyCurveByPoints(
            PolyCurve polyCurve, 
            List<Autodesk.DesignScript.Geometry.Point> points,
            float splitWidth = 1) 
        {
            // Get the curves of the polycurve.
            List<Curve> curves = polyCurve.Curves().ToList();

            // Get the distances of the points from the boundary curves.
            List<List<float>> pointDistances = new List<List<float>>();
            foreach (Autodesk.DesignScript.Geometry.Point point in points) 
            {
                List<float> distanceList = new List<float>();
                foreach (Curve curve in curves) 
                { 
                    distanceList.Add((float)point.DistanceTo(curve));
                }

                pointDistances.Add(distanceList);
            }

            // Get the minumum item in each distance list.
            List<float> minimumDistances = new List<float>();
            foreach (List<float> distanceList in pointDistances) 
            { 
                minimumDistances.Add(distanceList.Min());
            }

            // Group the minumum items and the distance lists. 
            List<List<List<float>>> zippedDistances = minimumDistances
                .Zip(pointDistances, (first, second) => new List<List<float>> { new List<float> { first }, second }).ToList();

            // Get the index of the minimum items in the distance lists.
            List<int> indices = new List<int>();
            foreach (List<List<float>> groupedDistances in zippedDistances) 
            {
                indices.Add(groupedDistances[1].IndexOf(groupedDistances[0][0]));
            }

            // Get the curves at the indices from the curve list. The closest curves to the entrance points. 
            List<Curve> closestCurves = new List<Curve>();
            foreach (int i in indices) 
            { 
                closestCurves.Add(curves[i]);
            }

            // Get the center point strings of the curves closest to the entrance points.
            List<string> curveCenterPointStrings = new List<string>();
            foreach (Curve curve in closestCurves) 
            {
                Autodesk.DesignScript.Geometry.Point centerPoint = curve.PointAtParameter(0.5);
                string centerPointString = centerPoint.ToString();
                curveCenterPointStrings.Add(centerPointString);
            }

            // Group the curves with the same center point string values.
            List<List<Curve>> groupedCurves = new List<List<Curve>>();
            Dictionary<string, List<Curve>> curveGroups = new Dictionary<string, List<Curve>> ();
            
            for (int i = 0; i < curveCenterPointStrings.Count; i++) 
            {
                string currentString = curveCenterPointStrings[i];
                Curve currentCurve = closestCurves[i];

                // If the group doesn't exist yet create it.
                if (!curveGroups.ContainsKey(currentString)) 
                { 
                    curveGroups[currentString] = new List<Curve> ();
                }

                // Add the curve to the appropriate group.
                curveGroups[currentString].Add(currentCurve);
            }

            // Convert the dictionary values to list of lists.
            List<List<Curve>> tempCurves = curveGroups.Values.ToList();

            // Remove duplicate curves from the curve lists.
            foreach (List<Curve> curveList in tempCurves) 
            {
                // Get the first curve if the list count is greater than 1.
                if (curveList.Count > 1) 
                {
                    groupedCurves.Add(new List<Curve> { curveList[0] });
                }
                else 
                {
                    groupedCurves.Add(curveList);
                }
            }

            // Group the points by the curve center point string values.
            List<List<Autodesk.DesignScript.Geometry.Point>> groupedPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            Dictionary<string, List<Autodesk.DesignScript.Geometry.Point>> pointGroups = new Dictionary<string, List<Autodesk.DesignScript.Geometry.Point>>();

            for (int i = 0; i < curveCenterPointStrings.Count; i++)
            {
                string currentString = curveCenterPointStrings[i];
                Autodesk.DesignScript.Geometry.Point currentPoint = points[i];

                // If the group doesn't exist yet, create it.
                if (!pointGroups.ContainsKey(currentString))
                {
                    pointGroups[currentString] = new List<Autodesk.DesignScript.Geometry.Point>();
                }

                // Add the point to the appropriate group.
                pointGroups[currentString].Add(currentPoint);
            }

            // Convert the dictionary values to a list of lists.
            groupedPoints = pointGroups.Values.ToList();

            // Project the points onto the curves.
            List<List<Autodesk.DesignScript.Geometry.Point>> projectedPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            for (int i = 0; i < groupedCurves.Count; i++) 
            {
                List<Autodesk.DesignScript.Geometry.Point> pointList = new List<Autodesk.DesignScript.Geometry.Point>();
                for (int j = 0; j < groupedPoints[i].Count; j++) 
                {
                    pointList.Add(groupedCurves[i][0].ClosestPointTo(groupedPoints[i][j]));
                }
                projectedPoints.Add(pointList);
            }

            // Get the curve parameters at the location of the projected points.
            List<List<float>> projectedPointParameters = new List<List<float>>();
            for (int i = 0; i < groupedCurves.Count; i++) 
            {
                List<float> parameterList = new List<float>();
                for (int j = 0;j < projectedPoints[i].Count; j++) 
                {
                    parameterList.Add((float)groupedCurves[i][0].ParameterAtPoint(projectedPoints[i][j]));
                }
                projectedPointParameters.Add(parameterList);
            }

            // Create a list of numbers to indicate half of the split width.
            List<float> entranceHalfDistances = Maths.Range(splitWidth / 2, -splitWidth / 2, 2);

            // Create points from the curve parameter with the split widths.
            List<List<Autodesk.DesignScript.Geometry.Point>> splitPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            for (int i = 0; i < groupedCurves.Count; i++) 
            {
                List<Autodesk.DesignScript.Geometry.Point> pointList = new List<Autodesk.DesignScript.Geometry.Point>();
                for (int j = 0; j < projectedPointParameters[i].Count; j++) 
                {
                    foreach (float distance in entranceHalfDistances) 
                    {
                        pointList.Add(groupedCurves[i][0].PointAtChordLength(distance, projectedPointParameters[i][j], true));
                    }
                }
                splitPoints.Add(pointList);
            }

            // Split the curves with the point lists.
            List<List<Curve>> splitCurveLists = new List<List<Curve>>();
            for (int i = 0; i < groupedCurves.Count; i++) 
            { 
                splitCurveLists.Add(groupedCurves[i][0].SplitByPoints(splitPoints[i]).ToList());
            }

            // Remove the split curves intersecting with the projected points.
            List<List<Curve>> filteredCurveLists = new List<List<Curve>>();
            for (int i = 0; i < projectedPoints.Count; i++)
            {
                List<Curve> curveList = new List<Curve>();

                foreach (Curve curve in splitCurveLists[i])
                {
                    bool intersects = false;
                    foreach (Autodesk.DesignScript.Geometry.Point point in projectedPoints[i])
                    {
                        if (curve.DoesIntersect(point))
                        {
                            intersects = true;
                            break; // Exit loop if intersection is found.
                        }
                    }
                    if (!intersects)
                    {
                        curveList.Add(curve);
                    }
                }

                filteredCurveLists.Add(curveList);
            }

            // Get the curves not close to the points.
            List<Curve> otherCurves = curves.ToList();
            List<int> sortedIndices = indices.ToList().Distinct().ToList();
            sortedIndices.Sort((a, b) => b.CompareTo(a));
            foreach (int i in sortedIndices)
            {
                otherCurves.RemoveAt(i);
            }

            // Flatten the filtered curve list.
            List<Curve> flattenedFilteredCurves = filteredCurveLists.SelectMany(curveList => curveList).ToList();

            // Combine the filtered curves and the other curves in a flat list.
            List<Curve> combinedCurves = new List<List<Curve>> { flattenedFilteredCurves, otherCurves }.SelectMany(curveList => curveList).ToList();

            // Cast the curves to geometry for grouping.
            List<Autodesk.DesignScript.Geometry.Geometry> combinedGeometry = new List<Autodesk.DesignScript.Geometry.Geometry>();
            foreach (Curve curve in combinedCurves) 
            { 
                combinedGeometry.Add(curve);
            }

            // Group the intersecting curves.
            List<List<Autodesk.DesignScript.Geometry.Geometry>> intersectingCurves = SortIntersectingGeometry(combinedGeometry);

            // Cast the intersecting curve groups. 
            List<List<Curve>> castIntersectingCurves = new List<List<Curve>> ();
            foreach (List<Autodesk.DesignScript.Geometry.Geometry> geometryList in intersectingCurves) 
            { 
                List<Curve> curveList = new List<Curve> ();
                foreach (Autodesk.DesignScript.Geometry.Geometry geometry in geometryList) 
                { 
                    curveList.Add(geometry as  Curve);
                }
                castIntersectingCurves.Add(curveList);
            }

            // Create polycurves from the grouped intersecting curves. 
            List<PolyCurve> polyCurves = new List<PolyCurve>(); 
            foreach (List<Curve> curveList in castIntersectingCurves) 
            {
                polyCurves.Add(PolyCurve.ByJoinedCurves(curveList, 0.001, false));
            }

            // Workflow to sort the polycurves in a clockwise direction.

            // Get the center point of the polycurve bounding boxes.
            List<Autodesk.DesignScript.Geometry.Point> centerPoints = new List<Autodesk.DesignScript.Geometry.Point>();
            foreach (PolyCurve curve in polyCurves) 
            { 
                // Get the bounding box.
                BoundingBox boundingBox = curve.BoundingBox;

                // Create the diagonal lines of the bounding boxes.
                Line diagonalLine = Line.ByStartPointEndPoint(boundingBox.MinPoint, boundingBox.MaxPoint);

                // Get the center point of the lines.
                Autodesk.DesignScript.Geometry.Point centerPoint = diagonalLine.PointAtParameter(0.5);

                centerPoints.Add(centerPoint);
            }

            // Get the center point of the input polycurve's bounding box.
            BoundingBox polyCurveBoundingBox = polyCurve.BoundingBox;
            Line polyCurveDiagonalLine = Line.ByStartPointEndPoint(polyCurveBoundingBox.MinPoint, polyCurveBoundingBox.MaxPoint);
            Autodesk.DesignScript.Geometry.Point polyCurveCenter = polyCurveDiagonalLine.PointAtParameter(0.5);

            // Get the new polycurve angles from the Y axis.
            List<float> anglesFromY = new List<float>();
            foreach (Autodesk.DesignScript.Geometry.Point centerPoint in centerPoints) 
            {
                // Create vector from the input polycurve center to the new polycurve centers.
                Autodesk.DesignScript.Geometry.Vector vector = Autodesk.DesignScript.Geometry.Vector.ByTwoPoints(polyCurveCenter, centerPoint);

                // Calculate the angles between the vectors and the Y axis.
                float angleFromY = (float)Autodesk.DesignScript.Geometry.Vector.YAxis().AngleAboutAxis(vector, Autodesk.DesignScript.Geometry.Vector.ZAxis());
                anglesFromY.Add(angleFromY);
            }

            // Sort the new polycurves using the angle values.
            List<PolyCurve> sortedPolyCurves = polyCurves.Zip(anglesFromY, (poly, angleFromY) => new { poly, angleFromY })
                .OrderBy(x => x.angleFromY)
                .Select(x => x.poly)
                .ToList();

            return sortedPolyCurves;
        }
    }
}
