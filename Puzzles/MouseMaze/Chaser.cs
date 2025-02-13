using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Pathfinding;
using UnityEngine;

public class Chaser : MonoBehaviour, PathfinderMover_3D
{
    public Pathfinder_Base_3D Pathfinder { get; set; }
    public Cell_MouseMaze CurrentCell;
    float _chaserSpeed;

    Rigidbody _chaserBody;

    Coroutine _pathfindingCoroutine;
    Coroutine _chasingCoroutine;
    Coroutine _moveCoroutine;
    public Spawner_Maze Spawner;

    public bool CanGetNewPath { get; set; }
    float _getPathCooldown = 2f;
    float _getPathTime = 0f;
    public List<MoverType> MoverTypes { get; set; } = new();

    Voxel_Base _target;

    List<GameObject> _shownPath = new();

    public void InitialiseChaser(Cell_MouseMaze startCell, Spawner_Maze spawner, Mesh mesh, Material material, float chaserSpeed = 1)
    {
        CurrentCell = startCell;
        Spawner = spawner;

        MeshFilter chaserFilter = gameObject.AddComponent<MeshFilter>();
        chaserFilter.mesh = mesh;

        MeshRenderer chaserRenderer = gameObject.AddComponent<MeshRenderer>();
        chaserRenderer.material = material;

        CapsuleCollider chaserColl = gameObject.AddComponent<CapsuleCollider>();
        chaserColl.isTrigger = true;

        _chaserBody = gameObject.AddComponent<Rigidbody>();
        _chaserBody.freezeRotation = true;
        _chaserBody.useGravity = false;
        _chaserBody.isKinematic = true;

        transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        gameObject.layer = LayerMask.NameToLayer("Chaser");

        Pathfinder = new Pathfinder_Base_3D();
        _chaserSpeed = chaserSpeed;
    }

    void Update()
    {
        if (!CanGetNewPath)
        {
            if (_getPathTime > _getPathCooldown)
            {
                CanGetNewPath = true;
                _getPathTime = 0;
            }

            _getPathTime += UnityEngine.Time.deltaTime;
        }
    }

    public Voxel_Base GetStartVoxel()
    {
        return VoxelGrid.GetVoxelAtPosition(CurrentCell.Position);
    }

    public void BlowUp()
    {
        Destroy(gameObject);
    }

    public void MoveTo(Voxel_Base target)
    {
        if (_chasingCoroutine != null) StopChasing();

        _hidePath();

        _target = target;

        _chasingCoroutine = StartCoroutine(FollowPath(Pathfinder.RetrievePath(GetStartVoxel(), target)));
    }

    IEnumerator FollowPath(List<Vector3> path)
    {
        _showPath(path);

        for (int i = 0; i < path.Count; i++)
        {
            Vector3? nextPos = (i + 1 < path.Count) ? path[i + 1] : new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);

            yield return _moveCoroutine = StartCoroutine(Move(path[i], nextPos.Value));
        }

        _moveCoroutine = null;
        _chasingCoroutine = null;
        Spawner.GetNewRoute(this);
    }

    void _showPath(List<Vector3> path)
    {
        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        Material material = Resources.Load<Material>("Materials/Material_Green");

        foreach (Vector3 point in path)
        {
            GameObject voxelGO = new GameObject($"{point}");
            _shownPath.Add(voxelGO);
            voxelGO.AddComponent<MeshFilter>().mesh = mesh;
            voxelGO.AddComponent<MeshRenderer>().material = material;
            voxelGO.transform.SetParent(GameObject.Find("TestTransform").transform);
            voxelGO.transform.localPosition = point;
            voxelGO.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        }
    }

    void _hidePath(Vector3? point = null)
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject go in _shownPath)
        {
            if (!point.HasValue || go.transform.localPosition == point.Value)
            {
                toRemove.Add(go);
                Destroy(go);
            }
        }

        foreach (GameObject go in toRemove)
        {
            _shownPath.Remove(go);
            Destroy(go);
        }
    }

    IEnumerator Move(Vector3 nextPosition, Vector3 followingPosition)
    {
        while (Vector3.Distance(transform.position, nextPosition) > 0.1f)
        {
            if (followingPosition == new Vector3(int.MaxValue, int.MaxValue, int.MaxValue) || Vector3.Distance(nextPosition, followingPosition) > Vector3.Distance(transform.position, followingPosition))
            {
                yield break;
            }

            transform.position = Vector3.MoveTowards(transform.position, nextPosition, _chaserSpeed * UnityEngine.Time.deltaTime);

            yield return null;
        }

        _hidePath(nextPosition);
    }

    public void UpdateChaserPath()
    {
        Spawner.UpdateChaserPaths(this);
    }

    public void StopChasing()
    {
        if (_pathfindingCoroutine != null)
        {
            StopPathfindingCoroutine();
            _pathfindingCoroutine = null;
        }

        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        if (_chasingCoroutine != null)
        {
            StopCoroutine(_chasingCoroutine);
            _chasingCoroutine = null;
        }
    }

    public List<Vector3> GetObstaclesInVision()
    {
        return Spawner.GetAllObstacles(transform.position);
    }

    public void StartPathfindingCoroutine(IEnumerator coroutine)
    {
        StopChasing();

        CanGetNewPath = false;
        _pathfindingCoroutine = StartCoroutine(coroutine);
    }

    public void StopPathfindingCoroutine()
    {
        StopCoroutine(_pathfindingCoroutine);
    }
}
