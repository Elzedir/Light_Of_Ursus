using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Voxel_Base
    {
        public long VoxelID =>
            ((long)Position.x << 42) |
            ((long)Position.y << 21) |
            (long)Position.z;

        public Vector3 Position;
        public readonly int Size;

        public Dictionary<MoverType, float> MoverTypeCosts;

        public Voxel_Base[] Children;

        public Voxel_Base(Vector3 position, int size, Dictionary<MoverType, float> moverTypeCosts)
        {
            Position = position;
            Size = size;
            MoverTypeCosts = moverTypeCosts;
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
                Children[i] = new Voxel_Base(newPos, newSize, MoverTypeCosts);
            }
        }

        public bool IsWalkable(List<MoverType> movers)
        {
            foreach (var mover in movers)
            {
                if (MoverTypeCosts[mover] >= float.PositiveInfinity)
                {
                    return false;
                }
            }

            return true;
        }

        public void Merge()
        {
            if (Children == null) return;

            var firstChildCosts = Children[0].MoverTypeCosts;
            var allSame = true;

            foreach (var child in Children)
            {
                foreach (var mover in firstChildCosts.Keys)
                {
                    if (!child.MoverTypeCosts.TryGetValue(mover, out var cost))
                    {
                        Debug.LogError($"Child is missing a mover type {mover}");
                        allSame = false;
                        break;
                    }

                    if (Mathf.Approximately(cost, firstChildCosts[mover])) continue;
                    
                    allSame = false;
                    break;
                }

                if (!allSame) break;
            }

            if (!allSame) return;

            MoverTypeCosts = new Dictionary<MoverType, float>(firstChildCosts);
            Children = null;
        }
    }
}