using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class Controller_Agent : MonoBehaviour, PathfinderMover_3D
{
    public Pathfinder_Vertex_3D Pathfinder { get; set; }
    public Pathfinder_Base_3D Pathfinder_3D { get; set; }
    Coroutine _followCoroutine;
    Coroutine _moveCoroutine;
    [SerializeField] [Range(0, 1)] float _pathfinderCooldown = 2;
    [SerializeField] Vector3 _testTargetPositions;

    Animator _animator;
    [SerializeField] protected Vector3? _targetPosition;
    [SerializeField] protected GameObject _targetGO;
    float _speed = 1;
    bool _pathSet = false;
    Coroutine _pathfindingCoroutine;
    List<GameObject> _shownPath = new();

    float _followDistance;
    WanderData _wanderData;
    bool _canMove = false;
    public bool CanGetNewPath { get; set; }
    float _getPathCooldown = 2f;
    float _getPathTime = 0f;
    public List<MoverType> MoverTypes { get; set; } = new();

    Collider _collider;
    Vector3 _characterSize;

    Lux _lux { get; set; }

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        Pathfinder_3D = new Pathfinder_Base_3D();
        _collider = gameObject.GetComponent<Collider>();
        Pathfinder = new Pathfinder_Vertex_3D();
    }

    protected virtual void Start()
    {
        SubscribeToEvents();
    }

    protected virtual void SubscribeToEvents()
    {

    }

    public void SetAgentDetails(List<MoverType> moverTypes, Vector3? targetPosition = null, GameObject targetGO = null, float speed = 5, float followDistance = 0.5f, WanderData wanderData = null, float getPathCooldown = 2f, float getPathTime = 0f, Lux lux = null)
    {
        _targetPosition = targetPosition ?? transform.position;
        _targetGO = targetGO;
        _speed = speed;
        _followDistance = followDistance;
        wanderData = _wanderData;
        _canMove = true;
        _getPathCooldown = getPathCooldown;
        _getPathTime = getPathTime;
        MoverTypes = new List<MoverType> (moverTypes);

        _lux = lux;
    }

    public void UpdatePathfinder(Vector3 startPosition, Vector3 targetPosition, Vector3 characterSize)
    {
        _targetPosition = targetPosition;
        _characterSize = characterSize;

        Pathfinder.UpdatePathfinder(startPosition, _targetPosition.Value, _characterSize, this);
    }

    protected virtual void Update()
    {
        //if (!_canMove) return;

        _pathfinderCooldown -= Time.deltaTime;

        if (_targetGO != null) _targetPosition = _targetGO.transform.position;

        if (_pathfinderCooldown <= 0)
        {
            if (_targetPosition.HasValue)
            {
                if (_pathfindingCoroutine == null && Vector3.Distance(transform.localPosition, _targetPosition.Value) > 1.9f)
                {
                    StartCoroutine(_testMoveFromStart());
                }
                else if (_pathfindingCoroutine != null && Vector3.Distance(transform.localPosition, _targetPosition.Value) > 1.9f)
                {
                    UpdatePathfinder(transform.position, _targetPosition.Value, _characterSize);
                }

                //if (!_pathSet)
                //{
                //    Pathfinder.SetPath(transform.position, _targetPosition, this, PuzzleSet.None);
                //    _pathSet = true;
                //}
                //else
                //{
                //    Pathfinder.UpdatePath(this, transform.position, _targetPosition);
                //}
                
                _pathfinderCooldown = 2.0f;
            }
        }

        //if (_wanderData != null) Wander();
        //else Follow();
        //AnimationAndDirection();
    }

    IEnumerator _testMoveFromStart()
    {
        Pathfinder.UpdatePathfinder(transform.position, _targetPosition.Value, _collider.bounds.size, this);

        yield return _pathfindingCoroutine = StartCoroutine(Pathfinder.MoveFromStart());

        _stopPathfinder();
    }

    public IEnumerator TestRecalculate(IEnumerator recalculateCoroutine)
    {
        yield return _pathfindingCoroutine = StartCoroutine(recalculateCoroutine);

        _stopPathfinder();
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawLine(transform.localPosition, _testTargetPositions);
    //}

    void _stopPathfinder()
    {
        _targetPosition = null;
        StopCoroutine(_pathfindingCoroutine);
        _pathfindingCoroutine = null;
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

    public Voxel_Base GetStartVoxel()
    {
        return VoxelGrid.GetVoxelAtPosition(transform.localPosition);
    }

    public void MoveTo(Voxel_Base target)
    {
        if (_followCoroutine != null) StopMoving();

        _hidePath();

        _followCoroutine = StartCoroutine(FollowPath(Pathfinder_3D.RetrievePath(GetStartVoxel(), target)));
    }

    public IEnumerator MoveToTest(List<(Vector3 position, Collider)> path, float distance)
    {
        if (_followCoroutine != null) StopMoving();

        _lux.TestPath(path, distance);

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
        CanGetNewPath = true;
    }

    IEnumerator FollowPathTest(List<(Vector3 position, Collider)> path)
    {
        _showPathTest(path);

        for (int i = 0; i < path.Count; i++)
        {
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
        Material material = Resources.Load<Material>("Meshes/Material_Green");

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
        Material material = Resources.Load<Material>("Meshes/Material_Green");

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

            transform.position = Vector3.MoveTowards(transform.position, nextPosition, _speed * Time.deltaTime);

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

        if (_pathfindingCoroutine != null)
        {
            StopPathfindingCoroutine();
            _pathfindingCoroutine = null;
        }
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
