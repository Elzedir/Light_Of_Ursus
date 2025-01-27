using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;

public class Pathfinder_Vertex_3D
{
    Dictionary<(Vector3Int targetPosInt, int pathID), (List<(Vector3 position, Collider previousCollider)> pointPath, float distance, bool reachedTarget)> _allPaths = new();
    ((Vector3Int targetPosInt, int pathID) key, (List<(Vector3 position, Collider previousCollider)> pointPath, float distance, bool reachedTarget) value)? _bestPath;
    int _pathCount = 0;
    float _allowedPathExtraPercent = 1.2f;
    float _shortestDistance = float.PositiveInfinity;
    Vector3 _startPosition;
    Vector3? _targetPosition;
    Vector3 _characterSize;
    Controller_Agent _agent;

    //Can also set 3rd previous collider, and do a check that if we are going around the same collider 3 times, then we might as well check the other direction, do a move from start.

    public void ResetBestPath() {  _bestPath = null; }

    public void UpdatePathfinder(Vector3 startPosition, Vector3 targetPosition, Vector3 characterSize, Controller_Agent agent)
    {
        _startPosition = startPosition;
        _targetPosition = targetPosition;
        _characterSize = characterSize;
        _agent = agent;

        _recalculatePath();
    }

    void _recalculatePath()
    {
        Physics.Raycast(_startPosition, _targetPosition.Value - _startPosition, out RaycastHit directHitCheck);

        Debug.DrawRay(_startPosition, _targetPosition.Value - _startPosition, Color.magenta, 50);

        if (directHitCheck.transform.name.Contains("Player") || !_bestPath.HasValue || _bestPath == null)
        {
            Manager_Game.S_Instance.StartCoroutine(_agent.RunPathfinding(MoveFromStart()));

            return;
        }

        var path = _bestPath.Value.value.pointPath;

        if (_targetPosition.HasValue && _bestPath.HasValue)
        {
            if (Vector3Int.RoundToInt(_targetPosition.Value) == _bestPath.Value.key.targetPosInt)
            {
                path[^1] = (_targetPosition.Value, path[^1].previousCollider);

                _updateBestPath(path, true);

                Manager_Game.S_Instance.StartCoroutine(_agent.MoveToTest(_bestPath.Value.value.pointPath, _bestPath.Value.value.distance, _bestPath.Value.key.pathID));

                return;
            }
        }

        bool earlierHitAvailable = false;
        RaycastHit? earlyHit = null;

        for (int i = _agent.CurrentPathIndex; i < path.Count; i++)
        {
            if (earlierHitAvailable)
            {
                path.RemoveAt(i);
                i--;
                continue;
            }
            else
            {
                Physics.Raycast(path[i].position, _targetPosition.Value - path[i].position, out RaycastHit hit);

                if (!hit.transform.name.Contains("Player")) continue;

                earlyHit = hit;
                earlierHitAvailable = true;
            }
        }

        if (earlierHitAvailable)
        {
            if (_bestPath.Value.value.pointPath.Count > 2)
            {
                if (!earlyHit.HasValue)
                {
                    Debug.Log("RaycastHit earlyHit is null");
                    return;
                }

                _updateBestPath(path);

                Manager_Game.S_Instance.StartCoroutine(_startVirtualCoroutinesForBestPath(earlyHit.Value));
            }
            else
            {
                Manager_Game.S_Instance.StartCoroutine(_agent.RunPathfinding(MoveFromStart()));
            }
        }
        else
        {
            if (Vector3.Distance(_targetPosition.Value, path[^1].position) < 0.25f) return;

            Physics.Raycast(path[^1].position, _targetPosition.Value - path[^1].position, out RaycastHit hit);

            if (hit.transform.name.Contains("Player") && _bestPath.Value.value.pointPath.Count > 2)
            {
                path[^1] = (_targetPosition.Value, path[^1].previousCollider);

                _updateBestPath(path, true);

                Manager_Game.S_Instance.StartCoroutine(_agent.MoveToTest(_bestPath.Value.value.pointPath, _bestPath.Value.value.distance, _bestPath.Value.key.pathID));
            }
            else
            {
                Manager_Game.S_Instance.StartCoroutine(_agent.RunPathfinding(MoveFromStart()));
            }
        }
    }

    void _updateBestPath(List<(Vector3 position, Collider previousCollider)> path, bool reachedTarget = false)
    {
        float distance = _calculatePathDistance(path);
        _bestPath = ((_bestPath.Value.key.targetPosInt, _bestPath.Value.key.pathID), (path, distance, reachedTarget));
    }

    float _calculatePathDistance(List<(Vector3 position, Collider previousCollider)> path)
    {
        float distance = 0;
        for (int i = 1; i < path.Count; i++)
        {
            distance += Vector3.Distance(path[i].position, path[i - 1].position);
        }
        return distance;
    }

