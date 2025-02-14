using UnityEngine;

namespace Pathfinding.FlowPath
{
    public class Voxel_Base
    {
        public long NodeID => 
            ((long)Position.x << 42) | 
            ((long)Position.y << 21) | 
            (long)Position.z;
        public Vector3 Position;
        public readonly int Size;
        public bool IsWalkable;
        public Voxel_Base[] Children;

        public Voxel_Base(Vector3 pos, int s, bool walkable)
        {
            Position = pos;
            Size = s;
            IsWalkable = walkable;
            Children = null;
        }

        public void Subdivide()
        {
            if (Children != null) return;

            var newSize = Size / 2;
            Children = new Voxel_Base[8];

            for (var i = 0; i < 8; i++)
            {
                var newPos = Position + new Vector3(
                    (i & 1) * newSize,
                    ((i >> 1) & 1) * newSize,
                    ((i >> 2) & 1) * newSize);
                Children[i] = new Voxel_Base(newPos, newSize, IsWalkable);
            }
        }

        public void Merge()
        {
            if (Children == null) return; // Already merged
            bool allSame = true;
            bool firstState = Children[0].IsWalkable;

            foreach (var child in Children)
            {
                if (child.IsWalkable != firstState)
                {
                    allSame = false;
                    break;
                }
            }

            if (allSame)
            {
                IsWalkable = firstState;
                Children = null; // Merge back into a single voxel
            }
        }
    }
}