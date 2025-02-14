using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Pathfinding
{
    public class Octree_Paths
    {
        public readonly Dictionary<Vector3, Voxel_Walkable> AllWalkableVoxels;

        public Octree_Paths(List<MoverType> moverTypes)
        {
            AllWalkableVoxels = new Dictionary<Vector3, Voxel_Walkable>();
            _convertToGraph(moverTypes);
        }

        void _convertToGraph(List<MoverType> moverTypes, Voxel_Base voxelBase = null)
        {
            voxelBase ??= Octree_Map.S_RootMapVoxel;
            
            if (!voxelBase.IsWalkable(moverTypes)) return;

            var voxelPath = new Voxel_Walkable(voxelBase.Position, voxelBase.Size);
            AllWalkableVoxels[voxelBase.Position] = voxelPath;

            if (voxelBase.Children == null)
            {
                AddNeighbors(voxelPath, voxelBase.Size, moverTypes);
            }
            else
            {
                foreach (var child in voxelBase.Children)
                {
                    _convertToGraph(moverTypes, child);
                }
            }
        }

        void AddNeighbors(Voxel_Walkable voxelWalkable, int size, List<MoverType> moverTypes)
        {
            var directions = _getMovementDirections(moverTypes);

            foreach (var dir in directions)
            {
                var neighborPos = voxelWalkable.Position + dir * size;
                if (AllWalkableVoxels.TryGetValue(neighborPos, out var neighbor))
                {
                    voxelWalkable.Neighbors.Add(neighbor);
                }
            }
        }

        static Vector3[] _getMovementDirections(List<MoverType> moverTypes)
        {
            var directions = new List<Vector3>();
            
            foreach (var moverType in moverTypes)
            {
                directions.AddRange(MoverPaths[moverType]);
            }
            
            return directions.ToArray();
        }

        public Voxel_Walkable GetClosestVoxel(Vector3 position)
        {
            return AllWalkableVoxels.OrderBy(n => Vector3.Distance(n.Key, position)).First().Value;
        }

        static Dictionary<MoverType, List<Vector3>> s_moverPaths;
        static Dictionary<MoverType, List<Vector3>> MoverPaths => s_moverPaths ??= _getMoverPaths(); 
        
        static Dictionary<MoverType, List<Vector3>> _getMoverPaths()
        {
            var cardinal_Horizontal = new List<Vector3>
            {
                Vector3.left, Vector3.right, Vector3.forward, Vector3.back
            };

            var diagonal_Horizontal = new List<Vector3>
            {
                Vector3.left + Vector3.forward, Vector3.left + Vector3.back, 
                Vector3.right + Vector3.forward, Vector3.right + Vector3.back
            };

            var cardinal_Vertical = new List<Vector3>
            {
                Vector3.up, Vector3.down, 
                Vector3.left + Vector3.up, Vector3.left + Vector3.down,
                Vector3.right + Vector3.up, Vector3.right + Vector3.down, 
                Vector3.forward + Vector3.up, Vector3.forward + Vector3.down, 
                Vector3.back + Vector3.up, Vector3.back + Vector3.down
            };

            var diagonal_Vertical = new List<Vector3>
            {
                Vector3.left + Vector3.forward + Vector3.up, Vector3.left + Vector3.back + Vector3.up,
                Vector3.right + Vector3.forward + Vector3.up, Vector3.right + Vector3.back + Vector3.up,
                Vector3.left + Vector3.forward + Vector3.down, Vector3.left + Vector3.back + Vector3.down,
                Vector3.right + Vector3.forward + Vector3.down, Vector3.right + Vector3.back + Vector3.down
            };

            var horizontal = cardinal_Horizontal.Concat(diagonal_Horizontal).ToList();
            var vertical = cardinal_Vertical.Concat(diagonal_Vertical).ToList();
            var omni = horizontal.Concat(vertical).ToList();
            
            return new Dictionary<MoverType, List<Vector3>>
            {
                { MoverType.Land, horizontal },
                { MoverType.Ice, horizontal },
                { MoverType.Lava, horizontal },
                
                { MoverType.Air, omni },
                { MoverType.Water, omni },
                { MoverType.Dig, omni }
            };
        }
    }
    
    public enum MoverType
    {
        None, 
        Land,
        Air,
        Water,
        Dig,
        Lava,
        Ice
    }
}