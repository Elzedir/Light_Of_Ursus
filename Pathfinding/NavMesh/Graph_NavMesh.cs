using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pathfinding.NavMesh
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

            //* Also, the epsilon value for generating triangles might be wrong since when the step is small enough for the grid, it connects trinangles
            //* together that it shouldn't. Check the minimum values.
            
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

        public static Node_Triangle CreateSuperTriangle(HashSet<Vector3> vertices)
        {
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

            foreach (var vertex in vertices)
            {
                minX = Mathf.Min(minX, vertex.x);
                minY = Mathf.Min(minY, vertex.y);
                minZ = Mathf.Min(minZ, vertex.z);
                maxX = Mathf.Max(maxX, vertex.x);
                maxY = Mathf.Max(maxY, vertex.y);
                maxZ = Mathf.Max(maxZ, vertex.z);
            }

            var dx = maxX - minX;
            var dy = maxY - minY;
            var dz = maxZ - minZ;
            var maxDelta = Mathf.Max(dx, Mathf.Max(dy, dz)) * 10;

            // var superTriangle = new Node_Triangle(
            //     new Vector3(minX - maxDelta, minY, minZ - maxDelta), 
            //     new Vector3(maxX + maxDelta, minY, minZ - maxDelta), 
            //     new Vector3(minX + dx / 2, minY, maxZ + maxDelta));
            
            var superTriangle = new Node_Triangle(
                new Vector3(minX - maxDelta, 0, minZ - maxDelta), 
                new Vector3(maxX + maxDelta, 0, minZ - maxDelta), 
                new Vector3(minX + dx / 2, 0, maxZ + maxDelta));

            return superTriangle;
        }

        public void _performTriangulationUpdate(HashSet<Vector3> points)
        {
            var sortedPoints = points.OrderBy(p => p.x).ThenBy(p => p.z).ToHashSet();
            _triangles = new Dictionary<ulong, Node_Triangle>();

            DelaunayTriangulation(sortedPoints, _triangles);
        }

        int _iteration;
        ControlSpeed _controlSpeed;
        ControlSpeed ControlSpeed => _controlSpeed ??= GameObject.Find("VisibleObjects").GetComponent<ControlSpeed>();

        public float Speed => ControlSpeed.Speed;

        void DelaunayTriangulation(HashSet<Vector3> pointsToProcess,
            Dictionary<ulong, Node_Triangle> triangles)
        {
            var superNodeTriangle = CreateSuperTriangle(pointsToProcess);
            triangles.Add(superNodeTriangle.ID, superNodeTriangle);

            _showTriangle(superNodeTriangle, colour: 0, altitude: 1);

            foreach (var point in pointsToProcess)
            {
                var badTriangles = new HashSet<Node_Triangle>();

                //yield return _showVoxel(point, 1, altitude: 2);

                foreach (var triangle in triangles.Values)
                {
                    if (!triangle.IsPointInsideCircumcircle(point)) continue;

                    //yield return _showVoxel(point, 1, 1, altitude: 2);

                    //_showTriangle(triangle, 1, true, 1);

                    badTriangles.Add(triangle);
                }
                
                a
                    //* Check constrained vs this

                var polygon = new Dictionary<(Vector3, Vector3), Half_Edge>();

                foreach (var badTriangle in badTriangles)
                {
                    var currentEdge = badTriangle.Half_Edge;

                    do
                    {
                        if (badTriangles.Count(t => Node_Triangle.SharesEdge(t, currentEdge)) <= 1)
                        {
                            var edgeKey = _getEdgeKey(currentEdge.Vertex.Position, currentEdge.Next.Vertex.Position);

                            polygon.TryAdd(edgeKey, currentEdge);
                        }

                        currentEdge = currentEdge.Next;
                    } 
                    while (!Equals(currentEdge, badTriangle.Half_Edge));

                    triangles.Remove(badTriangle.ID);

                    //yield return _hideTriangle(badTriangle, 0);
                }

                var newTriangles = new List<Node_Triangle>();

                _createNewTriangles(point, polygon, triangles, newTriangles);

                _showVoxel(point, 0.5f, 2);
            }

            foreach (var triangle in triangles.Values.ToList())
            {
                //yield return _hideTriangle(triangle, 0.1f);
                
                if (superNodeTriangle.SharesVertex(triangle))
                {
                    triangles.Remove(triangle.ID);

                    //yield return _hideTriangle(triangle, 0.1f);   
                    continue;
                }
                
                _showTriangle(triangle, 0);
            }
        }

        static (Vector3, Vector3) _getEdgeKey(Vector3 a, Vector3 b)
        {
            return a.x < b.x || (Mathf.Approximately(a.x, b.x) && a.z < b.z) ? (a, b) : (b, a);
        }

        void _createNewTriangles(Vector3 point, Dictionary<(Vector3, Vector3), Half_Edge> edgesToReplace,
            Dictionary<ulong, Node_Triangle> triangles, List<Node_Triangle> newTriangles)
        {
            for (var i = 0; i < 3; i++)
            {
                newTriangles.Clear();
                
                foreach (var edge in edgesToReplace.Values)
                {
                    var a = edge.Vertex.Position;
                    var b = edge.Next.Vertex.Position;

                    if (Node_Triangle.ArePointsColinear(a, b, point)) continue;

                    var triangle = new Node_Triangle(a, b, point);
                    triangles[triangle.ID] = triangle;
                    newTriangles.Add(triangle);

                    //_showTriangle(triangle, 1);
                }
                
                edgesToReplace.Clear();
            
                _connectAdjacentTriangles(newTriangles);
                
                //var intersectionEdges = _getTriangleIntersections(newTriangles);
                
                //yield return _performEdgeFlipping(edgesToReplace, triangles, intersectionEdges);   
            }
        }

        static Dictionary<((Vector3, Vector3), (Vector3, Vector3)), (Half_Edge, Half_Edge)> _getTriangleIntersections(List<Node_Triangle> newTriangles)
        {
            var intersectingEdges = new Dictionary<((Vector3, Vector3), (Vector3, Vector3)), (Half_Edge, Half_Edge)>();
            
            foreach (var newTriangle in newTriangles)
            {
                var newEdges = new List<Half_Edge>
                {
                    newTriangle.Half_Edge,
                    newTriangle.Half_Edge.Next,
                    newTriangle.Half_Edge.Previous
                };

                foreach (var newEdge in newEdges)
                {
                    foreach (var triangle in newTriangles)
                    {
                        var otherEdge = triangle.Half_Edge;
                        var iteration = 0;
                        
                        do
                        {
                            var newEdgeStart = newEdge.Vertex.Position;
                            var newEdgeEnd = newEdge.Next.Vertex.Position;
                    
                            var otherEdgeStart = otherEdge.Vertex.Position;
                            var otherEdgeEnd = otherEdge.Next.Vertex.Position;

                            if (_vector3Equals(newEdgeStart, otherEdgeStart)
                                || _vector3Equals(newEdgeEnd, otherEdgeEnd)
                                || _vector3Equals(newEdgeEnd, otherEdgeStart)
                                || _vector3Equals(newEdgeStart, otherEdgeEnd))
                            {
                                otherEdge = otherEdge.Next;
                                continue;
                            }

                            if (!_doEdgesIntersect(
                                    newEdgeStart, newEdgeEnd,
                                    otherEdgeStart, otherEdgeEnd))
                            {
                                otherEdge = otherEdge.Next;
                                continue;
                            }
                            
                            var newEdgeKey = _getEdgeKey(newEdgeStart, newEdgeEnd);
                            var otherEdgeKey = _getEdgeKey(otherEdgeStart, otherEdgeEnd);
                            
                            intersectingEdges.TryAdd((newEdgeKey, otherEdgeKey), (newEdge, otherEdge));

                            otherEdge = otherEdge.Next;
                        } 
                        while (!Equals(otherEdge, triangle.Half_Edge) && iteration++ < 50);

                        if (iteration < 50) continue;
                        Debug.LogError("Infinite loop detected.");
                        break;
                    }
                }   
            }
            
            return intersectingEdges;
        }

        static bool _vector3Equals(Vector3 a, Vector3 b)
        {
            return (a.x - b.x) * (a.x - b.x) +
                (a.y - b.y) * (a.y - b.y) +
                (a.z - b.z) * (a.z - b.z) < Mathf.Epsilon * Mathf.Epsilon;
        }

        static bool _doEdgesIntersect(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
        {
            var denominator = (b2.z - b1.z) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.z - a1.z);

            if (Mathf.Abs(denominator) < Mathf.Epsilon)
                return false;

            var ua = ((b2.x - b1.x) * (a1.z - b1.z) - (b2.z - b1.z) * (a1.x - b1.x)) / denominator;
            var ub = ((a2.x - a1.x) * (a1.z - b1.z) - (a2.z - a1.z) * (a1.x - b1.x)) / denominator;

            return ua is >= 0 and <= 1 && ub is >= 0 and <= 1;
        }

        static void _connectAdjacentTriangles(List<Node_Triangle> newTriangles)
        {
            var edgeMap = new Dictionary<(Vector3, Vector3), Half_Edge>();

            foreach (var triangle in newTriangles)
            {
                var startEdge = triangle.Half_Edge;
                var currentEdge = startEdge;
                var iteration = 0;

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
                    else edgeMap[edgeKey] = currentEdge;

                    currentEdge = currentEdge.Next;
                } 
                while (!Equals(currentEdge, startEdge) && iteration++ < 50);
                
                if (iteration < 50) continue;
                
                Debug.LogError("Infinite loop detected.");
                break;
            }
        }

        IEnumerator _performEdgeFlipping(Dictionary<(Vector3, Vector3), Half_Edge> edgesToReplace,
            Dictionary<ulong, Node_Triangle> triangles,
            Dictionary<((Vector3, Vector3), (Vector3, Vector3)), (Half_Edge, Half_Edge)> intersectingEdges)
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
                    var iterationA = 0;

                    do
                    {
                        if (currentEdge.Opposite != null)
                        {
                            var nonSharedVertex = currentEdge.Opposite.Previous.Vertex;

                            if (!_isDelaunayViolation(currentEdge, nonSharedVertex))
                            {
                                var a = currentEdge.Vertex.Position;
                                var b = currentEdge.Next.Vertex.Position;
                                var c = currentEdge.Previous.Vertex.Position;
                                var d = nonSharedVertex.Position;

                                if (_isQuadrilateralConvex(a, b, c, d))
                                {
                                    yield return _showMessage("Flipping edges to resolve intersection.",
                                        (a + b + c + d) * 0.25f, 0.5f, 1);
                                    yield return _flipEdge(currentEdge);
                                    flippedEdge = true;

                                    break;
                                }
                            }
                        }

                        currentEdge = currentEdge.Next;
                    } 
                    while (!Equals(currentEdge, startEdge) && iterationA++ < 50);
                    
                    if (flippedEdge) break;
                    
                    if (iterationA < 50) continue;
                    Debug.LogError("Infinite loop detected.");
                    break;
                }
            } while (flippedEdge && iteration++ < 5);

            var trianglesToRemove = new HashSet<Node_Triangle>();
            
            foreach (var (edge1, edge2) in intersectingEdges.Values)
            {
                var triangle1 = edge1.Triangle;
                var triangle2 = edge2.Triangle;

                var vertexPositions = new HashSet<Vector3>
                {
                    edge1.Vertex.Position,
                    edge1.Next.Vertex.Position,
                    edge1.Previous.Vertex.Position,
                    edge2.Vertex.Position,
                    edge2.Next.Vertex.Position,
                    edge2.Previous.Vertex.Position
                };

                var vertices = vertexPositions.ToArray();

                yield return _showMessage($"Processing intersection with {vertices.Length} unique vertices.",
                    vertices.Aggregate(Vector3.zero, (sum, v) => sum + v) / vertices.Length, 0.5f, 1);

                if (vertices.Length != 4)
                {
                    Debug.Log($"Intersection with {vertices.Length} vertices.");
                    continue;
                    
                    var delaunayTriangles =
                        ConstrainedDelaunayTriangulation.TriangulateIntersection(triangle1, triangle2, vertices);
                    
                    foreach (var triangleVertices in delaunayTriangles)
                    {
                        var triangle = new Node_Triangle(
                            triangleVertices.Item1, 
                            triangleVertices.Item2,
                            triangleVertices.Item3);
                        
                        var vertexA = triangle.A;
                        var vertexB = triangle.B;
                        var vertexC = triangle.C;

                        var edgeA = new Half_Edge(vertexA)
                            { Next = new Half_Edge(vertexB), Previous = new Half_Edge(vertexC) };
                        var edgeB = new Half_Edge(vertexB)
                            { Next = new Half_Edge(vertexC), Previous = new Half_Edge(vertexA) };
                        var edgeC = new Half_Edge(vertexC)
                            { Next = new Half_Edge(vertexA), Previous = new Half_Edge(vertexB) };

                        var edgeKey1 = _getEdgeKey(vertexA.Position, vertexB.Position);
                        var edgeKey2 = _getEdgeKey(vertexB.Position, vertexC.Position);
                        var edgeKey3 = _getEdgeKey(vertexC.Position, vertexA.Position);

                        edgesToReplace[edgeKey1] = edgeA;
                        edgesToReplace[edgeKey2] = edgeB;
                        edgesToReplace[edgeKey3] = edgeC;
                        
                        _showTriangle(triangle, 2, true);
                    }
                    
                    yield return new WaitForSeconds(20);

                    continue;
                }

                _showTriangle(triangle1, 2, true);
                _showTriangle(triangle2, 2, true);
                yield return _showMessage("Classic quadrilateral intersection case.",
                    vertices.Aggregate(Vector3.zero, (sum, v) => sum + v) * 0.25f, 0.5f, 1);

                var a = vertices[0];
                var b = vertices[1];
                var c = vertices[2];
                var d = vertices[3];

                var possibleTriangulations = new List<(List<(Vector3, Vector3, Vector3)> triangles, float score)>();

                if (!_diagonalsIntersect(a, c, b, d))
                {
                    var triangulation1 = new List<(Vector3, Vector3, Vector3)>
                    {
                        (a, b, c),
                        (a, c, d)
                    };

                    var triangulation2 = new List<(Vector3, Vector3, Vector3)>
                    {
                        (a, b, d),
                        (b, c, d)
                    };

                    var score1 = _evaluateTriangulation(triangulation1);
                    var score2 = _evaluateTriangulation(triangulation2);

                    possibleTriangulations.Add((triangulation1, score1));
                    possibleTriangulations.Add((triangulation2, score2));
                }

                if (possibleTriangulations.Count > 0)
                {
                    possibleTriangulations.Sort((t1, t2) => t2.score.CompareTo(t1.score));

                    var bestTriangulation = possibleTriangulations[0].triangles;

                    trianglesToRemove.Add(triangle1);
                    trianglesToRemove.Add(triangle2);

                    foreach (var triangle in bestTriangulation)
                    {
                        var vertexA = new Vertex(triangle.Item1);
                        var vertexB = new Vertex(triangle.Item2);
                        var vertexC = new Vertex(triangle.Item3);

                        var edgeA = new Half_Edge(vertexA)
                            { Next = new Half_Edge(vertexB), Previous = new Half_Edge(vertexC) };
                        var edgeB = new Half_Edge(vertexB)
                            { Next = new Half_Edge(vertexC), Previous = new Half_Edge(vertexA) };
                        var edgeC = new Half_Edge(vertexC)
                            { Next = new Half_Edge(vertexA), Previous = new Half_Edge(vertexB) };

                        var edgeKey1 = _getEdgeKey(vertexA.Position, vertexB.Position);
                        var edgeKey2 = _getEdgeKey(vertexB.Position, vertexC.Position);
                        var edgeKey3 = _getEdgeKey(vertexC.Position, vertexA.Position);

                        edgesToReplace[edgeKey1] = edgeA;
                        edgesToReplace[edgeKey2] = edgeB;
                        edgesToReplace[edgeKey3] = edgeC;
                    }

                    yield return _showMessage("Created optimal triangulation.",
                        vertices.Aggregate(Vector3.zero, (sum, v) => sum + v) * 0.25f, 0.5f, 1);
                }
                else
                {
                    if (_evaluateTriangle(triangle1) < _evaluateTriangle(triangle2))
                    {
                        trianglesToRemove.Add(triangle1);
                        yield return _showMessage("Removing lower quality triangle.", triangle1.Centroid, 0.5f, 1);
                        _showTriangle(triangle2, 2);
                    }
                    else
                    {
                        trianglesToRemove.Add(triangle2);
                        yield return _showMessage("Removing lower quality triangle.", triangle2.Centroid, 0.5f, 1);
                        _showTriangle(triangle1, 2);
                    }
                }
            }

            foreach (var triangleToRemove in trianglesToRemove)
            {
                triangles.Remove(triangleToRemove.ID);
                yield return _hideTriangle(triangleToRemove);
            }
        }

        static float _evaluateTriangulation(List<(Vector3, Vector3, Vector3)> triangles)
        {
            float totalQuality = 0;
    
            foreach (var triangle in triangles)
            {
                var area = _triangleArea(triangle.Item1, triangle.Item2, triangle.Item3);
                var minAngle = _calculateMinAngle(triangle.Item1, triangle.Item2, triangle.Item3);
                
                totalQuality += area * minAngle;
            }
    
            return totalQuality / triangles.Count;
        }
        
        float _evaluateTriangle(Node_Triangle triangle)
        {
            var a = triangle.Vertices[0].Position;
            var b = triangle.Vertices[1].Position;
            var c = triangle.Vertices[2].Position;
    
            var area = _triangleArea(a, b, c);
            var minAngle = _calculateMinAngle(a, b, c);
    
            return area * minAngle;
        }

        static float _calculateMinAngle(Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = b - a;
            var bc = c - b;
            var ca = a - c;
            
            ab.Normalize();
            bc.Normalize();
            ca.Normalize();
            
            var angleA = Mathf.Acos(Vector3.Dot(-ca, ab));
            var angleB = Mathf.Acos(Vector3.Dot(-ab, bc));
            var angleC = Mathf.Acos(Vector3.Dot(-bc, ca));
            
            return Mathf.Min(angleA, angleB, angleC);
        }

        static float _triangleArea(Vector3 a, Vector3 b, Vector3 c) => 0.5f * Mathf.Abs(_orientation(a, b, c));

        static bool _isDelaunayViolation(Half_Edge edge, Vertex nonSharedVertex)
        {
            return edge.Triangle.IsPointInsideCircumcircle(nonSharedVertex.Position);
        }

        static bool _isQuadrilateralConvex(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            return _isTriangleClockwise(a, b, c)
                   && _isTriangleClockwise(a, c, d);
        }

        static bool _isTriangleClockwise(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z) < 0;
        }

        static bool _diagonalsIntersect(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
        {
            var o1 = _orientation(a1, a2, b1);
            var o2 = _orientation(a1, a2, b2);
            var o3 = _orientation(b1, b2, a1);
            var o4 = _orientation(b1, b2, a2);

            return !Mathf.Approximately(o1, o2) && !Mathf.Approximately(o3, o4);
        }

        static float _orientation(Vector3 a, Vector3 b, Vector3 c) =>
            (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);

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

            _showTriangle(triangle_1, 0);
            _showTriangle(triangle_2, 0);
        }

        void _showVoxel(Vector3 position, float sizeMultiplier = 1f, int colourIndex = 0, int altitude = 0)
        {
            if (_shownVoxels.TryGetValue(position, out var shownVoxel)) Object.Destroy(shownVoxel);

            var voxel = Visualise_Voxel.Show_Voxel(position + Vector3.up * altitude, Vector3.one * sizeMultiplier,
                colourIndex);
            voxel.name = $"Voxel: {position}";

            _shownVoxels[position] = voxel;

            //yield return new WaitForSeconds(0);
        }

        void _showTriangle(Node_Triangle triangle, int colour = -1,
            bool showTriangle = false, int altitude = 0)
        {
            if (_shownTriangles.TryGetValue(triangle.ID_Vectors, out var shownTriangle))
                Manager_Game.S_Instance.StartCoroutine(_hideTriangle(triangle, 0));

            var triangleGO = Visualise_Triangle.Show_Triangle(new[]
                {
                    triangle.Vertices[0].Position + Vector3.up * altitude,
                    triangle.Vertices[1].Position + Vector3.up * altitude,
                    triangle.Vertices[2].Position + Vector3.up * altitude
                },
                colour);
            triangleGO.name =
                $"Triangle: {triangle.Vertices[0].Position} - {triangle.Vertices[1].Position} - {triangle.Vertices[2].Position}";

            if (!showTriangle) Object.Destroy(triangleGO.GetComponent<MeshRenderer>());

            for (var i = 0; i < triangle.Vertices.Length; i++)
            {
                var start = triangle.Vertices[i];
                var end = triangle.Vertices[(i + 1) % triangle.Vertices.Length];

                Manager_Game.S_Instance.StartCoroutine(_showEdge(start.Position, end.Position, colour, triangleGO.transform, altitude));
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