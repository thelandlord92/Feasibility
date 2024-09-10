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
using System.Windows.Controls;

namespace Common.GeometryTools
{
    /// <summary>
    /// Wrapper class for geometry utilities.
    /// Contains common geometrical operations.
    /// </summary>
    public class GeometryUtilities
    {
        // this hides the overall class as a node.
        private GeometryUtilities() { }


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
        /// <param name="mirrorHorizontal">Mirror the geometry horizontally.</param>
        /// <param name="mirrorVertical">Mirror the geometry vertically.</param>
        /// <returns name="transformedGeometry">The transformed geometry.</returns>
        public static List<Autodesk.DesignScript.Geometry.Geometry> AddTransformations(
            List<Autodesk.DesignScript.Geometry.Geometry> geometry,
            Autodesk.DesignScript.Geometry.Point geometryLocation,
            Plane hostPlane,
            [DefaultArgument("Vector.ZAxis()")] Autodesk.DesignScript.Geometry.Vector geometryPlaneNormal,
            float rotation = 0,
            float planeOffset = 0,
            float scaleFactor = 1,
            bool mirrorHorizontal = false,
            bool mirrorVertical = false)
        {
            // Add a plane at the location point of the geometry.
            Plane geometryPlane = Plane.ByOriginNormal(geometryLocation, geometryPlaneNormal);

            // Transform the geometry to the host plane.
            List<Autodesk.DesignScript.Geometry.Geometry> transGeometry = new List<Autodesk.DesignScript.Geometry.Geometry>();
            foreach (Autodesk.DesignScript.Geometry.Geometry geom in geometry) 
            { 
                if (geom != null) 
                {
                    transGeometry.Add(geom.Transform(CoordinateSystem.ByPlane(geometryPlane), CoordinateSystem.ByPlane(hostPlane)));
                }
            }

            // Rotate the geometry at the host plane.
            List<Autodesk.DesignScript.Geometry.Geometry> rotatedGeometry = new List<Autodesk.DesignScript.Geometry.Geometry>();  
            foreach (Autodesk.DesignScript.Geometry.Geometry geom in transGeometry) 
            { 
                if (geom != null) 
                {
                    rotatedGeometry.Add(geom.Rotate(hostPlane, rotation));
                }
            }

            // Scale the geometry at the host plane.
            List<Autodesk.DesignScript.Geometry.Geometry> scaledGeometry = new List<Autodesk.DesignScript.Geometry.Geometry>();
            foreach (Autodesk.DesignScript.Geometry.Geometry geom in rotatedGeometry) 
            {
                if (geom != null) 
                { 
                    scaledGeometry.Add(geom.Scale(hostPlane, scaleFactor, scaleFactor, scaleFactor));
                }
            }

            // Move the geometry along the host plane normal.
            List<Autodesk.DesignScript.Geometry.Geometry> movedGeometry = new List<Autodesk.DesignScript.Geometry.Geometry>();
            foreach (Autodesk.DesignScript.Geometry.Geometry geom in scaledGeometry) 
            { 
                if (geom != null) 
                { 
                    movedGeometry.Add(geom.Translate(hostPlane.Normal, planeOffset));
                }
            }

            // Get the x and y axis of the host plane.
            Autodesk.DesignScript.Geometry.Vector planeX = hostPlane.XAxis;
            Autodesk.DesignScript.Geometry.Vector planeY = hostPlane.YAxis;

            // Create vertical and horizontal mirror planes.
            Plane horizontalMirrorPlane = Plane.ByOriginNormal(hostPlane.Origin, planeX);
            Plane verticalMirrorPlane = Plane.ByOriginNormal(hostPlane.Origin, planeY);

            // Mirror the geometry horizontally.
            List<Autodesk.DesignScript.Geometry.Geometry> geometryHorizontalMirror = new List<Autodesk.DesignScript.Geometry.Geometry>();
            if (mirrorHorizontal == true) 
            {
                foreach (Autodesk.DesignScript.Geometry.Geometry geom in movedGeometry) 
                {
                    geometryHorizontalMirror.Add(geom.Mirror(horizontalMirrorPlane));
                }
            }
            else 
            {
                geometryHorizontalMirror = movedGeometry;
            }

            // Mirror the geometry vertically.
            List<Autodesk.DesignScript.Geometry.Geometry> geometryVerticalMirror = new List<Autodesk.DesignScript.Geometry.Geometry>();
            if (mirrorVertical == true)
            {
                foreach (Autodesk.DesignScript.Geometry.Geometry geom in geometryHorizontalMirror)
                {
                    geometryVerticalMirror.Add(geom.Mirror(verticalMirrorPlane));
                }
            }
            else 
            {
                geometryVerticalMirror = geometryHorizontalMirror;
            }

            return geometryVerticalMirror;
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
    }
}
