using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Octree_Map
    {
        public static Voxel_Base S_RootMapVoxel;
        public Dictionary<int, Octree_Path> AllPaths;
        
        a
            //* Reform this class around the having the different paths to access, check the last 
            //* chat with ChatGPT

        public Octree_Map(int worldSize)
        {
            //* Find a way to correct initialise DefaultMoverTypeCosts. Currently, all just 1.
            
            AllPaths = new Dictionary<int, Octree_Path>();
            
            S_RootMapVoxel = new Voxel_Base(Vector3.zero, worldSize, DefaultMoverTypeCosts);
        }
        
        public Octree_Path GetOrCreatePath(List<MoverType> moverTypes)
        {
            int key = _generateKey(moverTypes);

            if (AllPaths.TryGetValue(key, out var octreePath)) return octreePath;
            
            octreePath = new Octree_Path(moverTypes);
            AllPaths[key] = octreePath;

            return octreePath;
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

        static void _setIsWalkable(Voxel_Base voxel_Base, Vector3 position, int size, MoverType moverType, bool walkable, float cost = 1f)
        {
            while (true)
            {
                if (size == 1 || voxel_Base.Children == null)
                {
                    //* Change not walkable cost to Positive.Infinity.
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

        public bool IsWalkable(Vector3 pos, List<MoverType> movers) => 
            GetVoxel(S_RootMapVoxel, pos, S_RootMapVoxel.Size).IsWalkable(movers);

        public Voxel_Base GetVoxel(Voxel_Base rootVoxel, Vector3 pos, int size)
        {
            while (true)
            {
                if (rootVoxel.Children == null) return rootVoxel;

                var index = _getChildIndex(pos, rootVoxel, size);

                rootVoxel = rootVoxel.Children[index];
                size /= 2;
            }
        }

        Dictionary<MoverType, float> _defaultMoverTypeCosts;
        public Dictionary<MoverType, float> DefaultMoverTypeCosts => _defaultMoverTypeCosts ??= _getDefaultMoverTypeCosts();

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