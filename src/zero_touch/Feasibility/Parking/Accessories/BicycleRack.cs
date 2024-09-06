using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Parking.Accessories
{
    /// <summary>
    /// Wrapper class for the bicycle racks.
    /// </summary>
    public class BicycleRack
    {
        /// <summary>
        /// The target plane of the bicycle rack.
        /// </summary>
        public Plane TargetPlane { private get; set; }

        private float _tubeDiameter;

        /// <summary>
        /// The diameter of the bicycle rack's tube.
        /// </summary>
        public float TubeDiameter
        {
            get { return _tubeDiameter; }
            set
            {
                if (value == 0)
                {
                    throw new ArgumentException("The rack tube diameter cannot be zero");
                }
                _tubeDiameter = value;
            }
        }

        private float _rackHeight;

        /// <summary>
        /// The height of the bicycle rack.
        /// </summary>
        public float RackHeight
        {
            get { return _rackHeight; }
            set
            {
                if (value == 0)
                {
                    throw new ArgumentException("The rack height cannot be zero");
                }
                _rackHeight = value;
            }
        }

        private float _rackLength;

        /// <summary>
        /// The length of the bicycle rack.
        /// </summary>
        public float RackLength
        {
            get { return _rackLength; }
            set
            {
                if (value == 0)
                {
                    throw new ArgumentException("The rack length cannot be zero");
                }
                _rackLength = value;
            }
        }

        /// <summary>
        /// The angle of the bicycle rack.
        /// </summary>
        public float RackAngle { private get; set; }

        /// <summary>
        /// The bicycle rack type.
        /// </summary>
        internal BicycleRackTypes BicycleRackType {  get; set; }


        /// <summary>
        /// Creates a bicycle rack instance.
        /// </summary>
        /// <param name="targetPlane">The target plane to transform the rack.</param>
        /// <param name="rackAngle">The angle of the bicycle rack.</param>
        public BicycleRack(
            Plane targetPlane = null,
            float rackAngle = 0f)
        {
            TargetPlane = targetPlane;
            RackAngle = rackAngle;
        }

        /// <summary>
        /// To create an inverted U bicycle rack.
        /// </summary>
        /// <param name="rackHeight">The height of the bicycle rack.</param>
        /// <param name="rackLength">The length of the bicycle rack.</param>
        /// <param name="cornerRadius">The corner radius of the rack tube.</param>
        /// <param name="basePlateDiameter">The diameter of the rack base plates.</param>
        /// <param name="basePlateThickness">Thickness of the rack base plates.</param>
        /// <param name="tubeDiameter">The diameter of the bicycle rack's tube.</param>
        /// <returns name="rackSolid">The solid of the bicycle rack.</returns>
        /// <exception cref="Exception"></exception>
        public Solid CreateInvertedURack(
            float rackHeight = 1f,
            float rackLength = 1f,
            float cornerRadius = 0.15f, 
            float basePlateDiameter = 0.1f, 
            float basePlateThickness = 0.01f,
            float tubeDiameter = 0.032f) 
        {
            // Check if the rack height is zero.
            if (rackHeight <= 0) 
            {
                throw new ArgumentException("The rack height cannot be zero");
            }

            // Check if the rack length is zero.
            if (rackLength <= 0)
            {
                throw new ArgumentException("The rack length cannot be zero");
            }

            // Check if the corner radius is zero.
            if (cornerRadius <= 0)
            {
                throw new ArgumentException("The corner radius cannot be zero.");
            }

            // Check if the base plate diameter is zero.
            if (basePlateDiameter <= 0)
            {
                throw new ArgumentException("The base plate diameter cannot be zero.");
            }

            // Check if the base plate diameter is zero.
            if (basePlateThickness <= 0)
            {
                throw new ArgumentException("The base plate thickness cannot be zero.");
            }

            // Check if the tube diameter is zero.
            if (tubeDiameter <= 0)
            {
                throw new ArgumentException("The rack tube diameter cannot be zero");
            }

            // Set the class attributes.
            RackHeight = rackHeight;
            RackLength = rackLength;
            TubeDiameter = tubeDiameter;
            BicycleRackType = BicycleRackTypes.InvertedURack;

            // Create the rack center point.
            Autodesk.DesignScript.Geometry.Point point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0);

            // Create the rack points.
            Autodesk.DesignScript.Geometry.Point firstPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates
                ((-rackLength / 2) + (tubeDiameter / 2), 0);
            Autodesk.DesignScript.Geometry.Point secondPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                -rackLength / 2 + tubeDiameter / 2,
                0,
                rackHeight - tubeDiameter/2);
            Autodesk.DesignScript.Geometry.Point thirdPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                rackLength / 2 - tubeDiameter / 2,
                0,
                rackHeight - tubeDiameter / 2);
            Autodesk.DesignScript.Geometry.Point fourthPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                rackLength / 2 - tubeDiameter / 2, 0);

            // Add the rack points to a list.
            List<Autodesk.DesignScript.Geometry.Point> points = new List<Autodesk.DesignScript.Geometry.Point>();
            points.Add(firstPoint);
            points.Add(secondPoint);
            points.Add(thirdPoint);
            points.Add(fourthPoint);

            PolyCurve polyCurve = null;
            try
            {
                // Create a polycurve from the points.
                polyCurve = PolyCurve.ByPoints(points);
            }
            catch 
            {
                throw new Exception("Rack geometry could not be created. Adjust width / height dimensions");
            }

            PolyCurve filletPolyCurve = null;
            try 
            {
                // Fillet the polycurve.
                filletPolyCurve = polyCurve.Fillet(cornerRadius, false);
            }
            catch 
            {
                throw new Exception("The fillet could not be created. Adjust corner radius / rack length / rack height.");
            }
            
            // Create a circle at the first point.
            Circle circle = Circle.ByCenterPointRadius(firstPoint, tubeDiameter / 2);
 
            // Create a list to hold all the solids.
            List<Solid> rackSolids = new List<Solid>();

            try
            {
                // Create base plates.
                Curve circle1 = Circle.ByCenterPointRadius(firstPoint, basePlateDiameter / 2);
                Curve circle2 = Circle.ByCenterPointRadius(fourthPoint, basePlateDiameter / 2);
                rackSolids.Add(circle1.ExtrudeAsSolid(basePlateThickness));
                rackSolids.Add(circle2.ExtrudeAsSolid(basePlateThickness));
            }
            catch
            {
                throw new Exception("Base plates not creted. Adjust dimensions");
            }


            try
            {
                // Sweep the circle along the polycurve.
                Solid tubeSolid = Solid.BySweep(circle, filletPolyCurve, true) as Solid;
                rackSolids.Add(tubeSolid);
            }
            catch
            {
                throw new Exception("Sweep not created. Adjust dimensions");
            }

            Solid rackSolid = null;
            try 
            {
                // Join the rack solids.
                rackSolid = Solid.ByUnion(rackSolids);
            }
            catch
            {
                throw new Exception("Could not join the rack solids together. Adjust dimensions.");
            }

            // Create a temp target plane if the target plane input is null.
            Plane _targetPlane = null;
            if (TargetPlane == null) 
            {
                _targetPlane = Plane.ByOriginNormal(point, Autodesk.DesignScript.Geometry.Vector.ZAxis());
            }
            else 
            { 
                _targetPlane = TargetPlane;
            }

            // Add transformations to the rack.
            List<Geometry> transformedRack = Common.GeometryTools.GeometryUtilities.AddTransformations(
                new List<Geometry>() { rackSolid as Geometry },
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0),
                _targetPlane,
                Autodesk.DesignScript.Geometry.Vector.ZAxis(),
                RackAngle,
                0,
                1
            );

            return transformedRack[0] as Solid;
        }


        /// <summary>
        /// Create a wave-shaped bicycle rack.
        /// </summary>
        /// <param name="rackHeight">The height of the bicycle rack.</param>
        /// <param name="rackLength">The length of the bicycle rack.</param>
        /// <param name="numberOfWaves">The number of waves in the rack.</param>
        /// <param name="waveOffsetFromBase">The vertical offset of the wave tube from the base level.</param>
        /// <param name="plateDiameter">The diameter of the base plates.</param>
        /// <param name="plateThickness">The thickness of the base plates.</param>
        /// <param name="tubeDiameter">The diameter of the bicycle rack's tube.</param>
        /// <returns>A list of curves representing the wave bicycle rack.</returns>
        /// <exception cref="ArgumentException">Thrown when an invalid argument is provided.</exception>
        public Solid CreateWaveRack(
            float rackHeight = 1f,
            float rackLength = 1f,
            int numberOfWaves = 1,
            float waveOffsetFromBase = 0.1f,
            float plateDiameter = 0.1f,
            float plateThickness = 0.01f,
            float tubeDiameter = 0.032f)
        {
            // Validate inputs
            if (rackHeight <= 0)
            {
                throw new ArgumentException("The rack height cannot be zero");
            }

            if (rackLength <= 0)
            {
                throw new ArgumentException("The rack length cannot be zero");
            }

            if (numberOfWaves < 1)
            {
                throw new ArgumentException("The number of waves must be at least 1.");
            }

            if (plateDiameter <= 0)
            {
                throw new ArgumentException("The plate diameter must be greater than zero.");
            }

            if (plateThickness <= 0)
            {
                throw new ArgumentException("The plate thickness must be greater than zero.");
            }

            if (tubeDiameter <= 0)
            {
                throw new ArgumentException("The rack tube diameter cannot be zero");
            }

            // Set the class attributes.
            RackHeight = rackHeight;
            RackLength = rackLength;
            TubeDiameter = tubeDiameter;
            BicycleRackType = BicycleRackTypes.WaveRack;

            // Create the central point of the rack
            Autodesk.DesignScript.Geometry.Point origin = Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0);

            // Calculate the wave radius
            float waveRadius = ((rackLength - tubeDiameter) / (numberOfWaves * 2 + 1)) / 2;

            // Create the starting point for the wave
            Autodesk.DesignScript.Geometry.Point startPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                (-rackLength / 2) + (tubeDiameter / 2), 0);

            // Create the end point for the wave
            Autodesk.DesignScript.Geometry.Point endPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                (rackLength / 2) - (tubeDiameter / 2), 0);

            // Create a circle at the start point (representing a cross-section of the rack)
            Circle crossSection = Circle.ByCenterPointRadius(startPoint, tubeDiameter / 2);

            // Create the first vertical line of the wave
            List<Curve> leftCurves = new List<Curve>();
            Curve verticalLine = Line.ByStartPointDirectionLength(
                startPoint,
                Autodesk.DesignScript.Geometry.Vector.ZAxis(),
                rackHeight - waveRadius - tubeDiameter / 2
            ) as Curve;

            Curve arc;
            try
            {
                // Create the arc for the wave
                Autodesk.DesignScript.Geometry.Point arcCenter = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                    startPoint.X + waveRadius, 0, rackHeight - waveRadius - tubeDiameter / 2);
                Plane arcPlane = Plane.ByOriginNormal(arcCenter, Autodesk.DesignScript.Geometry.Vector.YAxis());
                arc = EllipseArc.ByPlaneRadiiAngles(arcPlane, waveRadius, waveRadius, -90, 90);
            }
            catch 
            {
                throw new Exception("The wave arc could not be created. Adjust rack length / rack height / number of waves.");
            }

            // Add the vertical line and arc to the list of left curves
            leftCurves.Add(verticalLine);
            leftCurves.Add(arc);

            // Join the left curves into a polycurve
            Curve leftJoinedCurves = PolyCurve.ByJoinedCurves(leftCurves, 0.001, false);

            // Mirror the joined left curves to create the right side of the rack
            Plane mirrorPlane = Plane.ByOriginNormal(origin, Autodesk.DesignScript.Geometry.Vector.XAxis());
            Curve rightJoinedCurves = leftJoinedCurves.Mirror(mirrorPlane) as Curve;

            Curve waveBaseArc;
            Autodesk.DesignScript.Geometry.Point waveBasePoint;
            try 
            {
                // Create the first wave
                waveBasePoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                    (- rackLength / 2) + (tubeDiameter / 2) + (waveRadius * 3),
                    0, 
                    waveOffsetFromBase + tubeDiameter / 2 + waveRadius);
                Plane waveBaseArcPlane = Plane.ByOriginNormal(waveBasePoint, Autodesk.DesignScript.Geometry.Vector.YAxis());
                waveBaseArc = EllipseArc.ByPlaneRadiiAngles(waveBaseArcPlane, waveRadius, waveRadius, -90, -180);
            }
            catch 
            {
                throw new Exception("The wave arc could not be created. Adjust rack length / rack height / number of waves.");
            }
            
            // Create the left side of the wave
            List<Curve> waveLeftCurves = new List<Curve>();
            Curve mirroredArc = arc.Mirror(Plane.ByOriginNormal(arc.EndPoint, Autodesk.DesignScript.Geometry.Vector.XAxis())) as Curve;
            Curve connectingLine = Line.ByStartPointEndPoint(mirroredArc.StartPoint, waveBaseArc.EndPoint);
            waveLeftCurves.Add(connectingLine);
            waveLeftCurves.Add(mirroredArc);

            // Join the left side of the wave
            Curve leftWaveJoinedCurves = PolyCurve.ByJoinedCurves(waveLeftCurves, 0.001, false) as Curve;

            // Create mirror plane at the center of the base arc.
            Plane baseArcCenterPlane = Plane.ByOriginNormal(waveBasePoint, Autodesk.DesignScript.Geometry.Vector.XAxis());

            // Mirror the left wave to create the right side of the wave
            Curve rightWaveJoinedCurves = leftWaveJoinedCurves.Mirror(baseArcCenterPlane) as Curve;

            // Combine all the wave curves
            List<Curve> waveCurves = new List<Curve>();
            waveCurves.Add(leftWaveJoinedCurves);
            waveCurves.Add(waveBaseArc);
            waveCurves.Add(rightWaveJoinedCurves);
            Curve completeWave = PolyCurve.ByJoinedCurves(waveCurves, 0.001, false) as Curve;

            // Copy the wave as required.
            List<Curve> copiedWaves = new List<Curve>();

            // Calculate the width of one wave.
            float waveLength = waveRadius * 4;

            // Add the created wave to the wave list if only one wave.
            if (numberOfWaves == 0)
            {
                throw new Exception("The number of waves cannot be zero. Adjust wave number parameter.");
            }
            else if (numberOfWaves == 1)
            {
                copiedWaves.Add(completeWave);
            }
            else
            {
                for (int i = 0; i < numberOfWaves; i++)
                {
                    copiedWaves.Add(completeWave.Translate(Autodesk.DesignScript.Geometry.Vector.XAxis(), i * waveLength) as Curve);
                }
            }

            // Join the copied waves.
            Curve joinedCopiedWaves = PolyCurve.ByJoinedCurves(copiedWaves, 0.001, false) as Curve;

            // Create a list to hold all the sweep curves
            List<Curve> sweepCurves = new List<Curve>();
            sweepCurves.Add(leftJoinedCurves);
            sweepCurves.Add(rightJoinedCurves);
            sweepCurves.Add(joinedCopiedWaves);

            // Join the sweep curves.
            Curve joinedSweepCurves = PolyCurve.ByJoinedCurves(sweepCurves, 0.001, false);

            // Create the solids of the rack.
            List<Solid> rackSolids = new List<Solid>();

            // Create the base plates.
            rackSolids.Add(Circle.ByCenterPointRadius(startPoint, plateDiameter / 2).ExtrudeAsSolid(plateThickness));
            rackSolids.Add(Circle.ByCenterPointRadius(endPoint, plateDiameter / 2).ExtrudeAsSolid(plateThickness));

            // Create the wave sweep solid.
            try 
            {
                rackSolids.Add(Solid.BySweep(crossSection, joinedSweepCurves, false));
            }
            catch 
            {
                throw new Exception("Wave sweep could not be created. Adjust number of waves / rack tube diameter.");
            }

            // Join the rack solids 
            Solid rackSolid = Solid.ByUnion(rackSolids);

            // Create a temp target plane if the target plane input is null.
            Plane _targetPlane = null;
            if (TargetPlane == null)
            {
                _targetPlane = Plane.ByOriginNormal(origin, Autodesk.DesignScript.Geometry.Vector.ZAxis());
            }
            else
            {
                _targetPlane = TargetPlane;
            }

            // Add transformations to the rack.
            List<Geometry> transformedRack = Common.GeometryTools.GeometryUtilities.AddTransformations(
                new List<Geometry>() { rackSolid as Geometry },
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0),
                _targetPlane,
                Autodesk.DesignScript.Geometry.Vector.ZAxis(),
                RackAngle,
                0,
                1
            );

            return transformedRack[0] as Solid;
        }


        /// <summary>
        /// Create a post and ring bicycle rack.
        /// </summary>
        /// <param name="rackHeight">The height of the bicycle rack.</param>
        /// <param name="rackLength">The length of the bicycle rack.</param>
        /// <param name="plateDiameter">The diameter of the base plates.</param>
        /// <param name="plateThickness">The thickness of the base plate.</param>
        /// <param name="ringHeight">The height of the rack ring.</param>
        /// <param name="ringCornerRadius">The radius of the ring corners.</param>
        /// <param name="ringOffset">The offset of the ring from the top of the rack.</param>
        /// <param name="tubeDiameter">The diameter of the bicycle rack's tube.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Solid CreatePostandRingRack(
            float rackHeight = 1f,
            float rackLength = 0.5f,
            float plateDiameter = 0.1f,
            float plateThickness = 0.01f,
            float ringHeight = 0.5f,
            float ringCornerRadius = 0.15f,
            float ringOffset = 0.1f,
            float tubeDiameter = 0.032f)
        {
            // Validate inputs
            if (rackHeight <= 0)
            {
                throw new ArgumentException("The rack height cannot be zero");
            }

            if (rackLength <= 0)
            {
                throw new ArgumentException("The rack length cannot be zero");
            }

            if (plateDiameter <= 0)
            {
                throw new ArgumentException("The plate diameter must be greater than zero.");
            }

            if (plateThickness <= 0)
            {
                throw new ArgumentException("The plate thickness must be greater than zero.");
            }

            if (ringHeight <= ringCornerRadius * 2 + tubeDiameter)
            {
                throw new ArgumentException("The ring height must be at least 2x the corner radius plus the tube diameter.");
            }

            if (rackLength <= ringCornerRadius * 2 + tubeDiameter)
            {
                throw new ArgumentException("The rack length must be at least 2x the corner radius plus the tube diameter.");
            }

            if (tubeDiameter <= 0)
            {
                throw new ArgumentException("The rack tube diameter cannot be zero");
            }

            // Set the class attributes.
            RackHeight = rackHeight;
            RackLength = rackLength;
            TubeDiameter = tubeDiameter;
            BicycleRackType = BicycleRackTypes.PostandRingRack;

            // List to hold the rack solids.
            List<Solid> rackSolids = new List<Solid>();

            // Create the central point of the rack
            Autodesk.DesignScript.Geometry.Point origin = Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0);

            // Create point at center of the ring.
            Autodesk.DesignScript.Geometry.Point ringCenter = Autodesk.DesignScript.Geometry.Point
                .ByCoordinates(0, 0, rackHeight - (ringOffset + ringHeight / 2));

            // Create a plane at the ring center.
            Plane rackCenterPlane = Plane.ByOriginNormal(ringCenter, Autodesk.DesignScript.Geometry.Vector.YAxis());

            // Create a rectangle at the ring center plane.
            PolyCurve ringRectangle = Rectangle
                .ByWidthLength(rackCenterPlane, ringHeight - tubeDiameter / 2, rackLength - tubeDiameter / 2);

            // Round the corners of the ring rectangle.
            PolyCurve roundedRectangle = ringRectangle.Fillet(ringCornerRadius, false);

            // Create a circle at the start of the rounded ring rectangle.
            Circle circle = Circle.ByPlaneRadius(roundedRectangle.PlaneAtParameter(0), tubeDiameter / 2);

            // Create the ring sweep solid.
            Solid ringSolid = Solid.BySweep(circle, roundedRectangle, false);

            // Top point of the cyclinder at the sphere center.
            Autodesk.DesignScript.Geometry.Point sphereCenter = Autodesk.DesignScript.Geometry.Point
                .ByCoordinates(0, 0, rackHeight - tubeDiameter / 2);

            // Create the post.
            Solid post = Cylinder.ByPointsRadii(origin, sphereCenter, tubeDiameter / 2, tubeDiameter / 2);

            // Add a sphere at the post top.
            Solid sphere = Sphere.ByCenterPointRadius(sphereCenter, tubeDiameter / 2);

            // Create the base plate.
            Solid basePlate = Circle.ByCenterPointRadius(origin, plateDiameter / 2).ExtrudeAsSolid(plateThickness);

            // Add the solids to the rack solids list and join them.
            rackSolids.Add(post);
            rackSolids.Add(sphere);
            rackSolids.Add(ringSolid);
            rackSolids.Add(basePlate);
            Solid rackSolid = Solid.ByUnion(rackSolids);

            // Create a temp target plane if the target plane input is null.
            Plane _targetPlane = null;
            if (TargetPlane == null)
            {
                _targetPlane = Plane.ByOriginNormal(origin, Autodesk.DesignScript.Geometry.Vector.ZAxis());
            }
            else
            {
                _targetPlane = TargetPlane;
            }

            // Add transformations to the rack.
            List<Geometry> transformedRack = Common.GeometryTools.GeometryUtilities.AddTransformations(
                new List<Geometry>() { rackSolid as Geometry },
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0),
                _targetPlane,
                Autodesk.DesignScript.Geometry.Vector.ZAxis(),
                RackAngle,
                0,
                1
            );

            return transformedRack[0] as Solid;
        }
    }
}
