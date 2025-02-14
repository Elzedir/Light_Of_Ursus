using UnityEngine;

namespace Pathfinding
{
    public class Node_Base
    {
        public long NodeID => ((long)(uint)Position.x << 32) | (uint)Position.y;
        
        public readonly Vector2Int Position;
        public Node_Base Parent;
        public float GCost, HeuristicCost;
        public float TotalCost => GCost + HeuristicCost;

        public Node_Base(Vector2Int position)
        {
            Position = position;
            GCost = float.MaxValue;
        }
    }
}