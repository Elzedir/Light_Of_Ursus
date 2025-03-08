using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding
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
                return false;
            }
            
            var circumcentre = _calculateCircumcentre(a, b, c);
            var distanceToPoint = Vector3.SqrMagnitude(circumcentre - point);
            var radiusMagnitude = Vector3.SqrMagnitude(circumcentre - a);
            var insideCircumcircle = distanceToPoint < radiusMagnitude;

            return insideCircumcircle;
        }
        
        public static bool ArePointsColinear(Vector3 a, Vector3 b, Vector3 c) => 
            Mathf.Abs((b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z)) < Mathf.Epsilon;

        static Vector3 _calculateCircumcentre(Vector3 a, Vector3 b, Vector3 c)
        {
            float ax = a.x, az = a.z;
            float bx = b.x, bz = b.z;
            float cx = c.x, cz = c.z;

            var d = 2 * (ax * (bz - cz) + bx * (cz - az) + cx * (az - bz));

            if (Mathf.Abs(d) < Mathf.Epsilon)
                return Vector3.zero;

            var x =
                ((ax * ax + az * az) * (bz - cz) + (bx * bx + bz * bz) * (cz - az) + (cx * cx + cz * cz) * (az - bz)) /
                d;
            var z =
                ((ax * ax + az * az) * (cx - bx) + (bx * bx + bz * bz) * (ax - cx) + (cx * cx + cz * cz) * (bx - ax)) /
                d;

            return new Vector3(x, 0, z);
        }

        float _calculateCircumradius() => Vector3.Distance(Circumcentre, A.Position);
        
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