    IEnumerator _startVirtualCoroutinesForBestPath(RaycastHit earlyHit)
    {
        yield return Manager_Game.S_Instance.StartCoroutine(
            _findBestPath(_bestPath.Value.value.pointPath[^1], earlyHit, earlyHit: true));

        yield return Manager_Game.S_Instance.StartCoroutine(
            _agent.MoveToTest(_bestPath.Value.value.pointPath, _bestPath.Value.value.distance, _bestPath.Value.key.pathID));
    }

    public IEnumerator MoveFromStart()
    {
        _allPaths.Clear();
        _bestPath = null;

        if (!Physics.Raycast(_startPosition, _targetPosition.Value - _startPosition, out RaycastHit hit))
        {
            Debug.Log($"Raycast hit nothing somehow.");
            yield break;
        }

        if (hit.transform.name.Contains("Player"))
        {
            yield return Manager_Game.S_Instance.StartCoroutine(_agent.RunPathfinding(_agent.MoveToTest(new List<(Vector3 position, Collider)> { (_startPosition, null), (_targetPosition.Value, null) }, Vector3.Distance(_startPosition, _targetPosition.Value))));
        }
        else if (hit.transform.name.Contains("Wall"))
        {
            yield return Manager_Game.S_Instance.StartCoroutine((_findBestPath((_startPosition, null), hit)));

            if (_bestPath == null) { Debug.Log($"Best Path is null after running pathfinder."); yield break; }

            yield return Manager_Game.S_Instance.StartCoroutine(_agent.RunPathfinding(_agent.MoveToTest(_bestPath.Value.value.pointPath, _bestPath.Value.value.distance, _bestPath.Value.key.pathID)));
        }
        else
        {
            Debug.Log($"Hit at position: {hit.point} but was not a wall, instead was a {hit.transform.name}");
        }
    }

    IEnumerator _findBestPath((Vector3 position, Collider previousCollider) currentPosition, RaycastHit hit, int pathID = 0, bool earlyHit = false)
    {
        if (pathID > 100 || !_targetPosition.HasValue)
        {
            Debug.Log($"TargetPosition: {_targetPosition} is null or pathID exceeded.");
            yield break;
        }

        (Vector3Int, int) pathKey = (Vector3Int.RoundToInt(_targetPosition.Value), pathID);

        if (earlyHit)
        {
            _bestPath.Value.value.pointPath.Add((hit.point, currentPosition.previousCollider));
            _bestPath = ((_bestPath.Value.key.targetPosInt, _bestPath.Value.key.pathID), (_bestPath.Value.value.pointPath, _bestPath.Value.value.distance + Vector3.Distance(hit.point, currentPosition.position), true));
            _shortestDistance = _bestPath.Value.value.distance;
        }
        else
        {
            (var position_1, var position_2) = _selectVertices(pathID, hit, currentPosition, hit.collider);
            _addOrUpdatePaths(pathKey, position_1, position_2, currentPosition);
        }

        yield return Manager_Game.S_Instance.StartCoroutine(_evaluatePaths(pathID));
    }

    IEnumerator _evaluatePaths(int pathID)
    {
        for (int i = 0; i < _pathCount; i++)
        {
            (Vector3Int, int) pathKeyI = (Vector3Int.RoundToInt(_targetPosition.Value), i);

            if (!_allPaths.ContainsKey(pathKeyI)) continue;

            var path = _allPaths[pathKeyI];

            if (path.reachedTarget) continue;

            var lastPosition = path.pointPath[^1];

            if (!Physics.Raycast(lastPosition.position, _targetPosition.Value - lastPosition.position, out RaycastHit pathHit))
            {
                Debug.Log("Raycast hit nothing.");
                continue;
            }

            _handleRaycastHit(path, pathKeyI, pathHit, lastPosition, i);
        }

        yield return null;
    }

    void _handleRaycastHit((List<(Vector3 position, Collider previousCollider)> pointPath, float distance, bool reachedTarget) path,
                              (Vector3Int, int) pathKeyI,
                              RaycastHit pathHit,
                              (Vector3 position, Collider previousCollider) lastPosition,
                              int pathID)
    {
        Vector3 roundedPoint = new Vector3(
        (float)Math.Round(pathHit.point.x, 2),
        (float)Math.Round(pathHit.point.y, 2),
        (float)Math.Round(pathHit.point.z, 2)
        );

        if (pathHit.transform.name.Contains("Player"))
        {
            path.pointPath.Add((pathHit.point, path.pointPath[^1].previousCollider));
            path.distance += Vector3.Distance(pathHit.point, lastPosition.position);
            path.reachedTarget = true;

            if (path.distance < _shortestDistance)
            {
                _bestPath = (pathKeyI, path);
                _shortestDistance = path.distance;
            }
        }
        else if (pathHit.transform.name.Contains("Wall"))
        {
            Manager_Game.S_Instance.StartCoroutine(_findBestPath(lastPosition, pathHit, pathID));
        }
    }

