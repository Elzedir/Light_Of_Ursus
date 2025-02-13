using UnityEngine;

namespace Pathfinding.FlowPath
{
    public class Octree_Map
    {
        public readonly Voxel_Base Root;

        public Octree_Map(int worldSize)
        {
            Root = new Voxel_Base(Vector3.zero, worldSize, true);
        }

        public void InsertObstacle(Vector3 pos)
        {
            _setIsWalkable(Root, pos, Root.Size, false);
        }

        public void RemoveObstacle(Vector3 pos)
        {
            _setIsWalkable(Root, pos, Root.Size, true);
            Root.Merge();
        }

        void _setIsWalkable(Voxel_Base baseVoxel, Vector3 pos, int size, bool walkable)
        {
            if (size == 1 || baseVoxel.Children == null)
            {
                baseVoxel.IsWalkable = walkable;
                return;
            }

            var index = (pos.x >= baseVoxel.Position.x + size / 2 ? 1 : 0) +
                        (pos.y >= baseVoxel.Position.y + size / 2 ? 2 : 0) +
                        (pos.z >= baseVoxel.Position.z + size / 2 ? 4 : 0);

            baseVoxel.Children[index].Subdivide();
            _setIsWalkable(baseVoxel.Children[index], pos, size / 2, walkable);
        }

        public bool IsWalkable(Vector3 pos)
        {
            return _getNode(Root, pos, Root.Size).IsWalkable;
        }

        Voxel_Base _getNode(Voxel_Base voxelBase, Vector3 pos, int size)
        {
            if (voxelBase.Children == null) return voxelBase;

            var index = (pos.x >= voxelBase.Position.x + size / 2 ? 1 : 0) +
                        (pos.y >= voxelBase.Position.y + size / 2 ? 2 : 0) +
                        (pos.z >= voxelBase.Position.z + size / 2 ? 4 : 0);

            return _getNode(voxelBase.Children[index], pos, size / 2);
        }
    }
}