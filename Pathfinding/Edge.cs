using System;
using UnityEngine;

namespace Pathfinding
{
    public class Edge// : IEquatable<Edge>
    {
        // public Vertex Vertex_1;
        // public Vertex Vertex_2;
        //
        // public bool IsIntersecting = false;
        //
        // public Edge(Vertex vertex_1, Vertex vertex_2)
        // {
        //     Vertex_1 = vertex_1;
        //     Vertex_2 = vertex_2;
        // }
        //
        // public Edge(Vector3 point_1, Vector3 point_2)
        // {
        //     Vertex_1 = new Vertex(point_1);
        //     Vertex_2 = new Vertex(point_2);
        // }
        //
        // public Vector2 GetVertex2D(Vertex v) => new Vector2(v.Position.x, v.Position.z);
        //
        // public void FlipEdge()
        // {
        //     (Vertex_1, Vertex_2) = (Vertex_2, Vertex_1);
        // }
        //
        // public bool Equals(Edge other)
        // {
        //     if (other == null)
        //         return false;
        //     
        //     return (Vertex_1.Position == other.Vertex_1.Position && Vertex_2.Position == other.Vertex_2.Position) ||
        //            (Vertex_1.Position == other.Vertex_2.Position && Vertex_2.Position == other.Vertex_1.Position);
        // }
        //
        // public override bool Equals(object obj)
        // {
        //     if (obj is Edge other)
        //         return Equals(other);
        //     return false;
        // }
        //
        // public override int GetHashCode()
        // {
        //     return Vertex_1.Position.GetHashCode() ^ Vertex_2.Position.GetHashCode();
        // }
    }
}