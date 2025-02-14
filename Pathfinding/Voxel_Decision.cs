using System;

namespace Pathfinding
{
    public class Voxel_Decision : IComparable<Voxel_Decision>
    {
        public long VoxelID => Voxel_Walkable.VoxelID;
        public readonly Voxel_Walkable Voxel_Walkable;
        public float GCost, RHSCost, Heuristic;

        public Voxel_Decision(Voxel_Walkable voxelWalkable, float gCost, float rhsCost, float heuristic)
        {
            Voxel_Walkable = voxelWalkable;
            GCost = gCost;
            RHSCost = rhsCost;
            Heuristic = heuristic;
        }

        public int CompareTo(Voxel_Decision other)
        {
            var thisKey = Math.Min(GCost, RHSCost) + Heuristic;
            var otherKey = Math.Min(other.GCost, other.RHSCost) + other.Heuristic;
            return thisKey.CompareTo(otherKey);
        }
    }
}