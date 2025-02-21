using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    public class Graph_NavMesh
    {
        const int _size = 10;
        const float _spacing = 1f;
        const int _heightLevels = 5;

        Dictionary<ulong, Node_3D> _points;
        Dictionary<ulong, Node_Triangle> _triangles;
        Dictionary<ulong, Node_Triangle> Triangles => _triangles ??= _initialiseTriangles();

        Dictionary<ulong, Node_Triangle> _initialiseTriangles()
        {
            _points = new Dictionary<ulong, Node_3D>();

            for (var x = 0; x < _size; x++)
            {
                for (var y = 0; y < _heightLevels; y++)
                {
                    for (var z = 0; z < _size; z++)
                    {
                        var position = new Vector3(x * _spacing, y * _spacing, z * _spacing);

                        var node = new Node_3D(position);
                        _points.Add(node.ID, node);
                    }
                }
            }

            return _performTriangulationUpdate(_points);
        }

        const float _padding = 1000f;

        static Node_Triangle _createSuperTriangle(Dictionary<ulong, Node_3D> points)
        {
            var minX = points.Min(p => p.Value.Position.x);
            var maxX = points.Max(p => p.Value.Position.x);
            var minY = points.Min(p => p.Value.Position.y);
            var maxY = points.Max(p => p.Value.Position.y);

            var p1 = new Node_3D(new Vector3(minX - _padding, minY - _padding, 0));
            var p2 = new Node_3D(new Vector3(maxX + _padding, minY - _padding, 0));
            var p3 = new Node_3D(new Vector3((minX + maxX) / 2f, maxY + _padding, 0));

            return new Node_Triangle(p1, p2, p3);
        }

        public void AddPoint(Vector3 position)
        {
            var newNode = new Node_3D(position);
            _points.Add(newNode.ID, newNode);

            // Apply Bowyer-Watson to insert the new point into the existing triangulation
            _performTriangulationUpdate(_points, newNode);
        }

        a
            //* Make sure triangles are being set up properly
        
        // This function handles both initial triangulation (Sweep Line) and incremental point insertion (Bowyer-Watson)
        Dictionary<ulong, Node_Triangle> _performTriangulationUpdate(Dictionary<ulong, Node_3D> points,
            Node_3D newNode = null)
        {
            var sortedPoints = points.OrderBy(p => p.Value.Position.x)
                .ToDictionary(p => p.Key, p => p.Value);

            // Step 1: Initialize super-triangle (only if we are doing initial triangulation)
            Node_Triangle superNodeTriangle = null;
            var triangles = new Dictionary<ulong, Node_Triangle>();

            if (newNode == null) // If it's the initial triangulation (Sweep Line)
            {
                superNodeTriangle = _createSuperTriangle(points);
                triangles.Add(superNodeTriangle.ID, superNodeTriangle);
            }
            
            Debug.Log($"Super triangle: {superNodeTriangle?.GetVertices().Select(v => v.Position)}");

            // Step 2: Process each point (either the initial ones or the new node in Bowyer-Watson)
            var pointsToProcess = newNode == null
                ? sortedPoints
                : new Dictionary<ulong, Node_3D> { { newNode.ID, newNode } };

            foreach (var point in pointsToProcess)
            {
                var badTriangles = new List<Node_Triangle>();

                foreach (var triangle in triangles.Values)
                {
                    if (triangle.IsPointInsideCircumcircle(point.Value.Position))
                    {
                        badTriangles.Add(triangle);
                    }
                }

                foreach (var badTriangle in badTriangles)
                {
                    triangles.Remove(badTriangle.ID);
                }

                var boundaryEdges = new HashSet<Node_Edge>();

                foreach (var badTriangle in badTriangles)
                {
                    var vertices = badTriangle.GetVertices();

                    for (var i = 0; i < vertices.Count; i++)
                    {
                        var edge = new Node_Edge(vertices[i], vertices[(i + 1) % 3]);
                        boundaryEdges.Add(edge);
                    }
                }

                foreach (var edge in boundaryEdges)
                {
                    var newTriangle = new Node_Triangle(edge.Start, edge.End, point.Value);
                    triangles.Add(newTriangle.ID, newTriangle);
                }
            }

            if (superNodeTriangle != null)
            {
                foreach (var triangle in triangles.Values.ToList())
                {
                    if (triangle.GetVertices().Any(v => superNodeTriangle.GetVertices().Contains(v)))
                    {
                        triangles.Remove(triangle.ID);
                    }
                }
            }

            if (newNode != null)
            {
                // Edge flipping logic could be implemented here to maintain the Delaunay property
            }

            // Update the triangulation list (if needed, based on whether you're doing an incremental update)

            return newNode == null
                ? triangles
                : Triangles.Concat(triangles).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        bool initialsed = false;

        public List<Vector3> FindShortestPath(Vector3 start, Vector3 end)
        {
            Debug.Log("Using NavMesh pathfinding");

            

            var startTriangle = _getNearestTriangle(start);
            var endTriangle = _getNearestTriangle(end);
            
            if (!initialsed && startTriangle != null && endTriangle != null) _createVisible();

            Debug.Log($"Start: {startTriangle?.Centroid}, End: {endTriangle?.Centroid}");

            return startTriangle != endTriangle
                ? AStar_Triangle.RunAStar(startTriangle, endTriangle, Triangles)
                : new List<Vector3> { end };
        }

        void _createVisible()
        {
            foreach (var triangle in Triangles)
            {
                _showVisibleTriangles(triangle.Value.GetVertices());
            }
            
            initialsed = true;
        }

        static void _showVisibleTriangles(List<Node_3D> vertices)
        {
            var visibleVoxelsParent = GameObject.Find("VisibleVoxels").transform;
            var green = Resources.Load<Material>("Materials/Material_Green");
            var red = Resources.Load<Material>("Materials/Material_Red");
            var blue = Resources.Load<Material>("Materials/Material_Blue");

            var materials = new List<Material> { green, red, blue };

            // Here, vertices are already the triangle's 3 points
            _showTriangle(visibleVoxelsParent, vertices, materials, 0);
        }

        static void _showTriangle(Transform parentTransform, List<Node_3D> vertices, List<Material> materials,
            int colorIndex)
        {
            if (vertices.Count != 3)
            {
                Debug.LogError("A triangle must have exactly 3 vertices.");
                return;
            }

            var material = materials[colorIndex % materials.Count];

            // Here, we directly use the vertices passed as the triangle's corners
            var mesh = new Mesh();

            // Extract the positions of the vertices
            Vector3[] triangleVertices = new Vector3[3];
            triangleVertices[0] = vertices[0].Position;
            triangleVertices[1] = vertices[1].Position;
            triangleVertices[2] = vertices[2].Position;

            // Define the triangles (indices of the vertices to form one triangle)
            int[] triangles = new int[3] { 0, 1, 2 };

            // Set up the mesh with vertices and triangles
            mesh.vertices = triangleVertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals(); // Recalculate normals for proper lighting

            // Instantiate the triangle GameObject and apply the mesh and material
            var triangleGO = _testShowTriangle(parentTransform, triangleVertices[0], mesh, material);
            triangleGO.name = $"Triangle: {triangleVertices[0]}";
        }

        static GameObject _testShowTriangle(Transform transform, Vector3 position, Mesh mesh, Material material)
        {
            var triangleGO = new GameObject($"{position}");
            triangleGO.AddComponent<MeshFilter>().mesh = mesh;
            triangleGO.AddComponent<MeshRenderer>().material = material;
            triangleGO.transform.SetParent(transform);
            triangleGO.transform.localPosition = position;
            triangleGO.transform.localScale = Vector3.one;
            return triangleGO;
        }


        Node_Triangle _getNearestTriangle(Vector3 point)
        {
            return Triangles.Values
                .OrderBy(triangle => Vector3.Distance(triangle.Centroid, point))
                .FirstOrDefault();
        }
    }
}