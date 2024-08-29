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
        /// The target position of the bicycle rack.
        /// </summary>
        internal Autodesk.DesignScript.Geometry.Point TargetPosition { private get; set; }

        /// <summary>
        /// The diameter of the bicycle rack's tube.
        /// </summary>
        public float RackDiameter { get; set; }

        /// <summary>
        /// The height of the bicycle rack.
        /// </summary>
        public float RackHeight { get; set; }

        /// <summary>
        /// The length of the bicycle rack.
        /// </summary>
        public float RackLength { get; set; }

        /// <summary>
        /// The angle of the bicycle rack.
        /// </summary>
        public float RackAngle { private get; set; }

        /// <summary>
        /// The offset of the bicycle rack from the side of the parking bay.
        /// </summary>
        public float RackOffset { private get; set; }


        /// <summary>
        /// Creates a bicycle rack instance.
        /// </summary>
        /// <param name="rackDiameter">The diameter of the bicycle rack's tube.</param>
        /// <param name="rackHeight">The height of the bicycle rack.</param>
        /// <param name="rackLength">The length of the bicycle rack.</param>
        /// <param name="rackAngle">The angle of the bicycle rack.</param>
        /// <param name="rackOffset"></param>
        public BicycleRack(
            float rackDiameter = 0.032f, 
            float rackHeight = 1f, 
            float rackLength = 1.5f, 
            float rackAngle = 0f)
        {
            RackDiameter = rackDiameter;
            RackHeight = rackHeight;
            RackLength = rackLength;
            RackAngle = rackAngle;
        }

        /// <summary>
        /// To create an inverted U bicycle rack.
        /// </summary>
        /// <param name="cornerRadius">The corner radius of the rack tube.</param>
        /// <param name="basePlateDiameter">The diameter of the rack base plates.</param>
        /// <param name="basePlateThickness">Thickness of the rack base plates.</param>
        /// <returns name="rackSolid">The solid of the bicycle rack.</returns>
        /// <exception cref="Exception"></exception>
        public Solid CreateInvertedURack(
            float cornerRadius = 0.15f, 
            float basePlateDiameter = 0.1f, 
            float basePlateThickness = 0.01f) 
        {
            // Create the rack center point.
            Autodesk.DesignScript.Geometry.Point point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0);

            // Create the rack points.
            Autodesk.DesignScript.Geometry.Point firstPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates
                ((-RackLength / 2) + (RackDiameter / 2), 0);
            Autodesk.DesignScript.Geometry.Point secondPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                -RackLength / 2 + RackDiameter / 2,
                0,
                RackHeight - RackDiameter/2);
            Autodesk.DesignScript.Geometry.Point thirdPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                RackLength / 2 - RackDiameter / 2,
                0,
                RackHeight - RackDiameter / 2);
            Autodesk.DesignScript.Geometry.Point fourthPoint = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                RackLength / 2 - RackDiameter / 2, 0);

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
            Circle circle = Circle.ByCenterPointRadius(firstPoint, RackDiameter / 2);

            
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

            return rackSolid;
        }
    }
}
