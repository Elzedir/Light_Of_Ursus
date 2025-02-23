using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public abstract class Visualise_Voxel
    {
        static Transform s_parent;
        public static Transform Parent => s_parent ??= _getParent();
        static Transform _getParent() => GameObject.Find("VisualiseVoxels").transform;
        
        static List<Material> s_materials;
        static List<Material> Materials => s_materials ??= new List<Material>
        {
            Resources.Load<Material>("Materials/Material_Green"),
            Resources.Load<Material>("Materials/Material_Red"),
            Resources.Load<Material>("Materials/Material_Blue")
        };
        
        public static GameObject Show_Voxel(Vector3 position, Vector3 size, int colourIndex = -1, Quaternion rotation = default)
        {
            var material = colourIndex != -1 
                ? Materials[colourIndex % Materials.Count]
                : Materials[0];

            Mesh mesh = new();
            
            var cubeVertices = new[]
            {
                new Vector3(-0.5f * size.x, -0.5f * size.y, -0.5f * size.z),
                new Vector3(-0.5f * size.x, -0.5f * size.y, 0.5f * size.z),
                new Vector3(-0.5f * size.x, 0.5f * size.y, -0.5f * size.z),
                new Vector3(-0.5f * size.x, 0.5f * size.y, 0.5f * size.z),
                new Vector3(0.5f * size.x, -0.5f * size.y, -0.5f * size.z),
                new Vector3(0.5f * size.x, -0.5f * size.y, 0.5f * size.z),
                new Vector3(0.5f * size.x, 0.5f * size.y, -0.5f * size.z),
                new Vector3(0.5f * size.x, 0.5f * size.y, 0.5f * size.z)
            };
            
            var triangles = new[]
            {
                0, 1, 2,
                2, 1, 3,
                2, 3, 6,
                6, 3, 7,
                6, 7, 4,
                4, 7, 5,
                4, 5, 0,
                0, 5, 1,
                1, 5, 3,
                3, 5, 7,
                0, 2, 4,
                4, 2, 6
            };
            
            mesh.vertices = cubeVertices;
            mesh.triangles = triangles;
            
            mesh.RecalculateNormals();

            var voxelGO = _create_Object(position, rotation, mesh, material);
            voxelGO.name = $"Voxel: {position} - {size}";

            return voxelGO;
        }
        
        static GameObject _create_Object(Vector3 position, Quaternion rotation, Mesh mesh, Material material)
        {
            var go = new GameObject($"{position}");
            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = material;
            go.transform.SetParent(Parent);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            return go;
        }
    }
}