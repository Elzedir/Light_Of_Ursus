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

            var p1 = new Vector3(-10, 0, -10);
            var p2 = new Vector3(-10, 0, maxZ * 2);
            var p3 = new Vector3(maxX * 2, 0, -10);

            return new Node_Triangle(p1, p2, p3);
        }

        public void _performTriangulationUpdate(HashSet<Vector3> points)
        {
            var sortedPoints = points.OrderBy(p => p.x).ThenBy(p => p.z).ToHashSet();
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

                var polygon = new HashSet<Edge>();

                HashSet<Edge> completedEdges = new();

                foreach (var badTriangle in badTriangles)
                {
                    foreach (var edge in badTriangle.Edges)
                    {
                        if (badTriangles.Count == 1)
                        {
                            polygon.Add(edge);
                            continue;
                        }

                        foreach (var otherBadTriangle in badTriangles)
                        {
                            if (otherBadTriangle.ID_Vectors == badTriangle.ID_Vectors) continue;

                            if (otherBadTriangle.Edges.Contains(edge)
                                || otherBadTriangle.Edges.Contains(new Edge(edge.Vertex_2, edge.Vertex_1)))
                            {
                                if (!completedEdges.Contains(edge))
                                {
                                    // yield return _showTriangle(badTriangle, 0.25f, 1, true);
                                    // yield return _showTriangle(otherBadTriangle, 0.25f, 2, true);
                                    // yield return _showTriangle(badTriangle, 0.25f, 1);
                                    // yield return _showTriangle(otherBadTriangle, 0.25f, 1);   

                                    if (badTriangle.Transform is not null)
                                    {
                                        foreach (Transform child in badTriangle.Transform)
                                        {
                                            if (child.gameObject.name == $"Boundary Edge: {edge.Vertex_1.Position} - {edge.Vertex_2.Position}"
                                                || child.gameObject.name ==
                                                $"Boundary Edge: {edge.Vertex_2.Position} - {edge.Vertex_1.Position}")
                                            {
                                                Object.Destroy(child.gameObject);
                                            }
                                        }
                                    }

                                    if (otherBadTriangle.Transform is not null)
                                    {
                                        foreach (Transform child in otherBadTriangle.Transform)
                                        {
                                            if (child.gameObject.name == $"Boundary Edge: {edge.Vertex_1.Position} - {edge.Vertex_2.Position}"
                                                || child.gameObject.name ==
                                                $"Boundary Edge: {edge.Vertex_2.Position} - {edge.Vertex_1.Position}")
                                            {
                                                Object.Destroy(child.gameObject);
                                            }

                                        }
                                    }

                                    completedEdges.Add(edge);
                                    completedEdges.Add(new Edge(edge.Vertex_2, edge.Vertex_1));
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
                    if (ArePointsColinear(edge.Vertex_1.Position, edge.Vertex_2.Position, point))
                    {
                        yield return _showMessage("Points are colinear", point, 1);

                        continue;
                    }

                    var triangle = new Node_Triangle(edge.Vertex_1.Position, edge.Vertex_2.Position, point);

                    yield return _hideTriangle(triangle, 0);
                    yield return _showTriangle(triangle, 0.25f, 2);

                    triangles.TryAdd(triangle.ID_Vectors, triangle);
                }

                yield return _performEdgeFlipping(triangles);
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

            _returnUpdatedTriangles(triangles);
        }

        public static bool ArePointsColinear(Vector3 a, Vector3 b, Vector3 c)
        {
            var area = (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
            return Mathf.Abs(area) < Mathf.Epsilon;
        }

        IEnumerator _flipEdge(Vector3 a, Vector3 b, Node_Triangle triangle_1, Node_Triangle triangle_2,
            Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles)
        {   
            var triangle_1C = triangle_1.GetThirdVertex(a, b);
            var triangle_2C = triangle_2.GetThirdVertex(a, b);

            if (!Node_Triangle.IsPointInsideCircumcircle(triangle_1C.Position, a, b, triangle_2C.Position)) yield break;

            triangles.Remove(triangle_1.ID_Vectors);
            triangles.Remove(triangle_2.ID_Vectors);

            yield return _showMessage($"Flipping Edge: {a} - {b}", (a + b) * 0.5f, 1);

            yield return _showTriangle(triangle_1, 0.1f, 1, true);
            yield return _showTriangle(triangle_2, 0.1f, 2, true);

            yield return _hideTriangle(triangle_1);
            yield return _hideTriangle(triangle_2);


            var newTriangle1 = new Node_Triangle(triangle_1C.Position, triangle_2C.Position, a);
            var newTriangle2 = new Node_Triangle(triangle_1C.Position, triangle_2C.Position, b);

            yield return _showTriangle(newTriangle1, 0.25f, 1, true);
            yield return _showTriangle(newTriangle2, 0.25f, 2, true);

            yield return _hideTriangle(newTriangle1);
            yield return _hideTriangle(newTriangle2);

            triangles[newTriangle1.ID_Vectors] = newTriangle1;
            triangles[newTriangle2.ID_Vectors] = newTriangle2;

            yield return _showTriangle(newTriangle1, 0.25f, 1);
            yield return _showTriangle(newTriangle2, 0.25f, 1);
        }
        
        public static List<Edge_Half> TransformFromTriangleToHalfEdge(List<Node_Triangle> triangles)
        {
            //OrientTrianglesClockwise(triangles); Triangles should be counter-clockwise.
            
            var halfEdges = new List<Edge_Half>(triangles.Count * 3);

            foreach (var triangle in triangles)
            {
                var firstHalfEdge = new Edge_Half(triangle.A);
                var secondHalfEdge = new Edge_Half(triangle.B);
                var thirdHalfEdge = new Edge_Half(triangle.C);

                firstHalfEdge.Next = secondHalfEdge;
                secondHalfEdge.Next = thirdHalfEdge;
                thirdHalfEdge.Next = firstHalfEdge;

                firstHalfEdge.Previous = thirdHalfEdge;
                secondHalfEdge.Previous = firstHalfEdge;
                thirdHalfEdge.Previous = secondHalfEdge;
                
                firstHalfEdge.Vertex.EdgeHalf = secondHalfEdge;
                secondHalfEdge.Vertex.EdgeHalf = thirdHalfEdge;
                thirdHalfEdge.Vertex.EdgeHalf = firstHalfEdge;
                
                triangle.EdgeHalf = firstHalfEdge;

                firstHalfEdge.Triangle = triangle;
                secondHalfEdge.Triangle = triangle;
                thirdHalfEdge.Triangle = triangle;
                
                halfEdges.Add(firstHalfEdge);
                halfEdges.Add(secondHalfEdge);
                halfEdges.Add(thirdHalfEdge);
            }
            
            for (var i = 0; i < halfEdges.Count; i++)
            {
                var halfEdge = halfEdges[i];

                var goingToVertex = halfEdge.Vertex;
                var goingFromVertex = halfEdge.Previous.Vertex;

                for (var j = 0; j < halfEdges.Count; j++)
                {
                    if (i == j) continue;

                    var heOpposite = halfEdges[j];

                    if (goingFromVertex.Position != heOpposite.Vertex.Position ||
                        goingToVertex.Position != heOpposite.Previous.Vertex.Position) continue;
                    
                    halfEdge.Opposite = heOpposite;

                    break;
                }
            }

            return halfEdges;
        }
        
        // public static List<Node_Triangle> TriangulateByFlippingEdges(List<Vector3> sites)
        // {
        //     List<Node_Triangle> triangles = IncrementalTriangulation(vertices);
        //     
        //     var halfEdges = TransformFromTriangleToHalfEdge(triangles);
        //     
        //     var safety = 0;
        //
        //     while (true)
        //     {
        //         safety += 1;
        //
        //         if (safety > 100000)
        //         {
        //             Debug.Log("Stuck in endless loop");
        //
        //             break;
        //         }
        //
        //         var hasFlippedEdge = false;
        //         
        //         foreach (var thisEdge in halfEdges)
        //         {
        //             if (thisEdge.Opposite == null)
        //             {
        //                 continue;
        //             }
        //             
        //             var a = thisEdge.Vertex;
        //             var b = thisEdge.Next.Vertex;
        //             var c = thisEdge.Previous.Vertex;
        //             var d = thisEdge.Opposite.Next.Vertex;
        //
        //             var aPos = a.GetPos2D_XZ();
        //             var bPos = b.GetPos2D_XZ();
        //             var cPos = c.GetPos2D_XZ();
        //             var dPos = d.GetPos2D_XZ();
        //             
        //             if (!(Geometry.IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f)) continue;
        //             
        //             if (!Geometry.IsQuadrilateralConvex(aPos, bPos, cPos, dPos)) continue;
        //             
        //             if (Geometry.IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f)
        //             {
        //                 continue;
        //             }
        //
        //             hasFlippedEdge = true;
        //
        //             _flipEdge(thisEdge);
        //         }
        //         
        //         if (!hasFlippedEdge)
        //         {
        //             break;
        //         }
        //     }
        //
        //     return triangles;
        // }
        //
        // static void _flipEdge(Edge_Half one)
        // {
        //     var two = one.Next;
        //     var three = one.Previous;
        //     
        //     var four = one.Opposite;
        //     var five = one.Opposite.Next;
        //     var six = one.Opposite.Previous;
        //     
        //     var a = one.Vertex;
        //     var b = one.Next.Vertex;
        //     var c = one.Previous.Vertex;
        //     var d = one.Opposite.Next.Vertex;
        //     
        //     a.EdgeHalf = one.Next;
        //     c.EdgeHalf = one.Opposite.Next;
        //
        //     one.Next = three;
        //     one.Previous = five;
        //
        //     two.Next = four;
        //     two.Previous = six;
        //
        //     three.Next = five;
        //     three.Previous = one;
        //
        //     four.Next = six;
        //     four.Previous = two;
        //
        //     five.Next = one;
        //     five.Previous = three;
        //
        //     six.Next = two;
        //     six.Previous = four;
        //     
        //     one.Vertex = b;
        //     two.Vertex = b;
        //     three.Vertex = c;
        //     four.Vertex = d;
        //     five.Vertex = d;
        //     six.Vertex = a;
        //     
        //     var t1 = one.Triangle;
        //     var t2 = four.Triangle;
        //
        //     one.Triangle = t1;
        //     three.Triangle = t1;
        //     five.Triangle = t1;
        //
        //     two.Triangle = t2;
        //     four.Triangle = t2;
        //     six.Triangle = t2;
        //
        //     t1.Vertices[0] = b;
        //     t1.Vertices[1] = c;
        //     t1.Vertices[2] = d;
        //
        //     t2.Vertices[0] = b;
        //     t2.Vertices[1] = d;
        //     t2.Vertices[2] = a;
        //
        //     t1.EdgeHalf = three;
        //     t2.EdgeHalf = four;
        // }
        //
        // public static bool IsQuadrilateralConvex(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        // {
        //     var isConvex = false;
        //
        //     bool abc = Geometry.IsTriangleOrientedClockwise(a, b, c);
        //     bool abd = Geometry.IsTriangleOrientedClockwise(a, b, d);
        //     bool bcd = Geometry.IsTriangleOrientedClockwise(b, c, d);
        //     bool cad = Geometry.IsTriangleOrientedClockwise(c, a, d);
        //
        //     if (abc && abd && bcd & !cad)
        //     {
        //         isConvex = true;
        //     }
        //     else if (abc && abd && !bcd & cad)
        //     {
        //         isConvex = true;
        //     }
        //     else if (abc && !abd && bcd & cad)
        //     {
        //         isConvex = true;
        //     }
        //     //The opposite sign, which makes everything inverted
        //     else if (!abc && !abd && !bcd & cad)
        //     {
        //         isConvex = true;
        //     }
        //     else if (!abc && !abd && bcd & !cad)
        //     {
        //         isConvex = true;
        //     }
        //     else if (!abc && abd && !bcd & !cad)
        //     {
        //         isConvex = true;
        //     }
        //
        //
        //     return isConvex;
        // }s

        IEnumerator _showVoxel(Vector3 position, float duration = 1f, int sizeMultiplier = 1, int colourIndex = 0)
        {
            if (_shownVoxels.TryGetValue(position, out var shownVoxel)) Object.Destroy(shownVoxel);

            var voxel = Visualise_Voxel.Show_Voxel(position, Vector3.one * sizeMultiplier, colourIndex);
            voxel.name = $"Voxel: {position}";

            _shownVoxels[position] = voxel;

            yield return new WaitForSeconds(duration);
        }

        IEnumerator _showTriangle(Node_Triangle triangle, float duration = 1f, int colour = -1,
            bool showTriangle = false)
        {
            if (_shownTriangles.TryGetValue(triangle.ID_Vectors, out var shownTriangle)) Object.Destroy(shownTriangle);

            var triangleGO = Visualise_Triangle.Show_Triangle(new[]
                {
                    triangle.Vertices[0].Position + Vector3.up * 15,
                    triangle.Vertices[1].Position + Vector3.up * 15,
                    triangle.Vertices[2].Position + Vector3.up * 15
                },
                colour);

            if (!showTriangle) Object.Destroy(triangleGO.GetComponent<MeshRenderer>());

            for (var i = 0; i < triangle.Vertices.Length; i++)
            {
                var start = triangle.Vertices[i];
                var end = triangle.Vertices[(i + 1) % triangle.Vertices.Length];

                yield return _showEdge(start.Position, end.Position, duration, colour, triangleGO.transform);
            }

            triangle.Transform = triangleGO.transform;
            _shownTriangles[triangle.ID_Vectors] = triangleGO;
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
                new Vector3(position.x, 18, position.z), message);

            yield return new WaitForSeconds(duration);

            Object.Destroy(messageGO);
        }

        IEnumerator _performEdgeFlipping(Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles)
        {
            var edgesToCheck = new HashSet<Edge>();

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
                    .Where(t => t.Edges.Contains(edge) || t.Edges.Contains(new Edge(edge.Vertex_2, edge.Vertex_1)))
                    .ToList();

                if (adjacentTriangles.Count != 2) continue;

                yield return _flipEdge(edge.Vertex_1.Position, edge.Vertex_2.Position, adjacentTriangles[0], adjacentTriangles[1], triangles);
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