using UnityEngine;

namespace Pathfinding
{
    a
    //* rename it, node is 2D, but this voxel will apply to AStart and Dijkstra only since it will have 
    //* GCost, H, and FCost, instead of RHS.
    public class Node_3D
    {
        public ulong NodeID => GetVoxelIDFromPosition(Position);

        public readonly Vector3 Position;
        public Node_3D Parent;
        public float GCost, HeuristicCost;
        public float TotalCost => GCost + HeuristicCost;

        public Node_3D(Vector3 position)
        {
            Position = position;
            GCost = float.PositiveInfinity;
        }
        
        public static ulong GetVoxelIDFromPosition(Vector3 position)
        {
            position.x = Mathf.Round(position.x / MinimumVoxelSize);
            position.y = Mathf.Round(position.y / MinimumVoxelSize);
            position.z = Mathf.Round(position.z / MinimumVoxelSize);
            
            var hash = _fnv1aHashComponent(position.x);
            hash = _fnv1aHashComponent(position.y) ^ hash;
            hash = _fnv1aHashComponent(position.z) ^ hash;

            return hash;
        }
        
        static ulong _fnv1aHashComponent(float component)
        {
            var hash = 14695981039346656037UL;
            
            component = Mathf.Round(component / MinimumVoxelSize);
            
            var bytes = System.BitConverter.GetBytes(component);
            
            foreach (var byt in bytes)
            {
                hash ^= byt;
                hash *= 1099511628211UL;
            }
            
            return hash;
        }
    }
}