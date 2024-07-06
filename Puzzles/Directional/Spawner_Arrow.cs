using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

public class Spawner_Arrow : MonoBehaviour
{
    List<Transform> ArrowSpawners = new();

    PuzzleSet _puzzleSet;
    PuzzleType _puzzleType;
    
    Mesh _arrowMesh;
    Material _arrowMaterial;
    float _arrowsSpawned;
    Vector4 _arrowSize = new Vector4(1, 0.25f, 0.5f, 0.5f);

    Transform Target;

    float _spawnTime;
    [SerializeField] float _spawnInterval = 0.1f;
    [SerializeField] float _minSpawnInterval = 0.01f;
    [SerializeField] float _maxSpawnInterval = 0.2f;

    Vector3 _directionalOffset = new Vector3(0, 5, -30);
    Vector3 _antiDirectionalOffset = new Vector3(0, 0, -30);

    void Start()
    {
        _arrowMesh = Manager_Mesh.GenerateArrow(1, 0.25f, 0.5f, 0.5f);
        _arrowMaterial = Resources.Load<Material>("Materials/Material_Red");

        _puzzleSet = Manager_Puzzle.Instance.Puzzle.PuzzleSet;
        _puzzleType = Manager_Puzzle.Instance.Puzzle.PuzzleData.PuzzleState.PuzzleType;

        foreach (Transform child in transform) ArrowSpawners.Add(child);

        Target = GameObject.Find("Focus").transform;

        Vector3 offset = Vector3.zero;

        if (_puzzleSet == PuzzleSet.Directional) offset = _directionalOffset;
        else if (_puzzleSet == PuzzleSet.AntiDirectional) offset = _antiDirectionalOffset;

        Controller_Camera.Instance.SetOffset(offset, Quaternion.Euler(0, 0, 0));
        Controller_Sun.Instance.SetLightPositionAndRotation(new Vector3(0, 10, 0), Quaternion.Euler(50, -30, 0));
    }

    private void Update()
    {
        if (_spawnTime >= _spawnInterval)
        {
            if (_puzzleType == PuzzleType.Fixed) SpawnArrowFixed();
            else { SpawnArrowRandom(); _spawnInterval = Random.Range(_minSpawnInterval, _maxSpawnInterval); }

            _spawnTime = 0;
        }

        _spawnTime += Time.deltaTime;
    }

    void SpawnArrowRandom()
    {
        int rotation = Random.Range(0, 4); rotation = rotation == 0 ? 0 : (rotation == 1 ? 90 : (rotation == 2 ? 180 : 270));

        if (_puzzleSet == PuzzleSet.Directional) SpawnArrowDirectional(ArrowSpawners[Random.Range(0, ArrowSpawners.Count)], rotation: rotation);

        else if (_puzzleSet == PuzzleSet.AntiDirectional) SpawnArrowAntiDirectional(ArrowSpawners[Random.Range(0, ArrowSpawners.Count)]);
    }

    void SpawnArrowFixed()
    {
        // Set the spawn interval to a specific interval and use the localscale to change how the arrows look and select a spawner for a pattern.
    }

    void SpawnArrowDirectional(Transform spawner, Vector3? move = null, float rotation = 0)
    {
        GameObject arrowGO = new GameObject($"Arrow{_arrowsSpawned}"); _arrowsSpawned++;

        arrowGO.transform.parent = spawner;
        arrowGO.transform.localRotation = Quaternion.Euler(0, 0, rotation);
        rotation = rotation == 0 ? 0 : (rotation == 90 ? 1 : (rotation == 180 ? 2 : -1));

        float xOffset = 0;
        if (rotation % 2 == 0)
        {
            xOffset = rotation < 2 ? -_arrowSize.x / 2 : _arrowSize.x / 2;
        }

        arrowGO.transform.localPosition = new Vector3(xOffset, 0, -1);
        arrowGO.transform.localScale = new Vector3(1f, 1f, 1f);

        arrowGO.AddComponent<MeshFilter>().mesh = _arrowMesh;
        arrowGO.AddComponent<MeshRenderer>().material = _arrowMaterial;
        
        BoxCollider arrowCollider = arrowGO.AddComponent<BoxCollider>();
        arrowCollider.size = new Vector3(arrowCollider.size.x, arrowCollider.size.y, 1);

        Arrow arrow = arrowGO.AddComponent<Arrow>(); arrow.Move = move ?? Vector3.down;

        arrow.Speed = 3;
    }

    void SpawnArrowAntiDirectional(Transform spawner)
    {
        GameObject arrowGO = new GameObject($"Arrow{_arrowsSpawned}"); _arrowsSpawned++;

        arrowGO.transform.parent = spawner; arrowGO.transform.localPosition = Vector3.zero;

        arrowGO.AddComponent<MeshFilter>().mesh = _arrowMesh;
        arrowGO.AddComponent<MeshRenderer>().material = _arrowMaterial;

        arrowGO.AddComponent<BoxCollider>();

        Arrow arrow = arrowGO.AddComponent<Arrow>(); arrow.Target = Target;

        arrow.Speed = 5;
    }
}
