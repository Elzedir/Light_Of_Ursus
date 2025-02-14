using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Voxel_Path
    {
        public long NodeID => 
            ((long)Position.x << 42) | 
            ((long)Position.y << 21) | 
            (long)Position.z;
        public Vector3 Position;
        public float Cost;
        public readonly List<Voxel_Path> Neighbors;

        public Voxel_Path(Vector3 pos, float moveCost)
        {
            Position = pos;
            Cost = moveCost;
            Neighbors = new List<Voxel_Path>();
        }
    }
}