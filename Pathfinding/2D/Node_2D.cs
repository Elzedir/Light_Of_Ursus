using UnityEngine;

namespace Pathfinding._2D
{
    public class Node_2D
    {
        public ulong NodeID =>
            (((ulong)(Position.x + 2_147_483_648) & 0xFFFFFFFF) << 32) |
            ((ulong)(Position.y + 2_147_483_648) & 0xFFFFFFFF);
        
        public readonly Vector2Int Position;
        public Node_2D Parent;
        public float GCost, HeuristicCost;
        public float TotalCost => GCost + HeuristicCost;

        public Node_2D(Vector2Int position)
        {
            Position = position;
            GCost = float.PositiveInfinity;
        }

        public static ulong GetNodeIDFromPosition(Vector2Int position)
        {
            return (((ulong)(position.x + 2_147_483_648) & 0xFFFFFFFF) << 32) |
                   ((ulong)(position.y + 2_147_483_648) & 0xFFFFFFFF);
        }
    }
}