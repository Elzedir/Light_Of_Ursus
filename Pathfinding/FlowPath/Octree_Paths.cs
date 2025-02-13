using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding.FlowPath
{
    public class Octree_Paths
    {
        public readonly Dictionary<Vector3, Voxel_Path> PathNodes;

        public Octree_Paths(Octree_Map octreeMap)
        {
            PathNodes = new Dictionary<Vector3, Voxel_Path>();
            ConvertToGraph(octreeMap.Root);
        }

        void ConvertToGraph(Voxel_Base voxelBase)
        {
            if (voxelBase is not { IsWalkable: true }) return;

            var voxelPath = new Voxel_Path(voxelBase.Position, voxelBase.Size);
            PathNodes[voxelBase.Position] = voxelPath;

            if (voxelBase.Children == null)
            {
                AddNeighbors(voxelPath, voxelBase.Size);
            }
            else
            {
                foreach (var child in voxelBase.Children)
                {
                    ConvertToGraph(child);
                }
            }
        }

        void AddNeighbors(Voxel_Path voxelPath, int size)
        {
            Vector3[] directions =
            {
                Vector3.left, Vector3.right,
                Vector3.forward, Vector3.back,
                Vector3.up, Vector3.down
            };

            foreach (var dir in directions)
            {
                Vector3 neighborPos = voxelPath.Position + dir * size;
                if (PathNodes.TryGetValue(neighborPos, out var neighbor))
                {
                    voxelPath.Neighbors.Add(neighbor);
                }
            }
        }

        public Voxel_Path GetClosestNode(Vector3 position)
        {
            return PathNodes.OrderBy(n => Vector3.Distance(n.Key, position)).First().Value;
        }
    }
}