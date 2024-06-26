using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;

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

    public void UpdatePathfinder(Vector3 startPosition, Vector3 targetPosition, Vector3 characterSize, Controller_Agent agent)
    {
        _startPosition = startPosition;
        _targetPosition = targetPosition;
        _characterSize = characterSize;
        _agent = agent;

        if (_bestPath.HasValue)
        {
            Manager_Game.Instance.StartVirtualCoroutine(_agent.TestRecalculate(_recalculatePath()));
        }
    }

    IEnumerator _recalculatePath()
    {
        List<(Vector3 position, Collider previousCollider)> path = _bestPath.Value.value.pointPath;

        bool earlierHitAvailable = false;
        int earlierHitIteration = -1;

        for (int i = 0; i < path.Count; i++)
        {
            if (!earlierHitAvailable) Debug.DrawRay(path[i].position, _targetPosition.Value - path[i].position, Color.magenta, 5);

            if (earlierHitAvailable)
            {
                path.RemoveAt(i);
            }
            else
            {
                Physics.Raycast(path[i].position, _targetPosition.Value - path[i].position, out RaycastHit hit);

                if (!hit.transform.name.Contains("Player")) continue;

                earlierHitAvailable = true;
                earlierHitIteration = i;

                Debug.Log($"earlierHit available at {earlierHitIteration} : {path[i].position}");
            }
        }

        if (earlierHitAvailable)
        {
            float distance = 0;

            for (int i = 1; i < path.Count; i++)
            {
                distance += Vector3.Distance(path[i].position, path[i - 1].position);
            }

            _bestPath = ((_bestPath.Value.key.targetPosInt, _bestPath.Value.key.pathID), (path, distance, _bestPath.Value.value.reachedTarget));

            _findBestPath(_bestPath.Value.value.pointPath[earlierHitIteration], hit: null);

            yield return Manager_Game.Instance.StartVirtualCoroutine(_agent.MoveToTest(_bestPath.Value.value.pointPath, _bestPath.Value.value.distance));

            _bestPath = null;
        }
        else
        {
            Physics.Raycast(path[^1].position, _targetPosition.Value - path[^1].position, out RaycastHit hit);

            if (hit.transform.name.Contains("Player"))
            {
                path[^1] = (_targetPosition.Value, path[^1].previousCollider);

                _bestPath = ((_bestPath.Value.key.targetPosInt, _bestPath.Value.key.pathID), (path, _bestPath.Value.value.distance, _bestPath.Value.value.reachedTarget));

                yield return Manager_Game.Instance.StartVirtualCoroutine(_agent.MoveToTest(_bestPath.Value.value.pointPath, _bestPath.Value.value.distance));

                _bestPath = null;
            }
            else
            {
                MoveFromStart();
            }
        }
    }

    public IEnumerator MoveFromStart()
    {
        if (!Physics.Raycast(_startPosition, _targetPosition.Value - _startPosition, out RaycastHit hit))
        {
            Debug.Log($"Raycast hit nothing somehow.");
            yield break;
        }

        if (hit.transform.name.Contains("Player"))
        {
            yield return Manager_Game.Instance.StartVirtualCoroutine(_agent.MoveToTest(new List<(Vector3 position, Collider)> { (_targetPosition.Value, null) }, Vector3.Distance(_startPosition, _targetPosition.Value)));
        }
        else if (hit.transform.name.Contains("Wall"))
        {
            yield return Manager_Game.Instance.StartVirtualCoroutine((_findBestPath((_startPosition, null), hit)));

            if (_bestPath == null) { Debug.Log($"Best Path is null after running pathfinder."); yield break; }

            yield return Manager_Game.Instance.StartVirtualCoroutine(_agent.MoveToTest(_bestPath.Value.value.pointPath, _bestPath.Value.value.distance));

            Debug.Log("Nulled Best Path");

            _bestPath = null;
        }
        else
        {
            Debug.Log($"Hit at position: {hit.point} but was not a wall, instead was a {hit.transform.name}");
        }
    }

    IEnumerator _findBestPath((Vector3 position, Collider previousCollider) currentPosition, RaycastHit? hit, int pathID = 0)
    {
        if (pathID > 100) yield break;

        if (!_targetPosition.HasValue) { Debug.Log($"TargetPosition: {_targetPosition} is null."); yield break; }

        var bestPath = _bestPath;

        (Vector3Int, int) pathKey = (Vector3Int.RoundToInt(_targetPosition.Value), pathID);

        if (!hit.HasValue)
        {
            _addOrUpdatePaths(pathKey,
                (_targetPosition.Value, currentPosition.previousCollider, Vector3.Distance(_targetPosition.Value, currentPosition.position)),
                null,
                currentPosition,
                bestPath.Value);

            yield break;
        }

        ((Vector3 position, Collider previousCollider, float distance) position_1,
            (Vector3 position, Collider previousCollider, float distance)? position_2)
            = _selectVertices(pathID, hit.Value, currentPosition, hit.Value.collider);

        _addOrUpdatePaths(pathKey, position_1, position_2, currentPosition, bestPath);

        for (int i = 0; i < _pathCount; i++)
        {
            (Vector3Int, int) pathKeyI = (Vector3Int.RoundToInt(_targetPosition.Value), i);

            if (!_allPaths.ContainsKey(pathKeyI)) continue;

            var path = _allPaths[pathKeyI];

            if (path.reachedTarget)
            {
                //if (pathExists(pathKeyI)) yield break;
                continue;
            }

            (Vector3 position, Collider previousCollider) lastPosition = path.pointPath.Last();

            if (!Physics.Raycast(lastPosition.position, _targetPosition.Value - lastPosition.position, out RaycastHit pathHit)) { Debug.Log("Raycast hit nothing."); continue; }

            string hitName = pathHit.transform.name;

            if (hitName.Contains("Player"))
            {
                path.pointPath.Add((pathHit.point, path.pointPath.Last().previousCollider));
                path = (path.pointPath, path.distance += Vector3.Distance(pathHit.point, lastPosition.position), true);

                if (path.distance < _shortestDistance)
                {
                    _bestPath = (pathKeyI, path);
                    _shortestDistance = path.distance;
                }
            }
            else if (pathHit.transform.name.Contains("Wall"))
            {
                Manager_Game.Instance.StartVirtualCoroutine(_findBestPath(lastPosition, pathHit, pathID: i));
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

    void _addOrUpdatePaths((Vector3Int, int) pathKey, (Vector3 position, Collider previousCollider, float distance) position_1, (Vector3 position, Collider previousCollider, float distance)? position_2, 
        (Vector3, Collider) currentPosition, ((Vector3Int targetPosInt, int pathID) key, (List<(Vector3 position, Collider previousCollider)> pointPath, float distance, bool reachedTarget) value)? preExistingPath)
    {
        var existingPath = _allPaths.ContainsKey(pathKey)
            ? _allPaths.FirstOrDefault(kvp => kvp.Value.Item1.SequenceEqual(_allPaths[pathKey].Item1))
            : default(KeyValuePair<(Vector3Int, int), (List<(Vector3, Collider)>, float, bool)>);

        if (preExistingPath.HasValue)
        {
            _updatePathDistances(position_1, position_2, preExistingPath.Value.key, out float distancePath_01, out float distancePath_02);
            addToPath(distancePath_01, (position_1.position, position_1.previousCollider), distancePath_02, position_2.HasValue ? (position_2.Value.position, position_2.Value.previousCollider) : null, preExistingPath.Value.key, true);
        }
        else if (!existingPath.Equals(default(KeyValuePair<(Vector3Int, int), (List<(Vector3, Collider)>, float, bool)>)))
        {
            if (_allPaths[existingPath.Key].reachedTarget) return;

            _updatePathDistances(position_1, position_2, existingPath.Key, out float distancePath_01, out float distancePath_02);
            addToPath(distancePath_01, (position_1.position, position_1.previousCollider), distancePath_02, position_2.HasValue ? (position_2.Value.position, position_2.Value.previousCollider) : null, existingPath.Key);
        }
        else if (_allPaths.ContainsKey(pathKey))
        {
            _updatePathDistances(position_1, position_2, pathKey, out float distancePath_01, out float distancePath_02);
            addToPath(distancePath_01, (position_1.position, position_1.previousCollider), distancePath_02, position_2.HasValue ? (position_2.Value.position, position_2.Value.previousCollider) : null, pathKey);
        }
        else
        {
            addToPath(position_1.distance, (position_1.position, position_1.previousCollider), 
                position_2.HasValue ? position_2.Value.distance : float.PositiveInfinity, position_2.HasValue ? (position_2.Value.position, position_2.Value.previousCollider) : null, null);
        }

        void addToPath(float distance_01, (Vector3, Collider) position_01, float distance_02, (Vector3, Collider)? position_02, (Vector3Int, int)? key, bool preExistingPathUsed = false)
        {
            if (preExistingPathUsed)
            {
                insertPathWithPosition((position_01), distance_01, key.HasValue ? key.Value.Item2 + 1 : _pathCount, true);
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
                    insertPathWithPosition(position_01, distance_01, key.HasValue ? key.Value.Item2 + 1 : _pathCount);
                    _pathCount++;
                }
            }
            
            if (key.HasValue) _allPaths.Remove(pathKey);

            void insertPathWithPosition((Vector3, Collider) position, float distance, int pathID, bool preExistingPathUsed = false)
            {
                if (preExistingPathUsed)
                {
                    List<(Vector3, Collider)> pathPoints = new List<(Vector3, Collider)>(preExistingPath.Value.value.pointPath) { position };
                    _insertPath(pathID, pathPoints, distance, false);
                }
                else
                {
                    List<(Vector3, Collider)> pathPoints = key.HasValue
                    ? new List<(Vector3, Collider)>(_allPaths[key.Value].pointPath) { position }
                    : new List<(Vector3, Collider)> { currentPosition, position };

                    _insertPath(pathID, pathPoints, distance, false);
                }
            }
        }
    }

    void _updatePathDistances((Vector3 position, Collider previousCollider, float distance) position_1, (Vector3 position, Collider previousCollider, float distance)? position_2, (Vector3Int, int) key,
        out float distance_01, out float distance_02)
    {
        distance_01 = position_1.distance + _allPaths[key].distance;
        distance_02 = position_2.HasValue ? position_2.Value.distance + _allPaths[key].distance : float.PositiveInfinity;
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
        (Vector3 position, Collider previousCollider, float distance)? position_2 = sameObstacle ? (currentPosition.position, currentPosition.previousCollider, float.PositiveInfinity) : null;

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
        if (iterations > 100) { Debug.Log($"{pathID} Iterated to {iterations}"); return currentTargetPosition; }

        if (Physics.Raycast(currentPosition.position, currentTargetPosition - currentPosition.position, out RaycastHit hit, (currentTargetPosition - currentPosition.position).magnitude))
        {
            getMinAndMax(hit.collider, out Vector3 min, out Vector3 max);
            getDistances(currentPosition.position, min, max, out float dxMin, out float dxMax, out float dyMin, out float dymax, out float dzMin, out float dzMax);
            getBounds(pathID, currentPosition.position, currentTargetPosition, hit.collider == currentPosition.previousCollider, hit, min, max, dxMin, dxMax, dzMin, dzMax, out Vector3 closestBound, out Vector3 furthestBound);

            return nextAvailablePoint(pathID, currentPosition, hit.collider == currentPosition.previousCollider ? furthestBound : closestBound, hit.collider, out returnedCollider);
        }
        else
        {
            return currentTargetPosition;
        }
    }
}
