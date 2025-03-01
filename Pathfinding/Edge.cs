using UnityEngine;

namespace Pathfinding
{
    public class Edge
    {
        public Vertex Vertex_1;
        public Vertex Vertex_2;
        
        public bool IsIntersecting = false;

        public Edge(Vertex vertex_1, Vertex vertex_2)
        {
            Vertex_1 = vertex_1;
            Vertex_2 = vertex_2;
        }

        public Edge(Vector3 vertex_1, Vector3 vertex_2)
        {
            Vertex_1 = new Vertex(vertex_1);
            Vertex_2 = new Vertex(vertex_2);
        }
        
        public Vector2 GetVertex2D(Vertex v) => new Vector2(v.Position.x, v.Position.z);
        
        public void FlipEdge()
        {
            (Vertex_1, Vertex_2) = (Vertex_2, Vertex_1);
        }
    }
}