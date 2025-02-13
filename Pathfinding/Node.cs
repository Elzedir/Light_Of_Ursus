using UnityEngine;

namespace Pathfinding
{
    public class Node
    {
        public long NodeID => ((long)(uint)Position.x << 32) | (uint)Position.y;
        
        public readonly Vector2Int Position;
        public Node Parent;
        public float GCost, HeuristicCost;
        public float TotalCost => GCost + HeuristicCost;

        public Node(Vector2Int position)
        {
            Position = position;
            GCost = float.MaxValue;
        }
    }
}