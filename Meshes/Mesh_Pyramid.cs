using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mesh_Pyramid : MonoBehaviour
{
    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        Mesh pyramidMesh = CreatePyramidMesh();

        meshFilter.mesh = pyramidMesh;
        meshCollider.sharedMesh = pyramidMesh;
    }

    Mesh CreatePyramidMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, 1, 0),   // Top vertex
            new Vector3(-1, 0, 1),  // Bottom front-left
            new Vector3(1, 0, 1),   // Bottom front-right
            new Vector3(1, 0, -1),  // Bottom back-right
            new Vector3(-1, 0, -1)  // Bottom back-left
        };

        int[] triangles = new int[]
        {
            0, 1, 2,  // Front face
            0, 2, 3,  // Right face
            0, 3, 4,  // Back face
            0, 4, 1,  // Left face
            1, 4, 3,  // Bottom face (part 1)
            1, 3, 2   // Bottom face (part 2)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
