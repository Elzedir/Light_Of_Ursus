using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public abstract class Visualise_Circle
    {
        static Transform s_parent;
        public static Transform Parent => s_parent ??= _getParent();
        static Transform _getParent() => GameObject.Find("VisualiseCircles").transform;
        
        static List<Material> s_materials;
        static List<Material> Materials => s_materials ??= new List<Material>
        {
            Resources.Load<Material>("Materials/Material_Green"),
            Resources.Load<Material>("Materials/Material_Red"),
            Resources.Load<Material>("Materials/Material_Blue")
        };
        
        public static GameObject Show_Circle(Vector3 position, float radius, int segments = 32, int colourIndex = -1)
        {
            var material = colourIndex != -1 
                ? Materials[colourIndex % Materials.Count]
                : Materials[0];
    
            var vertices = new List<Vector3> { Vector3.zero };
            var triangles = new List<int>();

            for (var i = 0; i <= segments; i++)
            {
                var angle = i * Mathf.PI * 2 / segments;
                var x = Mathf.Cos(angle) * radius;
                var z = Mathf.Sin(angle) * radius;
        
                vertices.Add(new Vector3(x, 0, z));

                if (i <= 0) continue;
                
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i);
            }
            
            triangles.Add(0);
            triangles.Add(segments);
            triangles.Add(1);
            
            Mesh mesh = new()
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
    
            var circleGO = _create_Object(position, mesh, material);
            circleGO.name = $"Circle: {position} - {radius}";

            return circleGO;
        }
        
        static GameObject _create_Object(Vector3 position, Mesh mesh, Material material)
        {
            var go = new GameObject($"{position}");
            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = material;
            go.transform.SetParent(Parent);
            go.transform.localPosition = position;
            return go;
        }
    }
}