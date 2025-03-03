using UnityEngine;

namespace Pathfinding
{
    public class Vertex
    {
        public Vector3 Position;
        
        public Half_Edge HalfEdge;

        public Vertex(Vector3 position)
        {
            Position = position;
        }
        
        public Vector2 GetPos2D_XZ() => new Vector2(Position.x, Position.z);
    }
}