using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CustomMesh { Arrow, Pyramid }
public static class Manager_Mesh
{
    public static Mesh GenerateArrow(float stemLength = 2, float stemWidth = 0.5f, float tipLength = 1f, float tipWidth = 1f)
    {
        Mesh mesh = new Mesh();

        Vector3 stemOrigin = Vector3.zero;
        float stemHalfWidth = stemWidth / 2f;
        float tipHalfWidth = tipWidth / 2;

        Vector3[] vertices = new Vector3[]
        {
        stemOrigin + (stemHalfWidth * Vector3.down),
        stemOrigin + (stemHalfWidth * Vector3.up),
        stemOrigin + (stemHalfWidth * Vector3.down) + (stemLength * Vector3.right),
        stemOrigin + (stemHalfWidth * Vector3.up) + (stemLength * Vector3.right),
        stemOrigin + (stemLength * Vector3.right) + (tipHalfWidth * Vector3.up),
        stemOrigin + (stemLength * Vector3.right) + (tipHalfWidth * Vector3.down),
        stemOrigin + (stemLength * Vector3.right) + (tipLength * Vector3.right)
        };

        int[] triangles = new int[]
        {
        0, 1, 3,
        0, 3, 2,
        4, 6, 5,
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    public static Mesh GeneratePyramid(float baseWidth = 1f, float baseDepth = 1f, float tipHeight = 1f)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
        new Vector3(0, tipHeight, 0),
        new Vector3(-baseWidth / 2, 0, baseDepth / 2),
        new Vector3(baseWidth / 2, 0, baseDepth / 2),
        new Vector3(baseWidth / 2, 0, -baseDepth / 2),
        new Vector3(-baseWidth / 2, 0, -baseDepth / 2)
        };

        int[] triangles = new int[]
        {
        0, 1, 2,
        0, 2, 3,
        0, 3, 4,
        0, 4, 1,
        1, 4, 3,
        1, 3, 2
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
