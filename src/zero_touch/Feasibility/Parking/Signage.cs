using Autodesk.DesignScript.Geometry;
using DSCore;
using ProtoCore.AST.ImperativeAST;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Parking
{
    /// <summary>
    /// Defines enums for the types of parking signage.
    /// </summary>
    public enum SignageType 
    {
        /// <summary>
        /// Electric vehicle parking.
        /// </summary>
        EV,

        /// <summary>
        /// Parking for Persons With Disabilities.
        /// </summary>
        PWD,

        /// <summary>
        /// Generic parking P signage.
        /// </summary>
        Standard,

        /// <summary>
        /// Car parking.
        /// </summary>
        Car,

        /// <summary>
        /// Bicycle parking.
        /// </summary>
        Bicycle,

        /// <summary>
        /// Motorbike parking.
        /// </summary>
        Motorbike,

        /// <summary>
        /// No parking signage.
        /// </summary>
        NoParking,

        /// <summary>
        /// No signage requirement for the parking bay.
        /// </summary>
        NoSignage,
    }


    /// <summary>
    /// Wrapper class for the parking signage.
    /// </summary>
    public class Signage
    {
        // hides the overall class as a node.
        private Signage() { }


        /// <summary>
        /// Method to load embedded resource files. 
        /// </summary>
        /// <param name="resourcePath">The resource file path.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetEmbeddedResourceContent(string resourcePath) 
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath)) 
            {
                if (stream == null) 
                { 
                    throw new ArgumentException("Resource not found: " + resourcePath);
                }
                using (StreamReader reader = new StreamReader(stream)) 
                { 
                    return reader.ReadToEnd();
                }
            }
        }


        /// <summary>
        /// Helper method to list all embedded resources (for debugging)
        /// </summary>
        /// <returns name="resourcePath">The resource file paths.</returns>
        public static List<string> ListAllEmbeddedResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            List<string> result = new List<string>();
            foreach (string resourcePath in assembly.GetManifestResourceNames())
            {
                result.Add(resourcePath);
            }

            return result;
        }


        /// <summary>
        /// To the load the contents of the embedded SAT file.
        /// </summary>
        /// <param name="resourcePath">The path of the resource file.</param>
        /// <returns></returns>
        public static Geometry[] LoadEmbeddedSAT(string resourcePath) 
        { 
            string satContent = GetEmbeddedResourceContent(resourcePath);
            return Geometry.ImportFromSAT(satContent, 100);
        }


        /// <summary>
        /// To load the geometry contents of the embedded JSON file.
        /// </summary>
        /// <param name="resourcePath">The path of the resource file.</param>
        /// <returns name="geometry[]">The geometry content of the json file.</returns>
        public static Geometry[] LoadEmbeddedJSON(string resourcePath)
        {
            string jsonContent = GetEmbeddedResourceContent(resourcePath);
            return Geometry.FromSolidDef(jsonContent);
        }


        /// <summary>
        /// To add all the required transformations to the signage.
        /// </summary>
        /// <param name="locationPlane">The target plane of the signage.</param>
        /// <param name="signageRotation">The rotation value of the signage.</param>
        /// <param name="initialSignageOffset">The offset of the signage from the base plane.</param>
        /// <param name="initialHostWidth">The initial host width at which the signage was drawn.</param>
        /// <param name="userHostWidth">The final host width to scale the signage.</param>
        /// <param name="resourcePath">The path of the symbol resource file.</param>
        /// <returns name="signageGeometry">Returns the transformed signage geometry.</returns>
        public static List<Geometry> TransformSignage( 
            Plane locationPlane,
            float signageRotation,
            float initialSignageOffset = (float)0.01,
            float initialHostWidth = (float)2.5,
            float userHostWidth = (float)2.5,
            string resourcePath = "Feasibility.Parking.Symbols.StandardParkingSymbol.json") 
        {
            // load the signage geometry.
            Geometry[] geometries = LoadEmbeddedJSON(resourcePath);

            // create the center point of the signage.
            Point signageCenter = Point.ByCoordinates(0, 0);

            // add a plane at the center point.
            Plane plane = Plane.ByOriginNormal(signageCenter, Vector.ZAxis());

            // rotate the signage geometries.
            List<Geometry> rotatedGeometry= new List<Geometry>();
            foreach (Geometry geom in geometries) 
            { 
                if (geom != null)
                {
                    rotatedGeometry.Add(geom.Rotate(plane, signageRotation));
                }
            }

            // scale the signage based on the width of the parking bay.
            float scaleFactor = userHostWidth / initialHostWidth;

            List<Geometry> scaledGeometry = new List<Geometry>();
            foreach (Geometry geom in rotatedGeometry) 
            { 
                scaledGeometry.Add(geom.Scale(plane, scaleFactor, scaleFactor));
            }

            // move the signage along the plane normal.
            float offsetDistance = initialSignageOffset  * scaleFactor;

            List<Geometry> movedGeometry = new List<Geometry>();
            foreach (Geometry geom in scaledGeometry) 
            { 
                movedGeometry.Add(geom.Translate(plane.Normal, offsetDistance));
            }

            // transform the signage geometry to the target location plane.
            List<Geometry> transGeometry = new List<Geometry>();
            foreach (Geometry geom in movedGeometry) 
            { 
                transGeometry.Add(geom.Transform(CoordinateSystem.ByPlane(plane), CoordinateSystem.ByPlane(locationPlane)));
            }

            return transGeometry;
        }


        /// <summary>
        /// To get the signage outline.
        /// </summary>
        /// <param name="locationPlane">The target plane of the signage.</param>
        /// <param name="signageRotation">The rotation value of the signage.</param>
        /// <param name="initialSignageOffset">The offset of the signage from the base plane.</param>
        /// <param name="initialHostWidth">The initial host width at which the signage was drawn.</param>
        /// <param name="userHostWidth">The final host width to scale the signage.</param>
        /// <param name="resourcePath">The path of the symbol resource file.</param>
        /// <returns name="signageOutline">The signage outline curves.</returns>
        public static List<Curve[]> GetSignageOutline(
            Plane locationPlane,
            float signageRotation,
            float initialSignageOffset = (float)0.01,
            float initialHostWidth = (float)2.5,
            float userHostWidth = (float)2.5,
            string resourcePath = "Feasibility.Parking.Symbols.StandardParkingSymbol.json")
        {
            // create the signage geometry.
            List<Geometry> geometry = TransformSignage(
                locationPlane,
                signageRotation,
                initialSignageOffset,
                initialHostWidth,
                userHostWidth,
                resourcePath);

            // get the signage outline.
            List<Curve[]> curves = new List<Curve[]>();
            foreach (Geometry geom in geometry)
            {
                try
                {
                    Surface surface = geom as Surface;
                    curves.Add(surface.PerimeterCurves());
                }
                catch
                {
                    curves.Add(null);
                }
            }

            return curves;
        }


        /// <summary>
        /// Get the 2d standard parking signage curves.
        /// </summary>
        /// <param name="locationPlane">The target plane of the signage.</param>
        /// <param name="signageRotation">The rotation value of the signage.</param>
        /// <param name="initialSignageOffset">The offset of the signage from the base plane.</param>
        /// <param name="initialHostWidth">The initial host width at which the signage was drawn.</param>
        /// <param name="userHostWidth">The final host width to scale the signage.</param>
        /// <param name="resourcePath">The path of the symbol resource file.</param>
        /// <returns name="signageOutline">The signage outline curves.</returns>
        public static List<Curve[]> ParkingSymbol2D(
            Plane locationPlane,
            float signageRotation,
            float initialSignageOffset = (float)0.01,
            float initialHostWidth = (float)2.5,
            float userHostWidth = (float)2.5,
            string resourcePath = "Feasibility.Parking.Symbols.carParkingSymbol.json")
        {
            // add the symbol outlines.
            List<Curve[]> curves = GetSignageOutline(
                locationPlane,
                signageRotation,
                initialSignageOffset,
                initialHostWidth,
                userHostWidth,
                resourcePath);

            return curves;
        }
    }
}
