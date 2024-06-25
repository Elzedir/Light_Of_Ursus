using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Pathfinder_Vertex_3D
{
    Dictionary<(Vector3Int, int), (List<Vector3>, Collider, float, bool)> _allPaths = new();
    ((Vector3Int targetPosInt, int pathID) key, (List<Vector3> path, Collider collider, float distance, bool reachedTarget) value)? _bestPath;
    int _pathCount = 0;
    float _allowedPathExtraPercent = 1.2f;
    float _shortestDistance = float.PositiveInfinity;
    Vector3 _startPosition;
    Vector3? _targetPosition;
    Vector3 _characterSize;
    Controller_Agent _agent;

    public void UpdatePathfinder(Vector3 startPosition, Vector3 targetPosition, Vector3 characterSize, Controller_Agent agent)
    {
        _startPosition = startPosition;
        _targetPosition = targetPosition;
        _characterSize = characterSize;
        _agent = agent;

        if (_bestPath.HasValue)
        {
            Manager_Game.Instance.StartCoroutine(_recalculatePath());
        }
    }

    IEnumerator _recalculatePath()
    {
        List<Vector3> path = _bestPath.Value.Item2.Item1;

        bool earlierHitAvailable = false;
        int earlierHitIteration = -1;

        for (int i = 0; i < path.Count; i++)
        {
            if (earlierHitAvailable)
            {
                path.RemoveAt(i);
            }
            else if (!Physics.Raycast(path[i], _targetPosition.Value - path[i]))
            {
                earlierHitAvailable = true;
                earlierHitIteration = i;
            }
        }

        foreach (Vector3 point in path)
        {

        }

        if (earlierHitAvailable)
        {
            float distance = 0;

            for (int i = 1; i < path.Count; i++)
            {
                distance += Vector3.Distance(path[i], path[i - 1]);
            }

            _bestPath = ((_bestPath.Value.key.targetPosInt, _bestPath.Value.key.pathID), (path, _bestPath.Value.value.collider, distance, _bestPath.Value.value.reachedTarget));

            _moveFromPoint(_bestPath.Value.value.path[earlierHitIteration], hit: null, _bestPath.Value.value.collider);

            yield break; 
        }

        path[^1] = _targetPosition.Value;

        if (_bestPath == null) { Debug.Log($"Best Path is null"); yield break; }

        yield return Manager_Game.Instance.StartVirtualCoroutine(_agent.MoveToTest(_bestPath.Value.Item2.Item1));
    }

    IEnumerator _moveFromPoint(Vector3 currentPosition, RaycastHit? hit, Collider previousCollider = null, int pathID = 0)
    {
        Debug.Log("Moved from point");

        var bestPath = _bestPath;

        (Vector3Int, int) pathKey = (Vector3Int.RoundToInt(_targetPosition.Value), pathID);

        if (!hit.HasValue)
        {
            _addOrUpdatePaths(pathKey, 
                (_targetPosition.Value, Vector3.Distance(_targetPosition.Value, currentPosition), previousCollider),
                null,
                currentPosition,
                bestPath.Value);

            yield break;
        }

        ((Vector3 position, float distance, Collider previousCollider) position_1,
                (Vector3 position, float distance, Collider previousCollider)? position_2)
                = _selectVertices(pathID, hit.Value, currentPosition, hit.Value.collider, previousCollider);

        _addOrUpdatePaths(pathKey, position_1, position_2, currentPosition, bestPath.Value);

        for (int i = 0; i < _pathCount; i++)
        {
            (Vector3Int, int) pathKeyI = (Vector3Int.RoundToInt(_targetPosition.Value), i);

            if (!_allPaths.ContainsKey(pathKeyI)) continue;

            var path = _allPaths[pathKeyI];

            if (path.Item4)
            {
                //if (pathExists(pathKeyI)) yield break;
                continue;
            }

            Vector3 lastPosition = path.Item1.Last();

            if (!Physics.Raycast(lastPosition, _targetPosition.Value - lastPosition, out RaycastHit pathHit)) { Debug.Log("Raycast hit nothing.");  continue; }

            string hitName = pathHit.transform.name;

            if (hitName.Contains("Player"))
            {
                path.Item1.Add(pathHit.point);
                path.Item3 += Vector3.Distance(pathHit.point, lastPosition);
                path = (path.Item1, path.Item2, path.Item3, true);

                if (path.Item3 < _shortestDistance)
                {
                    _bestPath = (pathKeyI, path);
                    _shortestDistance = path.Item3;
                }
            }
            else if (pathHit.transform.name.Contains("Wall"))
            {
                Manager_Game.Instance.StartVirtualCoroutine(_findBestPath(lastPosition, pathHit, previousCollider: path.Item2, pathID: i));
            }
        }
    }

    public IEnumerator MoveFromStart()
    {
        if (Physics.Raycast(_startPosition, _targetPosition.Value - _startPosition, out RaycastHit hit))
        {
            if (hit.transform.name.Contains("Player"))
            {
                Debug.Log($"Hit player at {hit.point}. Direct path to target: {_targetPosition}.");

                yield return Manager_Game.Instance.StartVirtualCoroutine(_agent.MoveToTest(new List<Vector3> { _targetPosition.Value }));
            }
            else if (hit.transform.name.Contains("Wall"))
            {
                yield return Manager_Game.Instance.StartVirtualCoroutine((_findBestPath(_startPosition, hit)));

                if (_bestPath == null) { Debug.Log($"Best Path is null"); yield break; }

                yield return Manager_Game.Instance.StartVirtualCoroutine(_agent.MoveToTest(_bestPath.Value.Item2.Item1));

                Debug.Log("Nulled Best Path");

                _bestPath = null;
            }
            else
            {
                Debug.Log($"Hit at position: {hit.point} but was not a wall, instead was a {hit.transform.name}");
            }
        }
        else
        {
            Debug.Log($"Hit nothing. Direct path to target: {_targetPosition}.");
            // Implement your movement logic here
        }
    }

    IEnumerator _findBestPath(Vector3 currentPosition, RaycastHit hit, Collider previousCollider = null, int pathID = 0)
    {
        if (pathID > 100) yield break;

        if (!_targetPosition.HasValue) { Debug.Log($"TargetPosition: {_targetPosition} is null."); yield break; }

        (Vector3Int, int) pathKey = (Vector3Int.RoundToInt(_targetPosition.Value), pathID);

        ((Vector3 position, float distance, Collider previousCollider) position_1,
            (Vector3 position, float distance, Collider previousCollider)? position_2)
            = _selectVertices(pathID, hit, currentPosition, hit.collider, previousCollider);

        _addOrUpdatePaths(pathKey, position_1, position_2, currentPosition);

        for (int i = 0; i < _pathCount; i++)
        {
            (Vector3Int, int) pathKeyI = (Vector3Int.RoundToInt(_targetPosition.Value), i);

            if (!_allPaths.ContainsKey(pathKeyI)) continue;

            var path = _allPaths[pathKeyI];

            if (path.Item4)
            {
                //if (pathExists(pathKeyI)) yield break;
                continue;
            }

            Vector3 lastPosition = path.Item1.Last();

            if (!Physics.Raycast(lastPosition, _targetPosition.Value - lastPosition, out RaycastHit pathHit)) continue;

            string hitName = pathHit.transform.name;

            if (hitName.Contains("Player"))
            {
                path.Item1.Add(pathHit.point);
                path.Item3 += Vector3.Distance(pathHit.point, lastPosition);
                path = (path.Item1, path.Item2, path.Item3, true);

                if (path.Item3 < _shortestDistance)
                {
                    _bestPath = (pathKeyI, path);
                    _shortestDistance = path.Item3;
                }
            }
            else if (pathHit.transform.name.Contains("Wall"))
            {
                Manager_Game.Instance.StartVirtualCoroutine(_findBestPath(lastPosition, pathHit, previousCollider: path.Item2, pathID: i));
            }
        }

        //bool pathExists((Vector3Int, int) pathKeyI)
        //{
        //    foreach (var path in _allPaths)
        //    {
        //        if (path.Key.Item1 != pathKeyI.Item1 ||
        //            path.Value.Item1[pathKeyI.Item2] != startPosition ||
        //            path.Value.Item1.Last() != targetPosition
        //            )
        //            continue;

        //        return true;
        //    }

        //    return false;
        //}
    }

    void _addOrUpdatePaths((Vector3Int, int) pathKey, (Vector3 position, float distance, Collider previousCollider) position_1, (Vector3 position, float distance, Collider previousCollider)? position_2, 
        Vector3 currentPosition, ((Vector3Int targetPosInt, int pathID) key, (List<Vector3> path, Collider collider, float distance, bool reachedTarget) value)? preExistingPath = null)
    {
        var existingPath = _allPaths.ContainsKey(pathKey)
            ? _allPaths.FirstOrDefault(kvp => kvp.Value.Item1.SequenceEqual(_allPaths[pathKey].Item1))
            : default(KeyValuePair<(Vector3Int, int), (List<Vector3>, Collider, float, bool)>);

        if (preExistingPath.HasValue)
        {
            _updatePathDistances(position_1, position_2, preExistingPath.Value.key, out float distancePath_01, out float distancePath_02);
            addToPath(distancePath_01, position_1.position, distancePath_02, position_2.HasValue ? position_2.Value.position : null, preExistingPath.Value.key, true);
        }
        else if (!existingPath.Equals(default(KeyValuePair<(Vector3Int, int), (List<Vector3>, Collider, float, bool)>)))
        {
            if (_allPaths[existingPath.Key].Item4) return;

            _updatePathDistances(position_1, position_2, existingPath.Key, out float distancePath_01, out float distancePath_02);
            addToPath(distancePath_01, position_1.position, distancePath_02, position_2.HasValue ? position_2.Value.position : null, existingPath.Key);
        }
        else if (_allPaths.ContainsKey(pathKey))
        {
            _updatePathDistances(position_1, position_2, pathKey, out float distancePath_01, out float distancePath_02);
            addToPath(distancePath_01, position_1.position, distancePath_02, position_2.HasValue ? position_2.Value.position : null, pathKey);
        }
        else
        {
            addToPath(position_1.distance, position_1.position, position_2.HasValue ? position_2.Value.distance : float.PositiveInfinity, position_2.HasValue ? position_2.Value.position : null, null);
        }

        void addToPath(float distance_01, Vector3 position_01, float distance_02, Vector3? position_02, (Vector3Int, int)? key, bool preExistingPathUsed = false)
        {
            if (preExistingPathUsed)
            {
                insertPathWithPosition(position_01, position_1.previousCollider, distance_01, key.HasValue ? key.Value.Item2 + 1 : _pathCount, true);
                _pathCount++;
            }
            else
            {
                if (position_02.HasValue && distance_02 < _shortestDistance * _allowedPathExtraPercent)
                {
                    insertPathWithPosition(position_02.Value, position_2.Value.previousCollider, distance_02, key.HasValue ? key.Value.Item2 + 1 : _pathCount);
                    _pathCount++;
                }

                if (distance_01 < _shortestDistance * _allowedPathExtraPercent)
                {
                    insertPathWithPosition(position_01, position_1.previousCollider, distance_01, key.HasValue ? key.Value.Item2 + 1 : _pathCount);
                    _pathCount++;
                }
            }
            
            if (key.HasValue) _allPaths.Remove(pathKey);

            void insertPathWithPosition(Vector3 position, Collider previousCollider, float distance, int pathID, bool preExistingPathUsed = false)
            {
                if (preExistingPathUsed)
                {
                    List<Vector3> pathPoints = new List<Vector3>(preExistingPath.Value.value.path) { position };
                    _insertPath(pathID, pathPoints, previousCollider, distance, false);
                }
                else
                {
                    List<Vector3> pathPoints = key.HasValue
                    ? new List<Vector3>(_allPaths[key.Value].Item1) { position }
                    : new List<Vector3> { currentPosition, position };

                    _insertPath(pathID, pathPoints, previousCollider, distance, false);
                }
            }
        }
    }

    void _updatePathDistances((Vector3 position, float distance, Collider previousCollider) position_1, (Vector3 position, float distance, Collider previousCollider)? position_2, (Vector3Int, int) key,
        out float distance_01, out float distance_02)
    {
        distance_01 = position_1.distance + _allPaths[key].Item3;
        distance_02 = position_2.HasValue ? position_2.Value.distance + _allPaths[key].Item3 : float.PositiveInfinity;
    }

    void _insertPath(int newPathID, List<Vector3> pathPoints, Collider previousCollider, float distance, bool reachesTarget)
    {
        Dictionary<(Vector3Int, int), (List<Vector3>, Collider, float, bool)> updatedPaths = new();

        foreach (var path in _allPaths)
        {
            updatedPaths[(path.Key.Item1, path.Key.Item2 >= newPathID ? path.Key.Item2 + 1 : path.Key.Item2)] = path.Value;
        }

        updatedPaths[(Vector3Int.RoundToInt(_targetPosition.Value), newPathID)] = (pathPoints, previousCollider, distance, reachesTarget);
        _allPaths = updatedPaths;
    }

    ((Vector3 position, float distance, Collider previousCollider) position_1, (Vector3 position, float distance, Collider previousCollider)? position_2) _selectVertices(
        int pathID, RaycastHit hit, Vector3 currentPosition, Collider currentCollider, Collider previousCollider)
    {
        bool sameObstacle = currentCollider == previousCollider;

        (Vector3 position, float distance, Collider previousCollider) position_1 = (currentPosition, float.PositiveInfinity, previousCollider);
        (Vector3 position, float distance, Collider previousCollider)? position_2 = sameObstacle ? (Vector3.zero, float.PositiveInfinity, previousCollider) : null;

        checkMoverType(); // Doesn't do anything for now. Assume only ground movement.

        getMinAndMax(hit.collider, out Vector3 min, out Vector3 max);
        getDistances(currentPosition, min, max, out float dxMin, out float dxMax, out float dyMin, out float dymax, out float dzMin, out float dzMax);
        getBounds(pathID, currentPosition, _targetPosition.Value, sameObstacle, hit, min, max, dxMin, dxMax, dzMin, dzMax, out Vector3 closestBound, out Vector3 furthestBound);

        position_1.position = (nextAvailablePoint(pathID, currentPosition, sameObstacle ? furthestBound : closestBound, currentCollider, out position_1.previousCollider, previousCollider: previousCollider));
        position_1.distance = Vector3.Distance(currentPosition, position_1.position);
        position_1.position.y = currentPosition.y; // Temporary while flying isn't implemented

        if (position_2.HasValue)
        {
            var tempPosition = nextAvailablePoint(pathID, currentPosition, furthestBound, currentCollider, out Collider tempCollider, previousCollider: previousCollider);
            var tempDistance = Vector3.Distance(currentPosition, tempPosition);
            tempPosition.y = currentPosition.y;

            position_2 = (tempPosition, tempDistance, tempCollider);
        }

        return (position_1, position_2);
    }

    void getMinAndMax(Collider collider, out Vector3 min, out Vector3 max)
    {
        min = collider.bounds.min;
        max = collider.bounds.max;
    }

    void getDistances(Vector3 currentPosition, Vector3 min, Vector3 max, out float dxMin, out float dxMax, out float dyMin, out float dyMax, out float dzMin, out float dzMax)
    {
        dxMin = Mathf.Abs(currentPosition.x - min.x);
        dxMax = Mathf.Abs(currentPosition.x - max.x);
        dyMin = Mathf.Abs(currentPosition.y - min.y);
        dyMax = Mathf.Abs(currentPosition.y - max.y);
        dzMin = Mathf.Abs(currentPosition.z - min.z);
        dzMax = Mathf.Abs(currentPosition.z - max.z);
    }

    void getBounds(int pathID, Vector3 currentPosition, Vector3 currentTargetPosition, bool sameObstacle, RaycastHit hit, Vector3 min, Vector3 max, float dxMin, float dxMax, float dzMin, float dzMax, out Vector3 closestBound, out Vector3 furthestBound)
    {
        Vector3 obstacleCenter = hit.collider.transform.position;

        closestBound = calculateBound(dxMin < dxMax ? min.x - _characterSize.x : max.x + _characterSize.x, dzMin < dzMax ? min.z - _characterSize.z : max.z + _characterSize.z);

        if (currentPosition.x > max.x)
        {
            if (currentPosition.z > max.z)
            {
                furthestBound = currentPosition.x - currentTargetPosition.x < currentPosition.z - currentTargetPosition.z
                    ? sameObstacle
                        ? calculateBound(max.x + _characterSize.x, min.z - _characterSize.z)
                        : calculateBound(min.x - _characterSize.x, max.z + _characterSize.z)
                    : calculateBound(max.x + _characterSize.x, min.z - _characterSize.z);
            }
            else if (currentPosition.z < min.z)
            {
                furthestBound = currentPosition.x - currentTargetPosition.x < currentPosition.z - currentTargetPosition.z
                    ? sameObstacle
                        ? calculateBound(max.x + _characterSize.x, max.z + _characterSize.z)
                        : calculateBound(min.x - _characterSize.x, min.z - _characterSize.z)
                    : calculateBound(max.x + _characterSize.x, max.z + _characterSize.z);
            }
            else
            {
                furthestBound = currentPosition.z > obstacleCenter.z
                    ? calculateBound(max.x + _characterSize.x, min.z - _characterSize.z)
                    : calculateBound(max.x + _characterSize.x, max.z + _characterSize.z);
            }
        }
        else if (currentPosition.x < min.x)
        {
            if (currentPosition.z > max.z)
            {
                furthestBound = currentPosition.x - currentTargetPosition.x < currentPosition.z - currentTargetPosition.z
                    ? sameObstacle
                        ? calculateBound(min.x - _characterSize.x, min.z - _characterSize.z)
                        : calculateBound(max.x + _characterSize.x, max.z + _characterSize.z)
                    : calculateBound(min.x - _characterSize.x, min.z - _characterSize.z);
            }
            else if (currentPosition.z < min.z)
            {
                furthestBound = currentPosition.x - currentTargetPosition.x < currentPosition.z - currentTargetPosition.z
                    ? sameObstacle
                        ? calculateBound(min.x - _characterSize.x, max.z + _characterSize.z)
                        : calculateBound(max.x + _characterSize.x, min.z - _characterSize.z)
                    : calculateBound(min.x - _characterSize.x, max.z + _characterSize.z);
            }
            else
            {
                furthestBound = currentPosition.z > obstacleCenter.z
                    ? calculateBound(min.x - _characterSize.x, min.z - _characterSize.z)
                    : calculateBound(min.x - _characterSize.x, max.z + _characterSize.z);
            }
        }
        else
        {
            if (currentPosition.z > max.z)
            {
                furthestBound = currentPosition.x > obstacleCenter.x
                    ? calculateBound(min.x - _characterSize.x, max.z + _characterSize.z)
                    : calculateBound(max.x + _characterSize.x, max.z + _characterSize.z);
            }
            else if (currentPosition.z < min.z)
            {
                furthestBound = currentPosition.x > obstacleCenter.x
                    ? calculateBound(min.x - _characterSize.x, min.z - _characterSize.z)
                    : calculateBound(max.x + _characterSize.x, min.z - _characterSize.z);
            }
            else
            {
                Debug.LogError("You are inside the obstacle");
                furthestBound = closestBound;
            }
        }

        Vector3 calculateBound(float x, float z)
        {
            return new Vector3(x, currentPosition.y, z);
        }
    }

    void checkMoverType()
    {
        Vector3 characterCanMoveInY = _characterSize;
        // Need to put in what type of obstacle it is, and therefore whether the different types of movers can move over, under or through it.
        if (!_agent.MoverTypes.Contains(MoverType.Fly) || !_agent.MoverTypes.Contains(MoverType.Dig))
        {
            characterCanMoveInY.y = 0;
        }
    }

    Vector3 nextAvailablePoint(int pathID, Vector3 currentPosition, Vector3 currentTargetPosition, Collider currentCollider, out Collider returnedCollider, int iterations = 0, Collider previousCollider = null)
    {
        returnedCollider = currentCollider;

        iterations++;
        if (iterations > 100) { Debug.Log($"{pathID} Iterated to {iterations}"); return currentTargetPosition; }

        if (Physics.Raycast(currentPosition, currentTargetPosition - currentPosition, out RaycastHit hit, (currentTargetPosition - currentPosition).magnitude))
        {
            getMinAndMax(hit.collider, out Vector3 min, out Vector3 max);
            getDistances(currentPosition, min, max, out float dxMin, out float dxMax, out float dyMin, out float dymax, out float dzMin, out float dzMax);
            getBounds(pathID, currentPosition, currentTargetPosition, hit.collider == previousCollider, hit, min, max, dxMin, dxMax, dzMin, dzMax, out Vector3 closestBound, out Vector3 furthestBound);

            return nextAvailablePoint(pathID, currentPosition, hit.collider == previousCollider ? furthestBound : closestBound, hit.collider, out returnedCollider, previousCollider: currentCollider);
        }
        else
        {
            return currentTargetPosition;
        }
    }
}
