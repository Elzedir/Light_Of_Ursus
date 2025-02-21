using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    public abstract class Pathfinding_Manager
    {
        static readonly Graph_World _graph_World = new();
        static readonly Grid_Node _grid_Node = new();
        static readonly Graph_NavMesh _graph_NavMesh = new();

        public static List<Vector3> GetPath(Vector3 start, Vector3 end, HashSet<MoverType> moverTypes)
        {
            var worldPath = _graph_World.FindShortestPath(start, end);
            
            if (worldPath == null || worldPath.Count == 0)
                return null;
            
            _showVisibleVoxels(worldPath);
            
            if (worldPath.Count != 1) //* && if (!withinPlayerRenderRange) 
                return worldPath;
            
            var localStart = worldPath.Last();

            var localPath = moverTypes.Contains(MoverType.Air) || moverTypes.Contains(MoverType.Dig)
                ? _grid_Node.FindShortestPath(localStart, end)
                : _graph_NavMesh.FindShortestPath(localStart, end);

            foreach (var point in localPath)
                Debug.Log($"Local Path: {point}");
            
            _showVisibleVoxels(localPath);

            return localPath;

            //* Instead of running DStarLite from start to end, instead run it from individual node to node, so it's limited
            //* in size per character. Also, pass this path through to each character, and their individual DStarLte pathfinders
            //* will navigate their small circles around them.
        }

        static void _showVisibleVoxels(List<Vector3> path)
        {
            var visibleVoxelsParent = GameObject.Find("VisibleVoxels").transform;
            var mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var green = Resources.Load<Material>("Materials/Material_Green");
            var red = Resources.Load<Material>("Materials/Material_Red");
            var blue = Resources.Load<Material>("Materials/Material_Blue");
            
            var materials = new List<Material> { green, red, blue };
            
            foreach (var point in path)
            {
                _showVoxel(visibleVoxelsParent, point, mesh, materials, Vector3.one, 0);
            }
        }

        static void _showVoxel(Transform parentTransform, Vector3 position, Mesh mesh, List<Material> materials, Vector3 size, int colorIndex)
        {
            var material = materials[colorIndex % materials.Count]; 
            
            var voxelGO = _testShowVoxel(parentTransform, position, mesh, material, size);
            voxelGO.name = $"Merged: {position} - {size}";
        }

        static GameObject _testShowVoxel(Transform transform, Vector3 position ,Mesh mesh, Material material, Vector3 size)
        {
            var voxelGO = new GameObject($"{position}");
            voxelGO.AddComponent<MeshFilter>().mesh = mesh;
            voxelGO.AddComponent<MeshRenderer>().material = material;
            voxelGO.transform.SetParent(transform);
            voxelGO.transform.localPosition = position;
            voxelGO.transform.localScale = size;
            return voxelGO;
        }
    }
}