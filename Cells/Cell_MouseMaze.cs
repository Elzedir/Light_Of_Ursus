using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Pathfinding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using z_Abandoned;

public enum Wall { None, Top, Bottom, Left, Right }

public class Cell_MouseMaze : Cell_Base
{
    public Node_Base_2D_Deprecated Node { get; private set; }

    public bool Visited;
    //bool _initialised = false;
    public TextMeshPro CellText;

    public List<Wall_MouseMaze> Walls { get; private set; } = new();

    Spawner_Maze _spawner;

    public void InitialiseCell(Vector3 position, Spawner_Maze spawner)
    {
        Position = position;
        _spawner = spawner;

        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        Material materialFloor = Resources.Load<Material>("Materials/Material_White");
        Material materialWalls = Resources.Load<Material>("Materials/Material_Black");

        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshFilter.mesh = mesh;
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = materialFloor;

        if (Position.y != 0)
        {
            _createWalls(mesh, materialWalls);

            BoxCollider coll = gameObject.AddComponent<BoxCollider>();
            coll.size = new Vector3(0.75f, 0.75f, 0.75f);
            coll.isTrigger = true;
        }

        //GameObject textGO = new GameObject();
        //textGO.transform.parent = transform;
        //textGO.transform.localPosition = Vector3.zero;
        //CellText = textGO.AddComponent<TextMeshPro>();
        //CellText.text = $"{Position}";
        //CellText.alignment = TextAlignmentOptions.Center;
        //CellText.sortingLayerID = -967159649;
        //CellText.fontSize = 3; CellText.color = Color.black;
    }

    public void RecreateWalls(List<Wall> walls, Mesh mesh, Material material)
    {
        for(int i = 0; i < Walls.Count; i++)
        {
            ClearWall(Walls[i].Wall);
        }

        foreach(Wall wall in walls)
        {
            if (wall == Wall.Top)
            {
                Walls.Add(new GameObject("Wall_Top").AddComponent<Wall_MouseMaze>().CreateWall(Wall.Top, mesh, material, this.transform));
            }
            if (wall == Wall.Bottom)
            {
                Walls.Add(new GameObject("Wall_Bottom").AddComponent<Wall_MouseMaze>().CreateWall(Wall.Bottom, mesh, material, this.transform));
            }
            if (wall == Wall.Right)
            {
                Walls.Add(new GameObject("Wall_Right").AddComponent<Wall_MouseMaze>().CreateWall(Wall.Right, mesh, material, this.transform));
            }
            if (wall == Wall.Left)
            {
                Walls.Add(new GameObject("Wall_Left").AddComponent<Wall_MouseMaze>().CreateWall(Wall.Left, mesh, material, this.transform));
            }
        }
    }

    void _createWalls(Mesh mesh, Material material)
    {
        Walls.Add(new GameObject("Wall_Top").AddComponent<Wall_MouseMaze>().CreateWall(Wall.Top, mesh, material, this.transform));
        Walls.Add(new GameObject("Wall_Bottom").AddComponent<Wall_MouseMaze>().CreateWall(Wall.Bottom, mesh, material, this.transform));
        Walls.Add(new GameObject("Wall_Right").AddComponent<Wall_MouseMaze>().CreateWall(Wall.Right, mesh, material, this.transform));
        Walls.Add(new GameObject("Wall_Left").AddComponent<Wall_MouseMaze>().CreateWall(Wall.Left, mesh, material, this.transform));
    }

    public Vector3? ClearWall(Wall wall, bool initialisation = false)
    {
        Wall_MouseMaze toRemove = null;

        foreach (Wall_MouseMaze side in Walls)
        {
            if (side.Wall == wall)
            {
                toRemove = side;
                break;
            }
        }

        if (!toRemove) return null;

        Vector3? wallPosition = null;

        if (!initialisation) { wallPosition = toRemove.transform.position; }
        Walls.Remove(toRemove);
        Destroy(toRemove.gameObject);
        return wallPosition;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Focus") _spawner.RefreshMaze(this);

        else if (collider.gameObject.name.StartsWith("Chaser_"))
        {
            collider.TryGetComponent<Chaser>(out Chaser chaser);

            chaser.CurrentCell = this;
            chaser.UpdateChaserPath();
        }
    }

    public override void Show()
    {
        if (_spawner.Background) _meshRenderer.enabled = false;
    }

    public override void Hide()
    {
        if (_spawner.Background) _meshRenderer.enabled = true;
    }

    public override void MarkCell(Material material)
    {
        base.MarkCell(material);

        foreach (Wall_MouseMaze wall in Walls)
        {
            wall.Mark(material);
        }
    }
}

public class Wall_MouseMaze : MonoBehaviour
{
    public Wall Wall;
    public BoxCollider BoxCollider;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

    public Wall_MouseMaze CreateWall(Wall wall, Mesh mesh, Material material, Transform parent)
    {
        Wall = wall;
        MeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshFilter.mesh = mesh;
        MeshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshRenderer.material = material;
        transform.parent = parent;

        BoxCollider = gameObject.AddComponent<BoxCollider>();

        gameObject.layer = LayerMask.NameToLayer("Wall");

        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        rigidbody.freezeRotation = true;

        switch (Wall)
        {
            case Wall.Top: transform.localPosition = new Vector3(0, 0, 0.5f); gameObject.transform.localScale = new Vector3(1f, 1f, 0.1f); break;
            case Wall.Bottom: transform.localPosition = new Vector3(0, 0, -0.5f); gameObject.transform.localScale = new Vector3(1f, 1f, 0.1f); break;
            case Wall.Right: transform.localPosition = new Vector3(0.5f, 0, 0); gameObject.transform.localScale = new Vector3(0.1f, 1f, 1f); break;
            case Wall.Left: transform.localPosition = new Vector3(-0.5f, 0, 0); gameObject.transform.localScale = new Vector3(0.1f, 1f, 1f); break;
            default: break;
        }

        return this;
    }

    public void Mark(Material material)
    {
        MeshRenderer.material = material;
    }
}
