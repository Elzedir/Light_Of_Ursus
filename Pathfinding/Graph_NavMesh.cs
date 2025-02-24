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
        HashSet<Vector3> _nodes;
        Dictionary<ulong, Node_Triangle> _triangles;
        Dictionary<ulong, Node_Triangle> Triangles => _triangles ??= _initialiseTriangles();

        Dictionary<(Vector3, Vector3, Vector3), GameObject> ShownTriangles = new();

        Dictionary<ulong, Node_Triangle> _initialiseTriangles()
        {
            var terrain = Terrain.activeTerrain;
            if (terrain is null) throw new Exception("No terrain found.");

            var terrainSize = terrain.terrainData.size;
            var worldSize = Mathf.Max(terrainSize.x, terrainSize.z);
            _nodes = new HashSet<Vector3>();

            var navRelevantObjects = GameObject.FindGameObjectsWithTag("NavRelevant");

            foreach (var navMeshObject in navRelevantObjects)
            {
                var position = navMeshObject.transform.position;
                position.y = terrain.SampleHeight(position);

                _nodes.Add(position);

                var voxel = Visualise_Voxel.Show_Voxel(position, Vector3.one * 0.1f);
                voxel.name = $"Object Node: {position}";
            }

            for (var x = 0f; x <= worldSize; x += 50)
            {
                for (var z = 0f; z <= worldSize; z += 50)
                {
                    var position = new Vector3(x, 0, z);
                    position.y = terrain.SampleHeight(position);

                    _nodes.Add(position);

                    var voxel = Visualise_Voxel.Show_Voxel(position, Vector3.one * 0.1f);
                    voxel.name = $"Grid Node: {position}";
                }
            }

            _performTriangulationUpdate(_nodes);

            return new Dictionary<ulong, Node_Triangle>();
        }

        static Node_Triangle _createSuperTriangle(HashSet<Vector3> points)
        {
            var maxX = points.Max(p => p.x);
            var maxZ = points.Max(p => p.z);

            var p1 = new Vector3(-1, 0, -1);
            var p2 = new Vector3(-1, 0, maxZ * 2);
            var p3 = new Vector3(maxX * 2, 0, -1);

            return new Node_Triangle(p1, p2, p3);
        }

        public void _performTriangulationUpdate(HashSet<Vector3> points)
        {
            var sortedPoints = points.OrderBy(p => p.x).ToHashSet();
            var triangles = new Dictionary<(Vector3, Vector3, Vector3), Node_Triangle>();

            var superNodeTriangle = _createSuperTriangle(points);
            triangles.Add(superNodeTriangle.ID_Vectors, superNodeTriangle);

            var superTriangle = Visualise_Triangle.Show_Triangle(new[]
            {
                superNodeTriangle.Vertices[0] + Vector3.down,
                superNodeTriangle.Vertices[1] + Vector3.down,
                superNodeTriangle.Vertices[2] + Vector3.down
            });

            ShownTriangles.Add(superNodeTriangle.ID_Vectors, superTriangle);

            Manager_Game.S_Instance.StartCoroutine(Triangulate(sortedPoints, triangles, superNodeTriangle));
            //Manager_Game.S_Instance.StartCoroutine(DelaunayTriangulation(sortedPoints, triangles, superNodeTriangle));
        }
        
        a
            //* Just... Figure out delauney shared edges and flipping. Make it more efficient then work on nodes.

        public IEnumerator Triangulate(HashSet<Vector3> points,
            Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles, Node_Triangle superTriangle)
        {
            yield return null;
            foreach (var point in points)
            {
                var badTriangles = new List<Node_Triangle>();

                foreach (var triangle in triangles.Values)
                {
                    if (triangle.IsPointInsideCircumcircle(point))
                    {
                        badTriangles.Add(triangle);
                    }
                }

                foreach (var badTriangle in badTriangles)
                {
                    triangles.Remove(badTriangle.ID_Vectors);
                }

                var newTriangles = new List<Node_Triangle>();

                foreach (var badTriangle in badTriangles)
                {
                    foreach (var edge in badTriangle.Edges)
                    {
                        var newTriangle = new Node_Triangle(edge.Item1, edge.Item2, point);
                        if (triangles.TryAdd(newTriangle.ID_Vectors, newTriangle))
                        {
                            newTriangles.Add(newTriangle);   
                        }
                    }
                }

                var finalTriangles = new List<Node_Triangle>();
                foreach (var triangle in newTriangles)
                {
                    var isBoundary = false;

                    foreach (var otherTriangle in newTriangles)
                    {
                        if (triangle != otherTriangle && triangle.GetSharedEdges(otherTriangle).Count > 0)
                        {
                            isBoundary = true;
                            break;
                        }
                    }

                    if (!isBoundary)
                    {
                        finalTriangles.Add(triangle);
                    }
                }

                foreach (var finalTriangle in finalTriangles)
                {
                    triangles.TryAdd(finalTriangle.ID_Vectors, finalTriangle);
                }
            }

            foreach (var triangle in triangles.Values.ToList())
            {
                foreach (var vertex in triangle.Vertices)
                {
                    if (!superTriangle.Vertices.Contains(vertex)) continue;

                    triangles.Remove(triangle.ID_Vectors);
                }
            }

            _returnUpdatedTriangles(triangles);

            foreach (var triangle in triangles.Values)
            {
                if (ShownTriangles.TryGetValue(triangle.ID_Vectors, out var shownTriangle))
                {
                    Object.Destroy(shownTriangle);
                    ShownTriangles.Remove(triangle.ID_Vectors);
                }

                var triangleGO = Visualise_Triangle.Show_Triangle(new[]
                    { triangle.Vertices[0], triangle.Vertices[1], triangle.Vertices[2] });

                for (var i = 0; i < triangle.Vertices.Length; i++)
                {
                    var start = triangle.Vertices[i];
                    var end = triangle.Vertices[(i + 1) % triangle.Vertices.Length];

                    var center = (start + end) * 0.5f;
                    var size = new Vector3(0.1f, 0.1f, Vector3.Distance(start, end));

                    var line = Visualise_Voxel.Show_Voxel(center + Vector3.up, size, 2,
                        rotation: Quaternion.LookRotation(end - start));
                    line.transform.SetParent(triangleGO.transform);
                    line.name = $"Boundary Edge: {start} - {end}";
                }

                ShownTriangles.Add(triangle.ID_Vectors, triangleGO);
            }
        }

        // IEnumerator DelaunayTriangulation(HashSet<Vector3> pointsToProcess,
        //     Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles, Node_Triangle superNodeTriangle)
        // {
        //     yield return null;
        //
        //     foreach (var point in pointsToProcess)
        //     {
        //         var badTriangles = new HashSet<Node_Triangle>();
        //         var polygon = new HashSet<(Vector3, Vector3)>();
        //
        //         foreach (var triangle in triangles.Values)
        //         {
        //             if (!triangle.IsPointInsideCircumcircle(point)) continue;
        //             
        //             badTriangles.Add(triangle);
        //         }
        //
        //         foreach (var badTriangle in badTriangles)
        //         {
        //             foreach(var edge in badTriangle.Edges)
        //             {
        //                 if (polygon.Contains(edge) || polygon.Contains((edge.Item2, edge.Item1))) continue;
        //                 
        //                 polygon.Add(edge);
        //             }
        //             
        //             triangles.Remove(badTriangle.ID_Vectors);
        //         }
        //
        //         foreach (var edge in polygon)
        //         {
        //             var triangle = new Node_Triangle(edge.Item1, edge.Item2, point);
        //
        //             triangles.Add(triangle.ID_Vectors, triangle);
        //         }
        //     }
        //     
        //     foreach (var triangle in triangles.Values.ToList())
        //     {
        //         foreach (var vertex in triangle.Vertices)
        //         {
        //             if (!superNodeTriangle.Vertices.Contains(vertex)) continue;
        //
        //             triangles.Remove(triangle.ID_Vectors);
        //         }
        //     }
        //
        //     _returnUpdatedTriangles(triangles);
        //     
        //     foreach (var triangle in triangles.Values)
        //     {
        //         if (ShownTriangles.TryGetValue(triangle.ID_Vectors, out var shownTriangle))
        //         {
        //             Object.Destroy(shownTriangle);
        //             ShownTriangles.Remove(triangle.ID_Vectors);
        //         }
        //         
        //         var triangleGO = Visualise_Triangle.Show_Triangle(new[]
        //             { triangle.Vertices[0], triangle.Vertices[1], triangle.Vertices[2] });
        //
        //         for (var i = 0; i < triangle.Vertices.Length; i++)
        //         {
        //             var start = triangle.Vertices[i];
        //             var end = triangle.Vertices[(i + 1) % triangle.Vertices.Length];
        //
        //             var center = (start + end) * 0.5f;
        //             var size = new Vector3(0.1f, 0.1f, Vector3.Distance(start, end));
        //
        //             var line = Visualise_Voxel.Show_Voxel(center + Vector3.up, size, 2,
        //                 rotation: Quaternion.LookRotation(end - start));
        //             line.transform.SetParent(triangleGO.transform);
        //             line.name = $"Boundary Edge: {start} - {end}";
        //         }
        //
        //         ShownTriangles.Add(triangle.ID_Vectors, triangleGO);
        //     }
        //}

        void _returnUpdatedTriangles(Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> updatedTriangles)
        {
            _triangles = updatedTriangles.ToDictionary(
                triangle => triangle.Value.ID,
                triangle => triangle.Value);
        }

        public void AddPoint(Vector3 position)
        {
            _nodes.Add(position);

            //* Will need to add edge flipping for recalculation of triangles.
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