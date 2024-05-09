using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner_XOY : MonoBehaviour
{
    int _numberOfXoys;
    public Material[] XoyMaterials { get; private set; }
    List<XOY> _xoys = new();

    public Transform XParent {  get; private set; }
    public Transform OParent {  get; private set; }
    public Transform YParent {  get; private set; }

    [SerializeField] int _width = 10;
    [SerializeField] int _height = 10;
    [SerializeField] int _depth = 1;
    [SerializeField] float spacing = 1.0f;

    void Start()
    {
        Material X = Resources.Load<Material>("Meshes/Material_Yellow");
        Material O = Resources.Load<Material>("Meshes/Material_Red");
        Material Y = Resources.Load<Material>("Meshes/Material_Blue");

        XoyMaterials = new Material[] { X, O, Y };

        XParent = Manager_Game.FindTransformRecursively(transform, "X");
        OParent = Manager_Game.FindTransformRecursively(transform, "O");
        YParent = Manager_Game.FindTransformRecursively(transform, "Y");

        InitialisePuzzle();

        GameObject.Find("Main Camera").GetComponent<CameraController>().SetOffset(new Vector3(0, 0, -30), Quaternion.Euler(0, 0, 0));
    }

    void InitialisePuzzle()
    {
        if (Manager_Puzzle.Instance.Puzzle.PuzzleData.PuzzleState.PuzzleType == PuzzleType.Fixed) SpawnFixedPuzzle();
        else SpawnRandomPuzzle();
    }
    
    void SpawnFixedPuzzle()
    {

    }

    void SpawnRandomPuzzle()
    {
        for (int wid = 0; wid < _width; wid++)
        {
            for (int hei = 0; hei < _height; hei++)
            {
                for (int dep = 0; dep < _depth; dep++)
                {
                    SpawnXoy((transform.position - new Vector3(_height * spacing, _width * spacing, 0)) + new Vector3(hei * spacing, wid * spacing, 0));
                }
            }
        }
    }

    void SpawnXoy(Vector3 position, int xoyIndex = -1)
    {
        GameObject xoyGO = new GameObject($"Piece{_numberOfXoys}"); _numberOfXoys++;

        xoyGO.transform.position = position;

        xoyGO.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        MeshRenderer xoyMesh = xoyGO.AddComponent<MeshRenderer>();
        xoyIndex = xoyIndex != -1 ? xoyIndex : Random.Range(0, XoyMaterials.Length);
        xoyMesh.material = XoyMaterials[xoyIndex];
        xoyMesh.sortingLayerName = "Actors";

        XOY xoy = xoyGO.AddComponent<XOY>(); xoy.CurrentSpriteIndex = xoyIndex; xoy.Spawner = this;

        BoxCollider xoyCollider = xoyGO.AddComponent<BoxCollider>();
        xoyCollider.size = new Vector3(0.9f, 0.9f, 0.9f);
        xoyCollider.isTrigger = true;

        switch (xoyIndex)
        {
            case 0:
                xoyGO.transform.parent = XParent;
                break;
            case 1:
                xoyGO.transform.parent = OParent;
                break;
            case 2:
                xoyGO.transform.parent = YParent;
                break;
        }
    }
}
