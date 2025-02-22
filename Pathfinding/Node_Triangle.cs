using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pathfinding
{
    public class Node_Triangle
    {
        ulong _id;
        public ulong ID => _id != 0
        ? _id
        : _id = _getTriangleID();
        readonly Node_3D _a, _b, _c;
        public List<Node_3D> Vertices => new() { _a, _b, _c };

        public Dictionary<MoverType, float> MoverCosts;

        public Node_Triangle(Node_3D a, Node_3D b, Node_3D c)
        {
            _a = a;
            _b = b;
            _c = c;

            var edge1 = b.Position - a.Position;
            var edge2 = c.Position - a.Position;
            var normal = Vector3.Cross(edge1, edge2).normalized;

            if (normal.y >= 0) return;

            (b, c) = (c, b);

            _b = b;
            _c = c;
        }
        
        public void SetMoverCosts(Dictionary<MoverType, float> costs)
        {
            MoverCosts = costs;
        }

        public Vector3 Centroid
        {
            get
            {
                var aPos = _a.Position;
                var bPos = _b.Position;
                var cPos = _c.Position;
                
                return new Vector3(
                    (aPos.x + bPos.x + cPos.x) / 3,
                    (aPos.y + bPos.y + cPos.y) / 3,
                    (aPos.z + bPos.z + cPos.z) / 3
                );
            }
        }
        
        LineRenderer _circumcircleRenderer;
        
        public void InitializeLineRenderer(GameObject parentObject)
        {
            _circumcircleRenderer ??= parentObject.GetComponent<LineRenderer>();
        }
        
        public IEnumerator IsPointInsideCircumcircle(Vector3 point, Action<bool> callback)
        {
            var a = _a.Position;
            var b = _b.Position;
            var c = _c.Position;
            
            float ax = a.x, az = a.z;
            float bx = b.x, bz = b.z;
            float cx = c.x, cz = c.z;
            float px = point.x, pz = point.z;

            var d = (ax - px) * (bz - cz) - (bx - cx) * (az - pz);
            var e = (ax * ax - px * px + az * az - pz * pz) * (bz - cz) -
                    (bx * bx - cx * cx + bz * bz - cz * cz) * (az - pz) +
                    (cx * cx - px * px + cz * cz - pz * pz) * (az - bz);

            InitializeLineRenderer(GameObject.Find("LineRenderer"));
    
            yield return Manager_Game.S_Instance.StartCoroutine(VisualizeCircumcircle(_circumcircleRenderer, 0.5f));

            callback(e > 0);
        }

        
        public IEnumerator VisualizeCircumcircle(LineRenderer lineRenderer, float duration)
        {
            var circumcenter = CalculateCircumcenter();
            
            if (circumcenter == Vector3.zero)
            {
                Debug.LogError("Circumcenter is undefined, circumcircle cannot be visualized.");
                yield break;
            }
            
            var radius = CalculateCircumradius(circumcenter);

            _drawCircumcircle(lineRenderer, circumcenter, radius);
            
            yield return new WaitForSeconds(duration);
        }
        
        public Vector3 CalculateCircumcenter()
        {
            var a = _a.Position;
            var b = _b.Position;
            var c = _c.Position;
            
            _generateVisibleTriangle(new List<Node_3D> { _a, _b, _c });

            Debug.Log($"a.x: {a.x}, a.z: {a.z} | b.x: {b.x}, b.z: {b.z} | c.x: {c.x}, c.z: {c.z}");

            // Use x and z values for the circumcenter calculation
            float D = 2 * (a.x * (b.z - c.z) + b.x * (c.z - a.z) + c.x * (a.z - b.z));

            if (Mathf.Approximately(D, 0f))
            {
                Debug.LogError($"Points are collinear, circumcenter is undefined. D: {D}");
                return Vector3.zero; // Handle this case as needed
            }

            float Ux = ((a.x * a.x + a.z * a.z) * (b.z - c.z) + (b.x * b.x + b.z * b.z) * (c.z - a.z) +
                        (c.x * c.x + c.z * c.z) * (a.z - b.z)) / D;
            float Uz = ((a.x * a.x + a.z * a.z) * (c.x - b.x) + (b.x * b.x + b.z * b.z) * (a.x - c.x) +
                        (c.x * c.x + c.z * c.z) * (b.x - a.x)) / D;

            Debug.Log($"Ux: {Ux}, Uz: {Uz}");
            
            Debug.Log($"A: {a}, B: {b}, C: {c}");
            Debug.Log($"Circumcenter: {new Vector3(Ux, 0, Uz)}");

            return new Vector3(Ux, 0, Uz); // Set y to 0 for ground plane
        }

        public float CalculateCircumradius(Vector3 circumcenter)
        {
            Debug.Log($"Circumcenter: {circumcenter}");
            Debug.Log($"A: {_a.Position}");
            Debug.Log($"Distance: {Vector3.Distance(circumcenter, _a.Position)}");
            return Vector3.Distance(circumcenter, _a.Position);
        }
        
        const int _segments = 100;
        const float _angleStep = 360f / _segments;

        void _drawCircumcircle(LineRenderer lineRenderer, Vector3 center, float radius)
        {
            lineRenderer.positionCount = _segments + 1;
            for (var i = 0; i < _segments; i++)
            {
                var angle = i * _angleStep * Mathf.Deg2Rad;
                var position = new Vector3(center.x + radius * Mathf.Cos(angle), center.y, center.z + radius * Mathf.Sin(angle));
                lineRenderer.SetPosition(i, position);
            }

            lineRenderer.SetPosition(_segments, lineRenderer.GetPosition(0));
        }

        public IEnumerator HideCircumcircleAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            _circumcircleRenderer.positionCount = 0;
        }

        public List<Node_Edge> GetEdges()
        {
            return new List<Node_Edge>
            {
                new(_a, _b),
                new(_b, _c),
                new(_c, _a)
            };
        }
        
        public List<Node_Triangle> GetAdjacentTriangles(Dictionary<ulong, Node_Triangle> allTriangles)
        {
            var adjacentTriangles = new List<Node_Triangle>();

            // Iterate through all triangles to check for shared edges
            foreach (var triangle in allTriangles.Values)
            {
                if (triangle != this) // Don't compare the triangle with itself
                {
                    var commonEdges = 0;

                    foreach (var edge in GetEdges())
                    {
                        if (triangle.GetEdges().Contains(edge))
                        {
                            commonEdges++;
                        }
                    }

                    if (commonEdges > 0) // If one or more edges are shared, it's an adjacent triangle
                    {
                        adjacentTriangles.Add(triangle);
                    }
                }
            }

            return adjacentTriangles;
        }
        
        ulong _getTriangleID()
        {
            var hash = 14695981039346656037UL;
            
            hash = _fnv1aHashComponent(_a.ID, hash);
            hash = _fnv1aHashComponent(_b.ID, hash);
            hash = _fnv1aHashComponent(_c.ID, hash);

            return hash;
        }
        
        static ulong _fnv1aHashComponent(ulong component, ulong hash)
        {
            var bytes = System.BitConverter.GetBytes(component);
            
            foreach (var byt in bytes)
            {
                hash ^= byt;
                hash *= 1099511628211UL;
            }
            
            return hash;
        }
        
        static GameObject _generateVisibleTriangle(List<Node_3D> vertices, Transform parentTransform = null,
            List<Material> materials = null, int colorIndex = 0)
        {
            parentTransform ??= GameObject.Find("VisibleVoxels").transform;

            if (materials == null)
            {
                var green = Resources.Load<Material>("Materials/Material_Green");
                var red = Resources.Load<Material>("Materials/Material_Red");
                var blue = Resources.Load<Material>("Materials/Material_Blue");

                materials = new List<Material> { green, red, blue };
            }

            return _showTriangle(parentTransform, vertices, materials, Random.Range(0, materials.Count - 1));
        }

        static GameObject _showTriangle(Transform parentTransform, List<Node_3D> vertices, List<Material> materials,
            int colorIndex)
        {
            if (vertices.Count != 3)
            {
                Debug.LogError("A triangle must have exactly 3 vertices.");
                return null;
            }

            var material = materials[colorIndex % materials.Count];

            var mesh = new Mesh();

            var triangleVertices = new Vector3[3];
            triangleVertices[0] = vertices[0].Position;
            triangleVertices[1] = vertices[1].Position;
            triangleVertices[2] = vertices[2].Position;

            if (GameObject.Find($"Triangle: {triangleVertices[0]}") is not null) return null;
            
            _showVoxel(triangleVertices[0], Vector3.one, parentTransform, mesh, materials, colorIndex);
            _showVoxel(triangleVertices[1], Vector3.one, parentTransform, mesh, materials, colorIndex);
            _showVoxel(triangleVertices[2], Vector3.one, parentTransform, mesh, materials, colorIndex);

            var triangles = new int[3] { 0, 1, 2 };

            mesh.vertices = triangleVertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            var triangleGO = _testShowTriangle(parentTransform, triangleVertices[0], mesh, material);
            triangleGO.name = $"Triangle: {triangleVertices[0]}";

            return triangleGO;
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
    }
}