using System.Collections.Generic;
using System.Linq;
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

            s_rootMapVoxel = new Voxel_Base(Vector3.zero, _nextPowerOfTwo((int)worldSize * 2), null);

            _setUpdateVoxelCosts();

            return s_rootMapVoxel;
        }
        
        static int _nextPowerOfTwo(int value)
        {
            if (value <= 1) return 1;
            
            return (int)Mathf.Pow(2, Mathf.Ceil(Mathf.Log(value, 2)));
        }

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
            var parentTransform = GameObject.Find("VisibleVoxels").transform;
            var mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var green = Resources.Load<Material>("Materials/Material_Green");
            var red = Resources.Load<Material>("Materials/Material_Red");
            var blue = Resources.Load<Material>("Materials/Material_Blue");
            
            var queue = new Queue<Voxel_Base>();
            queue.Enqueue(S_RootMapVoxel);

            var iteration = 0;
            while (queue.Count > 0 && iteration++ < 10000)
            {
                var voxel = queue.Dequeue();
                
                if (!_hasMultipleTerrainsInVoxel(voxel)) continue;
                
                if (!voxel.Subdivide()) continue;
                
                foreach (var child in voxel.Children)
                    queue.Enqueue(child);
            }
            
            _attemptMerge(S_RootMapVoxel);
            
            var materials = new List<Material> { blue, green, red };

            _showMergedVoxels(parentTransform, S_RootMapVoxel, mesh, materials,  Vector3.one * S_RootMapVoxel.Size);
        }
        
        //* See if you can combine into custom shapes instead of purely octree based shaped. Maybe a matrix?

        static void _showMergedVoxels(Transform transform, Voxel_Base voxel, Mesh mesh, List<Material> materials, Vector3 size)
        {
            if (voxel.DominantTerrainType == -1) return;
            
            var material = materials[voxel.DominantTerrainType % materials.Count];
            
            Debug.Log(material);
            
            if (voxel.Children == null)
            {
                var mergedVoxel = _testShowVoxel(transform, voxel.Position, mesh, material, size);
                mergedVoxel.name = $"Merged: {voxel.Position} - {voxel.Size}";
                return;
            }

            foreach (var child in voxel.Children)
                _showMergedVoxels(transform, child, mesh, materials, Vector3.one * child.Size);
        }
        
        static void _mergeVoxels(Voxel_Base voxel)
        {
            if (voxel.Children == null) return;

            foreach (var child in voxel.Children)
                _mergeVoxels(child);

            voxel.Merge();
        }
        
        static GameObject _testShowVoxel(Transform transform, Vector3 position, Mesh mesh, Material material, Vector3 size)
        {
            var voxelGO = new GameObject($"{position}");
            voxelGO.AddComponent<MeshFilter>().mesh = mesh;
            voxelGO.AddComponent<MeshRenderer>().material = material;
            voxelGO.transform.SetParent(transform);
            voxelGO.transform.localPosition = position;
            voxelGO.transform.localScale = size;
            return voxelGO;
        }
        
        static bool _hasMultipleTerrainsInVoxel(Voxel_Base voxel)
        {
            var terrainTypes = new HashSet<int>();
            
            var sampleResolution = Mathf.CeilToInt(voxel.Size * 0.5f);
            sampleResolution = Mathf.Max(sampleResolution, 8);
            var stepSize = voxel.Size / (float)sampleResolution;

            var minPos = voxel.Position - Vector3.one * (voxel.Size * 0.5f);

            for (var x = minPos.x; x <= minPos.x + voxel.Size; x += stepSize)
            {
                for (var z = minPos.z; z <= minPos.z + voxel.Size; z += stepSize)
                {
                    var terrainIndex = TerrainManager.GetTextureIndexAtPosition(new Vector3(x, 0, z));
                    if (terrainIndex != -1) terrainTypes.Add(terrainIndex);

                    if (terrainTypes.Count > 1) return true;
                }
            }

            return false;
        }
        
        static void _attemptMerge(Voxel_Base voxel)
        {
            if (voxel.Children == null) return;

            foreach (var child in voxel.Children)
                _attemptMerge(child);

            voxel.Merge();
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
    }
}