using System.Collections.Generic;
using Terrains;
using UnityEngine;

namespace Pathfinding
{
    public abstract class Octree_Map
    {
        static Voxel_Base s_rootMapVoxel;
        public static Voxel_Base S_RootMapVoxel => s_rootMapVoxel ??= InitialiseOctree_Map(); 
        static Dictionary<int, Octree_Path> s_allPaths;
        //* Find a way to correct initialise DefaultMoverTypeCosts. Currently, all just 1.
        public static Dictionary<int, Octree_Path> S_AllPaths => s_allPaths ??= new Dictionary<int, Octree_Path>();

        public static Voxel_Base InitialiseOctree_Map()
        {
            var terrain = Terrain.activeTerrain;
            if (terrain is null) throw new System.Exception("No terrain found.");

            var terrainSize = terrain.terrainData.size;
            var worldSize = Mathf.Max(terrainSize.x, terrainSize.z);
            var baseVoxelSize = worldSize;

            s_rootMapVoxel = new Voxel_Base(Vector3.zero, (int)worldSize, DefaultMoverTypeCosts);

            _setUpdateVoxelCosts();

            return s_rootMapVoxel;
        }
        
        
            //* Root voxel is subdividing adn then remerging beceause there are no obstacles. Try and get the terrain
            //* height from the terrain componnent and set the voxel walkability based on that.

        public static Octree_Path GetOrCreatePath(List<MoverType> moverTypes)
        {
            var key = _generateKey(moverTypes);

            if (S_AllPaths.TryGetValue(key, out var octreePath)) return octreePath;

            return S_AllPaths[key] = new Octree_Path(moverTypes);
        }

        static int _generateKey(List<MoverType> moverTypes)
        {
            var key = 0;
            
            foreach (var mover in moverTypes)
            {
                key |= 1 << (int)mover;
            }
            
            return key;
        }

        static void _setUpdateVoxelCosts()
        {
            var queue = new Queue<Voxel_Base>();
            queue.Enqueue(S_RootMapVoxel);

            while (queue.Count > 0)
            {
                var voxel = queue.Dequeue();
                var position = voxel.Position;
                var terrainHeight = TerrainManager.GetTerrainHeight(position);
                var textureIndex = TerrainManager.GetTextureIndexAtPosition(position);

                var movementCost = GetMovementCost(textureIndex);

                voxel.MoverTypeCosts[MoverType.Land] = movementCost;
                voxel.MoverTypeCosts[MoverType.Air] = 1;

                if (!voxel.Subdivide()) continue;

                foreach (var child in voxel.Children)
                    queue.Enqueue(child);
            }
        }

        static float GetMovementCost(int textureIndex)
        {
            switch (textureIndex)
            {
                case 0: return 1f;  // Grass
                case 1: return 1.5f; // Sand
                case 2: return 2f;  // Mud
                case 3: return float.PositiveInfinity; // Lava (unwalkable)
                default: return 1f;
            }
        }
        
        //* Call in a foreach on first terrain generation to allocate all voxel movement costs.
        //* Then, also call whenever the terrain changes.
        static void _setIsWalkable(Voxel_Base voxel_Base, Vector3 position, int size, MoverType moverType, bool walkable, float cost = 1f)
        {
            while (true)
            {
                if (size == 1 || voxel_Base.Children == null)
                {
                    voxel_Base.MoverTypeCosts[moverType] = walkable ? cost : float.PositiveInfinity;
                    return;
                }
                
                var index = _getChildIndex(position, voxel_Base, size);

                voxel_Base.Children[index].Subdivide();
                voxel_Base = voxel_Base.Children[index];
                size /= 2;
            }
        }
        
        static int _getChildIndex(Vector3 position, Voxel_Base voxel_Base, int size)
        {
            var halfSize = size >> 1;
            
            return (position.x >= voxel_Base.Position.x + halfSize ? 1 : 0) |
                   (position.y >= voxel_Base.Position.y + halfSize ? 2 : 0) |
                   (position.z >= voxel_Base.Position.z + halfSize ? 4 : 0);
        }

        public bool IsWalkable(Vector3 position, List<MoverType> moverTypes) => 
            GetVoxelAtPosition(S_RootMapVoxel, position, S_RootMapVoxel.Size).IsWalkable(moverTypes);

        public static Voxel_Base GetVoxelAtPosition(Voxel_Base rootVoxel, Vector3 pos, int size)
        {
            while (true)
            {
                if (rootVoxel.Children == null) return rootVoxel;

                var index = _getChildIndex(pos, rootVoxel, size);

                rootVoxel = rootVoxel.Children[index];
                size /= 2;
            }
        }

        static Dictionary<MoverType, float> s_defaultMoverTypeCosts;
        public static Dictionary<MoverType, float> DefaultMoverTypeCosts => s_defaultMoverTypeCosts ??= _getDefaultMoverTypeCosts();

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
    }
}