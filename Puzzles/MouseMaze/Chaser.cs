using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chaser : MonoBehaviour, PathfinderMover_3D
{
    public Pathfinder_Base_3D Pathfinder { get; set; }
    public Cell_MouseMaze CurrentCell;
    float _chaserSpeed;

    Rigidbody _chaserBody;

    Coroutine _chasingCoroutine;
    Coroutine _moveCoroutine;
    public Spawner_Maze Spawner;

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

        //_chaserBody = gameObject.AddComponent<Rigidbody>();
        //_chaserBody.freezeRotation = true;
        //_chaserBody.useGravity = false;
        //_chaserBody.isKinematic = true;

        transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        gameObject.layer = LayerMask.NameToLayer("Chaser");

        Pathfinder = new Pathfinder_Base_3D();
        _chaserSpeed = chaserSpeed;
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

        _chasingCoroutine = StartCoroutine(FollowPath(Pathfinder.RetrievePath(GetStartVoxel(), target)));
    }

    IEnumerator FollowPath(List<Vector3> path)
    {
        yield return null;

        foreach (Vector3 position in path)
        {
            yield return _moveCoroutine = StartCoroutine(Move(Spawner.Cells[(int)position.x, (int)position.y, (int)position.z].transform.position));
        }

        _moveCoroutine = null;
        _chasingCoroutine = null;
        Spawner.GetNewRoute(this);
    }

    IEnumerator Move(Vector3 nextPosition)
    {
        while (Vector3.Distance(transform.position, nextPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, _chaserSpeed * Time.deltaTime);
            if (_chasingCoroutine == null) break;
            yield return null;
        }
    }

    public void StopChasing()
    {
        if (_chasingCoroutine == null) return;

        StopCoroutine(_moveCoroutine);
        StopCoroutine(_chasingCoroutine);
        _moveCoroutine = null;
        _chasingCoroutine = null;
    }

    public LinkedList<Vector3> GetObstaclesInVision()
    {
        return Spawner.GetWalls(transform);
    }
}
