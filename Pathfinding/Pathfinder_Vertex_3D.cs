using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder_Vertex_3D
{
    Dictionary<(Vector3Int, int), (List<Vector3>, Collider, float, bool)> _allPaths = new();
    int _pathCount = 0;
    List<Vector3> _bestPath = new();
    float _shortestDistance = float.PositiveInfinity;
    Vector3 _startPosition;
    Vector3? _targetPosition;
    Vector3 _characterSize;
    Controller_Agent _agent;

    public void SetPathfinder(Vector3 startPosition, Vector3 targetPosition, Vector3 characterSize, Controller_Agent agent)
    {
        _startPosition = startPosition;
        _targetPosition = targetPosition;
        _characterSize = characterSize;
        _agent = agent;

        // Call a function calledRecalculatePath(). Either adapt the end point, or from one of the earlier points, or recalculate the whole path.
    }

    public IEnumerator Move()
    {
        yield return new WaitForSeconds(0.5f);

        if (Physics.Raycast(_startPosition, _targetPosition.Value - _startPosition, out RaycastHit hit))
        {
            Debug.DrawRay(_startPosition, _targetPosition.Value - _startPosition, Color.red, 50.0f);

            if (hit.transform.name.Contains("Player"))
            {
                Debug.Log($"Hit player at {hit.point}. Direct path to target: {_targetPosition}.");

                // Implement your movement logic here
                yield break;
            }
            else if (hit.transform.name.Contains("Wall"))
            {
                yield return Manager_Game.Instance.StartVirtualCoroutine((_findBestPath(_agent, _startPosition, _targetPosition.Value, _characterSize, hit)));

                Vector3 lastPosition = _startPosition;

                if (_bestPath == null) { Debug.Log($"Best Path is null"); yield break; }

                _agent.MoveToTest(_bestPath);
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

    IEnumerator _findBestPath(Controller_Agent agent, Vector3 startPosition, Vector3 targetPosition, Vector3 characterSize, RaycastHit hit, Collider previousCollider = null, int pathID = 0)
    {
        if (pathID > 100) yield break;

        (Vector3Int, int) pathKey = (Vector3Int.RoundToInt(targetPosition), pathID);

        ((Vector3 position, float distance, Collider previousCollider) position_1,
            (Vector3? position, float distance, Collider previousCollider) position_2)
            = _selectVertices(pathID, agent, hit, startPosition, targetPosition, characterSize, hit.collider, previousCollider);

        _addOrUpdatePaths(pathKey, position_1, position_2, startPosition, targetPosition);

        for (int i = 0; i < _pathCount; i++)
        {
            (Vector3Int, int) pathKeyI = (Vector3Int.RoundToInt(targetPosition), i);

            if (!_allPaths.ContainsKey(pathKeyI)) continue;

            var path = _allPaths[pathKeyI];

            if (path.Item4)
            {
                //if (pathExists(pathKeyI)) yield break;
                continue;
            }

            Vector3 lastPosition = path.Item1.Last();

            if (!Physics.Raycast(lastPosition, targetPosition - lastPosition, out RaycastHit pathHit)) continue;

            string hitName = pathHit.transform.name;

            if (hitName.Contains("Player"))
            {
                path.Item1.Add(pathHit.point);
                path.Item3 += Vector3.Distance(pathHit.point, lastPosition);
                path = (path.Item1, path.Item2, path.Item3, true);

                if (path.Item3 < _shortestDistance)
                {
                    _bestPath = path.Item1;
                    _shortestDistance = path.Item3;
                }
            }
            else if (pathHit.transform.name.Contains("Wall"))
            {
                Manager_Game.Instance.StartVirtualCoroutine(_findBestPath(agent, lastPosition, targetPosition, characterSize, pathHit, previousCollider: path.Item2, pathID: i));
            }
        }

        bool pathExists((Vector3Int, int) pathKeyI)
        {
            foreach (var path in _allPaths)
            {
                if (path.Key.Item1 != pathKeyI.Item1 ||
                    path.Value.Item1[pathKeyI.Item2] != startPosition ||
                    path.Value.Item1.Last() != targetPosition
                    )
                    continue;

                return true;
            }

            return false;
        }
    }

    void _addOrUpdatePaths((Vector3Int, int) pathKey, (Vector3 position, float distance, Collider previousCollider) position_1, (Vector3? position, float distance, Collider previousCollider) position_2,
        Vector3 startPosition, Vector3 targetPosition)
    {
        var existingPath = _allPaths.ContainsKey(pathKey)
            ? _allPaths.FirstOrDefault(kvp => kvp.Value.Item1.SequenceEqual(_allPaths[pathKey].Item1))
            : default(KeyValuePair<(Vector3Int, int), (List<Vector3>, Collider, float, bool)>);

        if (!existingPath.Equals(default(KeyValuePair<(Vector3Int, int), (List<Vector3>, Collider, float, bool)>)))
        {
            if (_allPaths[existingPath.Key].Item4) return;

            _updatePathDistances(position_1, position_2, existingPath.Key, out float distancePath_01, out float distancePath_02);
            addOrUpdatePath(distancePath_01, position_1.position, distancePath_02, position_2.position, existingPath.Key);
        }
        else if (_allPaths.ContainsKey(pathKey))
        {
            _updatePathDistances(position_1, position_2, pathKey, out float distancePath_01, out float distancePath_02);
            addOrUpdatePath(distancePath_01, position_1.position, distancePath_02, position_2.position, pathKey);
        }
        else
        {
            addOrUpdatePath(position_1.distance, position_1.position, position_2.distance, position_2.position, null);
        }

        void addOrUpdatePath(float distance_01, Vector3 position_01, float distance_02, Vector3? position_02, (Vector3Int, int)? key)
        {
            if (distance_02 < _shortestDistance && position_02.HasValue)
            {
                insertPathWithPosition(position_02.Value, position_2.previousCollider, distance_02, key.HasValue ? key.Value.Item2 + 1 : _pathCount);
                _pathCount++;
            }

            if (distance_01 < _shortestDistance)
            {
                insertPathWithPosition(position_01, position_1.previousCollider, distance_01, key.HasValue ? key.Value.Item2 + 1 : _pathCount);
                _pathCount++;
            }

            if (key.HasValue) _allPaths.Remove(pathKey);

            void insertPathWithPosition(Vector3 position, Collider previousCollider, float distance, int pathID)
            {
                List<Vector3> pathPoints = key.HasValue
                    ? new List<Vector3>(_allPaths[key.Value].Item1) { position }
                    : new List<Vector3> { startPosition, position };

                insertPath(Vector3Int.RoundToInt(targetPosition), pathID, pathPoints, previousCollider, distance, false);
            }
        }
    }

    void _updatePathDistances((Vector3 position, float distance, Collider previousCollider) position_1, (Vector3? position, float distance, Collider previousCollider) position_2, (Vector3Int, int) key,
        out float distance_01, out float distance_02)
    {
        distance_01 = position_1.distance + _allPaths[key].Item3;
        distance_02 = position_2.position.HasValue ? position_2.distance + _allPaths[key].Item3 : float.PositiveInfinity;
    }

    void insertPath(Vector3Int targetDestination, int newPathID, List<Vector3> pathPoints, Collider previousCollider, float distance, bool reachesTarget)
    {
        Dictionary<(Vector3Int, int), (List<Vector3>, Collider, float, bool)> updatedPaths = new();

        foreach (var path in _allPaths)
        {
            updatedPaths[(path.Key.Item1, path.Key.Item2 >= newPathID ? path.Key.Item2 + 1 : path.Key.Item2)] = path.Value;
        }

        updatedPaths[(targetDestination, newPathID)] = (pathPoints, previousCollider, distance, reachesTarget);
        _allPaths = updatedPaths;
    }

    ((Vector3 position, float distance, Collider previousCollider) position_1, (Vector3? position, float distance, Collider previousCollider) position_2) _selectVertices(
        int pathID, Controller_Agent agent, RaycastHit hit, Vector3 startPosition, Vector3 targetPosition, Vector3 characterSize, Collider currentCollider, Collider previousCollider)
    {
        bool sameObstacle = currentCollider == previousCollider;

        (Vector3 position, float distance, Collider previousCollider) position_1 = (startPosition, float.PositiveInfinity, previousCollider);
        (Vector3? position, float distance, Collider previousCollider) position_2 = (null, float.PositiveInfinity, previousCollider);

        checkMoverType(agent, characterSize);

        getMinAndMax(hit.collider, out Vector3 min, out Vector3 max);
        getDistances(startPosition, min, max, out float dxMin, out float dxMax, out float dyMin, out float dymax, out float dzMin, out float dzMax);
        getBounds(pathID, startPosition, targetPosition, sameObstacle, characterSize, hit, min, max, dxMin, dxMax, dzMin, dzMax, out Vector3 closestBound, out Vector3 furthestBound);

        position_1.position = nextAvailablePoint(pathID, startPosition, sameObstacle ? furthestBound : closestBound, characterSize, currentCollider, out position_1.previousCollider, previousCollider: previousCollider);
        position_2.position = sameObstacle
            ? null
            : nextAvailablePoint(pathID, startPosition, furthestBound, characterSize, currentCollider, out position_2.previousCollider, previousCollider: previousCollider);

        // Temporary while flying isn't implemented
        position_1.position.y = startPosition.y;

        if (position_2.position.HasValue) position_2 = (new Vector3(position_2.position.Value.x, startPosition.y, position_2.position.Value.z), float.PositiveInfinity, position_2.previousCollider);

        position_1.distance = Vector3.Distance(startPosition, position_1.position);
        position_2.distance = position_2.position.HasValue ? Vector3.Distance(startPosition, position_2.position.Value) : float.PositiveInfinity;

        return (position_1, position_2);
    }

    void getMinAndMax(Collider collider, out Vector3 min, out Vector3 max)
    {
        min = collider.bounds.min;
        max = collider.bounds.max;
    }

    void getDistances(Vector3 startPosition, Vector3 min, Vector3 max, out float dxMin, out float dxMax, out float dyMin, out float dyMax, out float dzMin, out float dzMax)
    {
        dxMin = Mathf.Abs(startPosition.x - min.x);
        dxMax = Mathf.Abs(startPosition.x - max.x);
        dyMin = Mathf.Abs(startPosition.y - min.y);
        dyMax = Mathf.Abs(startPosition.y - max.y);
        dzMin = Mathf.Abs(startPosition.z - min.z);
        dzMax = Mathf.Abs(startPosition.z - max.z);
    }

    void getBounds(int pathID, Vector3 startPosition, Vector3 targetPosition, bool sameObstacle, Vector3 characterSize, RaycastHit hit, Vector3 min, Vector3 max, float dxMin, float dxMax, float dzMin, float dzMax, out Vector3 closestBound, out Vector3 furthestBound)
    {
        Vector3 obstacleCenter = hit.collider.transform.position;

        closestBound = calculateBound(dxMin < dxMax ? min.x - characterSize.x : max.x + characterSize.x, dzMin < dzMax ? min.z - characterSize.z : max.z + characterSize.z);

        if (startPosition.x > max.x)
        {
            if (startPosition.z > max.z)
            {
                furthestBound = startPosition.x - targetPosition.x < startPosition.z - targetPosition.z
                    ? sameObstacle
                        ? calculateBound(max.x + characterSize.x, min.z - characterSize.z)
                        : calculateBound(min.x - characterSize.x, max.z + characterSize.z)
                    : calculateBound(max.x + characterSize.x, min.z - characterSize.z);
            }
            else if (startPosition.z < min.z)
            {
                furthestBound = startPosition.x - targetPosition.x < startPosition.z - targetPosition.z
                    ? sameObstacle
                        ? calculateBound(max.x + characterSize.x, max.z + characterSize.z)
                        : calculateBound(min.x - characterSize.x, min.z - characterSize.z)
                    : calculateBound(max.x + characterSize.x, max.z + characterSize.z);
            }
            else
            {
                furthestBound = startPosition.z > obstacleCenter.z
                    ? calculateBound(max.x + characterSize.x, min.z - characterSize.z)
                    : calculateBound(max.x + characterSize.x, max.z + characterSize.z);
            }
        }
        else if (startPosition.x < min.x)
        {
            if (startPosition.z > max.z)
            {
                furthestBound = startPosition.x - targetPosition.x < startPosition.z - targetPosition.z
                    ? sameObstacle
                        ? calculateBound(min.x - characterSize.x, min.z - characterSize.z)
                        : calculateBound(max.x + characterSize.x, max.z + characterSize.z)
                    : calculateBound(min.x - characterSize.x, min.z - characterSize.z);
            }
            else if (startPosition.z < min.z)
            {
                furthestBound = startPosition.x - targetPosition.x < startPosition.z - targetPosition.z
                    ? sameObstacle
                        ? calculateBound(min.x - characterSize.x, max.z + characterSize.z)
                        : calculateBound(max.x + characterSize.x, min.z - characterSize.z)
                    : calculateBound(min.x - characterSize.x, max.z + characterSize.z);
            }
            else
            {
                furthestBound = startPosition.z > obstacleCenter.z
                    ? calculateBound(min.x - characterSize.x, min.z - characterSize.z)
                    : calculateBound(min.x - characterSize.x, max.z + characterSize.z);
            }
        }
        else
        {
            if (startPosition.z > max.z)
            {
                furthestBound = startPosition.x > obstacleCenter.x
                    ? calculateBound(min.x - characterSize.x, max.z + characterSize.z)
                    : calculateBound(max.x + characterSize.x, max.z + characterSize.z);
            }
            else if (startPosition.z < min.z)
            {
                furthestBound = startPosition.x > obstacleCenter.x
                    ? calculateBound(min.x - characterSize.x, min.z - characterSize.z)
                    : calculateBound(max.x + characterSize.x, min.z - characterSize.z);
            }
            else
            {
                Debug.LogError("You are inside the obstacle");
                furthestBound = closestBound;
            }
        }

        Vector3 calculateBound(float x, float z)
        {
            return new Vector3(x, startPosition.y, z);
        }
    }

    static void checkMoverType(Controller_Agent agent, Vector3 characterSize)
    {
        Vector3 characterCanMoveInY = characterSize;
        // Need to put in what type of obstacle it is, and therefore whether the different types of movers can move over, under or through it.
        if (!agent.MoverType.Contains(MoverType.Fly) || !agent.MoverType.Contains(MoverType.Dig))
        {
            characterCanMoveInY.y = 0;
        }
    }

    Vector3 nextAvailablePoint(int pathID, Vector3 startPosition, Vector3 targetPosition, Vector3 characterSize, Collider currentCollider, out Collider returnedCollider, int iterations = 0, Collider previousCollider = null)
    {
        returnedCollider = currentCollider;

        iterations++;
        if (iterations > 100) { Debug.Log($"{pathID} Iterated to {iterations}"); return targetPosition; }

        if (Physics.Raycast(startPosition, targetPosition - startPosition, out RaycastHit hit, (targetPosition - startPosition).magnitude))
        {
            getMinAndMax(hit.collider, out Vector3 min, out Vector3 max);
            getDistances(startPosition, min, max, out float dxMin, out float dxMax, out float dyMin, out float dymax, out float dzMin, out float dzMax);
            getBounds(pathID, startPosition, targetPosition, hit.collider == previousCollider, characterSize, hit, min, max, dxMin, dxMax, dzMin, dzMax, out Vector3 closestBound, out Vector3 furthestBound);

            return nextAvailablePoint(pathID, startPosition, hit.collider == previousCollider ? furthestBound : closestBound, characterSize, hit.collider, out returnedCollider, previousCollider: currentCollider);
        }
        else
        {
            return targetPosition;
        }
    }
}
