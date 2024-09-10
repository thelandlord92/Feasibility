using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.GeometryTools
{
    /// <summary>
    /// Wrapper class for curves.
    /// </summary>
    public class Curves
    {
        // Hides the overall class a node.
        private Curves() { }


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

            // check if the curve is planar and horizontal.
            Curve _curve = null;
            if (maxPoint.Z > minPoint.Z)
            {
                throw new ArgumentException("The curve must be horizontal and planar.");
            }
            _curve = curve;

            return _curve;
        }


        /// <summary>
        /// To project curves onto a planar and horizontal surface.
        /// </summary>
        /// <param name="surface">The input surface.</param>
        /// <param name="curve">The input curve.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static Curve ProjectCurvesOntoSurface(Surface surface, Curve curve)
        {
            // get the plane of the perimeter curve.
            Plane curvePlane = Common.GeometryTools.Surfaces.SurfacePlane(surface);

            // pull the curve onto the plane.
            Curve pulledCurve = curve.PullOntoPlane(curvePlane);

            return pulledCurve;
        }


        /// <summary>
        /// Returns the corner points of a polycurve.
        /// </summary>
        /// <param name="curve">The input polycurve.</param>
        /// <returns name="cornerPoints">The corner points.</returns>
        public static List<Autodesk.DesignScript.Geometry.Point> PolyCurveCorners(PolyCurve curve) 
        {
            // Explode the polycurve.
            List<Curve> explodedCurves = curve.Curves().ToList();

            // Get the corner points of the polycurve.
            List<Autodesk.DesignScript.Geometry.Point> cornerPoints = new List<Autodesk.DesignScript.Geometry.Point>();
            foreach (Curve curve1 in explodedCurves) 
            {
                cornerPoints.Add(curve1.StartPoint);
            }

            return cornerPoints;
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
            List<List<Autodesk.DesignScript.Geometry.Vector>> vectorPairs = PolyCurveCornerVectorPairs(curve);

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
        /// Returns the corner point pairs of a polycurve.
        /// Note that the concave corner points are dependent on the element length.
        /// For the concave corner spacing to be as in the concaveCornerSpacing parameter set the element length to zero.
        /// </summary>
        /// <param name="curve">The input polycurve</param>
        /// <param name="concaveCornerSpacing">Spacing of the points at the concave corners.</param>
        /// <param name="convexCornerSpacing">Spacing of the points at the convex corners.</param>
        /// <param name="elementLength">The length of the elements to be placed along the edges of the polycurve.</param>
        /// <returns name="concaveCornerPoints">The concave corner points.</returns>
        /// <returns name="convexCornerPoints">The convex corner points.</returns>
        [MultiReturn(new[] { "concaveCornerPoints", "convexCornerPoints" })]
        public static Dictionary<string, object> PolyCurveCornerPointPairs(
            PolyCurve curve,
            float concaveCornerSpacing = 1f,
            float convexCornerSpacing = 1f,
            float elementLength = 0f) 
        {
            // Check the planarity of the input polycurve.
            PolyCurve planarCurve = CheckCurvePlanarity(curve) as PolyCurve;

            // Get the corner angles of the polycurve.
            List<float> cornerAngles = PolyCurveCornerAngles(planarCurve);

            // Get the corner curve pairs of the polycuve.
            List<List<Curve>> cornerCurvePairs = PolyCurveCornerCurvePairs(planarCurve);

            // Create a tuple of the corner angles and curve pairs.
            List<Tuple<float, List<Curve>>> angleCurvePairs = cornerAngles
                .Zip(cornerCurvePairs, (angle, curveList) => Tuple.Create(angle, curveList)).ToList();

            // Sort concave and convex corner curves using LINQ.
            List<Tuple<float, List<Curve>>> convexCurvePairs = angleCurvePairs
                .Where(tuple => tuple.Item1 > 180)  // Convex angles
                .ToList();

            List<Tuple<float, List<Curve>>> concaveCurvePairs = angleCurvePairs
                .Where(tuple => tuple.Item1 <= 180) // Concave angles
                .ToList();

            // Add the corner points at the convex corners.
            object convexCornerPoints;
            if (convexCurvePairs.Count > 0) 
            {
                List<List<Autodesk.DesignScript.Geometry.Point>> cornerPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
                foreach (Tuple<float, List<Curve>> tuple in convexCurvePairs)
                {
                    List<Autodesk.DesignScript.Geometry.Point> pointList = new List<Autodesk.DesignScript.Geometry.Point>();
                    foreach (Curve curve1 in tuple.Item2)
                    {
                        pointList.Add(curve1.PointAtChordLength(convexCornerSpacing, 0, true));
                    }
                    cornerPoints.Add(pointList);
                }
                convexCornerPoints = cornerPoints;
            }
            else 
            {
                convexCornerPoints = null;
            }
            
            // Add the corner points at the concave corners.
            List<List<Autodesk.DesignScript.Geometry.Point>> concaveCornerPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            foreach (Tuple<float, List<Curve>> tuple in concaveCurvePairs)
            {
                List<Autodesk.DesignScript.Geometry.Point> pointList = new List<Autodesk.DesignScript.Geometry.Point>();
                foreach (Curve curve1 in tuple.Item2)
                {
                    if (tuple.Item1 == 180) 
                    {
                        // Create the point.
                        pointList.Add(curve1.PointAtChordLength(0, 0, true));
                    }
                    else 
                    {
                        // Calcualte the offset distance.
                        float distance1 = (float)(elementLength / DSCore.Math.Tan(tuple.Item1 / 2));
                        float offsetDistance = concaveCornerSpacing + distance1;

                        // Create the point.
                        pointList.Add(curve1.PointAtChordLength(offsetDistance, 0, true));
                    } 
                }
                concaveCornerPoints.Add(pointList);
            }

            return new Dictionary<string, object> 
            {
                { "concaveCornerPoints", concaveCornerPoints },
                { "convexCornerPoints", convexCornerPoints}
            };
        }


        /// <summary>
        /// Splits a polycurve using distances from its concave and convex corners.
        /// Note that the concave corner splits are dependent on the element length.
        /// For the concave corner spacing to be as in the concaveCornerSpacing parameter set the element length to zero.
        /// Segments with lengths smaller than the element width are removed.
        /// The convex corner spacing input has no effect if the input curve has no convex corners.
        /// </summary>
        /// <param name="curve">The input polycurve</param>
        /// <param name="concaveCornerSpacing">Spacing of the points at the concave corners.</param>
        /// <param name="convexCornerSpacing">Spacing of the points at the convex corners.</param>
        /// <param name="elementLength">The length of the elements to be placed along the edges of the polycurve.</param>
        /// <param name="elementWidth">The width of the elements to be placed along the edges of the polycurve.</param>
        /// <returns name="cornerPolyCurves">Polycurves at the corners of the input polycurve.</returns>
        /// <returns name="segmentPolyCurves">Polycurves segments betwween the corners of the input polycurve.</returns>
        [MultiReturn(new[] { "cornerPolyCurves", "segmentPolyCurves" })]
        public static Dictionary<string, List<PolyCurve>> SplitPolyCurveByCornerDistances(
            PolyCurve curve,
            float concaveCornerSpacing = 1f,
            float convexCornerSpacing = 1f,
            float elementLength = 0f,
            float elementWidth = 0f)
        {
            // Throw exception if concave or convex corner spacing are less than or equal to zero.
            if (concaveCornerSpacing <= 0f || convexCornerSpacing <= 0f)
            {
                throw new ArgumentException("The concave and convex corner spacing cannot be zero.");
            }

            // Throw an exception if the input curve is not closed.
            if (curve.IsClosed == false) 
            {
                throw new ArgumentException("Only closed curves are allowed");
            }

            // Check the planarity of the input polycurve.
            PolyCurve planarCurve = CheckCurvePlanarity(curve) as PolyCurve;

            // Get the corner angles of the polycurve.
            List<float> cornerAngles = PolyCurveCornerAngles(planarCurve);

            // Get the concave and concave corner point pairs.
            Dictionary<string, object> pointDictionary = PolyCurveCornerPointPairs(
                planarCurve,
                concaveCornerSpacing,
                convexCornerSpacing,
                elementLength
            );

            List<List<Autodesk.DesignScript.Geometry.Point>> concavePoints = pointDictionary["concaveCornerPoints"] as List<List<Autodesk.DesignScript.Geometry.Point>>;
            List<List<Autodesk.DesignScript.Geometry.Point>> convexPoints = pointDictionary["convexCornerPoints"] as List<List<Autodesk.DesignScript.Geometry.Point>>;

            // Create a list of indices indicating the position of the concave and convex corners.
            List<int> allIndices = new List<int>();
            List<int> concaveIndices = new List<int>();
            List<int> convexIndices = new List<int>();
            for (int i = 0; i < cornerAngles.Count; i++)
            {
                if (cornerAngles[i] <= 180)
                {
                    concaveIndices.Add(i);
                }
                else if (cornerAngles[i] > 180)
                {
                    convexIndices.Add(i);
                }
            }

            allIndices.AddRange(concaveIndices);

            if (convexIndices.Count > 0)
            {
                allIndices.AddRange(convexIndices); // Add the convex indices to the all indices if the convex indices list is not empty.
            }

            // Combine the convex and concave corner points.
            List<List<Autodesk.DesignScript.Geometry.Point>> cornerPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            cornerPoints.AddRange(concavePoints);

            if (convexIndices.Count > 0)
            {
                cornerPoints.AddRange(convexPoints); // Add the convex points to the corner points list if the convex points list is not empty.
            }

            // Sort the corner points using the indices.
            List<List<Autodesk.DesignScript.Geometry.Point>> sortedCornerPoints = cornerPoints
                .Zip(allIndices, (pointList, index) => new { pointList, index })
                .OrderBy(x => x.index)
                .Select(x => x.pointList)
                .ToList();

            // Reverse the order of the corner point lists in the sorted corner points.
            List<List<Autodesk.DesignScript.Geometry.Point>> reversedPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            foreach (List<Autodesk.DesignScript.Geometry.Point> pointList in sortedCornerPoints)
            {
                List<Autodesk.DesignScript.Geometry.Point> reversedList = pointList.ToList(); // Create a copy
                reversedList.Reverse(); // Reverse in place
                reversedPoints.Add(reversedList); // Add to the new list
            }

            // Flatten the corner points.
            List<Autodesk.DesignScript.Geometry.Point> flattenedCornerPoints = reversedPoints.SelectMany(pointList => pointList).ToList();

            // Shift the flattened points by -1.
            List<Autodesk.DesignScript.Geometry.Point> shiftedPoints = new List<Autodesk.DesignScript.Geometry.Point>();
            shiftedPoints.AddRange(flattenedCornerPoints.GetRange(1, flattenedCornerPoints.Count - 1)); // add the rest
            shiftedPoints.Add(flattenedCornerPoints[0]); // add the last element to the beginning

            // Chop the shifted points in segments of 2.
            List<List<Autodesk.DesignScript.Geometry.Point>> choppedPoints = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            for (int i = 0; i < shiftedPoints.Count; i += 2)
            {
                if (i + 1 < shiftedPoints.Count)
                {
                    choppedPoints.Add(new List<Autodesk.DesignScript.Geometry.Point> { shiftedPoints[i], shiftedPoints[i + 1] });
                }
            }

            // Split the polycurve edges using the points.
            List<Curve> curves = planarCurve.Curves().ToList();
            List<List<Curve>> splitCurves = new List<List<Curve>>();
            for (int i = 0; i < curves.Count; i++)
            {
                // Calculate the curve length. Calculated as below to accommodate for non straight curves.
                Autodesk.DesignScript.Geometry.Point startPoint = curves[i].StartPoint;
                Autodesk.DesignScript.Geometry.Point endPoint = curves[i].EndPoint;
                float curveLength = (float)Line.ByStartPointEndPoint(startPoint, endPoint).Length;

                // Exclude curves with lengths that are less than the width of the element to be placed.
                if (curveLength < elementWidth)
                {
                    continue;
                }
                else
                {
                    splitCurves.Add(curves[i].SplitByPoints(choppedPoints[i]).ToList());
                }
            }

            // Flatten the split curve list.
            List<Curve> flattenedSplitCurves = splitCurves.SelectMany(curveList => curveList).ToList();

            // Get the polycurve corner points.
            List<Autodesk.DesignScript.Geometry.Point> points = PolyCurveCorners(planarCurve);

            // Sort the corner and center curves using their intersections with the corner points.
            List<Autodesk.DesignScript.Geometry.Geometry> cornerCurves = new List<Autodesk.DesignScript.Geometry.Geometry>();
            List<Autodesk.DesignScript.Geometry.Geometry> segmentCurves = new List<Autodesk.DesignScript.Geometry.Geometry>();

            // Track the curves already categorized to prevent duplicates.
            HashSet<Curve> alreadyCategorized = new HashSet<Curve>();

            // Loop through the flattened curves.
            foreach (Curve curve1 in flattenedSplitCurves)
            {
                bool isCornerCurve = false;

                // Check if the curve intersects with any corner points and categorize it.
                for (int i = 0; i < points.Count; i++)
                {
                    if (curve1.DoesIntersect(points[i]))
                    {
                        // If the angle is 180, it's a segment curve.
                        if (cornerAngles[i] == 180)
                        {
                            segmentCurves.Add(curve1);
                        }
                        // Otherwise, it's a corner curve.
                        else
                        {
                            cornerCurves.Add(curve1);
                        }

                        isCornerCurve = true;
                        break;  // Exit the loop once it's categorized.
                    }
                }

                // If the curve wasn't categorized as a corner curve, it is a segment curve.
                if (!isCornerCurve && !alreadyCategorized.Contains(curve1))
                {
                    segmentCurves.Add(curve1);
                    alreadyCategorized.Add(curve1);  // Track the categorized curve.
                }
            }

            // Group the intersecting corner and segment curves.
            List<List<Autodesk.DesignScript.Geometry.Geometry>> groupedCornerCurves = GeometryTools.GeometryUtilities
                .SortIntersectingGeometry(cornerCurves);
            List<List<Autodesk.DesignScript.Geometry.Geometry>> groupedSegmentCurves = GeometryTools.GeometryUtilities
                .SortIntersectingGeometry(segmentCurves);

            // Cast the grouped geometry to curves to create polycurves.
            List<List<Curve>> castCornerCurves = new List<List<Curve>>();
            foreach (List<Autodesk.DesignScript.Geometry.Geometry> geometries in groupedCornerCurves) 
            { 
                List<Curve> curveList = new List<Curve>();
                foreach (Autodesk.DesignScript.Geometry.Geometry geometry in geometries) 
                {
                    curveList.Add(geometry as Curve);
                }
                castCornerCurves.Add(curveList);
            }

            List<List<Curve>> castSegmentCurves = new List<List<Curve>>();
            foreach (List<Autodesk.DesignScript.Geometry.Geometry> geometries in groupedSegmentCurves)
            {
                List<Curve> curveList = new List<Curve>();
                foreach (Autodesk.DesignScript.Geometry.Geometry geometry in geometries)
                {
                    curveList.Add(geometry as Curve);
                }
                castSegmentCurves.Add(curveList);
            }

            // Create polycurves from the curve groups.
            List<PolyCurve> cornerPolyCurves = new List<PolyCurve>();
            foreach (List<Curve> curveList in castCornerCurves) 
            { 
                cornerPolyCurves.Add(PolyCurve.ByJoinedCurves(curveList, 0.001, false, 0));
            }

            List<PolyCurve> segmentPolyCurves = new List<PolyCurve>();
            foreach (List<Curve> curveList in castSegmentCurves)
            {
                PolyCurve polyCurve = PolyCurve.ByJoinedCurves(curveList, 0.001, false, 0);

                // Check that the length of the polycurve is not shorter than the width of the element to be placed.
                Autodesk.DesignScript.Geometry.Point startPoint = polyCurve.StartPoint;
                Autodesk.DesignScript.Geometry.Point endPoint = polyCurve.EndPoint;
                float curveLength = (float)Line.ByStartPointEndPoint(startPoint, endPoint).Length;

                if (curveLength >= elementWidth) 
                {
                    segmentPolyCurves.Add(polyCurve);
                }
            }

            return new Dictionary<string, List<PolyCurve>> 
            {
                { "cornerPolyCurves", cornerPolyCurves },
                { "segmentPolyCurves", segmentPolyCurves }
            };
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
        /// Split a list of curves with a list of points and return new curves representing the splits.
        /// </summary>
        /// <param name="inputCurves">The input curves.</param>
        /// <param name="points">The list of points.</param>
        /// <param name="splitWidths">The width of the split gap at the points.</param>
        /// <returns name="gapCurves">The curves within the split gaps.</returns>
        /// <returns name="splitCenter">The input points projected onto the curves and at the center of the split.</returns>
        /// <returns name="splitCurves">The resulting curves from splitting the input curves.</returns>
        /// <returns name="splitPoints">The points on either side of the input points used in splitting the curves.</returns>
        [MultiReturn(new[] { "gapCurves", "splitCenter", "splitCurves", "splitPoints" })]
        public static Dictionary<string, object> SplitCurvesByPoints(
            List<Curve> inputCurves,
            List<Autodesk.DesignScript.Geometry.Point> points,
            List<float> splitWidths)
        {
            // Throw an error if the split width is less than 0.001.
            foreach (float width in splitWidths)
            {
                if (width < 0.001)
                {
                    throw new ArgumentException("The split width cannot be less than 0.001");
                }
            }

            // Ensure the splitWidths has the same length as points or duplicate the single value if only one provided.
            if (splitWidths.Count == 1)
            {
                float singleWidth = splitWidths[0];
                splitWidths = Enumerable.Repeat(singleWidth, points.Count).ToList();
            }
            else if (splitWidths.Count != points.Count)
            {
                throw new ArgumentException("The number of split widths must match the number of points");
            }

            // Check if there are any intersecting geometries
            for (int i = 0; i < inputCurves.Count; i++)
            {
                for (int j = 0; j < inputCurves.Count; j++)
                {
                    // Skip self-intersection check
                    if (i == j)
                        continue;

                    // Check if curve[i] intersects with curve[j]
                    if (inputCurves[i].DoesIntersect(inputCurves[j]))
                    {
                        throw new Exception("No intersecting curves allowed");
                    }
                }
            }

            // Get the curves of the input curves. Flatten any polycurves into their consistuent curves.
            List<Curve> allCurves = new List<Curve>();
            foreach (Curve curve in inputCurves)
            {
                if (curve is PolyCurve polyCurve)
                {
                    allCurves.AddRange(polyCurve.Curves());
                }
                else
                {
                    allCurves.Add(curve);
                }
            }

            // Get the distances of the points from the boundary curves.
            List<List<float>> pointDistances = new List<List<float>>();
            foreach (Autodesk.DesignScript.Geometry.Point point in points)
            {
                List<float> distanceList = new List<float>();
                foreach (Curve curve in allCurves)
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
                closestCurves.Add(allCurves[i]);
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
            Dictionary<string, List<Curve>> curveGroups = new Dictionary<string, List<Curve>>();

            for (int i = 0; i < curveCenterPointStrings.Count; i++)
            {
                string currentString = curveCenterPointStrings[i];
                Curve currentCurve = closestCurves[i];

                // If the group doesn't exist yet create it.
                if (!curveGroups.ContainsKey(currentString))
                {
                    curveGroups[currentString] = new List<Curve>();
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

            // Convert the point dictionary values to a list of lists.
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
                for (int j = 0; j < projectedPoints[i].Count; j++)
                {
                    parameterList.Add((float)groupedCurves[i][0].ParameterAtPoint(projectedPoints[i][j]));
                }
                projectedPointParameters.Add(parameterList);
            }

            // Create a list of numbers to indicate half of the split width.
            List<List<float>> entranceHalfDistances = new List<List<float>>();
            foreach (float width in splitWidths)
            {
                entranceHalfDistances.Add(Maths.Range(width / 2, -width / 2, 2));
            }

            // Group the half distances by the curve center point string values.
            List<List<List<float>>> groupedSplitDistances = new List<List<List<float>>>();
            Dictionary<string, List<List<float>>> distanceGroups = new Dictionary<string, List<List<float>>>();

            for (int i = 0; i < curveCenterPointStrings.Count; i++)
            {
                string currentString = curveCenterPointStrings[i];
                List<float> currentDistances = entranceHalfDistances[i];

                // If the group doesn't exist yet, create it.
                if (!distanceGroups.ContainsKey(currentString))
                {
                    distanceGroups[currentString] = new List<List<float>>();
                }

                // Add the point to the appropriate group.
                distanceGroups[currentString].Add(currentDistances);
            }

            // Convert the distance dictionary values to a list of lists.
            groupedSplitDistances = distanceGroups.Values.ToList();

            // Create points from the curve parameter with the split widths.##########Adjust here!!!!!!
            List<List<List<Autodesk.DesignScript.Geometry.Point>>> splitPoints = new List<List<List<Autodesk.DesignScript.Geometry.Point>>>();
            for (int i = 0; i < groupedCurves.Count; i++)
            {
                List<List<Autodesk.DesignScript.Geometry.Point>> pointListGroup = new List<List<Autodesk.DesignScript.Geometry.Point>>();
                for (int j = 0; j < projectedPointParameters[i].Count; j++)
                {
                    List<Autodesk.DesignScript.Geometry.Point> pointList = new List<Autodesk.DesignScript.Geometry.Point>();
                    for (int k = 0; k < groupedSplitDistances[i][j].Count; k++)
                    {
                        pointList.Add(groupedCurves[i][0].PointAtChordLength(groupedSplitDistances[i][j][k], projectedPointParameters[i][j], true));
                    }
                    pointListGroup.Add(pointList);
                }
                splitPoints.Add(pointListGroup);
            }

            // Split the curves with the new splitPoints structure.
            List<List<Curve>> splitCurveLists = new List<List<Curve>>();
            for (int i = 0; i < groupedCurves.Count; i++)
            {
                // Flatten the inner lists of splitPoints for the current curve
                List<Autodesk.DesignScript.Geometry.Point> flatPoints = splitPoints[i]
                    .SelectMany(innerList => innerList)
                    .ToList();

                // Split the curve using the flattened list of points
                splitCurveLists.Add(groupedCurves[i][0].SplitByPoints(flatPoints).ToList());
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

            // Get the split curves intersecting with the projected points.
            List<List<Curve>> splitGapCurveLists = new List<List<Curve>>();
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
                            break; // Exit loop if no intersection is found.
                        }
                    }
                    if (intersects)
                    {
                        curveList.Add(curve);
                    }
                }

                splitGapCurveLists.Add(curveList);
            }

            // Get the curves not close to the points.
            List<Curve> otherCurves = allCurves.ToList();
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
            List<List<Autodesk.DesignScript.Geometry.Geometry>> intersectingCurves = Common.GeometryTools.GeometryUtilities.SortIntersectingGeometry(combinedGeometry);

            // Cast the intersecting curve groups. 
            List<List<Curve>> castIntersectingCurves = new List<List<Curve>>();
            foreach (List<Autodesk.DesignScript.Geometry.Geometry> geometryList in intersectingCurves)
            {
                List<Curve> curveList = new List<Curve>();
                foreach (Autodesk.DesignScript.Geometry.Geometry geometry in geometryList)
                {
                    curveList.Add(geometry as Curve);
                }
                castIntersectingCurves.Add(curveList);
            }

            // Create polycurves from the grouped intersecting curves. 
            List<PolyCurve> polyCurves = new List<PolyCurve>();
            foreach (List<Curve> curveList in castIntersectingCurves)
            {
                polyCurves.Add(PolyCurve.ByJoinedCurves(curveList, 0.001, false));
            }

            // Cast the sorted polycurves to curves.
            List<Curve> castSortedPolyCurves = new List<Curve>();
            foreach (PolyCurve curve in polyCurves)
            {
                castSortedPolyCurves.Add(curve);
            }

            // Flatten the gap curve list.
            List<Curve> flattenedGapCurves = splitGapCurveLists.SelectMany(curveList => curveList).ToList();

            // Flatten the projected point list.
            List<Autodesk.DesignScript.Geometry.Point> flattenedPoints = projectedPoints.SelectMany(pointList => pointList).ToList();

            // Create a dictionary to provide various elements.
            Dictionary<string, object> curveDictionary = new Dictionary<string, object>
            {
                { "gapCurves", flattenedGapCurves }, // flattenedGapCurves
                { "splitCenter", flattenedPoints },
                { "splitCurves", castSortedPolyCurves }, // castSortedPolyCurves
                { "splitPoints", splitPoints }
            };

            return curveDictionary;
        }


        /// <summary>
        /// Reorder a list of curves based on their angle from the Y axis.
        /// Best for ordering disorganized curves generated from concave surface perimeters.
        /// </summary>
        /// <param name="curves">The input curves.</param>
        /// <param name="sortingPoint">A point at the center of the input curves.</param>
        /// <returns name="orderedPolyCurves">The ordered curves.</returns>
        public static List<Curve> ReorderCurvePositions(List<Curve> curves, Autodesk.DesignScript.Geometry.Point sortingPoint)
        {
            // Get the center point of the curve bounding boxes.
            List<Autodesk.DesignScript.Geometry.Point> centerPoints = new List<Autodesk.DesignScript.Geometry.Point>();
            foreach (Curve curve in curves)
            {
                // Get the bounding box.
                BoundingBox boundingBox = curve.BoundingBox;

                // Create the diagonal lines of the bounding boxes.
                Line diagonalLine = Line.ByStartPointEndPoint(boundingBox.MinPoint, boundingBox.MaxPoint);

                // Get the center point of the lines.
                Autodesk.DesignScript.Geometry.Point centerPoint = diagonalLine.PointAtParameter(0.5);

                centerPoints.Add(centerPoint);
            }

            // Get the curve angles from the Y axis.
            List<float> anglesFromY = new List<float>();
            foreach (Autodesk.DesignScript.Geometry.Point centerPoint in centerPoints)
            {
                // Create vector from the sorting point to the polycurve bounding box centers.
                Autodesk.DesignScript.Geometry.Vector vector = Autodesk.DesignScript.Geometry.Vector.ByTwoPoints(sortingPoint, centerPoint);

                // Calculate the angles between the vectors and the Y axis.
                float angleFromY = (float)Autodesk.DesignScript.Geometry.Vector.YAxis().AngleAboutAxis(vector, Autodesk.DesignScript.Geometry.Vector.ZAxis());
                anglesFromY.Add(angleFromY);
            }

            // Sort the curves using the angle values.
            List<Curve> sortedCurves = curves.Zip(anglesFromY, (poly, angleFromY) => new { poly, angleFromY })
                .OrderBy(x => x.angleFromY)
                .Select(x => x.poly)
                .ToList();

            return sortedCurves;
        }


        /// <summary>
        /// To reorder input curves in the same direction given a point at their center.
        /// Best for ordering disorganized curves generated from concave surface perimeters.
        /// </summary>
        /// <param name="curves">The input curves.</param>
        /// <param name="sortingPoint">A point at the center of the input curves.</param>
        /// <returns name="orderedCurves">The ordered curves.</returns>
        public static List<Curve> ReorderCurveDirections(List<Curve> curves, Autodesk.DesignScript.Geometry.Point sortingPoint)
        {
            List<float> sortingAngles = new List<float>();
            foreach (Curve curve in curves)
            {
                // Get the name of the polycurve type.
                String curveName = curve.GetType().Name;

                Curve selectedCurve;
                // Check if the curve is a polycurve. If so add the first curve of the polycurve to a list.
                if (curveName == "PolyCurve")
                {
                    PolyCurve currentCurve = (PolyCurve)curve;
                    selectedCurve = currentCurve.Curves().ToList()[0];
                }
                else
                {
                    selectedCurve = curve;
                }

                // Create a vector between the sorting point and the start point of the selected curve.
                Autodesk.DesignScript.Geometry.Vector sortingVector = Autodesk.DesignScript.Geometry.Vector
                    .ByTwoPoints(sortingPoint, selectedCurve.StartPoint);

                // Get the tangent at the start point of the selected curve.
                Autodesk.DesignScript.Geometry.Vector startTangent = selectedCurve.TangentAtParameter(0);

                // Get the angle of the tangent around the sorting vector.
                float tangentAngle = (float)sortingVector.AngleAboutAxis(startTangent, Autodesk.DesignScript.Geometry.Vector.ZAxis());
                sortingAngles.Add(tangentAngle);
            }

            // Combine the curve and sorting angle lists.
            List<Tuple<Curve, float>> curveAnglePairs = curves.Zip(sortingAngles, (curve, angle) => Tuple.Create(curve, angle)).ToList();

            // Reverse the curve directions if their tangent angle is greater than 180.
            List<Curve> reversedCurves = new List<Curve>();
            foreach (Tuple<Curve, float> tuple in curveAnglePairs)
            {
                // Reverse the curve if the sorting angle is greater than 180.
                if (tuple.Item2 > 180)
                {
                    reversedCurves.Add(tuple.Item1.Reverse());
                }
                else
                {
                    reversedCurves.Add(tuple.Item1);
                }
            }

            return reversedCurves;
        }


        /// <summary>
        /// Get the average center point of a list of input curves.
        /// </summary>
        /// <param name="curves">The input curves.</param>
        /// <returns name="averageCenter">The average center point.</returns>
        public static Autodesk.DesignScript.Geometry.Point CurveListAverageCenter(List<Curve> curves)
        {
            // Get the average center point of the input curves.
            List<Autodesk.DesignScript.Geometry.Point> allPoints = new List<Autodesk.DesignScript.Geometry.Point>();
            foreach (Curve curve in curves)
            {
                // Get the start, mid, and end points of all the input curves.
                allPoints.Add(curve.StartPoint);
                allPoints.Add(curve.PointAtParameter(0.5));
                allPoints.Add(curve.EndPoint);
            }
            Autodesk.DesignScript.Geometry.Point averagePoint = Line.ByBestFitThroughPoints(allPoints).PointAtParameter(0.5);

            return averagePoint;
        }


        /// <summary>
        /// Returns points spaced along the input curve at equal chord length based on the input number of divisions.
        /// Unlike the default node this node adds the start and end points.
        /// </summary>
        /// <param name="curve">The input curve.</param>
        /// <param name="divisions">Number of divisions.</param>
        /// <returns name="points">List of points on curve.</returns>
        public static List<Autodesk.DesignScript.Geometry.Point> PointsAtEqualChordLength(Curve curve, int divisions = 10) 
        {
            // Get the start and end points of the curve.
            Autodesk.DesignScript.Geometry.Point startPoint = curve.StartPoint;
            Autodesk.DesignScript.Geometry.Point endPoint = curve.EndPoint;

            // Add points along the curve using the division input.
            List<Autodesk.DesignScript.Geometry.Point> points = curve.PointsAtEqualChordLength(divisions).ToList();

            // Add the start and end points to the points list.
            points.Insert(0, startPoint);
            points.Add(endPoint);

            return points;
        }


        /// <summary>
        /// Get the parameters at given points along a curve.
        /// </summary>
        /// <param name="curve">The input curve.</param>
        /// <param name="points">List of points.</param>
        /// <returns>The parameters.</returns>
        internal static List<float> CurveParametersAtPoints(Curve curve, List<Autodesk.DesignScript.Geometry.Point> points) 
        {
            // Get the parameters.
            List<float> parameters = new List<float>();
            foreach (Autodesk.DesignScript.Geometry.Point point in points) 
            { 
                parameters.Add((float)curve.ParameterAtPoint(point));
            }

            return parameters;
        }


        /// <summary>
        /// Get the normals at given points along a curve.
        /// This node ensures the normals are pointing towards the same side of the curve even if the curve is a nurbscurve.
        /// </summary>
        /// <param name="curve">The input curve.</param>
        /// <param name="points">List of points.</param>
        /// <returns>The normals at the points.</returns>
        public static List<Autodesk.DesignScript.Geometry.Vector> CurveNormalsAtPoints(
            Curve curve, 
            List<Autodesk.DesignScript.Geometry.Point> points) 
        {
            // Get the normal vectors at the points.
            List<Autodesk.DesignScript.Geometry.Vector> normals = new List<Autodesk.DesignScript.Geometry.Vector>();
            foreach (Point point in points) 
            {
                // Get the parameter at the point.
                float parameter = (float)curve.ParameterAtPoint(point);

                // Get the normal at the point and add to the vector list.
                Autodesk.DesignScript.Geometry.Vector normal = curve.NormalAtParameter(parameter);

                // Get the tangent at the point.
                Autodesk.DesignScript.Geometry.Vector tangent = curve.TangentAtParameter(parameter);

                // Calculate the angle between the normal and the tangents.
                float angle = (float)tangent.AngleAboutAxis(normal, Autodesk.DesignScript.Geometry.Vector.ZAxis());

                // Reverse the direction of the normal if its angle around the tangent is greater than 90 degrees.
                if (angle > 90) 
                {
                    normals.Add(normal.Reverse());
                }
                else 
                { 
                    normals.Add(normal);
                }
            }

            return normals;
        }


        /// <summary>
        /// Get the tangents at given points along a curve.
        /// </summary>
        /// <param name="curve">The input curve.</param>
        /// <param name="points">List of points.</param>
        /// <returns>The tangents at the points.</returns>
        public static List<Autodesk.DesignScript.Geometry.Vector> CurveTangentsAtPoints(
            Curve curve,
            List<Autodesk.DesignScript.Geometry.Point> points)
        {
            // Get the tangent vectors at the points.
            List<Autodesk.DesignScript.Geometry.Vector> tangents = new List<Autodesk.DesignScript.Geometry.Vector>();
            foreach (Point point in points)
            {
                // Get the parameter at the point.
                float parameter = (float)curve.ParameterAtPoint(point);

                // Get the normal at the point and add to the vector list.
                tangents.Add(curve.TangentAtParameter(parameter));
            }

            return tangents;
        }
    }
}
