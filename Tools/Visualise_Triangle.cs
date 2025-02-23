using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public abstract class Visualise_Triangle
    {
        static Transform s_parent;
        public static Transform Parent => s_parent ??= _getParent();
        static Transform _getParent() => GameObject.Find("VisualiseTriangles").transform;
        
        static List<Material> s_materials;
        static List<Material> Materials => s_materials ??= new List<Material>
        {
            Resources.Load<Material>("Materials/Material_Green"),
            Resources.Load<Material>("Materials/Material_Red"),
            Resources.Load<Material>("Materials/Material_Blue")
        };
        
        public static GameObject Show_Triangle(Vector3[] vertices, int colourIndex = -1)
        {
            if (vertices.Length != 3)
            {
                Debug.LogError("A triangle must have exactly 3 vertices.");
                return null;
            }

            var triangleVertices = new[] { vertices[0], vertices[1], vertices[2] };
            
            var centroid = (triangleVertices[0] + triangleVertices[1] + triangleVertices[2]) / 3;

            var triangles = new[] { 0, 1, 2 };

            var mesh = new Mesh
            {
                vertices = triangleVertices,
                triangles = triangles
            };
            
            mesh.RecalculateNormals();
            
            var material = colourIndex != -1 
                ? Materials[colourIndex % Materials.Count]
                : Materials[0];

            var triangleGO = _create_Object(centroid, mesh, material);
            triangleGO.name = $"Triangle: {centroid}";

            return triangleGO;
        } 
        
        static GameObject _create_Object(Vector3 position, Mesh mesh, Material material)
        {
            var go = new GameObject($"{position}");
            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = material;
            go.transform.SetParent(Parent);
            return go;
        }
    }
}