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

        Dictionary<(Vector3, Vector3, Vector3), GameObject> _shownTriangles = new();
        Dictionary<Vector3, GameObject> _shownVoxels = new();

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
                position.y = 0; //terrain.SampleHeight(position);

                _nodes.Add(position);
            }

            for (var x = 0f; x <= worldSize; x += 50)
            {
                for (var z = 0f; z <= worldSize; z += 50)
                {
                    var position = new Vector3(x, 0, z);
                    //position.y = terrain.SampleHeight(position);

                    _nodes.Add(position);
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

            Manager_Game.S_Instance.StartCoroutine(DelaunayTriangulation(sortedPoints, triangles, superNodeTriangle));
        }

        int _iteration;
        
        IEnumerator DelaunayTriangulation(HashSet<Vector3> pointsToProcess,
            Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles, Node_Triangle superNodeTriangle)
        {
            yield return _showTriangle(superNodeTriangle, 0.25f, 0);
            
            foreach (var point in pointsToProcess)
            {
                var badTriangles = new HashSet<Node_Triangle>();
                
                yield return _showVoxel(point + Vector3.up * 15, 0.5f, 2);

                foreach (var triangle in triangles.Values)
                {
                    yield return _hideTriangle(triangle, 0);
                    yield return _showTriangle(triangle, 0, 0);
                    
                    if (!triangle.IsPointInsideCircumcircle(point)) continue;
                    
                    yield return _showVoxel(point + Vector3.up * 15, 0.5f, 2, 1);
                    
                    badTriangles.Add(triangle);

                    yield return _hideTriangle(triangle, 0);
                    yield return _showTriangle(triangle, 0.25f, 1);

                    yield return new WaitForSeconds(0.5f);
                }

                var polygon = new HashSet<(Vector3, Vector3)>();

                foreach (var badTriangle in badTriangles)
                {
                    foreach (var edge in badTriangle.Edges)
                    {
                        if (badTriangles.Count == 1)
                        {
                            polygon.Add(edge);
                            continue;
                        }
                        
                        a
                            //* Shared triangles aren't working for everythigng. If we have an ABC, and ACD, someimes they don't make 
                            //* ABC and ACD with a shared border, sometimes they make ABC and ABD with no shared border in between.
                            //* Fix.

                        foreach (var otherBadTriangle in badTriangles)
                        {
                            if (otherBadTriangle.ID_Vectors == badTriangle.ID_Vectors) continue;

                            if (otherBadTriangle.Edges.Contains(edge)
                                || otherBadTriangle.Edges.Contains((edge.Item2, edge.Item1)))
                            {
                                if (badTriangle.Transform is not null)
                                {
                                    foreach(Transform child in badTriangle.Transform)
                                    {
                                        if (child.gameObject.name == $"Boundary Edge: {edge.Item1} - {edge.Item2}"
                                            || child.gameObject.name == $"Boundary Edge: {edge.Item2} - {edge.Item1}") 
                                            Object.Destroy(child.gameObject);
                                    }   
                                }

                                if (otherBadTriangle.Transform is not null)
                                {
                                    foreach(Transform child in otherBadTriangle.Transform)
                                    {
                                        if (child.gameObject.name == $"Boundary Edge: {edge.Item1} - {edge.Item2}"
                                            || child.gameObject.name == $"Boundary Edge: {edge.Item2} - {edge.Item1}") 
                                            Object.Destroy(child.gameObject);
                                    }   
                                }
                                
                                continue;
                            }

                            polygon.Add(edge);
                        }
                    }

                    triangles.Remove(badTriangle.ID_Vectors);
                    yield return _hideTriangle(badTriangle, 0);
                    yield return _showVoxel(point + Vector3.up * 15, 1, 2, 2);
                }

                foreach (var edge in polygon)
                {
                    if (ArePointsColinear(edge.Item1, edge.Item2, point))
                    {
                        yield return _showMessage("Points are colinear", point, 1);
                        
                        continue;
                    }
                    
                    yield return _showMessage($"New Triangle Point 1 {edge.Item1}", edge.Item1, 0.5f);
                    yield return _showMessage($"New Triangle Point 2 {edge.Item2}", edge.Item2, 0.5f);
                    yield return _showMessage($"New Triangle Point 3 {point}", point, 0.5f);

                    var triangle = new Node_Triangle(edge.Item1, edge.Item2, point);
                    
                    if (Vector3.Dot(Vector3.Cross(triangle.Vertices[1] - triangle.Vertices[0],
                            triangle.Vertices[2] - triangle.Vertices[0]).normalized, Vector3.up) < 0)
                    {
                        triangle = new Node_Triangle(edge.Item2, edge.Item1, point);
                    }
                    
                    yield return _hideTriangle(triangle, 0);
                    yield return _showTriangle(triangle, 0.25f, 2);

                    triangles.TryAdd(triangle.ID_Vectors, triangle);
                }
            }

            foreach (var triangle in triangles.Values.ToList())
            {
                foreach (var vertex in triangle.Vertices)
                {
                    if (!superNodeTriangle.Vertices.Contains(vertex)) continue;

                    triangles.Remove(triangle.ID_Vectors);

                    yield return _hideTriangle(triangle);

                    yield return new WaitForSeconds(0.1f);
                }
            }

            //Manager_Game.S_Instance.StartCoroutine(_performEdgeFlipping(triangles));

            _returnUpdatedTriangles(triangles);
        }

        public static bool ArePointsColinear(Vector3 a, Vector3 b, Vector3 c)
        {
            var area = (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
            return Mathf.Abs(area) < Mathf.Epsilon;
        }

        IEnumerator _flipEdge(Vector3 a, Vector3 b, Node_Triangle t1, Node_Triangle t2,
            Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles)
        {
            var c = t1.GetThirdVertex(a, b);
            var d = t2.GetThirdVertex(a, b);

            if (!Node_Triangle.IsPointInsideCircumcircle(c, a, b, d)) yield break;

            triangles.Remove(t1.ID_Vectors);
            triangles.Remove(t2.ID_Vectors);

            yield return _showMessage($"Flipping Edge: {a} - {b}", (a + b) * 0.5f, 1);

            yield return _hideTriangle(t1);
            yield return _showMessage($"Removing Triangle: {t1.ID_Vectors}", t1.Centroid, 1);
            yield return _hideTriangle(t2);
            yield return _showMessage($"Removing Triangle: {t2.ID_Vectors}", t2.Centroid, 1);


            var newTriangle1 = new Node_Triangle(c, d, a);
            var newTriangle2 = new Node_Triangle(c, d, b);

            yield return _showTriangle(newTriangle1);
            yield return _showMessage($"Adding Triangle: {newTriangle1.ID_Vectors}", newTriangle1.Centroid, 1);
            yield return _showTriangle(newTriangle2);
            yield return _showMessage($"Adding Triangle: {newTriangle2.ID_Vectors}", newTriangle2.Centroid, 1);

            triangles[newTriangle1.ID_Vectors] = newTriangle1;
            triangles[newTriangle2.ID_Vectors] = newTriangle2;
        }

        IEnumerator _showVoxel(Vector3 position, float duration = 1f, int sizeMultiplier = 1, int colourIndex = 0)
        {
            if (_shownVoxels.TryGetValue(position, out var shownVoxel)) Object.Destroy(shownVoxel);
            
            var voxel = Visualise_Voxel.Show_Voxel(position, Vector3.one * sizeMultiplier, colourIndex);
            voxel.name = $"Voxel: {position}";
            
            _shownVoxels[position] = voxel;
            
            yield return new WaitForSeconds(duration);
        }

        IEnumerator _showTriangle(Node_Triangle triangle, float duration = 1f, int colour = -1)
        {
            var triangleGO = Visualise_Triangle.Show_Triangle(new[]
                {
                    triangle.Vertices[0] + Vector3.up * 15,
                    triangle.Vertices[1] + Vector3.up * 15,
                    triangle.Vertices[2] + Vector3.up * 15
                },
                colour);

            Object.Destroy(triangleGO.GetComponent<MeshRenderer>());

            for (var i = 0; i < triangle.Vertices.Length; i++)
            {
                var start = triangle.Vertices[i];
                var end = triangle.Vertices[(i + 1) % triangle.Vertices.Length];

                yield return _showEdge(start, end, duration, colour, triangleGO.transform);
            }

            triangle.Transform = triangleGO.transform;
            _shownTriangles.TryAdd(triangle.ID_Vectors, triangleGO);
        }

            IEnumerator _hideTriangle(Node_Triangle triangle, float duration = 0.5f)
        {
            if (!_shownTriangles.TryGetValue(triangle.ID_Vectors, out var shownTriangle)) yield break;

            Object.Destroy(shownTriangle);
            _shownTriangles.Remove(triangle.ID_Vectors);
            triangle.Transform = null;

            yield return new WaitForSeconds(duration);
        }
            
        IEnumerator _hideAllTriangles(Node_Triangle superNodeTriangle)
        {
            foreach (var triangle in _shownTriangles.Values)
            {
                Object.Destroy(triangle);
            }

            _shownTriangles.Clear();
            
            yield return _showTriangle(superNodeTriangle, 0.25f, 0);

            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator _showEdge(Vector3 start, Vector3 end, float duration, int colour = -1,
            Transform parent = null)
        {
            var center = (start + end) * 0.5f + Vector3.up * 16;
            var size = new Vector3(0.25f, 0.25f, Vector3.Distance(start, end));

            var line = Visualise_Voxel.Show_Voxel(center, size, colour,
                rotation: Quaternion.LookRotation(end - start));

            if (parent is not null) line.transform.SetParent(parent.transform);
            line.name = $"Boundary Edge: {start} - {end}";

            yield return new WaitForSeconds(duration);
        }

        public IEnumerator _showMessage(string message, Vector3 position, float duration)
        {
            var messageGO = Visualise_Message.Show_Message(
                new Vector3(position.x, 12, position.z), message);

            yield return new WaitForSeconds(duration);

            Object.Destroy(messageGO);
        }

        IEnumerator _performEdgeFlipping(Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles)
        {
            var edgesToCheck = new HashSet<(Vector3, Vector3)>();

            foreach (var triangle in triangles.Values)
            {
                foreach (var edge in triangle.Edges)
                {
                    edgesToCheck.Add(edge);
                }
            }

            foreach (var edge in edgesToCheck)
            {
                var adjacentTriangles = triangles.Values
                    .Where(t => t.Edges.Contains(edge) || t.Edges.Contains((edge.Item2, edge.Item1)))
                    .ToList();

                if (adjacentTriangles.Count != 2) continue;

                yield return _flipEdge(edge.Item1, edge.Item2, adjacentTriangles[0], adjacentTriangles[1], triangles);
            }
        }


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