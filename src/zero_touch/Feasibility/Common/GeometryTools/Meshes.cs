using MIConvexHull;
using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;

namespace Common.GeometryTools
{
    /// <summary>
    /// Defines a vertex class for internal use.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)] // Ensures this class is hidden in Dynamo
    internal class MeshVertex : IVertex
    {
        /// <summary>
        /// The XYZ postion of the vertex.
        /// </summary>
        public double[] Position { get; set; }

        internal MeshVertex(double x, double y, double z)
        {
            Position = new double[] { x, y, z };
        }
    }

    /// <summary>
    /// Defines a mesh face class for internal use.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)] // Ensures this class is hidden in Dynamo
    internal class MeshFace : TriangulationCell<MeshVertex, MeshFace>
    {
    }

    /// <summary>
    /// Provides mesh generation functionality for the project.
    /// </summary>
    internal class Mesh
    {
        // Hides the overall class as a node.
        private Mesh() { }

        /// <summary>
        /// Create a mesh from a list of XYZ points.
        /// This method will be visible in Dynamo as a node.
        /// </summary>
        [IsVisibleInDynamoLibrary(true)] // Ensures this method is visible in Dynamo
        internal static List<MeshFace> CreateMeshFacesFromPoints(List<double[]> pointsXYZ)
        {
            // Convert the XYZ points to MeshVertex
            List<MeshVertex> vertices = new List<MeshVertex>();
            foreach (var point in pointsXYZ)
            {
                vertices.Add(new MeshVertex(point[0], point[1], point[2]));
            }

            // Perform Delaunay triangulation
            var delaunayTriangulation = DelaunayTriangulation<MeshVertex, MeshFace>.Create(vertices, 0.0001);

            // Get the mesh faces of the triangulation. 
            List<MeshFace> faces = new List<MeshFace>(delaunayTriangulation.Cells);

            // Return the faces of the mesh
            return faces;
        }

        internal static List<List<Autodesk.DesignScript.Geometry.Point>> MeshFacePoints(List<double[]> pointsXYZ) 
        {
            // Get the mesh faces.
            List<MeshFace> faces = CreateMeshFacesFromPoints(pointsXYZ);

            List <List<Autodesk.DesignScript.Geometry.Point>> facePointLists = new List<List<Autodesk.DesignScript.Geometry.Point>>();
            // Get the vertices of the mesh faces.
            foreach (var face in faces) 
            {
                // Get the face vertices.
                var vertices = face.Vertices;

                // Get the face points.
                List<Autodesk.DesignScript.Geometry.Point> facePoints = new List<Autodesk.DesignScript.Geometry.Point>();
                foreach(var vertex in vertices) 
                { 
                    double[] position = vertex.Position;

                    // Create a points from the vertex values.
                    Autodesk.DesignScript.Geometry.Point point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(position[0], position[1], position[2]);
                    facePoints.Add(point);
                }
                facePointLists.Add(facePoints);
            }

            return facePointLists;
        }
    }
}