    void _addOrUpdatePaths((Vector3Int, int) pathKey, (Vector3 position, Collider previousCollider, float distance) position_1, (Vector3 position, Collider previousCollider, float distance)? position_2,
        (Vector3, Collider) currentPosition, ((Vector3Int targetPosInt, int pathID) key, (List<(Vector3 position, Collider previousCollider)> pointPath, float distance, bool reachedTarget) value)? preExistingPath = null)
    {
        if (!_allPaths.ContainsKey(pathKey))
        {
            addToPath(position_2.HasValue ? position_2.Value.distance : float.PositiveInfinity, position_2.HasValue ? (position_2.Value.position, position_2.Value.previousCollider) : null,
                position_1.distance, (position_1.position, position_1.previousCollider), null);

            return;
        }

        if (preExistingPath.HasValue)
        {
            _updatePathDistances(position_1, position_2, preExistingPath.Value.key, out float distancePrePath_01, out float distancePrePath_02);
            addToPath(distancePrePath_01, (position_1.position, position_1.previousCollider), 
                distancePrePath_02, position_2.HasValue ? (position_2.Value.position, position_2.Value.previousCollider) : null, preExistingPath.Value.key, preExistingPathUsed: true);

            return;
        }

        if (_allPaths[pathKey].reachedTarget) { Debug.LogError("Equal path already reached target"); return; }

        var allIdenticalPaths = _allPaths.Where(kvp => kvp.Key.Item1 == pathKey.Item1 && kvp.Value.reachedTarget).ToList();
        float shortestDistance = float.PositiveInfinity;
        var firstIdenticalPath = default(KeyValuePair<(Vector3Int, int), (List<(Vector3, Collider)>, float, bool)>);

        foreach (var path in allIdenticalPaths)
        {
            if (path.Value.distance < shortestDistance)
            {
                shortestDistance = path.Value.distance;
                firstIdenticalPath = path;
            }
        }
        
        if (!firstIdenticalPath.Equals(default(KeyValuePair<(Vector3Int, int), (List<(Vector3, Collider)>, float, bool)>)))
        {
            firstIdenticalPath.Value.Item1.RemoveAt(firstIdenticalPath.Value.Item1.Count - 1);

            _updatePathDistances(position_1, position_2, firstIdenticalPath.Key, out float distanceIdenticalPath_01, out float distanceIdenticalPath_02);
            addToPath(distanceIdenticalPath_01, (position_1.position, position_1.previousCollider), 
                distanceIdenticalPath_02, position_2.HasValue ? (position_2.Value.position, position_2.Value.previousCollider) : null, firstIdenticalPath.Key, removePath: false);
        }

        _updatePathDistances(position_1, position_2, pathKey, out float distancePath_01, out float distancePath_02);
        addToPath(distancePath_01, (position_1.position, position_1.previousCollider), distancePath_02, position_2.HasValue ? (position_2.Value.position, position_2.Value.previousCollider) : null, pathKey);

        void _updatePathDistances((Vector3 position, Collider previousCollider, float distance) position_1, (Vector3 position, Collider previousCollider, float distance)? position_2, (Vector3Int, int) key,
        out float distance_01, out float distance_02)
        {
            distance_01 = position_1.distance + _allPaths[key].distance;
            distance_02 = position_2.HasValue ? position_2.Value.distance + _allPaths[key].distance : float.PositiveInfinity;
        }

        void addToPath(float distance_01, (Vector3, Collider)? position_01, float distance_02, (Vector3, Collider)? position_02, (Vector3Int, int)? key, bool preExistingPathUsed = false, bool removePath = true)
        {
            if (preExistingPathUsed)
            {
                insertPathWithPosition((position_01.Value), distance_01, key.HasValue ? key.Value.Item2 + 1 : _pathCount, preExistingPathUsed: true);
                _pathCount++;
            }
            else
            {
                if (position_02.HasValue && distance_02 < _shortestDistance * _allowedPathExtraPercent)
                {
                    insertPathWithPosition(position_02.Value, distance_02, key.HasValue ? key.Value.Item2 + 1 : _pathCount);
                    _pathCount++;
                }

                if (distance_01 < _shortestDistance * _allowedPathExtraPercent)
                {
                    insertPathWithPosition(position_01.Value, distance_01, key.HasValue ? key.Value.Item2 + 1 : _pathCount);
                    _pathCount++;
                }
            }

            if (key.HasValue && removePath) _allPaths.Remove(pathKey);

            void insertPathWithPosition((Vector3, Collider) position, float distance, int pathID, bool preExistingPathUsed = false)
            {
                if (preExistingPathUsed)
                {
                    List<(Vector3, Collider)> pathPoints = new List<(Vector3, Collider)>(preExistingPath.Value.value.pointPath) { position };

                    _insertPath(pathID, pathPoints, distance, reachesTarget: true);
                }
                else
                {
                    List<(Vector3, Collider)> pathPoints = key.HasValue
                    ? new List<(Vector3, Collider)>(_allPaths[key.Value].pointPath) { position }
                    : new List<(Vector3, Collider)> { currentPosition, position };

                    _insertPath(pathID, pathPoints, distance, reachesTarget: false);
                }
            }
        }
    }

