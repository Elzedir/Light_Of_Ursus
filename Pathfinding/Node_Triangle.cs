using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Node_Triangle
    {
        ulong _id;
        public ulong ID => _id != 0
        ? _id
        : _id = _getTriangleID();
        readonly Node_3D _a, _b, _c;
        public List<Node_3D> GetVertices() => new() { _a, _b, _c };

        public Node_Triangle(Node_3D a, Node_3D b, Node_3D c)
        {
            _a = a;
            _b = b;
            _c = c;
        }
        
        public Vector3 Centroid
        {
            get
            {
                // Return the average of the three vertices' positions
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

        // Check if a point is inside the circumcircle of this triangle
        public bool IsPointInsideCircumcircle(Vector3 point)
        {
            // Extract triangle vertex coordinates
            var a = _a.Position;
            var b = _b.Position;
            var c = _c.Position;

            // Calculate the determinant matrix components
            float ax = a.x, ay = a.y, bx = b.x, by = b.y, cx = c.x, cy = c.y;
            float px = point.x, py = point.y;

            var d = (ax - px) * (by - cy) - (bx - cx) * (ay - py);
            var e = (ax * ax - px * px + ay * ay - py * py) * (by - cy) -
                    (bx * bx - cx * cx + by * by - cy * cy) * (ay - py) +
                    (cx * cx - px * px + cy * cy - py * py) * (ay - by);

            // If the determinant is > 0, the point is inside the circumcircle
            return e > 0;
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
    }
}