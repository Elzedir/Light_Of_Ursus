using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding.NavMesh
{
    public class Node_Triangle
    {
        ulong _id;
        
        (Vector3, Vector3, Vector3) _id_Vector = (Vector3.zero, Vector3.zero, Vector3.zero);

        public readonly Vertex A, B, C;

        Vector3 _centroid, _circumcentre;
        float _circumradius;
        
        public Dictionary<MoverType, float> MoverCosts;
        
        public Vertex[] Vertices => new[] { A, B, C };
        
        public Half_Edge Half_Edge;
        
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

        public Vector3 Circumcentre => _circumcentre != Vector3.zero
            ? _circumcentre
            : _circumcentre = _calculateCircumcentre(A.Position, B.Position, C.Position);
        public float Circumradius => _circumradius != 0
            ? _circumradius
            : _circumradius = _calculateCircumradius();

        public Node_Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            var vertices = new[] { new Vertex(a), new Vertex(b), new Vertex(c) };
            Array.Sort(vertices,
                (v1, v2) => !Mathf.Approximately(v1.Position.x, v2.Position.x)
                    ? v1.Position.x.CompareTo(v2.Position.x)
                    : v1.Position.z.CompareTo(v2.Position.z));

            A = vertices[0];
            B = vertices[1];
            C = vertices[2];
            
            if (Vector3.Dot(Vector3.Cross(B.Position - A.Position, C.Position - A.Position).normalized, Vector3.up) < 0)
            {
                (B, C) = (C, B);
            }
            
            var firstHalfEdge = new Half_Edge(A);
            var secondHalfEdge = new Half_Edge(B);
            var thirdHalfEdge = new Half_Edge(C);
            
            firstHalfEdge.Next = secondHalfEdge;
            secondHalfEdge.Next = thirdHalfEdge;
            thirdHalfEdge.Next = firstHalfEdge;
    
            firstHalfEdge.Previous = thirdHalfEdge;
            secondHalfEdge.Previous = firstHalfEdge;
            thirdHalfEdge.Previous = secondHalfEdge;
            
            firstHalfEdge.Triangle = secondHalfEdge.Triangle = thirdHalfEdge.Triangle = this;
            
            A.HalfEdge = B.HalfEdge = C.HalfEdge = thirdHalfEdge;
            Half_Edge = firstHalfEdge;
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
                Debug.Log($"Points {a}, {b}, {c} are colinear, no circumcircle exists!");
                return false;
            }
            
            var circumcentre = _calculateCircumcentre(a, b, c);
            var distanceToPoint = Vector3.SqrMagnitude(circumcentre - point);
            var radiusMagnitude = Vector3.SqrMagnitude(circumcentre - a);
            var insideCircumcircle = distanceToPoint < radiusMagnitude;

            return insideCircumcircle;
        }
        
        public static bool IsPointInsideTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            var v0 = c - a;
            var v1 = b - a;
            var v2 = p - a;

            var dot00 = Vector3.Dot(v0, v0);
            var dot01 = Vector3.Dot(v0, v1);
            var dot02 = Vector3.Dot(v0, v2);
            var dot11 = Vector3.Dot(v1, v1);
            var dot12 = Vector3.Dot(v1, v2);

            var inverseDenominator = 1 / (dot00 * dot11 - dot01 * dot01);
            
            var u = (dot11 * dot02 - dot01 * dot12) * inverseDenominator;
            var v = (dot00 * dot12 - dot01 * dot02) * inverseDenominator;

            return u >= 0 && v >= 0 && u + v <= 1;
        }

        public static bool ArePointsColinear(Vector3 a, Vector3 b, Vector3 c) => 
            Vector3.Cross(b - a, c - a).sqrMagnitude < 1e-6f;

        static Vector3 _calculateCircumcentre(Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = b - a;
            var ac = c - a;
            
            if (Vector3.Cross(ab, ac).sqrMagnitude < 1e-6f)
            {
                Debug.LogError("Points are colinear or nearly colinear — no valid circumcircle!");
                return Vector3.zero;
            }
            
            var abMid = (a + b) * 0.5f;
            var acMid = (a + c) * 0.5f;
            
            var triangleNormal = Vector3.Cross(ab, ac).normalized;
            var abNormal = Vector3.Cross(ab, triangleNormal).normalized;
            var acNormal = Vector3.Cross(ac, triangleNormal).normalized;
            
            var circumcentre = _lineIntersection(abMid, abMid + abNormal, acMid, acMid + acNormal);
            return circumcentre;
        }
        
        static Vector3 _lineIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            var p13 = p1 - p3;
            var p43 = p4 - p3;
            var p21 = p2 - p1;

            var d1343 = Vector3.Dot(p13, p43);
            var d4321 = Vector3.Dot(p43, p21);
            var d1321 = Vector3.Dot(p13, p21);
            var d4343 = Vector3.Dot(p43, p43);
            var d2121 = Vector3.Dot(p21, p21);

            var denominator = d2121 * d4343 - d4321 * d4321;
            if (Mathf.Abs(denominator) < 1e-6f) return Vector3.zero;

            var numerator = d1343 * d4321 - d1321 * d4343;
            var mua = numerator / denominator;
            return p1 + mua * p21;
        }
        
        float _calculateCircumradius() => Vector3.Distance(Circumcentre, A.Position);
        
        public bool SharesVertex(Node_Triangle other)
        {
            return Vector3.SqrMagnitude(A.Position - other.A.Position) < 0.0000001f ||
                   Vector3.SqrMagnitude(A.Position - other.B.Position) < 0.0000001f ||
                   Vector3.SqrMagnitude(A.Position - other.C.Position) < 0.0000001f ||
                   Vector3.SqrMagnitude(B.Position - other.A.Position) < 0.0000001f ||
                   Vector3.SqrMagnitude(B.Position - other.B.Position) < 0.0000001f ||
                   Vector3.SqrMagnitude(B.Position - other.C.Position) < 0.0000001f ||
                   Vector3.SqrMagnitude(C.Position - other.A.Position) < 0.0000001f ||
                   Vector3.SqrMagnitude(C.Position - other.B.Position) < 0.0000001f ||
                   Vector3.SqrMagnitude(C.Position - other.C.Position) < 0.0000001f;
        }

        public static bool SharesEdge(Node_Triangle triangle, Half_Edge edge)
        {
            var triangleVertices = (triangle.A.Position, triangle.B.Position, triangle.C.Position);
            var edgeVertices = (edge.Vertex.Position, edge.Next.Vertex.Position);
            
            return SharesEdge(triangleVertices, edgeVertices);
        }
        
        public static bool SharesEdge((Vector3, Vector3, Vector3) triangle, (Vector3, Vector3) edge)
        {
            return Vector3.SqrMagnitude(triangle.Item1 - edge.Item1) < 0.0000001f &&
                   Vector3.SqrMagnitude(triangle.Item2 - edge.Item2) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item1 - edge.Item2) < 0.0000001f &&
                   Vector3.SqrMagnitude(triangle.Item2 - edge.Item1) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item2 - edge.Item1) < 0.0000001f &&
                   Vector3.SqrMagnitude(triangle.Item3 - edge.Item2) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item2 - edge.Item2) < 0.0000001f &&
                   Vector3.SqrMagnitude(triangle.Item3 - edge.Item1) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item3 - edge.Item1) < 0.0000001f &&
                   Vector3.SqrMagnitude(triangle.Item1 - edge.Item2) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item3 - edge.Item2) < 0.0000001f &&
                   Vector3.SqrMagnitude(triangle.Item1 - edge.Item1) < 0.0000001f;
        }
        
        public static bool SharesVertexPosition((Vector3, Vector3, Vector3) triangle, (Vector3, Vector3, Vector3) other)
        {
            return Vector3.SqrMagnitude(triangle.Item1 - other.Item1) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item1 - other.Item2) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item1 - other.Item3) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item2 - other.Item1) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item2 - other.Item2) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item2 - other.Item3) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item3 - other.Item1) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item3 - other.Item2) < 0.0000001f ||
                   Vector3.SqrMagnitude(triangle.Item3 - other.Item3) < 0.0000001f;
        }
        
        public List<Node_Triangle> GetAdjacentTriangles()
        {
            var adjacentTriangles = new List<Node_Triangle>();

            addOppositeTriangle(Half_Edge);
            addOppositeTriangle(Half_Edge.Next);
            addOppositeTriangle(Half_Edge.Previous);

            return adjacentTriangles;

            void addOppositeTriangle(Half_Edge edge)
            {
                var oppositeTriangle = edge.Opposite?.Triangle;
                
                if (oppositeTriangle != null) adjacentTriangles.Add(oppositeTriangle);
            }
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