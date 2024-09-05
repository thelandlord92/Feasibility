using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
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
        private static Curve ProjectCurves(Surface surface, Curve curve)
        {
            // get the plane of the perimeter curve.
            Plane curvePlane = Common.GeometryTools.Surfaces.SurfacePlane(surface);

            // pull the curve onto the plane.
            Curve pulledCurve = curve.PullOntoPlane(curvePlane);

            return pulledCurve;
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
        /// Split a polycuve with a list of points and return new curves representing the splits.
        /// </summary>
        /// <param name="inputCurves">The input polycurve.</param>
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
    }
}
