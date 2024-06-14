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
        /// <returns></returns>
        public static Geometry[] SignageTransformations(
            string resoursePath, 
            Plane locationPlane,
            float signageRotation,
            float bayWidth = (float)2.5) 
        {
            // load the signage geometry.
            Geometry[] geometries = LoadEmbeddedJSON(resoursePath);

            // create the center point of the signage.
            Point signageCenter = Point.ByCoordinates(0, 0);

            // add a plane at the center point.
            Plane plane = Plane.ByOriginNormal(signageCenter, Vector.ZAxis());

            // rotate the signage geometries.
            List<Geometry> rotatedGeometry= new List<Geometry>();
            foreach (Geometry geom in geometries) 
            { 
                if (geom == null) continue;

                else if (geom.GetType() == typeof(Geometry)) 
                { 
                    rotatedGeometry.Add(geom.Rotate(plane, signageRotation));
                }
            }
            
            return rotatedGeometry.ToArray();
        }


        public static Geometry[] TestSymbol()
        {
            return LoadEmbeddedJSON("Feasibility.Parking.Symbols.StandardParkingSymbol.json");

        }


    }
}
