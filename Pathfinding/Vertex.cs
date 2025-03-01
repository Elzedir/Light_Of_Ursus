using UnityEngine;

namespace Pathfinding
{
    public class Vertex
    {
        public Vector3 Position;
        
        public Edge_Half EdgeHalf;
        
        public Node_Triangle Triangle;
        
        public Vertex PreviousVertex;
        public Vertex NextVertex;

        public bool IsReflex; 
        public bool IsConvex;

        public Vertex(Vector3 position)
        {
            Position = position;
        }
        
        public Vector2 GetPos2D_XZ() => new Vector2(Position.x, Position.z);
    }
}