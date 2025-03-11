using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Terrains
{
    public class TerrainQuadtree
{
    private class Node
    {
        public Rect Bounds;
        public int MaterialIndex;
        public bool IsLeaf;
        public Node[] Children;

        public Node(Rect bounds)
        {
            Bounds = bounds;
            IsLeaf = true;
            Children = new Node[4];
        }
    }

    private Node _root;
    private int _maxDepth;
    private float _minSize;

    public TerrainQuadtree(Rect bounds, int maxDepth, float minSize)
    {
        _root = new Node(bounds);
        _maxDepth = maxDepth;
        _minSize = minSize;
    }

    public void Subdivide(Terrain terrain, int depth = 0)
    {
        SubdivideNode(_root, terrain, depth);
    }

    private void SubdivideNode(Node node, Terrain terrain, int depth)
    {
        if (depth >= _maxDepth || node.Bounds.width <= _minSize)
            return;

        Vector3 center = node.Bounds.center;
        int materialAtCenter = GetMaterialIndex(center, terrain);
        int materialAtCorners = CheckCorners(node.Bounds, terrain);

        if (materialAtCenter == materialAtCorners)
        {
            node.MaterialIndex = materialAtCenter;
            node.IsLeaf = true;
        }
        else
        {
            node.IsLeaf = false;
            SplitNode(node);

            foreach (var child in node.Children)
            {
                SubdivideNode(child, terrain, depth + 1);
            }
        }
    }

    private void SplitNode(Node node)
    {
        float halfWidth = node.Bounds.width / 2f;
        float halfHeight = node.Bounds.height / 2f;

        node.Children[0] = new Node(new Rect(node.Bounds.x, node.Bounds.y, halfWidth, halfHeight));
        node.Children[1] = new Node(new Rect(node.Bounds.x + halfWidth, node.Bounds.y, halfWidth, halfHeight));
        node.Children[2] = new Node(new Rect(node.Bounds.x, node.Bounds.y + halfHeight, halfWidth, halfHeight));
        node.Children[3] = new Node(new Rect(node.Bounds.x + halfWidth, node.Bounds.y + halfHeight, halfWidth, halfHeight));
    }

    private int GetMaterialIndex(Vector3 position, Terrain terrain)
    {
        return Terrain_Manager.GetTextureIndexAtPosition(position);
    }

    private int CheckCorners(Rect bounds, Terrain terrain)
    {
        Vector3 bl = new Vector3(bounds.xMin, 0, bounds.yMin);
        Vector3 br = new Vector3(bounds.xMax, 0, bounds.yMin);
        Vector3 tl = new Vector3(bounds.xMin, 0, bounds.yMax);
        Vector3 tr = new Vector3(bounds.xMax, 0, bounds.yMax);

        HashSet<int> materials = new HashSet<int>
        {
            GetMaterialIndex(bl, terrain),
            GetMaterialIndex(br, terrain),
            GetMaterialIndex(tl, terrain),
            GetMaterialIndex(tr, terrain)
        };

        return materials.Count > 1 ? -1 : materials.FirstOrDefault();
    }

    public List<Vector3> CollectPoints()
    {
        List<Vector3> points = new List<Vector3>();
        CollectLeafCenters(_root, points);
        return points;
    }

    private void CollectLeafCenters(Node node, List<Vector3> points)
    {
        if (node.IsLeaf)
        {
            points.Add(new Vector3(node.Bounds.center.x, 0, node.Bounds.center.y));
        }
        else
        {
            foreach (var child in node.Children)
            {
                CollectLeafCenters(child, points);
            }
        }
    }
}

// Usage Example:
// TerrainQuadtree quadtree = new TerrainQuadtree(new Rect(0, 0, terrainData.size.x, terrainData.size.z), 6, 1f);
// quadtree.Subdivide(terrain);
// List<Vector3> points = quadtree.CollectPoints();
// Use these points in your Delaunay triangulation! 


// Let me know if you’d like me to refine this or integrate directly with the navmesh generator! 🚀 
}