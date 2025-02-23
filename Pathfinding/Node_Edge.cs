using UnityEngine;

namespace Pathfinding
{
    public class Node_Edge
    {
        public readonly Vector3 Start;
        public readonly Vector3 End;

        public Node_Edge(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }
    }
}