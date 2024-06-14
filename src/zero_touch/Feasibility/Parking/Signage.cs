using Autodesk.DesignScript.Geometry;
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
        /// <param name="resourceName">The resource file name.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetEmbeddedResourceContent(string resourceName) 
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)) 
            {
                if (stream == null) 
                { 
                    throw new ArgumentException("Resource not found: " +  resourceName);
                }
                using (StreamReader reader = new StreamReader(stream)) 
                { 
                    return reader.ReadToEnd();
                }
            }
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

        public static Geometry[] LoadEmbeddedJSON(string resourcePath)
        {
            string jsonContent = GetEmbeddedResourceContent(resourcePath);
            return Geometry.FromSolidDef(jsonContent);
        }


        public static Geometry[] TestSymbol() 
        {
            return LoadEmbeddedJSON("Feasibility.Parking.Symbols.StandardParkingSymbol.json");
            
        }


        // Helper method to list all embedded resources (for debugging)
        public static List<string> ListAllEmbeddedResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            List<string> result = new List<string>();
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                result.Add(resourceName);
            }

            return result;
        }

        public static Point SignageTransformations() 
        {
            // create the center point of the signage.
            Point signageCenter = Point.ByCoordinates(0, 0);

            return Point.ByCoordinates(0, 0);
        }


    }
}
