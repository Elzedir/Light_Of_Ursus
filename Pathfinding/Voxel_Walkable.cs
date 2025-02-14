using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Voxel_Walkable
    {
        public long VoxelID => 
            ((long)Position.x << 42) | 
            ((long)Position.y << 21) | 
            (long)Position.z;
        public Vector3 Position;
        public float Cost;
        public readonly List<Voxel_Walkable> Neighbors;

        public Voxel_Walkable(Vector3 position, float moveCost)
        {
            Position = position;
            Cost = moveCost;
            Neighbors = new List<Voxel_Walkable>();
        }
    }
}