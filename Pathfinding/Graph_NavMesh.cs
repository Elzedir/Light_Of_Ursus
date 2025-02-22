using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;

namespace Pathfinding
{
    public class Graph_NavMesh
    {
        Dictionary<ulong, Node_3D> _nodes;
        Dictionary<ulong, Node_Triangle> _triangles;
        Dictionary<ulong, Node_Triangle> Triangles => _triangles ??= _initialiseTrianglesGrid();
        
        const float _spacing = 1f;
        const float _minSpacing = 0.1f;
        const float _maxSpacing = 10f;
        const float _densityMultiplier = 10f;
        
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
                }
            }

            RefineWithDynamicSpacing(terrain);
            
            _performTriangulationUpdate(_nodes);
            
            return new Dictionary<ulong, Node_Triangle>();
        }

        public void RefineWithDynamicSpacing(Terrain terrain)
        {
            var nodeDensities = new Dictionary<ulong, float>();

            foreach (var node in _nodes.Values.ToList())
            {
                var density = _calculateNodeDensity(terrain, node.Position);
                nodeDensities[node.ID] = density;
            }

            var sortedNodes = nodeDensities
                .OrderByDescending(x => x.Value)
                .Select(x => _nodes[x.Key])
                .ToList();

            foreach (var node in sortedNodes)
            {
                var density = nodeDensities[node.ID];
                _moveNodeBasedOnDensity(node, sortedNodes, density);
            }
        }
        
        a
            //* find a more efficient way to distribute nodes

        static void _moveNodeBasedOnDensity(Node_3D node, List<Node_3D> sortedNodes, float density)
        {
            if (density < 10f)
            {
                var averagePosition = Vector3.zero;
                var count = 0;
                
                foreach (var otherNode in sortedNodes)
                {
                    if (otherNode.ID == node.ID) continue;
                    
                    var distance = Vector3.Distance(node.Position, otherNode.Position);
                    var influenceFactor = Mathf.Max(0,
                        (1 - distance / _spacing) * Mathf.Abs(density -
                                                              _calculateNodeDensity(Terrain.activeTerrain,
                                                                  otherNode.Position)));

                    averagePosition += otherNode.Position * influenceFactor;
                    count++;
                }

                if (count > 0)
                {
                    averagePosition /= count;
                    var movement = (averagePosition - node.Position) * density;
                    
                    Debug.Log($"Density: {density}, Movement: {movement.magnitude}");
                    
                    node.Position = Vector3.MoveTowards(node.Position, averagePosition, movement.magnitude);
                }
            }
            
            if (density > 0.01f)
            {
                var repulsionForce = Vector3.zero;
                
                foreach (var otherNode in sortedNodes)
                {
                    if (otherNode == node) continue;
                    
                    var direction = node.Position - otherNode.Position;
                    var distance = direction.magnitude;
                    
                    if (distance < _spacing)
                    {
                        repulsionForce += direction.normalized / distance;
                    }
                }
                
                Debug.Log($"Density: {density}, Repulsion: {repulsionForce.magnitude} Repulsion total: {repulsionForce  * (1 - density)}");
                
                node.Position += repulsionForce * (1 - density);
            }
            
            _showVoxel(node.Position, Vector3.one * density);
        }

        static float _calculateNodeDensity(Terrain terrain, Vector3 position)
        {
            var heightCenter = terrain.SampleHeight(position);
            
            var heightX = terrain.SampleHeight(position + new Vector3(_spacing, 0, 0));
            var heightZ = terrain.SampleHeight(position + new Vector3(0, 0, _spacing));

            var slopeX = Mathf.Abs(heightX - heightCenter);
            var slopeZ = Mathf.Abs(heightZ - heightCenter);
            
            //* Check the validity of this formula
            return Mathf.Sqrt(slopeX * slopeX + slopeZ * slopeZ);
            
            //* Old formula
            // var slopeMagnitude = Mathf.Sqrt(slopeX * slopeX + slopeZ * slopeZ);
            //
            // return Mathf.Clamp(_spacing / (1f + slopeMagnitude * _densityMultiplier), _minSpacing, _maxSpacing);
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
                _showVoxel(position, Vector3.one);
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

            var p1 = new Node_3D(new Vector3(minX - padding, 0, minZ - padding));
            var p2 = new Node_3D(new Vector3(maxX + padding, 0, minZ - padding));
            var p3 = new Node_3D(new Vector3((minX + maxX) / 2f, 0, maxZ + padding));

            return new Node_Triangle(p1, p2, p3);
        }

        public void AddPoint(Vector3 position)
        {
            var newNode = new Node_3D(position);
            _nodes.Add(newNode.ID, newNode);

            // Apply Bowyer-Watson to insert the new point into the existing triangulation
            _performTriangulationUpdate(_nodes, newNode);
        }

        // This function handles both initial triangulation (Sweep Line) and incremental point insertion (Bowyer-Watson)
        public void _performTriangulationUpdate(Dictionary<ulong, Node_3D> points, Node_3D newNode = null)
        {
            var sortedPoints = points.OrderBy(p => p.Value.Position.x)
                .ToDictionary(p => p.Key, p => p.Value);

            Node_Triangle superNodeTriangle = null;
            var triangles = new Dictionary<ulong, Node_Triangle>();

            if (newNode == null)
            {
                superNodeTriangle = _createSuperTriangle(points);
                triangles.Add(superNodeTriangle.ID, superNodeTriangle);
            }

            var superTriangle = _generateVisibleTriangle(superNodeTriangle.Vertices);
            
            foreach (var vertex in superNodeTriangle.Vertices)
            {
                _showVoxel(
                    vertex.Position,
                    Vector3.one / 10,
                    superTriangle.transform,
                    Resources.GetBuiltinResource<Mesh>("Cube.fbx"),
                    new List<Material> { Resources.Load<Material>("Materials/Material_Green") },
                    0);
            }

            superTriangle.name = $"SuperTriangle";

            var pointsToProcess = newNode == null
                ? sortedPoints
                : new Dictionary<ulong, Node_3D> { { newNode.ID, newNode } };

            // Start the coroutine for the triangulation update process
            Manager_Game.S_Instance.StartCoroutine(PopulateCoroutine(pointsToProcess, triangles, superNodeTriangle,
                newNode));
        }

        IEnumerator PopulateCoroutine(Dictionary<ulong, Node_3D> pointsToProcess,
            Dictionary<ulong, Node_Triangle> triangles,
            Node_Triangle superNodeTriangle, Node_3D newNode)
        {
            List<Node_Triangle> badTriangles = new List<Node_Triangle>();

            // Callback function to handle adding bad triangles
            void callBack(bool isInside, Node_Triangle triangle)
            {
                if (isInside)
                {
                    badTriangles.Add(triangle);
                }
            }

            // Process each point
            foreach (var point in pointsToProcess)
            {
                // Check each triangle for being inside the circumcircle
                foreach (var triangle in triangles.Values)
                {
                    yield return Manager_Game.S_Instance.StartCoroutine(
                        triangle.IsPointInsideCircumcircle(point.Value.Position,
                            (isInside) => callBack(isInside, triangle)));
                    
                    yield return triangle.HideCircumcircleAfterTime(0.5f);
                }

                Debug.Log($"Bad Triangles: {badTriangles.Count}");
                
                // Process bad triangles
                foreach (var badTriangle in badTriangles)
                {
                    triangles.Remove(badTriangle.ID);
                }

                var boundaryEdges = new HashSet<Node_Edge>();

                // Create new triangles based on boundary edges
                foreach (var badTriangle in badTriangles)
                {
                    var vertices = badTriangle.Vertices;

                    for (var i = 0; i < vertices.Count; i++)
                    {
                        var edge = new Node_Edge(vertices[i], vertices[(i + 1) % 3]);
                        boundaryEdges.Add(edge);
                    }
                }

                // Add new triangles based on boundary edges
                foreach (var edge in boundaryEdges)
                {
                    var newTriangle = new Node_Triangle(edge.Start, edge.End, point.Value);
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
            Debug.Log(Triangles.Count);

            var visibleVoxelsParent = GameObject.Find("VisibleVoxels").transform;
            var green = Resources.Load<Material>("Materials/Material_Green");
            var red = Resources.Load<Material>("Materials/Material_Red");
            var blue = Resources.Load<Material>("Materials/Material_Blue");

            var materials = new List<Material> { green, red, blue };

            var colourIndex = 0;
            
            foreach (var triangle in Triangles)
            {
                _generateVisibleTriangle(triangle.Value.Vertices, visibleVoxelsParent, materials, colourIndex);
                colourIndex++;
            }

            initialsed = true;
        }

        static GameObject _generateVisibleTriangle(List<Node_3D> vertices, Transform parentTransform = null,
            List<Material> materials = null, int colourIndex = 0)
        {
            parentTransform ??= GameObject.Find("VisibleVoxels").transform;

            if (materials == null)
            {
                var green = Resources.Load<Material>("Materials/Material_Green");
                var red = Resources.Load<Material>("Materials/Material_Red");
                var blue = Resources.Load<Material>("Materials/Material_Blue");

                materials = new List<Material> { green, red, blue };
            }

            return _showTriangle(parentTransform, vertices, materials, colourIndex);
        }

        static GameObject _showTriangle(Transform parentTransform, List<Node_3D> vertices, List<Material> materials,
            int colourIndex)
        {
            if (vertices.Count != 3)
            {
                Debug.LogError("A triangle must have exactly 3 vertices.");
                return null;
            }

            var material = materials[colourIndex % materials.Count];

            var mesh = new Mesh();

            var triangleVertices = new Vector3[3];
            triangleVertices[0] = vertices[0].Position;
            triangleVertices[1] = vertices[1].Position;
            triangleVertices[2] = vertices[2].Position;

            var triangles = new int[3] { 0, 1, 2 };

            mesh.vertices = triangleVertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            var triangleGO = _testShowTriangle(parentTransform, triangleVertices[0], mesh, material);
            triangleGO.name = $"Triangle: {triangleVertices[0]}";

            return triangleGO;
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

        static void _showVisibleVoxels(List<Vector3> path)
        {
            var visibleVoxelsParent = GameObject.Find("VisibleVoxels").transform;
            var mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var green = Resources.Load<Material>("Materials/Material_Green");
            var red = Resources.Load<Material>("Materials/Material_Red");
            var blue = Resources.Load<Material>("Materials/Material_Blue");

            var materials = new List<Material> { green, red, blue };

            foreach (var point in path)
            {
                _showVoxel(point, Vector3.one / 10, visibleVoxelsParent, mesh, materials, 0);
            }
        }

        static void _showVoxel(Vector3 position, Vector3 size, Transform parentTransform = null, Mesh mesh = null,
            List<Material> materials = null, int colorIndex = 0)
        {
            parentTransform ??= GameObject.Find("VisibleVoxels").transform;
            mesh ??= Resources.GetBuiltinResource<Mesh>("Cube.fbx");

            if (materials == null)
            {
                var green = Resources.Load<Material>("Materials/Material_Green");
                var red = Resources.Load<Material>("Materials/Material_Red");
                var blue = Resources.Load<Material>("Materials/Material_Blue");

                materials = new List<Material> { green, red, blue };
            }

            var material = materials[colorIndex % materials.Count];

            var voxelGO = _testShowVoxel(parentTransform, position, mesh, material, size);
            voxelGO.name = $"Point: {position} - {size}";
        }

        static GameObject _testShowVoxel(Transform transform, Vector3 position, Mesh mesh, Material material,
            Vector3 size)
        {
            var voxelGO = new GameObject($"{position}");
            voxelGO.AddComponent<MeshFilter>().mesh = mesh;
            voxelGO.AddComponent<MeshRenderer>().material = material;
            voxelGO.transform.SetParent(transform);
            voxelGO.transform.localPosition = position;
            voxelGO.transform.localScale = size;
            return voxelGO;
        }
    }
}