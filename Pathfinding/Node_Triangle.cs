using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding
{
    public class Node_Triangle
    {
        ulong _id;

        public Transform Transform; 
        
        (Vector3, Vector3, Vector3) _id_Vector = (Vector3.zero, Vector3.zero, Vector3.zero);

        public readonly Vertex A, B, C;

        Vector3 _centroid, _circumcentre;
        float _circumradius;
        
        public Dictionary<MoverType, float> MoverCosts;
        
        public Vertex[] Vertices => new[] { A, B, C };
        
        public Edge_Half EdgeHalf;
        
        public ulong ID => _id != 0
        ? _id
        : _id = GetTriangleID(A.Position, B.Position, C.Position);
        
        public (Vector3, Vector3, Vector3) ID_Vectors => _id_Vector != (Vector3.zero, Vector3.zero, Vector3.zero)
            ? _id_Vector
            : _id_Vector = GetTriangleID_Vector(A.Position, B.Position, C.Position);
        
        public Vector3 Centroid => _centroid != Vector3.zero
            ? _centroid
            : _centroid = new Vector3(
                (A.Position.x + B.Position.x + C.Position.x) / 3,
                (A.Position.y + B.Position.y + C.Position.y) / 3,
                (A.Position.z + B.Position.z + C.Position.z) / 3
            );

        public static Vector3 GetCentroid(Vector3[] vertices)
        {
            return new Vector3(
                (vertices[0].x + vertices[1].x + vertices[2].x) / 3,
                (vertices[0].y + vertices[1].y + vertices[2].y) / 3,
                (vertices[0].z + vertices[1].z + vertices[2].z) / 3
            );
        }

        public Vector3 Circumcentre => _circumcentre != Vector3.zero
            ? _circumcentre
            : _circumcentre = _calculateCircumcentre(
                A.Position.x, A.Position.z, 
                B.Position.x, B.Position.z,
                C.Position.x, C.Position.z);
        public float Circumradius => _circumradius != 0
            ? _circumradius
            : _circumradius = _calculateCircumradius();
        
        public HashSet<Edge> Edges => new() { new Edge(A, B), new Edge(B, C), new Edge(C, A) };

        public Node_Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            A = new Vertex(a);
            B = new Vertex(b);
            C = new Vertex(c);
            
            if (Vector3.Dot(Vector3.Cross(B.Position - A.Position, C.Position - A.Position).normalized, Vector3.up) < 0)
            {
                (B, C) = (C, B);
            }
        }
        
        public bool IsPointInsideCircumcircle(Vector3 point)
        {
            var distanceToPoint = Vector3.SqrMagnitude(Circumcentre - point);
            var radiusMagnitude = Vector3.SqrMagnitude(Circumcentre - A.Position);
            var insideCircumcircle = distanceToPoint < radiusMagnitude;

            return insideCircumcircle;
        }
        
        public static bool IsPointInsideCircumcircle(Vector3 a, Vector3 b, Vector3 c, Vector3 point)
        {
            if (ArePointsColinear(a, b, c))
            {
                return false; // Ignore colinear points
            }
            
            var circumcentre = _calculateCircumcentre(a.x, a.z, b.x, b.z, c.x, c.z);
            var distanceToPoint = Vector3.SqrMagnitude(circumcentre - point);
            var radiusMagnitude = Vector3.SqrMagnitude(circumcentre - a);
            var insideCircumcircle = distanceToPoint < radiusMagnitude;

            return insideCircumcircle;
        }
        
        public static bool ArePointsColinear(Vector3 a, Vector3 b, Vector3 c)
        {
            // Calculate the area using the 2D cross product
            var area = (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
    
            // If area is close to zero, points are colinear
            return Mathf.Abs(area) < Mathf.Epsilon;
        }

        static Vector3 _calculateCircumcentre(float ax, float az, float bx, float bz, float cx, float cz)
        {
            var mx_Ab = (ax + bx) / 2;
            var mz_Ab = (az + bz) / 2;
            var mx_BC = (bx + cx) / 2;
            var mz_BC = (bz + cz) / 2;
            
            if (bx - ax == 0 || cx - bx == 0) return Vector3.zero;
            
            var slope_Ab = (bx - ax) == 0 
                ? float.MaxValue 
                : -(ax - bx) / (az - bz);
            var slope_BC = (cx - bx) == 0 
                ? float.MaxValue 
                : -(bx - cx) / (bz - cz);
            
            var x = (slope_Ab * mx_Ab - slope_BC * mx_BC + mz_BC - mz_Ab) / (slope_Ab - slope_BC);
            var z = slope_Ab * (x - mx_Ab) + mz_Ab;

            return new Vector3(x, 0, z);
        }
        
        float _calculateCircumradius() => Vector3.Distance(Circumcentre, A.Position);
        
        public Vertex GetThirdVertex(Vector3 point_1,Vector3 point_2)
        {
            foreach (var vertex in Vertices)
            {
                if (vertex.Position != point_1 && vertex.Position != point_2)
                {
                    return vertex;
                }
            }
            
            throw new Exception("Third vertex not found in triangle.");
        }
        
        public List<Node_Triangle> GetAdjacentTriangles(List<Node_Triangle> allTriangles)
        {
            var adjacentTriangles = new List<Node_Triangle>();
            
            foreach (var triangle in allTriangles)
            {
                if (triangle.ID_Vectors == ID_Vectors || GetSharedEdges(triangle).Count == 0) continue;

                adjacentTriangles.Add(triangle);
            }

            return adjacentTriangles;
        }
        
        public HashSet<Edge> GetSharedEdges(Node_Triangle otherTriangle)
        {
            var sharedEdges = new HashSet<Edge>();
                
            foreach (var edge in Edges)
            {
                if (otherTriangle.Edges.Contains(edge))
                    sharedEdges.Add(edge);
            }

            return sharedEdges;
        }

        public (Vector3, Vector3, Vector3) GetTriangleID_Vector(Vector3 a, Vector3 b, Vector3 c) => (a, b, c);

        public static ulong GetTriangleID(Vector3 a, Vector3 b, Vector3 c)
        {
            var hash = 14695981039346656037UL;
        
            hash = _fnv1aHashVector(a, hash);
            hash = _fnv1aHashVector(b, hash);
            hash = _fnv1aHashVector(c, hash);
        
            return hash;
        }
        
        static ulong _fnv1aHashVector(Vector3 vector, ulong hash)
        {
            hash = _fnv1aHashComponent(vector.x, hash);
            hash = _fnv1aHashComponent(vector.y, hash);
            hash = _fnv1aHashComponent(vector.z, hash);
        
            return hash;
        }
        
        static ulong _fnv1aHashComponent(float component, ulong hash)
        {
            var bytes = BitConverter.GetBytes(component);
        
            foreach (var byt in bytes)
            {
                hash ^= byt;
                hash *= 1099511628211UL;
            }
        
            return hash;
        }
    }
}