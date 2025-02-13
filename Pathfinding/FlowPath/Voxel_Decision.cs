using System;

namespace Pathfinding.FlowPath
{
    public class Voxel_Decision : IComparable<Voxel_Decision>
    {
        public long NodeID => Voxel_Path.NodeID;
        public readonly Voxel_Path Voxel_Path;
        public float GCost, RHSCost, Heuristic;

        public Voxel_Decision(Voxel_Path voxelPath, float gCost, float rhsCost, float heuristic)
        {
            Voxel_Path = voxelPath;
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