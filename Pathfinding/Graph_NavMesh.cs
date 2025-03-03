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
                    
                    yield return _hideTriangle(triangle);
                    
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
                            boundaryHalfEdges.Add(currentEdge);
                        
                        currentEdge = currentEdge.Next;
                    } 
                    while (!Equals(currentEdge, badTriangle.Half_Edge));
                    
                    triangles.Remove(badTriangle.ID_Vectors);
                    
                    yield return _hideTriangle(badTriangle);
                }
                
                var orderedBoundaryVertices = _organizeBoundaries(boundaryHalfEdges).ToList();
                var newTriangles = new List<Node_Triangle>();

                foreach (var boundaryVertex in orderedBoundaryVertices)
                {
                    yield return _showMessage("Boundary Vertex", boundaryVertex, 0.5f, 2);
                }
                
                for (var i = 0; i < orderedBoundaryVertices.Count; i++)
                {
                    var a = orderedBoundaryVertices[i];
                    var b = orderedBoundaryVertices[(i + 1) % orderedBoundaryVertices.Count];
               
                    a
                        //* FIX
                    
                    if (ArePointsColinear(a, b, point))
                        continue;
                
                    if (WillTriangleIntersectAnyBoundaryHalfEdge(a, b, point, boundaryHalfEdges)) continue;
                
                    yield return _showMessage($"Creating new triangle A {a}", a, 0.5f, 2);
                    yield return _showMessage($"Creating new triangle B {b}", b, 0.5f, 2);
                    yield return _showMessage($"Creating new triangle Point {point}", point, 0.5f, 2);
                
                    var triangle = new Node_Triangle(a, b, point);
                    triangles[triangle.ID_Vectors] = triangle;
                    newTriangles.Add(triangle);
                
                    yield return _showTriangle(triangle, 1);
                }
                
                _connectAdjacentTriangles(newTriangles);
                
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

        HashSet<Vector3> _organizeBoundaries(List<Half_Edge> boundaryHalfEdges)
        {
            if (boundaryHalfEdges.Count == 0)
                return new HashSet<Vector3>();
        
            var polygons = new HashSet<Vector3>();
            var visitedHalfEdges = new HashSet<Half_Edge>();
        
            foreach (var startHalfEdge in boundaryHalfEdges)
            {
                if (visitedHalfEdges.Contains(startHalfEdge))
                    continue;

                var orderedVertices = new List<Vector3>();
                var currentHalfEdge = startHalfEdge;

                do
                {
                    orderedVertices.Add(currentHalfEdge.Vertex.Position);
                    
                    visitedHalfEdges.Add(currentHalfEdge);
                    visitedHalfEdges.Add(currentHalfEdge.Opposite);
                    
                    currentHalfEdge = currentHalfEdge.Next;
                }
                while (!Equals(currentHalfEdge, startHalfEdge));
                
                if (orderedVertices.Count > 1 && !_vector3Equals(orderedVertices[0], orderedVertices[^1]))
                {
                    orderedVertices.Add(orderedVertices[0]);
                }
                
                foreach (var vertex in orderedVertices)
                {
                    polygons.Add(vertex);
                }
            }

            return polygons;
        }
        
            //* Check this intersecting and edge flipping
        
        bool WillTriangleIntersectAnyBoundaryHalfEdge(Vector3 a, Vector3 b, Vector3 c, List<Half_Edge> boundaryHalfEdges)
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
                    
                    if (_doEdgesIntersect(triangleEdge.Item1, triangleEdge.Item2, boundaryStart, boundaryEnd))
                    {
                        return true;
                    }
                }
            }
            
            return false;
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

        static void _connectAdjacentTriangles(List<Node_Triangle> triangles)
        {
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

        IEnumerator _flipEdge(Half_Edge one)
        {   
            var two = one.Next;
            var three = two.Next;

            var four = one.Opposite;
            var five = four.Next;
            var six = five.Next;

            var a = one.Vertex;
            var b = two.Vertex;
            var c = three.Vertex;
            var d = five.Vertex;

            a.HalfEdge = two;
            c.HalfEdge = five;
            
            one.Next = three;
            one.Previous = five;

            two.Next = four;
            two.Previous = six;

            three.Next = five;
            three.Previous = one;

            four.Next = six;
            four.Previous = two;

            five.Next = one;
            five.Previous = three;

            six.Next = two;
            six.Previous = four;
            
            one.Vertex = b;
            two.Vertex = b;
            three.Vertex = c;
            four.Vertex = d;
            five.Vertex = d;
            six.Vertex = a;
            
            var triangle_1 = one.Triangle;
            var triangle_2 = four.Triangle;
            
            yield return _showTriangle(triangle_1,  0, true);
            yield return _showTriangle(triangle_2,  1, true);
            
            yield return _hideTriangle(triangle_1);
            yield return _hideTriangle(triangle_2);

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
            
            yield return _showTriangle(triangle_1, 2, true);
            yield return _showTriangle(triangle_2, 2, true);

            yield return _hideTriangle(triangle_1);
            yield return _hideTriangle(triangle_2);

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
            if (_shownTriangles.TryGetValue(triangle.ID_Vectors, out var shownTriangle)) Object.Destroy(shownTriangle);

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

        static IEnumerator _showMessage(string message, Vector3 position, float duration, int altitude = 0)
        {
            var messageGO = Visualise_Message.Show_Message(position + Vector3.up * altitude, message);

            yield return new WaitForSeconds(duration);

            Object.Destroy(messageGO);
        }

        IEnumerator _performEdgeFlipping(Dictionary<(Vector3, Vector3, Vector3), Node_Triangle> triangles)
        {
            bool flippedEdge;
            do
            {
                flippedEdge = false;
                
                foreach (var triangle in triangles.Values)
                {
                    var startEdge = triangle.Half_Edge;
                    var currentEdge = startEdge;
            
                    do
                    {
                        if (currentEdge.Opposite != null)
                        {
                            var notSharedVertex = currentEdge.Opposite.Next.Next.Vertex;
                            
                            if (triangle.IsPointInsideCircumcircle(notSharedVertex.Position))
                            {
                                yield return _flipEdge(currentEdge);
                                flippedEdge = true;
                                
                                break;
                            }
                        }
                
                        currentEdge = currentEdge.Next;
                    } while (currentEdge != startEdge && !flippedEdge);
            
                    if (flippedEdge) break;
                }
            } while (flippedEdge);
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