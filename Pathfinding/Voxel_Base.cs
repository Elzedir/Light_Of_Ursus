using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Voxel_Base
    {
        public ulong ID => GetVoxelIDFromPosition(Position);

        public Vector3 Position;
        public readonly int Size;

        public Dictionary<MoverType, float> MoverTypeCosts;

        public Voxel_Base[] Children;
        
        public const float MinimumVoxelSize = 0.01f;

        public Voxel_Base(Vector3 position, int size, Dictionary<MoverType, float> moverTypeCosts)
        {
            Position = position;
            Size = size;
            MoverTypeCosts = moverTypeCosts;
        }

        public bool Subdivide()
        {
            if (Size <= MinimumVoxelSize || Children != null) return false;

            var halfSize = Size / 2;
            Children = new Voxel_Base[8];

            for (var i = 0; i < 8; i++)
            {
                var offset = Position + halfSize * new Vector3(
                    (i & 1) == 0 ? -0.5f : 0.5f,
                    (i & 2) == 0 ? -0.5f : 0.5f,
                    (i & 4) == 0 ? -0.5f : 0.5f 
                );
                
                Children[i] = new Voxel_Base(offset, halfSize, MoverTypeCosts);
            }
            
            Debug.Log($"Subdivided {Position} {Size} into {Children[0].Position} {Children[0].Size}");
            
            return true;
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
        
        public static ulong GetVoxelIDFromPosition(Vector3 position)
        {
            position.x = Mathf.Round(position.x / MinimumVoxelSize);
            position.y = Mathf.Round(position.y / MinimumVoxelSize);
            position.z = Mathf.Round(position.z / MinimumVoxelSize);
            
            var hash = _fnv1aHashComponent(position.x);
            hash = _fnv1aHashComponent(position.y) ^ hash;
            hash = _fnv1aHashComponent(position.z) ^ hash;
            
            Debug.Log($"Hash: {hash}");

            return hash;
        }
        
        static ulong _fnv1aHashComponent(float component)
        {
            var hash = 14695981039346656037UL;
            
            component = Mathf.Round(component / MinimumVoxelSize);
            
            var bytes = System.BitConverter.GetBytes(component);
            
            foreach (var byt in bytes)
            {
                hash ^= byt;
                hash *= 1099511628211UL;
            }
            
            return hash;
        }
    }
}