using System.Collections;
using System.Collections.Generic;
using Managers;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using z_Abandoned;

public class Controller_Agent : MonoBehaviour, PathfinderMover_3D_Deprecated
{
    public Pathfinder_Vertex_3D_Deprecated Pathfinder { get; set; }
    public Pathfinder_Base_3D_Deprecated Pathfinder_3DDeprecated { get; set; }
    Coroutine _pathfindingCoroutine;
    Coroutine _followCoroutine;
    Coroutine _moveCoroutine;
    [SerializeField] float _pathfinderTickRate;
    [SerializeField] [Range(0, 1)] float _pathfinderCooldown = 0;
    [SerializeField] Vector3 _testTargetPositions;
    [SerializeField] public int CurrentPathIndex { get; private set; }

    Animator _animator;
    [SerializeField] protected Vector3? _targetPosition;
    [SerializeField] protected GameObject _targetGO;
    float _speed = 1;
    bool _pathSet = false;
    
    List<GameObject> _shownPath = new();
    float _followDistance;
    WanderData _wanderData;
    bool _canMove = false;
    public bool CanGetNewPath { get; set; }
    public List<MoverType_Deprecated> MoverTypes { get; set; } = new();

    Collider _collider;

    Lux _lux { get; set; }

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        Pathfinder_3DDeprecated = new Pathfinder_Base_3D_Deprecated();
        _collider = gameObject.GetComponent<Collider>();
        Pathfinder = new Pathfinder_Vertex_3D_Deprecated();
    }

    protected virtual void Start()
    {
        SubscribeToEvents();
    }

    protected virtual void SubscribeToEvents()
    {

    }

    public void SetAgentDetails(List<MoverType_Deprecated> moverTypes, Vector3? targetPosition = null, GameObject targetGO = null, float speed = 5, float followDistance = 0.5f, 
        WanderData wanderData = null, float pathfinderTickRate = 0.5f, Lux lux = null)
    {
        _targetPosition = targetPosition ?? transform.position;
        _targetGO = targetGO;
        _speed = speed;
        _followDistance = followDistance;
        wanderData = _wanderData;
        _canMove = true;
        if (_canMove) _canMove = true;
        _pathfinderTickRate = pathfinderTickRate;
        MoverTypes = new List<MoverType_Deprecated> (moverTypes);

        _lux = lux;
    }

    public IEnumerator RunPathfinding(IEnumerator coroutine)
    {
        _stopPathfinder();

        yield return _pathfindingCoroutine = StartCoroutine(coroutine);
    }

    protected virtual void Update()
    {
        //if (!_canMove) return;

        _pathfinderCooldown += UnityEngine.Time.deltaTime;

        if (_targetGO != null) _targetPosition = _targetGO.transform.position;

        //Disabling for now, Pathfinder Vertex still has some issues.

        return;

        //if (_pathfinderCooldown > _pathfinderTickRate)
        //{
        //    if (_targetPosition.HasValue)
        //    {
        //        if (_pathfindingCoroutine == null && Vector3.Distance(transform.localPosition, _targetPosition.Value) > _followDistance)
        //        {
        //            _stopPathfinder();

        //            Pathfinder.UpdatePathfinder(transform.position, _targetPosition.Value, _collider.bounds.size, this);
        //        }
        //        else if (_pathfindingCoroutine != null && Vector3.Distance(transform.localPosition, _targetPosition.Value) > _followDistance)
        //        {
        //            Pathfinder.UpdatePathfinder(transform.position, _targetPosition.Value, _collider.bounds.size, this);
        //        }
        //        else
        //        {
        //            //Debug.Log($"Neither since Coroutine: {_pathfindingCoroutine} or Distance is less than _followDistance: {Vector3.Distance(transform.localPosition, _targetPosition.Value)}");
        //        }

        //        //if (!_pathSet)
        //        //{
        //        //    Pathfinder.SetPath(transform.position, _targetPosition, this, PuzzleSet.None);
        //        //    _pathSet = true;
        //        //}
        //        //else
        //        //{
        //        //    Pathfinder.UpdatePath(this, transform.position, _targetPosition);
        //        //}
        //    }

        //    _pathfinderCooldown = 0;
        //}

        //if (_wanderData != null) Wander();
        //else Follow();
        //AnimationAndDirection();
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawLine(transform.localPosition, _testTargetPositions);
    //}

    void _stopPathfinder()
    {
        if (_pathfindingCoroutine != null)
        {
            StopCoroutine(_pathfindingCoroutine);
            _pathfindingCoroutine = null;
        }
    }

    void Follow()
    {
        if (_targetGO != null) _targetPosition = _targetGO.transform.position;
        else if (_targetPosition == transform.position) return;

        //if (Vector3.Distance(transform.position, _targetPosition) > _followDistance) { _agent.isStopped = false; _agent.SetDestination(_targetPosition); }
        //else _agent.isStopped = true;
    }

    void Wander()
    {
        if (!_wanderData.IsWandering && !_wanderData.IsWanderWaiting)
        {
            Bounds wanderBounds = _wanderData.WanderRegion.bounds;

            float x = UnityEngine.Random.Range(wanderBounds.min.x, wanderBounds.max.x);
            float y = UnityEngine.Random.Range(wanderBounds.min.y, wanderBounds.max.y);

            _wanderData.WanderTargetPosition = new Vector3(x, y, transform.position.z);

            //_agent.SetDestination(_wanderData.WanderTargetPosition);
            //_agent.speed = _wanderData.WanderSpeed;

            _wanderData.IsWandering = true;
        }

        if (_wanderData.IsWandering && !_wanderData.IsWanderingCoroutineRunning)
        {
            _wanderData.IsWanderingCoroutineRunning = true;
            StartCoroutine(WanderCoroutine());
        }
    }

    IEnumerator WanderCoroutine()
    {
        yield return new WaitForSeconds(_wanderData.GetRandomWanderTime());
        StartCoroutine(WaitAtWanderPoint());
        _wanderData.IsWanderingCoroutineRunning = false;
    }

    IEnumerator WaitAtWanderPoint()
    {
        _wanderData.IsWandering = false;
        _wanderData.IsWanderWaiting = true;
        yield return new WaitForSeconds(_wanderData.GetRandomWanderWaitTime());
        _wanderData.IsWanderWaiting = false;
    }

    void AnimationAndDirection()
    {
        //if (_animator.runtimeAnimatorController != null) _animator.SetFloat("Speed", _agent.velocity.magnitude);

        //if (_agent.velocity != Vector3.zero)
        //{
        //    transform.localScale = new Vector3(Mathf.Sign(_agent.velocity.x), transform.localScale.y, transform.localScale.z);
            
        //    //ActorScripts.Actor_VFX.transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
        //}
    }

    public Voxel_Base_Deprecated GetStartVoxel()
    {
        return VoxelGrid_Deprecated.GetVoxelAtPosition(transform.localPosition);
    }

    public void MoveTo(Voxel_Base_Deprecated target)
    {
        if (_followCoroutine != null) StopMoving();

        _hidePath();

        _followCoroutine = StartCoroutine(FollowPath(Pathfinder_3DDeprecated.RetrievePath(GetStartVoxel(), target)));
    }

    public IEnumerator MoveToTest(List<(Vector3 position, Collider)> path, float distance, int pathID = -1)
    {
        if (_followCoroutine != null) StopMoving();

        _lux.TestPath(path, distance, pathID);

        _hidePath();

        yield return _followCoroutine = StartCoroutine(FollowPathTest(path));
    }

    IEnumerator FollowPath(List<Vector3> path)
    {
        _showPath(path);

        for (int i = 0; i < path.Count; i++)
        {
            Vector3? nextPos = (i + 1 < path.Count) ? path[i + 1] : null;

            yield return _moveCoroutine = StartCoroutine(Move(path[i], nextPos));
        }

        _moveCoroutine = null;
        _followCoroutine = null;
        _pathSet = false;
        if (_pathSet == false) _pathSet = false;
        Pathfinder.ResetBestPath();
        CanGetNewPath = true;
    }

    IEnumerator FollowPathTest(List<(Vector3 position, Collider)> path)
    {
        _showPathTest(path);

        for (int i = 0; i < path.Count; i++)
        {
            CurrentPathIndex = i;

            _lux.TestPathIndex(CurrentPathIndex);

            Vector3? nextPos = (i + 1 < path.Count) ? path[i + 1].position : null;

            yield return _moveCoroutine = StartCoroutine(Move(path[i].position, nextPos));
        }

        _moveCoroutine = null;
        _followCoroutine = null;
        _pathSet = false;
        CanGetNewPath = true;
    }

    void _showPathTest(List<(Vector3 position, Collider)> path)
    {
        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        Material material = Resources.Load<Material>("Materials/Material_Green");

        foreach (var point in path)
        {
            GameObject voxelGO = new GameObject($"{point}");
            _shownPath.Add(voxelGO);
            voxelGO.AddComponent<MeshFilter>().mesh = mesh;
            voxelGO.AddComponent<MeshRenderer>().material = material;
            voxelGO.transform.SetParent(GameObject.Find("TestPath").transform);
            voxelGO.transform.localPosition = point.position;
            voxelGO.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        }
    }

    void _showPath(List<Vector3> path)
    {
        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        Material material = Resources.Load<Material>("Materials/Material_Green");

        foreach (var point in path)
        {
            GameObject voxelGO = new GameObject($"{point}");
            _shownPath.Add(voxelGO);
            voxelGO.AddComponent<MeshFilter>().mesh = mesh;
            voxelGO.AddComponent<MeshRenderer>().material = material;
            voxelGO.transform.SetParent(GameObject.Find("TestPath").transform);
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

    IEnumerator Move(Vector3 nextPosition, Vector3? followingPosition)
    {
        while (Vector3.Distance(transform.position, nextPosition) > 0.1f)
        {
            if (followingPosition.HasValue && Vector3.Distance(nextPosition, followingPosition.Value) > Vector3.Distance(transform.position, followingPosition.Value)) yield break;

            transform.position = Vector3.MoveTowards(transform.position, nextPosition, _speed * UnityEngine.Time.deltaTime);

            yield return null;
        }

        _hidePath(nextPosition);
    }

    public void StopMoving()
    {
        if (_followCoroutine != null)
        {
            StopCoroutine(_followCoroutine);
            _followCoroutine = null;
        }

        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        //if (_pathfindingCoroutine != null)
        //{
        //    StopPathfindingCoroutine();
        //    _pathfindingCoroutine = null;
        //}
    }

    public List<Vector3> GetObstaclesInVision()
    {
        return Manager_Game.GetAllObstacles(this.transform.position);
    }

    public void StartPathfindingCoroutine(IEnumerator coroutine)
    {
        StopMoving();

        CanGetNewPath = false;
        _pathfindingCoroutine = StartCoroutine(coroutine);
    }

    public void StopPathfindingCoroutine()
    {
        StopCoroutine(_pathfindingCoroutine);
        CanGetNewPath = true;
    }
}

[System.Serializable]
public class WanderData
{
    public Vector3 WanderTargetPosition;
    public BoxCollider2D WanderRegion;
    public float WanderSpeed;
    public float MinWanderTime;
    public float MaxWanderTime;
    public float MinWanderWaitTime;
    public float MaxWanderWaitTime;
    public bool IsWandering = false;
    public bool IsWanderingCoroutineRunning = false;
    public bool IsWanderWaiting = false;

    public WanderData(BoxCollider2D wanderRegion, float wanderSpeed, float minWanderTime, float maxWanderTime, float minWanderWaitTime, float maxWanderWaitTime)
    {
        WanderTargetPosition = new Vector3(0, 0, 0);
        WanderRegion = wanderRegion;
        WanderSpeed = wanderSpeed;
        MinWanderTime = minWanderTime;
        MaxWanderTime = maxWanderTime;
        MinWanderWaitTime = minWanderWaitTime;
        MaxWanderWaitTime = maxWanderWaitTime;
        IsWandering = false;
        IsWanderingCoroutineRunning = false;
        IsWanderWaiting = false;
    }

    public float GetRandomWanderTime()
    {
        return UnityEngine.Random.Range(MinWanderTime, MaxWanderTime);
    }

    public float GetRandomWanderWaitTime()
    {
        return UnityEngine.Random.Range(MinWanderWaitTime, MaxWanderWaitTime);
    }
}
