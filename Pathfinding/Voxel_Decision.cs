using System;
using UnityEngine;

namespace Pathfinding
{
    public class Voxel_Decision : IComparable<Voxel_Decision>
    {
        public ulong ID => Voxel_Walkable.ID;

        public bool IsApproximatelyEqualCost => Mathf.Abs(GCost - RHSCost) < 1f * Voxel_Base.MinimumVoxelSize;
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