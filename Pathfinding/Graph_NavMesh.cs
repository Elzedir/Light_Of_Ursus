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

        Node_Triangle _createSuperTriangle(HashSet<Vector3> points)
        {
            var min = new Vector3(float.MaxValue, 0, float.MaxValue);
            var max = new Vector3(float.MinValue, 0, float.MinValue);
    
            foreach (var point in points)
            {
                min.x = Mathf.Min(min.x, point.x);
                min.z = Mathf.Min(min.z, point.z);
                max.x = Mathf.Max(max.x, point.x);
                max.z = Mathf.Max(max.z, point.z);
            }
            
            var dx = max.x - min.x;
            var dz = max.z - min.z;
            var deltaMax = Mathf.Max(dx, dz);
            var midX = (min.x + max.x) / 2;
            var midZ = (min.z + max.z) / 2;
    
            const float scale = 2f;
    
            var a = new Vector3(midX - scale * deltaMax, 0, midZ - deltaMax);
            var b = new Vector3(midX, 0, midZ + scale * deltaMax);
            var c = new Vector3(midX + scale * deltaMax, 0, midZ - deltaMax);
    
            var superTriangle = new Node_Triangle(a, b, c);
    
            return superTriangle;
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
        ControlSpeed _controlSpeed;
        ControlSpeed ControlSpeed => _controlSpeed ??= GameObject.Find("VisibleObjects").GetComponent<ControlSpeed>();

        public float Speed => ControlSpeed.Speed;
        
        IEnumerator DelaunayTriangulation(HashSet<Vector3> pointsToProcess,
            Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles, Node_Triangle superNodeTriangle)
        {
            yield return _showTriangle(superNodeTriangle, colour: 0, altitude: 1);
            
            foreach (var point in pointsToProcess)
            {
                var badTriangles = new HashSet<Node_Triangle>();
                
                yield return _showVoxel(point, 2, altitude: 2);

                foreach (var triangle in triangles.Values)
                {
                    if (!triangle.IsPointInsideCircumcircle(point)) continue;
                    
                    yield return _showVoxel(point, 2, 1, altitude: 2);
                    
                    yield return _showTriangle(triangle, 1, true, 1);

                    badTriangles.Add(triangle);
                }

                var boundaryHalfEdges = new List<Half_Edge>();

                foreach (var badTriangle in badTriangles)
                {
                    var currentEdge = badTriangle.Half_Edge;
                    
                    do
                    {
                        if (currentEdge.Opposite == null ||
                            currentEdge.Opposite != null && !badTriangles.Contains(currentEdge.Opposite.Triangle))
                        {
                            boundaryHalfEdges.Add(currentEdge);   
                        }
                        
                        currentEdge = currentEdge.Next;
                    } 
                    while (!Equals(currentEdge, badTriangle.Half_Edge));
                    
                    triangles.Remove(badTriangle.ID_Vectors);
                    
                    yield return _hideTriangle(badTriangle);
                }
                
                var newTriangles = new List<Node_Triangle>();
                
                yield return _createNewTriangles(0, point, boundaryHalfEdges, triangles, newTriangles);
                
                yield return _connectAdjacentTriangles(newTriangles);
                
                yield return _performEdgeFlipping(triangles);
                
                yield return _showVoxel(point, 2, 2);
            }

            foreach (var triangle in triangles.Values.ToList())
            {
                foreach (var vertex in triangle.Vertices)
                {
                    if (!superNodeTriangle.Vertices.Contains(vertex)) continue;

                    triangles.Remove(triangle.ID_Vectors);
                    
                    yield return _hideTriangle(triangle);
                }
            }

            _returnUpdatedTriangles(triangles);
        }
        
        Dictionary<Half_Edge, List<Half_Edge>> edgeIntersections = new();
        
        IEnumerator _createNewTriangles(int j, Vector3 point, List<Half_Edge> boundaryHalfEdges,
            Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles, List<Node_Triangle> newTriangles)
        {
            foreach(var edge in boundaryHalfEdges)
            {
                var a = edge.Vertex.Position;
                var b = edge.Next.Vertex.Position;
                
                if (ArePointsColinear(a, b, point)) continue;

                AddTriangleIntersections(a, b, point, boundaryHalfEdges);
                
                var triangle = new Node_Triangle(a, b, point);
                triangles[triangle.ID_Vectors] = triangle;
                newTriangles.Add(triangle);
                    
                yield return _showTriangle(triangle, 1);
            }
        }
        
        void AddTriangleIntersections(Vector3 a, Vector3 b, Vector3 c, List<Half_Edge> boundaryHalfEdges)
        {
            var triangleEdges = new List<(Vector3, Vector3)>
            {
                (a, b),
                (b, c),
                (c, a)
            };
            
            var boundaryVertices = new HashSet<Vector3>();
            foreach (var boundaryHalfEdge in boundaryHalfEdges)
            {
                boundaryVertices.Add(boundaryHalfEdge.Vertex.Position);
                boundaryVertices.Add(boundaryHalfEdge.Next.Vertex.Position);
            }
            
            foreach (var triangleEdge in triangleEdges)
            {
                if (boundaryVertices.Contains(triangleEdge.Item1) && boundaryVertices.Contains(triangleEdge.Item2))
                {
                    continue;
                }

                foreach (var boundaryHalfEdge in boundaryHalfEdges)
                {
                    var boundaryStart = boundaryHalfEdge.Vertex.Position;
                    var boundaryEnd = boundaryHalfEdge.Next.Vertex.Position;
                    
                    if (_vector3Equals(triangleEdge.Item1, boundaryStart) ||
                        _vector3Equals(triangleEdge.Item1, boundaryEnd) ||
                        _vector3Equals(triangleEdge.Item2, boundaryStart) ||
                        _vector3Equals(triangleEdge.Item2, boundaryEnd))
                    {
                        continue;
                    }

                    if (!_doEdgesIntersect(triangleEdge.Item1, triangleEdge.Item2, boundaryStart, boundaryEnd))
                        continue;
                    
                    if (!edgeIntersections.ContainsKey(boundaryHalfEdge))
                    {
                        edgeIntersections[boundaryHalfEdge] = new List<Half_Edge>();
                    }
                        
                    edgeIntersections[boundaryHalfEdge].Add(boundaryHalfEdge);
                }
            }
        }

        static bool _vector3Equals(Vector3 a, Vector3 b)
        {
            return (a.x - b.x) * (a.x - b.x) +
                (a.y - b.y) * (a.y - b.y) +
                (a.z - b.z) * (a.z - b.z) < Mathf.Epsilon * Mathf.Epsilon;
        }

        bool _doEdgesIntersect(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
        {
            var denominator = (b2.z - b1.z) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.z - a1.z);
            
            if (Mathf.Abs(denominator) < Mathf.Epsilon)
                return false;
                
            var ua = ((b2.x - b1.x) * (a1.z - b1.z) - (b2.z - b1.z) * (a1.x - b1.x)) / denominator;
            var ub = ((a2.x - a1.x) * (a1.z - b1.z) - (a2.z - a1.z) * (a1.x - b1.x)) / denominator;
            
            return ua is >= 0 and <= 1 && ub is >= 0 and <= 1;
        }

        public static bool ArePointsColinear(Vector3 a, Vector3 b, Vector3 c)
        {
            var area = (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
            return Mathf.Abs(area) < Mathf.Epsilon;
        }

        static IEnumerator _connectAdjacentTriangles(List<Node_Triangle> triangles)
        {
            yield return null;
            var edgeMap = new Dictionary<(Vector3, Vector3), Half_Edge>();
    
            foreach (var triangle in triangles)
            {
                var startEdge = triangle.Half_Edge;
                var currentEdge = startEdge;
        
                do
                {
                    var a = currentEdge.Vertex.Position;
                    var b = currentEdge.Next.Vertex.Position;
                    
                    var edgeKey = Half_Edge.CompareVector3(a, b) < 0 ? (a, b) : (b, a);
            
                    if (edgeMap.TryGetValue(edgeKey, out var existingEdge))
                    {
                        currentEdge.Opposite = existingEdge;
                        existingEdge.Opposite = currentEdge;
                        
                        edgeMap.Remove(edgeKey);
                    }
                    else
                    {
                        edgeMap[edgeKey] = currentEdge;
                    }
            
                    currentEdge = currentEdge.Next;
                    
                } while (!Equals(currentEdge, startEdge));
            }
        }
        
        a
            //* Fix edge flipping
        
        IEnumerator _performEdgeFlipping(Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles)
        {
            bool flippedEdge;
            var iteration = 0;
            
            do
            {
                flippedEdge = false;
                
                foreach (var triangle in Triangles.Values.ToList())
                {
                    var startEdge = triangle.Half_Edge;
                    var currentEdge = startEdge;
            
                    do
                    {
                        if (currentEdge.Opposite != null)
                        {
                            var nonSharedVertex = currentEdge.Opposite.Previous.Vertex;
                            
                            if (_isDelaunayViolation(currentEdge, nonSharedVertex) || _trianglesIntersect(currentEdge))
                            {
                                var a = currentEdge.Vertex.Position;
                                var b = currentEdge.Next.Vertex.Position;
                                var c = currentEdge.Previous.Vertex.Position;
                                var d = nonSharedVertex.Position;
                                
                                yield return _showTriangle(triangle, 2, true);
                                yield return _showMessage("Delaunay violation detected.", triangle.Centroid, 0.5f, 1);
                                yield return _showTriangle(triangle, 1);
                                yield return _showMessage("Delaunay violation detected.", a, 0.5f, 1);
                                yield return _showMessage("Delaunay violation detected.", b, 0.5f, 1);
                                yield return _showMessage("Delaunay violation detected.", c, 0.5f, 1);
                                yield return _showMessage("Delaunay violation detected.", d, 0.5f, 1);

                                if (IsQuadrilateralConvex(a, b, c, d))
                                {
                                    yield return _showMessage("Flipping edges to resolve intersection.", (a + b + c + d) * 0.25f, 0.5f, 1);
                                    yield return _flipEdge(currentEdge);
                                    flippedEdge = true;
                                }
                                else
                                {
                                    var area1 = _triangleArea(a, b, c);
                                    var area2 = _triangleArea(b, c, d);
                            
                                    if (area1 < area2)
                                    {
                                        yield return _showMessage("Discarding smaller intersecting triangle.", triangle.Centroid, 0.5f, 1);
                                        triangles.Remove((a, b, c));
                                    }
                                    else
                                    {
                                        yield return _showMessage("Discarding smaller intersecting triangle.", (a + b + c) * 0.333f, 0.5f, 1);
                                        triangles.Remove((b, c, d));
                                    }
                                }
                            }
                        }
                
                        currentEdge = currentEdge.Next;
                    } 
                    while (!Equals(currentEdge, startEdge));
            
                    if (flippedEdge) break;
                }
            } 
            while (flippedEdge && iteration++ < 10);

            foreach (var intersectingEdges in edgeIntersections)
            {
                var badTriangles = new List<Node_Triangle>();
                    
                foreach (var edge in intersectingEdges.Value)
                {
                    if (edge.Opposite == null) continue;
                    
                    badTriangles.Add(IsWorseTriangle(edge.Triangle, edge.Opposite.Triangle)
                        ? edge.Triangle
                        : edge.Opposite.Triangle);
                }
                
                foreach (var badTriangle in badTriangles)
                {
                    triangles.Remove(badTriangle.ID_Vectors);
                    
                    yield return _hideTriangle(badTriangle);
                }
            }
            
            edgeIntersections.Clear();
        }
        
        bool IsWorseTriangle(Node_Triangle triangle, Node_Triangle triangle2)
        {
            var area = _triangleArea(triangle.A.Position, triangle.B.Position, triangle.C.Position);
            var area2 = _triangleArea(triangle2.A.Position, triangle2.B.Position, triangle2.C.Position);
            return area < area2;
        }
        
        bool _trianglesIntersect(Half_Edge edge)
        {
            var a = edge.Vertex.Position;
            var b = edge.Next.Vertex.Position;
            var c = edge.Previous.Vertex.Position;

            var d = edge.Opposite.Previous.Vertex.Position;

            return _diagonalsIntersect(a, c, b, d);
        }

        bool _isDelaunayViolation(Half_Edge edge, Vertex nonSharedVertex)
        {
            return edge.Triangle.IsPointInsideCircumcircle(nonSharedVertex.Position);
        }

        bool IsQuadrilateralConvex(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            return _isTriangleClockwise(a, b, c)
                   && _isTriangleClockwise(a, c, d);
        }

        bool _isTriangleClockwise(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z) < 0;
        }

        bool _diagonalsIntersect(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
        {
            var o1 = _orientation(a1, a2, b1);
            var o2 = _orientation(a1, a2, b2);
            var o3 = _orientation(b1, b2, a1);
            var o4 = _orientation(b1, b2, a2);
            
            return !Mathf.Approximately(o1, o2) && !Mathf.Approximately(o3, o4);
        }
        
        float _triangleArea(Vector3 a, Vector3 b, Vector3 c)
        {
            return 0.5f * Mathf.Abs((b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z));
        }


        float _orientation(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
        }

        IEnumerator _flipEdge(Half_Edge one)
        {   
            var two = one.Next;
            var three = one.Previous;

            var four = one.Opposite;
            var five = four.Next;
            var six = four.Previous;

            var a = one.Vertex;
            var b = two.Vertex;
            var c = three.Vertex;
            var d = five.Vertex;

            one.Vertex = b;
            four.Vertex = d;
            
            one.Next = three;
            three.Previous = one;
            three.Next = five;
            five.Previous = three;
            five.Next = one;
            one.Previous = five;

            four.Next = six;
            six.Previous = four;
            six.Next = two;
            two.Previous = six;
            two.Next = four;
            four.Previous = two;
            
            one.Opposite = four;
            four.Opposite = one;
            
            var triangle_1 = one.Triangle;
            var triangle_2 = four.Triangle;
            
            yield return _hideTriangle(triangle_1, 0);
            yield return _hideTriangle(triangle_2, 0);

            one.Triangle = triangle_1;
            three.Triangle = triangle_1;
            five.Triangle = triangle_1;

            two.Triangle = triangle_2;
            four.Triangle = triangle_2;
            six.Triangle = triangle_2;
            
            triangle_1.Vertices[0] = b;
            triangle_1.Vertices[1] = c;
            triangle_1.Vertices[2] = d;

            triangle_2.Vertices[0] = b;
            triangle_2.Vertices[1] = d;
            triangle_2.Vertices[2] = a;

            triangle_1.Half_Edge = three;
            triangle_2.Half_Edge = four;

            yield return _showTriangle(triangle_1, 0);
            yield return _showTriangle(triangle_2, 0);
        }

        IEnumerator _showVoxel(Vector3 position, int sizeMultiplier = 1, int colourIndex = 0, int altitude = 0)
        {
            if (_shownVoxels.TryGetValue(position, out var shownVoxel)) Object.Destroy(shownVoxel);

            var voxel = Visualise_Voxel.Show_Voxel(position + Vector3.up * altitude, Vector3.one * sizeMultiplier, colourIndex);
            voxel.name = $"Voxel: {position}";

            _shownVoxels[position] = voxel;

            yield return new WaitForSeconds(Speed);
        }

        IEnumerator _showTriangle(Node_Triangle triangle, int colour = -1,
            bool showTriangle = false, int altitude = 0)
        {
            if (_shownTriangles.TryGetValue(triangle.ID_Vectors, out var shownTriangle)) yield return _hideTriangle(triangle, 0);

            var triangleGO = Visualise_Triangle.Show_Triangle(new[]
                {
                    triangle.Vertices[0].Position + Vector3.up * altitude,
                    triangle.Vertices[1].Position + Vector3.up * altitude,
                    triangle.Vertices[2].Position + Vector3.up * altitude
                },
                colour);
            triangleGO.name = $"Triangle: {triangle.Vertices[0].Position} - {triangle.Vertices[1].Position} - {triangle.Vertices[2].Position}";

            if (!showTriangle) Object.Destroy(triangleGO.GetComponent<MeshRenderer>());

            for (var i = 0; i < triangle.Vertices.Length; i++)
            {
                var start = triangle.Vertices[i];
                var end = triangle.Vertices[(i + 1) % triangle.Vertices.Length];

                yield return _showEdge(start.Position, end.Position, colour, triangleGO.transform, altitude);
            }
            
            _shownTriangles[triangle.ID_Vectors] = triangleGO;
        }

        IEnumerator _hideTriangle(Node_Triangle triangle, float duration = 0.5f)
        {
            if (!_shownTriangles.TryGetValue(triangle.ID_Vectors, out var shownTriangle)) yield break;

            Object.Destroy(shownTriangle);
            _shownTriangles.Remove(triangle.ID_Vectors);

            yield return new WaitForSeconds(duration);
        }

        IEnumerator _hideAllTriangles(Node_Triangle superNodeTriangle)
        {
            foreach (var triangle in _shownTriangles.Values)
            {
                Object.Destroy(triangle);
            }

            _shownTriangles.Clear();

            yield return _showTriangle(superNodeTriangle, 0);

            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator _showEdge(Vector3 start, Vector3 end, int colour = -1,
            Transform parent = null, int altitude = 0)
        {
            var center = (start + end) * 0.5f;
            var size = new Vector3(0.25f, 0.25f, Vector3.Distance(start, end));

            var line = Visualise_Voxel.Show_Voxel(center + Vector3.up * altitude, size, colour,
                rotation: Quaternion.LookRotation(end - start));

            if (parent is not null) line.transform.SetParent(parent);
            line.name = $"Boundary Edge: {start} - {end}";

            yield return new WaitForSeconds(Speed);
        }
        
        IEnumerator _showHalfEdge(Vector3 start, Vector3 end, int colour = -1,
            Transform parent = null, int altitude = 0)
        {
            var center = (start + end) * 0.5f;
            var size = new Vector3(0.25f, 0.25f, Vector3.Distance(start, end));

            var line = Visualise_Voxel.Show_Voxel(center + Vector3.up * altitude, size, colour,
                rotation: Quaternion.LookRotation(end - start));

            if (parent is not null) line.transform.SetParent(parent);
            line.name = $"Half Edge: {start} - {end}";

            yield return new WaitForSeconds(Speed);
        }
        
        static IEnumerator _showCircle(Vector3 position, float radius, int altitude = 0)
        {
            var circle = Visualise_Circle.Show_Circle(position + Vector3.up * altitude, radius, 32, 0);
            circle.name = "Circle";

            yield return new WaitForSeconds(0.5f);

            Object.Destroy(circle);
        }

        static IEnumerator _showMessage(string message, Vector3 position, float duration, int altitude = 0)
        {
            var messageGO = Visualise_Message.Show_Message(position + Vector3.up * altitude, message);

            yield return new WaitForSeconds(duration);

            Object.Destroy(messageGO);
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