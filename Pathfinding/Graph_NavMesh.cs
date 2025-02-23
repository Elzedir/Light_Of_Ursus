using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pathfinding
{
    public class Graph_NavMesh
    {
        Dictionary<ulong, Node_3D> _nodes;
        Dictionary<ulong, Node_Triangle> _triangles;
        Dictionary<ulong, Node_Triangle> Triangles => _triangles ??= _initialiseTrianglesGrid();

        Dictionary<ulong, Node_Triangle> _initialiseTrianglesGrid()
        {
            var terrain = Terrain.activeTerrain;
            if (terrain is null) throw new Exception("No terrain found.");

            var terrainSize = terrain.terrainData.size;
            var worldSize = Mathf.Max(terrainSize.x, terrainSize.z);
            _nodes = new Dictionary<ulong, Node_3D>();

            for (var x = 0f; x <= worldSize; x++)
            {
                for (var z = 0f; z <= worldSize; z++)
                {
                    var position = new Vector3(x, 0, z);
                    position.y = terrain.SampleHeight(position);

                    var node = new Node_3D(position);
                    _nodes.Add(node.ID, node);
                    var voxel = Visualise_Voxel.Show_Voxel(position, Vector3.one * 0.1f);
                    voxel.name = $"Grid Node: {position}";
                }
            }

            _performTriangulationUpdate(_nodes);

            return new Dictionary<ulong, Node_Triangle>();
        }

        Dictionary<ulong, Node_Triangle> _initialiseTrianglesObjects()
        {
            var nodes = new Dictionary<ulong, Node_3D>();

            var terrain = Terrain.activeTerrain;

            // Get all relevant objects (e.g., buildings, roads, trees)
            var navRelevantObjects = GameObject.FindGameObjectsWithTag("NavRelevant");

            foreach (var obj in navRelevantObjects)
            {
                var position = obj.transform.position;
                position.y = terrain.SampleHeight(position);

                var node = new Node_3D(position);
                nodes.Add(node.ID, node);
                //_showVoxel(position, Vector3.one);
            }

            // Optionally add extra nodes to connect sparse regions
            //_connectSparseRegions(nodes);

            _performTriangulationUpdate(nodes);

            return new Dictionary<ulong, Node_Triangle>();
        }

        static Node_Triangle _createSuperTriangle(Dictionary<ulong, Node_3D> points)
        {
            var minX = points.Min(p => p.Value.Position.x);
            var maxX = points.Max(p => p.Value.Position.x);
            var minZ = points.Min(p => p.Value.Position.z);
            var maxZ = points.Max(p => p.Value.Position.z);

            var padding = (maxX - minX) * 0.2f;

            var p1 = new Vector3(minX - padding, 0, minZ - padding);
            var p2 = new Vector3((minX + maxX) / 2f, 0, maxZ + padding);
            var p3 = new Vector3(maxX + padding, 0, minZ - padding);

            return new Node_Triangle(p1, p2, p3);
        }

        public void AddPoint(Vector3 position)
        {
            var newNode = new Node_3D(position);
            _nodes.Add(newNode.ID, newNode);

            // Apply Bowyer-Watson to insert the new point into the existing triangulation
            _performTriangulationUpdate(_nodes, newNode);
        }
        
        public void _performTriangulationUpdate(Dictionary<ulong, Node_3D> points, Node_3D newNode = null)
        {
            var sortedPoints = points
                .OrderBy(p => p.Value.Position.x)
                .ToDictionary(p => p.Key, p => p.Value);

            Node_Triangle superNodeTriangle = null;
            var triangles = new Dictionary<ulong, Node_Triangle>();

            if (newNode == null)
            {
                superNodeTriangle = _createSuperTriangle(points);
                triangles.Add(superNodeTriangle.ID, superNodeTriangle);

                var superTriangle = Visualise_Triangle.Show_Triangle(new[]
                {
                    superNodeTriangle.Vertices[0] + Vector3.down, 
                    superNodeTriangle.Vertices[1] + Vector3.down,
                    superNodeTriangle.Vertices[2] + Vector3.down
                });

                foreach (var vertex in superNodeTriangle.Vertices)
                {
                    var cornerVoxel = Visualise_Voxel.Show_Voxel(vertex, Vector3.one / 10);
                    cornerVoxel.name = $"SuperTriangle Corner: {vertex}";
                }
            }

            var pointsToProcess = newNode == null
                ? sortedPoints
                : new Dictionary<ulong, Node_3D> { { newNode.ID, newNode } };

            Manager_Game.S_Instance.StartCoroutine(PopulateCoroutine(pointsToProcess, triangles, superNodeTriangle,
                newNode));
        }

        IEnumerator PopulateCoroutine(Dictionary<ulong, Node_3D> pointsToProcess,
            Dictionary<ulong, Node_Triangle> triangles,
            Node_Triangle superNodeTriangle, Node_3D newNode)
        {
            var badTriangles = new List<Node_Triangle>();
            
            foreach (var point in pointsToProcess)
            {
                foreach (var triangle in triangles.Values)
                {
                    var isInsideCircumcircle = triangle.IsInsideCircumcircle(point.Value.Position);
                    
                    if (!isInsideCircumcircle) continue;
                
                    badTriangles.Add(triangle);
                }

                foreach (var badTriangle in badTriangles)
                {
                    triangles.Remove(badTriangle.ID);
                }

                var boundaryEdges = new HashSet<Node_Edge>();
                
                foreach (var badTriangle in badTriangles)
                {
                    var vertices = badTriangle.Vertices;

                    for (var i = 0; i < vertices.Length; i++)
                    {
                        var start = vertices[i];
                        var end = vertices[(i + 1) % 3];
                        
                        var message1 = Visualise_Message.Show_Message(
                            start + Vector3.up * 2,
                            $"Removing boundary edge: {start}");
                        
                        var message2 = Visualise_Message.Show_Message(
                            end + Vector3.up * 2,
                            $"Removing boundary edge: {end}");
                
                        yield return new WaitForSeconds(2f);
                
                        Object.Destroy(message1);
                        Object.Destroy(message2);

                        var edge = new Node_Edge(start, end);
                        boundaryEdges.Add(edge);
                        
                        var message3 = Visualise_Message.Show_Message(
                            (start + end) * 0.5f + Vector3.up * 2,
                            $"Adding boundary edge: {start} - {end} at midpoint {(start + end) * 0.5f}");
                
                        yield return new WaitForSeconds(2f);
                
                        Object.Destroy(message3);
                        
                        var center = (start + end) * 0.5f;
                        var length = Vector3.Distance(start, end);
                        
                        var size = new Vector3(0.1f, 0.1f, length);

                        var line = Visualise_Voxel.Show_Voxel(center + Vector3.up, size, 2,
                            rotation: Quaternion.LookRotation(end - start));
                        line.name = $"Boundary Edge: {start} - {end}";

                        yield return new WaitForSeconds(2f);

                        Object.Destroy(line);
                    }
                }

                // Add new triangles based on boundary edges
                foreach (var edge in boundaryEdges)
                {
                    var newTriangle = new Node_Triangle(edge.Start, edge.End, point.Value.Position);
                    triangles.Add(newTriangle.ID, newTriangle);
                }
            }

            // Remove super-triangle-related triangles
            if (superNodeTriangle != null)
            {
                foreach (var triangle in triangles.Values.ToList())
                {
                    if (triangle.Vertices.Any(v => superNodeTriangle.Vertices.Contains(v)))
                    {
                        triangles.Remove(triangle.ID);
                    }
                }
            }

            // After processing all points, handle the edge flipping logic if needed
            if (newNode != null)
            {
                // Implement edge flipping logic to maintain the Delaunay property if necessary
            }

            // Return updated triangles dictionary
            _returnUpdatedTriangles(triangles);
        }

        void _returnUpdatedTriangles(Dictionary<ulong, Node_Triangle> updatedTriangles)
        {
            _triangles = updatedTriangles;
        }


        public List<Vector3> FindShortestPath(Vector3 start, Vector3 end)
        {
            var startTriangle = _getNearestTriangle(start);
            var endTriangle = _getNearestTriangle(end);

            foreach (var triangle in Triangles.Values)
            {
                Visualise_Triangle.Show_Triangle(triangle.Vertices);
            }

            return startTriangle != endTriangle
                ? AStar_Triangle.RunAStar(startTriangle, endTriangle, Triangles)
                : new List<Vector3> { end };
        }

        Node_Triangle _getNearestTriangle(Vector3 point)
        {
            return Triangles.Values
                .OrderBy(triangle => Vector3.Distance(triangle.Centroid, point))
                .FirstOrDefault();
        }
    }
}