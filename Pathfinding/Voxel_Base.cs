using System.Collections.Generic;
using Terrains;
using UnityEngine;

namespace Pathfinding
{
    public class Voxel_Base
    {
        ulong _id;
        public ulong ID => _id != 0
            ? _id
            : _id = GetVoxelIDFromPosition(Position);

        public Vector3 Position;
        public readonly int Size;

        public Dictionary<MoverType, float> MoverTypeCosts;

        public Voxel_Base[] Children;
        
        //* Eventually allow down to 0.01f
        public const float MinimumVoxelSize = 5f;
        public int DominantTerrainType;

        public Voxel_Base(Vector3 position, int size, Dictionary<MoverType, float> moverTypeCosts)
        {
            Position = position;
            Size = size;
            MoverTypeCosts = moverTypeCosts ??= _getDefaultMoverTypeCosts();
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
                
                var moverTypeCosts = _getMovementCostsFromTerrain(offset);
                
                Children[i] = new Voxel_Base(offset, halfSize, moverTypeCosts);
            }
            
            return true;
        }

        Dictionary<MoverType, float> _getMovementCostsFromTerrain(Vector3 position)
        {
            var moverTypeCosts = new Dictionary<MoverType, float>();
            
            var terrainIndex = Terrain_Manager.GetTextureIndexAtPosition(position);
            DominantTerrainType = terrainIndex;
            
            switch (terrainIndex)
            {
                case (int)TerrainName.Grass: // Grass
                    moverTypeCosts[MoverType.Land] = 1f;
                    moverTypeCosts[MoverType.Air] = 1.5f;
                    moverTypeCosts[MoverType.Water] = float.PositiveInfinity;
                    break;
                case (int)TerrainName.Water: // Water
                    moverTypeCosts[MoverType.Land] = float.PositiveInfinity;
                    moverTypeCosts[MoverType.Air] = 1.5f; 
                    moverTypeCosts[MoverType.Water] = 1f;
                    break;
                case (int)TerrainName.Mud: // Mud
                    moverTypeCosts[MoverType.Land] = 3f;
                    moverTypeCosts[MoverType.Air] = 1.5f;
                    moverTypeCosts[MoverType.Water] = 2f;
                    break;
                default:
                    moverTypeCosts[MoverType.Land] = 1f;
                    moverTypeCosts[MoverType.Air] = 1.5f;
                    moverTypeCosts[MoverType.Water] = 1f;
                    break;
            }

            return moverTypeCosts;
        }

        static Dictionary<MoverType, float> _getDefaultMoverTypeCosts()
        {
            
            return new Dictionary<MoverType, float>
            {
                { MoverType.Land, 1 },
                { MoverType.Air, 1 },
                { MoverType.Water, 1 },
                { MoverType.Ice, 1 },
                { MoverType.Lava, -1 },
                { MoverType.Dig, 1 },
            };
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
            
            Debug.Log("Merged");
        }
        
        public static ulong GetVoxelIDFromPosition(Vector3 position)
        {
            position.x = Mathf.Round(position.x / MinimumVoxelSize);
            position.y = Mathf.Round(position.y / MinimumVoxelSize);
            position.z = Mathf.Round(position.z / MinimumVoxelSize);
            
            var hash = _fnv1aHashComponent(position.x);
            hash = _fnv1aHashComponent(position.y) ^ hash;
            hash = _fnv1aHashComponent(position.z) ^ hash;

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