    void _insertPath(int newPathID, List<(Vector3, Collider)> pathPoints, float distance, bool reachesTarget)
    {
        Dictionary<(Vector3Int, int), (List<(Vector3, Collider)>, float, bool)> updatedPaths = new();

        foreach (var path in _allPaths)
        {
            updatedPaths[(path.Key.Item1, path.Key.Item2 >= newPathID ? path.Key.Item2 + 1 : path.Key.Item2)] = path.Value;
        }

        updatedPaths[(Vector3Int.RoundToInt(_targetPosition.Value), newPathID)] = (pathPoints, distance, reachesTarget);
        _allPaths = updatedPaths;
    }

    ((Vector3 position, Collider previousCollider, float distance) position_1, (Vector3 position, Collider previousCollider, float distance)? position_2) _selectVertices(
        int pathID, RaycastHit hit, (Vector3 position, Collider previousCollider) currentPosition, Collider currentCollider)
    {
        bool sameObstacle = currentCollider == currentPosition.previousCollider;

        (Vector3 position, Collider previousCollider, float distance) position_1 = (currentPosition.position, currentPosition.previousCollider, float.PositiveInfinity);
        (Vector3 position, Collider previousCollider, float distance)? position_2 = sameObstacle ? null : (currentPosition.position, currentPosition.previousCollider, float.PositiveInfinity);

        checkMoverType(); // Doesn't do anything for now. Assume only ground movement.

        getMinAndMax(hit.collider, out Vector3 min, out Vector3 max);
        getDistances(currentPosition.position, min, max, out float dxMin, out float dxMax, out float dyMin, out float dymax, out float dzMin, out float dzMax);
        getBounds(pathID, currentPosition.position, _targetPosition.Value, sameObstacle, hit, min, max, dxMin, dxMax, dzMin, dzMax, out Vector3 closestBound, out Vector3 furthestBound);

        position_1.position = (nextAvailablePoint(pathID, currentPosition, sameObstacle ? furthestBound : closestBound, currentCollider, out position_1.previousCollider));
        position_1.distance = Vector3.Distance(currentPosition.position, position_1.position);
        position_1.position.y = currentPosition.position.y; // Temporary while flying isn't implemented

        if (position_2.HasValue)
        {
            var tempPosition = nextAvailablePoint(pathID, currentPosition, furthestBound, currentCollider, out Collider tempCollider);
            var tempDistance = Vector3.Distance(currentPosition.position, tempPosition);
            tempPosition.y = currentPosition.position.y;

            position_2 = (tempPosition, tempCollider, tempDistance);
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

    Vector3 nextAvailablePoint(int pathID, (Vector3 position, Collider previousCollider) currentPosition, Vector3 currentTargetPosition, Collider currentCollider, out Collider returnedCollider, int iterations = 0)
    {
        returnedCollider = currentCollider;

        iterations++;
        if (iterations > 5) return currentTargetPosition; //Debug.Log($"{pathID} Iterated to {iterations}");  }

            if (Physics.Raycast(currentPosition.position, currentTargetPosition - currentPosition.position, out RaycastHit hit, (currentTargetPosition - currentPosition.position).magnitude))
        {
            if (hit.transform.name.Contains("Player"))
            {
                Debug.LogError("Hit player");
                return currentTargetPosition;
            }

            getMinAndMax(hit.collider, out Vector3 min, out Vector3 max);
            getDistances(currentPosition.position, min, max, out float dxMin, out float dxMax, out float dyMin, out float dymax, out float dzMin, out float dzMax);
            getBounds(pathID, currentPosition.position, currentTargetPosition, hit.collider == currentPosition.previousCollider, hit, min, max, dxMin, dxMax, dzMin, dzMax, out Vector3 closestBound, out Vector3 furthestBound);

            return nextAvailablePoint(pathID, currentPosition, hit.collider == currentPosition.previousCollider ? furthestBound : closestBound, hit.collider, out returnedCollider, iterations);
        }
        else
        {
            return currentTargetPosition;
        }
    }
}
