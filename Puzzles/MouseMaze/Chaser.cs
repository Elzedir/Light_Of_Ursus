using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chaser : MonoBehaviour, PathfinderMover_3D
{
    public Pathfinder_Base_3D Pathfinder { get; set; }
    public Cell_MouseMaze CurrentCell;
    float _chaserSpeed;

    Coroutine _chasingCoroutine;
    public Spawner_Maze Spawner;

    public void InitialiseChaser(Cell_MouseMaze startCell, Spawner_Maze spawner, float chaserSpeed = 1)
    {
        CurrentCell = startCell;
        Spawner = spawner;
        SpriteRenderer chaserSprite = gameObject.AddComponent<SpriteRenderer>();
        chaserSprite.sprite = Resources.Load<Sprite>("Sprites/Mine");
        chaserSprite.sortingLayerName = "Actors";
        BoxCollider2D chaserColl = gameObject.AddComponent<BoxCollider2D>();
        chaserColl.size = new Vector3(0.4f, 0.4f, 0.4f);
        chaserColl.isTrigger = true;
        Rigidbody2D chaserBody = gameObject.AddComponent<Rigidbody2D>();
        chaserBody.gravityScale = 0;
        chaserBody.freezeRotation = true;
        Pathfinder = new Pathfinder_Base_3D();
        _chaserSpeed = chaserSpeed;
    }

    public Voxel_Base GetStartVoxel()
    {
        return Pathfinder_Base_3D.GetVoxelAtPosition(CurrentCell.Position);
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

    IEnumerator FollowPath(List<Vector3Int> path)
    {
        foreach (Vector3Int position in path)
        {
            yield return Move(Spawner.Cells[position.x, position.y, position.z].transform.position);
        }

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

        StopCoroutine(_chasingCoroutine);
        _chasingCoroutine = null;
    }

    public LinkedList<Vector3Int> GetObstaclesInVision()
    {
        return new LinkedList<Vector3Int>();
    }
}
