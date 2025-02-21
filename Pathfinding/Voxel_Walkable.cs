using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Voxel_Walkable
    {
        ulong _id;
        public ulong ID => _id != 0 
            ? _id
            : _id = _getVoxelIDFromPosition(Position);
        
        public Vector3 Position;
        public float Cost;
        public readonly List<Voxel_Walkable> Neighbors;

        public Voxel_Walkable(Vector3 position, float moveCost)
        {
            Position = position;
            Cost = moveCost;
            Neighbors = new List<Voxel_Walkable>();
        }

        static ulong _getVoxelIDFromPosition(Vector3 position)
        {
            position.x = Mathf.Round(position.x / Voxel_Base.MinimumVoxelSize);
            position.y = Mathf.Round(position.y / Voxel_Base.MinimumVoxelSize);
            position.z = Mathf.Round(position.z / Voxel_Base.MinimumVoxelSize);
            
            var hash = _fnv1aHashComponent(position.x);
            hash = _fnv1aHashComponent(position.y) ^ hash;
            hash = _fnv1aHashComponent(position.z) ^ hash;

            return hash;
        }
        
        static ulong _fnv1aHashComponent(float component)
        {
            var hash = 14695981039346656037UL;
            
            component = Mathf.Round(component / Voxel_Base.MinimumVoxelSize);
            
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