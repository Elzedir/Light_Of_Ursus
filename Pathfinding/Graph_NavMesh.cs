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
                position.y = 0;
                //position.y = terrain.SampleHeight(position);

                _nodes.Add(position);

                // var voxel = Visualise_Voxel.Show_Voxel(position + Vector3.up * 15, Vector3.one * 2);
                // voxel.name = $"Object Node: {position}";
            }

            for (var x = 0f; x <= worldSize; x += 50)
            {
                for (var z = 0f; z <= worldSize; z += 50)
                {
                    var position = new Vector3(x, 0, z);
                    //position.y = terrain.SampleHeight(position); Change back soon.

                    _nodes.Add(position);

                    // var voxel = Visualise_Voxel.Show_Voxel(position + Vector3.up * 15, Vector3.one * 2);
                    // voxel.name = $"Grid Node: {position}";
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
        
         IEnumerator DelaunayTriangulation(HashSet<Vector3> pointsToProcess,
             Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles, Node_Triangle superNodeTriangle)
         {
             foreach (var point in pointsToProcess)
             {
                 var voxel = Visualise_Voxel.Show_Voxel(point + Vector3.up * 15, Vector3.one * 2,0);
                 voxel.name = $"Grid Node: {point}";
                 
                 var badTriangles = new HashSet<Node_Triangle>();
        
                 foreach (var triangle in triangles.Values)
                 {
                     yield return ShowTriangle(triangle);
                     
                     if (!triangle.IsPointInsideCircumcircle(point)) continue;
                     
                     badTriangles.Add(triangle);
                     
                     yield return HideTriangle(triangle);
                     yield return ShowTriangle(triangle, 1);
                 }
                 
                 var polygon = new HashSet<(Vector3, Vector3)>();
        
                 foreach (var badTriangle in badTriangles)
                 {
                     foreach(var edge in badTriangle.Edges)
                     {
                         if (badTriangles.Count == 1)
                         {
                             Visualise_Voxel.Show_Voxel(edge.Item1 + Vector3.up * 15,
                                 new Vector3(0.1f, 0.1f, Vector3.Distance(edge.Item1, edge.Item2), 2);
                             polygon.Add(edge);
                             continue;
                         }
                         
                         foreach (var otherBadTriangle in badTriangles)
                         {
                             if (otherBadTriangle.ID_Vectors == badTriangle.ID_Vectors) continue;
                         
                             if (otherBadTriangle.Edges.Contains(edge) 
                                 || otherBadTriangle.Edges.Contains((edge.Item2, edge.Item1))) continue;
                         
                             polygon.Add(edge);
                         }
                     }
                     
                     triangles.Remove(badTriangle.ID_Vectors);
                 }
        
                 foreach (var edge in polygon)
                 {
                     if (ArePointsColinear(edge.Item1, edge.Item2, point))
                     {
                         continue;
                     }
                     
                     var triangle = new Node_Triangle(edge.Item1, edge.Item2, point);
                     
                     if (Vector3.Dot(Vector3.Cross(triangle.Vertices[1] - triangle.Vertices[0], 
                             triangle.Vertices[2] - triangle.Vertices[0]).normalized, Vector3.up) < 0)
                     {
                         triangle = new Node_Triangle(edge.Item2, edge.Item1, point);
                     }

                     triangles.TryAdd(triangle.ID_Vectors, triangle);
                 }
             }
             
             foreach (var triangle in triangles.Values.ToList())
             {
                 yield return ShowTriangle(triangle);
                 
                 yield return new WaitForSeconds(0.1f);
                    
                 foreach (var vertex in triangle.Vertices)
                 {
                     if (!superNodeTriangle.Vertices.Contains(vertex)) continue;
        
                     triangles.Remove(triangle.ID_Vectors);
                     
                     HideTriangle(triangle);
                     
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
             
             yield return ShowMessage($"Flipping Edge: {a} - {b}", (a + b) * 0.5f, 1);
             
             HideTriangle(t1);
             yield return ShowMessage($"Removing Triangle: {t1.ID_Vectors}", t1.Centroid, 1);
             HideTriangle(t2);
             yield return ShowMessage($"Removing Triangle: {t2.ID_Vectors}", t2.Centroid, 1);
             

             var newTriangle1 = new Node_Triangle(c, d, a);
             var newTriangle2 = new Node_Triangle(c, d, b);

             yield return ShowTriangle(newTriangle1);
             yield return ShowMessage($"Adding Triangle: {newTriangle1.ID_Vectors}", newTriangle1.Centroid, 1);
             yield return ShowTriangle(newTriangle2);
             yield return ShowMessage($"Adding Triangle: {newTriangle2.ID_Vectors}", newTriangle2.Centroid, 1);

             triangles[newTriangle1.ID_Vectors] = newTriangle1;
             triangles[newTriangle2.ID_Vectors] = newTriangle2;
         }

         public IEnumerator ShowTriangle(Node_Triangle triangle, int colour = -1)
         {
             var triangleGO = Visualise_Triangle.Show_Triangle(new[]
             {
                 triangle.Vertices[0] + Vector3.up * 15,
                 triangle.Vertices[1] + Vector3.up * 15,
                 triangle.Vertices[2] + Vector3.up * 15
             }, 
                 (int)colour);
        
             for (var i = 0; i < triangle.Vertices.Length; i++)
             {
                 var start = triangle.Vertices[i];
                 var end = triangle.Vertices[(i + 1) % triangle.Vertices.Length];
        
                 var center = (start + end) * 0.5f;
                 var size = new Vector3(0.1f, 0.1f, Vector3.Distance(start, end));
        
                 var line = Visualise_Voxel.Show_Voxel(center + Vector3.up * (15), size, (int)colour + 1,
                     rotation: Quaternion.LookRotation(end - start));
                 line.transform.SetParent(triangleGO.transform);
                 line.name = $"Boundary Edge: {start} - {end}";

                 yield return new WaitForSeconds(1);
             }
        
             ShownTriangles.TryAdd(triangle.ID_Vectors, triangleGO);
         }
         
         a
             //* Use ShowEdge to show how the edges are formed and which are checked and in which order to see why we are
             //* creating edges that extend across other edges.

         public IEnumerator HideTriangle(Node_Triangle triangle)
         {
             if (!ShownTriangles.TryGetValue(triangle.ID_Vectors, out var shownTriangle)) yield break;
             
             Object.Destroy(shownTriangle);
             ShownTriangles.Remove(triangle.ID_Vectors);

             yield return new WaitForSeconds(0.5f);
         }

         public IEnumerator ShowEdge((Vector3, Vector3) edge)
         {
             
         }
         
         public IEnumerator ShowMessage(string message, Vector3 position, float duration)
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