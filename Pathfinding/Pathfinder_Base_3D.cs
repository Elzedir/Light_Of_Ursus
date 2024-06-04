using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Pathfinder_Base_3D
{
    PriorityQueue_3D _mainPriorityQueue;
    double _priorityModifier;
    Voxel_Base _targetVoxel;
    Voxel_Base _startVoxel;
    PuzzleSet _puzzleSet;
    PathfinderMover_3D _mover;

    #region Initialisation
    static int _gridWidth;
    static int _gridHeight;
    static int _gridDepth;

    public static void SetGrid(int gridWidth, int gridHeight, int gridDepth)
    {
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
        _gridDepth = gridDepth;
    }

    public void SetPath(Vector3 start, Vector3 target, PathfinderMover_3D mover, PuzzleSet puzzleSet)
    {
        if (!VoxelGrid.Initialised) VoxelGrid.InitializeVoxelGrid();

        _puzzleSet = puzzleSet;
        _mover = mover;

        _startVoxel = VoxelGrid.GetVoxelAtPosition(start);
        _targetVoxel = VoxelGrid.GetVoxelAtPosition(target);

        mover.StartPathfindingCoroutine(_runPathfinder(mover));
    }

    public void UpdatePath(PathfinderMover_3D mover, Vector3 start, Vector3 target)
    {
        _startVoxel = VoxelGrid.GetVoxelAtPosition(start);
        _targetVoxel = VoxelGrid.GetVoxelAtPosition(target);

        if (mover.CanGetNewPath)
        {
            Debug.Log("Ran pathfinder again");
            mover.StartPathfindingCoroutine(_runPathfinder(mover));
        }
    }

    IEnumerator _runPathfinder(PathfinderMover_3D mover)
    {
        yield break;

        if (_startVoxel == null || _targetVoxel == null || _startVoxel.Equals(_targetVoxel))
        {
            Debug.Log($"StartVoxel: {_startVoxel} or TargetVoxel: {_targetVoxel} is null or equal.");
            yield break;
        }

        Voxel_Base currentVoxel = _startVoxel;
        List<Voxel_Base> currentPath = new();
        List<Voxel_Base> previousPath = null;
        List<Vector3> currentObstacles = new();
        List<Vector3> previousObstacles = null;
        bool pathIsComplete = false;

        _initialise();

        int iterationCount = 0;

        while (true)
        {
            bool changesInEnvironment = false;
            currentObstacles = _mover.GetObstaclesInVision();

            if (previousObstacles == null || !AreObstaclesEqual(previousObstacles, currentObstacles))
            {
                changesInEnvironment = true;

                foreach (Vector3 position in currentObstacles)
                {
                    Voxel_Base obstacleVoxel = VoxelGrid.GetVoxelAtPosition(position);

                    if (obstacleVoxel.IsObstacle) continue;

                    obstacleVoxel = VoxelGrid.AddSubvoxelToVoxelGrid(position, true);

                    foreach (Voxel_Base predecessor in obstacleVoxel.GetPredecessors())
                    {
                        _updateVertex(predecessor);
                    }
                }

                if (previousObstacles == null)
                {
                    previousObstacles = new List<Vector3>(currentObstacles);
                }
            }

            if (previousObstacles != null)
            {
                foreach (Vector3 position in previousObstacles)
                {
                    if (currentObstacles.Contains(position))
                    {
                        continue;
                    }

                    Voxel_Base removedObstacleVoxel = VoxelGrid.GetVoxelAtPosition(position);

                    if (removedObstacleVoxel == null)
                    {
                        Debug.Log($"{removedObstacleVoxel.WorldPosition} exists in obstacle lists but does not exist in VoxelGrid.");
                        continue;
                    }

                    removedObstacleVoxel.UpdateMovementCost(1);

                    foreach (Voxel_Base predecessor in removedObstacleVoxel.GetPredecessors())
                    {
                        _updateVertex(predecessor);
                    }

                    VoxelGrid.RemoveVoxelAtPosition(position);
                }

                previousObstacles = new List<Vector3>(currentObstacles);
            }

            _computeShortestPath();

            if (pathIsComplete)
            {
                currentVoxel = _startVoxel;
            }

            Voxel_Base nextVoxel = _minimumSuccessorVoxel(currentVoxel);
            currentPath.Add(nextVoxel);
            currentVoxel = nextVoxel;

            double previousPriorityModifier = _priorityModifier;
            _priorityModifier += _manhattanDistance(currentVoxel, nextVoxel);

            if (!changesInEnvironment)
            {
                _priorityModifier = previousPriorityModifier;
            }

            if (currentVoxel.Equals(_targetVoxel))
            {
                pathIsComplete = true;
            }

            if (pathIsComplete && (previousPath == null || !IsPathEqual(previousPath, currentPath)))
            {
                if (!VoxelGrid.VoxelsTestShown)
                {
                    VoxelGrid.TestShowAllVoxels();
                }
                
                _mover.MoveTo(_targetVoxel);
                currentVoxel = _startVoxel;

                previousPath = new List<Voxel_Base>(currentPath);
                currentPath.Clear();
                pathIsComplete = false;

                iterationCount = 0;
                yield return null;
            }

            iterationCount++;

            if (iterationCount >= 1000) break;
        }

        Debug.Log($"IterationCount: {iterationCount}");
    }

    bool AreObstaclesEqual(List<Vector3> previousObstacles, List<Vector3> currentObstacles)
    {
        if (previousObstacles == null)
        {
            return false;
        }

        if (previousObstacles.Count != currentObstacles.Count)
        {
            return false;
        }

        var previousEnumerator = previousObstacles.GetEnumerator();
        var currentEnumerator = currentObstacles.GetEnumerator();

        while (previousEnumerator.MoveNext() && currentEnumerator.MoveNext())
        {
            if (!previousEnumerator.Current.Equals(currentEnumerator.Current))
            {
                return false;
            }
        }

        return true;
    }

    bool IsPathEqual(List<Voxel_Base> path1, List<Voxel_Base> path2)
    {
        if (path1.Count != path2.Count)
        {
            return false;
        }

        for (int i = 0; i < path1.Count; i++)
        {
            if (!path1[i].Equals(path2[i]))
            {
                return false;
            }
        }

        return true;
    }


    void _initialise()
    {
        foreach (Voxel_Base voxel in VoxelGrid.Voxels) { if (voxel != null) { voxel.RHS = double.PositiveInfinity; voxel.G = double.PositiveInfinity; } }
        _mainPriorityQueue = new PriorityQueue_3D(_gridWidth * _gridHeight * _gridDepth);
        _priorityModifier = 0;
        _targetVoxel.RHS = 0;
        _mainPriorityQueue.Enqueue(_targetVoxel, _calculatePriority(_targetVoxel));
    }

    Priority_3D _calculatePriority(Voxel_Base node)
    {
        return new Priority_3D(Math.Min(node.G, node.RHS) + _manhattanDistance(node, _startVoxel) + _priorityModifier, Math.Min(node.G, node.RHS));
    }
    double _manhattanDistance(Voxel_Base a, Voxel_Base b)
    {
        Vector3 distance = a.WorldPosition - b.WorldPosition;
        return Math.Abs(distance.x) + Math.Abs(distance.y) + Math.Abs(distance.z);
    }

    void _updateVertex(Voxel_Base voxel)
    {
        if (!voxel.Equals(_targetVoxel))
        {
            voxel.RHS = _minimumSuccessorCost(voxel);
        }
        if (_mainPriorityQueue.Contains(voxel))
        {
            _mainPriorityQueue.Remove(voxel);
        }
        if (voxel.G != voxel.RHS)
        {
            _mainPriorityQueue.Enqueue(voxel, _calculatePriority(voxel));
        }
    }
    Voxel_Base _minimumSuccessorVoxel(Voxel_Base voxel)
    {
        double minimumCostToMove = Double.PositiveInfinity;
        Voxel_Base bestSucessor = null;

        //Debug.Log($"Original: WorldPos: {voxel.WorldPosition} Cost: {voxel.MovementCost}");

        foreach (Voxel_Base successor in voxel.GetSuccessors())
        {
            //Debug.Log($"WorldPos: {successor.WorldPosition} Cost: {successor.MovementCost} G: {successor.G}");

            double costToMove = voxel.GetMovementCostTo(successor, _puzzleSet) + successor.G;

            if (costToMove <= minimumCostToMove && !successor.IsObstacle)
            {
                minimumCostToMove = costToMove;
                bestSucessor = successor;
            }
        }

        bestSucessor.SetPredecessor(voxel);

        //Debug.Log($"Predecessor: {bestSucessor.Predecessor.WorldPosition}");

        //Debug.Log($"Best Succ WorldPos: {bestSucessor.WorldPosition} Cost: {bestSucessor.MovementCost}");

        return bestSucessor;
    }
    double _minimumSuccessorCost(Voxel_Base node)
    {
        double minimumCost = Double.PositiveInfinity;
        foreach (Voxel_Base successor in node.GetSuccessors())
        {
            double costToMove = node.GetMovementCostTo(successor, _puzzleSet) + successor.G;
            if (costToMove < minimumCost && !successor.IsObstacle) minimumCost = costToMove;
        }
        return minimumCost;
    }
    void _computeShortestPath()
    {
        while (_mainPriorityQueue.Peek().CompareTo(_calculatePriority(_startVoxel)) < 0 || _startVoxel.RHS != _startVoxel.G)
        {
            Priority_3D highestPriority = _mainPriorityQueue.Peek();
            Voxel_Base node = _mainPriorityQueue.Dequeue();
            if (node == null) break;

            if (highestPriority.CompareTo(_calculatePriority(node)) < 0)
            {
                _mainPriorityQueue.Enqueue(node, _calculatePriority(node));
            }
            else if (node.G > node.RHS)
            {
                node.G = node.RHS;
                foreach (Voxel_Base neighbour in node.GetPredecessors())
                {
                    _updateVertex(neighbour);
                }
            }
            else
            {
                node.G = Double.PositiveInfinity;
                _updateVertex(node);
                foreach (Voxel_Base neighbour in node.GetPredecessors())
                {
                    _updateVertex(neighbour);
                }
            }
        }
    }
    #endregion

    public List<Vector3> RetrievePath(Voxel_Base startVoxel, Voxel_Base targetVoxel)
    {
        List<Vector3> path = new List<Vector3>();
        Voxel_Base currentVoxel = targetVoxel;

        int iterationCount = 0;

        while (currentVoxel != null && !currentVoxel.Equals(startVoxel) && iterationCount < 1000)
        {
            if (currentVoxel == null || currentVoxel.Equals(startVoxel)) break;
            path.Add(currentVoxel.WorldPosition);
            currentVoxel = currentVoxel.Predecessor;
            iterationCount++;
        }

        if (currentVoxel != null)
        {
            path.Add(startVoxel.WorldPosition);
        }

        path.Reverse();

        return path;
    }

    public static void FindAllPredecessors(Voxel_Base node, int infiniteEnd, int infinityStart = 0)
    {
        infinityStart++;
        if (infinityStart > infiniteEnd) return;
        if (node.Predecessor == null) { Debug.Log($"{node.WorldPosition}_{node.WorldPosition} predecessor is null"); return; }

        Debug.Log($"{node.WorldPosition}_{node.WorldPosition} -> {node.Predecessor.WorldPosition}_{node.Predecessor.WorldPosition}");
        FindAllPredecessors(node.Predecessor, infinityStart, infiniteEnd);
    }
}

public class VoxelGrid
{
    public static bool Initialised { get; private set; }
    public static Voxel_Base[,,] Voxels;
    public static int Scale { get; private set; }
    //static Vector3 _offset;
    public static bool VoxelsTestShown { get; private set; } = false;
    static List<Voxel_Base> _testShowPathfinding = new();
    static List<GameObject> _testShowVoxels = new();
    static List<GameObject> _testShowSubVoxels = new();
    static Vector3 _defaultOffset = new Vector3(0.5f, 0, 0.5f);

    public static List<Voxel_Base> VoxelsTest = new();
    public static List<Vector3> NavigationList = new();

    public static void InitialiseVoxelGridTest(float width = 100, float height = 4, float depth = 100)
    {
        Collider groundCollider = Manager_Game.Instance.GroundCollider;
        bool hasGroundCollider = groundCollider != null;

        if (hasGroundCollider)
        {
            List<Collider> obstacles = new List<Collider>(UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None));
            obstacles.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            for (int i = 0; i < obstacles.Count; i++)
            {
                Collider collider = obstacles[i];
                Voxel_Base newVoxel = new Voxel_Base();

                newVoxel.SetVoxelProperties(
                    worldPosition: collider.transform.position,
                    size: collider.bounds.size,
                    voxelType: _getVoxelType(collider.gameObject),
                    g: double.PositiveInfinity,
                    rhs: double.PositiveInfinity
                    );

                newVoxel.UpdateMovementCostTest();

                VoxelsTest.Add(newVoxel);
            }
        }

        Initialised = true;
        //TestTestShowAllVoxels();
    }

    static Dictionary<int, (List<Vector3>, float, bool)> _allPaths = new();
    static int _pathCount = 0;
    static List<Vector3> _bestPath = new();
    static float _shortestDistance = float.PositiveInfinity;

    public static IEnumerator Move(Vector3 startPosition, Vector3 targetPosition, Vector3 characterSize, Controller_Agent agent)
    {
        yield return new WaitForSeconds(0.5f);

        if (Physics.Raycast(startPosition, targetPosition - startPosition, out RaycastHit hit))
        {
            Debug.DrawRay(startPosition, targetPosition - startPosition, Color.red, 50.0f);

            if (hit.transform.name.Contains("Player"))
            {
                Debug.Log($"Hit player at {hit.point}. Direct path to target: {targetPosition}.");

                // Implement your movement logic here
                yield break;
            }
            else if (hit.transform.name.Contains("Wall"))
            {
                yield return Manager_Game.Instance.StartVirtualCoroutine((_findBestPath(agent, startPosition, targetPosition, characterSize, hit)));

                Vector3 lastPosition = startPosition;

                foreach (Vector3 position in _bestPath)
                {
                    Debug.DrawRay(position, lastPosition - position, Color.green, 1f);
                    lastPosition = position;
                    yield return new WaitForSeconds(0.5f);
                }

                agent.MoveToTest(_bestPath);

                // Move through the path
            }
            else
            {
                Debug.Log($"Hit at position: {hit.point} but was not a wall, instead was a {hit.transform.name}");
            }
        }
        else
        {
            Debug.Log($"Hit nothing. Direct path to target: {targetPosition}.");
            // Implement your movement logic here
        }
    }

    static IEnumerator _findBestPath(Controller_Agent agent, Vector3 startPosition, Vector3 targetPosition, Vector3 characterSize, RaycastHit hit, Collider previousCollider = null, int pathID = 0)
    {
        if (pathID > 100) yield break;

        _selectVertices(agent, direction: targetPosition - startPosition, startPosition, targetPosition, characterSize, hit, out (Vector3 position, float distance) position_1, out (Vector3 position, float distance) position_2, previousCollider);

        addOrUpdatePaths();

        for (int i = 0; i < _pathCount; i++)
        {
            if (!_allPaths.ContainsKey(i) || _allPaths[i].Item3) continue;

            Vector3 lastPosition = _allPaths[i].Item1.Last();

            if (!Physics.Raycast(lastPosition, targetPosition - lastPosition, out RaycastHit pathHit)) continue;

            Debug.DrawRay(startPosition, targetPosition - startPosition, Color.red, 50.0f);

            string hitName = pathHit.transform.name;

            if (hitName.Contains("Player"))
            {
                _allPaths[i].Item1.Add(pathHit.point);
                _allPaths[i] = (_allPaths[i].Item1, _allPaths[i].Item2, true);

                if (_allPaths[i].Item2 < _shortestDistance)
                {
                    _bestPath = _allPaths[i].Item1;
                    _shortestDistance = _allPaths[i].Item2;
                }
            }
            else if (pathHit.transform.name.Contains("Wall"))
            {
                Manager_Game.Instance.StartVirtualCoroutine(_findBestPath(agent, lastPosition, targetPosition, characterSize, pathHit, previousCollider: hit.collider, pathID: i));
            }

        }

        void addOrUpdatePaths()
        {
            float distancePath_01 = float.PositiveInfinity;
            float distancePath_02 = float.PositiveInfinity;

            var existingPath = _allPaths.FirstOrDefault(keyValuePair => keyValuePair.Value.Item1.SequenceEqual(_allPaths[pathID].Item1));

            if (!existingPath.Equals(default(KeyValuePair<int, (List<Vector3>, float, bool)>)))
            {
                if (_allPaths[existingPath.Key].Item3) return;

                UpdatePathDistances(ref distancePath_01, ref distancePath_02, existingPath.Key);
                AddPathsIfShorter(distancePath_01, position_1.position, existingPath.Key);
                AddPathsIfShorter(distancePath_02, position_2.position, existingPath.Key, position_2 != position_1);
                _allPaths.Remove(existingPath.Key);
            }
            else if (_allPaths.ContainsKey(pathID))
            {
                UpdatePathDistances(ref distancePath_01, ref distancePath_02, pathID);
                AddPathsIfShorter(distancePath_01, position_1.position, pathID);
                AddPathsIfShorter(distancePath_02, position_2.position, pathID, position_2 != position_1);
                _allPaths.Remove(pathID);
            }
            else
            {
                AddNewPathIfShorter(position_1.distance, position_1.position);
                AddNewPathIfShorter(position_2.distance, position_2.position, position_2 != position_1);
            }

            void UpdatePathDistances(ref float distancePath_01, ref float distancePath_02, int key)
            {
                distancePath_01 = position_1.distance + _allPaths[key].Item2;
                distancePath_02 = position_2.distance + _allPaths[key].Item2;
            }

            void AddPathsIfShorter(float distance, Vector3 position, int key, bool differentObstacles = true)
            {
                if (differentObstacles && distance < _shortestDistance)
                {
                    _allPaths.Add(_pathCount, (new List<Vector3>(_allPaths[key].Item1) { position }, distance, false));
                    _pathCount++;
                }
            }

            void AddNewPathIfShorter(float distance, Vector3 position, bool differentObstacles = true)
            {
                if (differentObstacles && distance < _shortestDistance)
                {
                    _allPaths.Add(_pathCount, (new List<Vector3> { startPosition, position }, distance, false));
                    _pathCount++;
                }
            }
        }
    }

    public static void _selectVertices(Controller_Agent agent, Vector3 direction, Vector3 start, Vector3 target, Vector3 characterSize, RaycastHit hit, out (Vector3 position, float distance) position_1, out (Vector3 position, float distance) position_2, Collider previousCollider = null)
    {
        position_1 = (start, 0);
        position_2 = (start, 0);

        Vector3 min = hit.collider.bounds.min;
        Vector3 max = hit.collider.bounds.max;

        float dxMin = Mathf.Abs(hit.point.x - min.x);
        float dxMax = Mathf.Abs(hit.point.x - max.x);
        float dyMin = Mathf.Abs(hit.point.y - min.y);
        float dyMax = Mathf.Abs(hit.point.y - max.y);
        float dzMin = Mathf.Abs(hit.point.z - min.z);
        float dzMax = Mathf.Abs(hit.point.z - max.z);

        Vector3 characterCanMoveInY = characterSize;
        // Need to put in what type of obstacle it is, and therefore whether the different types of movers can move over, under or through it.
        if (!agent.MoverType.Contains(MoverType.Fly) || !agent.MoverType.Contains(MoverType.Dig))
        {
            characterCanMoveInY.y = 0;
        }

        bool sameObstacle = hit.collider == previousCollider;

        Vector3 findClosestBoundPoint(Vector3 point) =>
        new Vector3(Math.Abs(point.x - min.x) < Math.Abs(point.x - max.x) ? min.x : max.x, point.y, Math.Abs(point.z - min.z) < Math.Abs(point.z - max.z) ? min.z : max.z);

        Vector3 closestBound = Vector3.zero;
        closestBound.x = dxMin < dxMax ? min.x - characterSize.x : max.x + characterSize.x;
        closestBound.z = dzMin < dzMax ? min.z - characterSize.z : max.z + characterSize.z;

        Vector3? findPosition_1()
        {
            if (hit.point.x == min.x || hit.point.x == max.x)
            {
                return new Vector3(hit.point.x, start.y, closestBound.z);
            }
            else if (hit.point.x == min.z || hit.point.z == max.z)
            {
                return new Vector3(closestBound.x, start.y, hit.point.z);
            }
            else
            {
                Debug.LogError($"Raycast hit at {hit.point} did not hit any bounds.");
                return null;
            }
        }

        position_1.position = findPosition_1().Value;

        if (sameObstacle && findClosestBoundPoint(start) == findClosestBoundPoint(position_1.position))
        {
            closestBound.x = max.x + characterSize.x;
            closestBound.z = max.z + characterSize.z;
            position_1.position = findPosition_1().Value;
        }

        setPosition_2(position_1.position, out position_2.position);

        position_1.distance = Vector3.Distance(start, position_1.position);
        position_2.distance = Vector3.Distance(start, position_2.position);

        void setPosition_2(Vector3 p_1, out Vector3 p_2)
        {
            if (!sameObstacle)
            {
                p_2 = (Mathf.Abs(direction.x) < Mathf.Abs(direction.z)) ? new Vector3(-p_1.x, p_1.y, p_1.z) : new Vector3(p_1.x, p_1.y, -p_1.z);
            }
            else
            {
                p_2 = p_1;
            }
        }
    }

    public static void TestTestShowAllVoxels(bool showOpen = true, bool showObstacles = true, bool showGround = true)
    {
        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        Material green = Resources.Load<Material>("Meshes/Material_Green");
        Material red = Resources.Load<Material>("Meshes/Material_Red");
        Material blue = Resources.Load<Material>("Meshes/Material_Blue");
        Material white = Resources.Load<Material>("Meshes/Material_White");

        foreach (Voxel_Base voxel in VoxelsTest)
        {
            if (voxel == null) continue;

            if (voxel.VoxelType == VoxelType.Open && showOpen)
            {
                GameObject voxelGO = voxel.TestShowVoxel(GameObject.Find("OpenTest").transform, mesh, white);
                _testShowVoxels.Add(voxelGO);
            }
            else if (voxel.VoxelType == VoxelType.Obstacle && showObstacles)
            {
                GameObject voxelGO = voxel.TestShowVoxel(GameObject.Find("ObstacleTest").transform, mesh, green);
                _testShowVoxels.Add(voxelGO);
            }
            else if (voxel.VoxelType == VoxelType.Ground && showGround)
            {
                GameObject voxelGO = voxel.TestShowVoxel(GameObject.Find("GroundTest").transform, mesh, red);
                _testShowSubVoxels.Add(voxelGO);
            }
        }

        VoxelsTestShown = true;
    }

    static VoxelType _getVoxelType(GameObject voxelGO)
    {
        if (voxelGO.name.Contains("Water"))
        {
            return VoxelType.Water;
        }
        if (voxelGO.name.Contains("Air"))
        {
            return VoxelType.Air;
        }
        if (voxelGO.name.Contains("Ground"))
        {
            return VoxelType.Ground;
        }
        if (voxelGO.name.Contains("Obstacle"))
        {
            return VoxelType.Obstacle;
        }
        else
        {
            return VoxelType.Open;
        }
    }

    public static void InitializeVoxelGrid(float width = 100, float height = 4, float depth = 100, int scale = 10, Vector3? offset = null)
    {
        //Scale = scale;

        //Collider groundCollider = Manager_Game.Instance.GroundCollider;
        //bool hasGroundCollider = false;

        //_offset = offset ?? getCollider();

        //Vector3 getCollider()
        //{
        //    if (groundCollider != null)
        //    {
        //        hasGroundCollider = true;
        //        return groundCollider.bounds.size / 2f;
        //    }

        //    return Vector3.zero;
        //}

        //_offset = new Vector3(_offset.x, 0, _offset.z);

        //int gridWidth = (int)((width + 1) * Scale);
        //int gridHeight = (int)((height + 1) * Scale);
        //int gridDepth = (int)((depth + 1) * Scale);

        //Pathfinder_Base_3D.SetGrid(gridWidth, gridHeight, gridDepth);

        //Voxels = new Voxel_Base[gridWidth, gridHeight, gridDepth];

        //for (int x = (int)(_offset.x * Scale); x < gridWidth; x += Scale)
        //{
        //    for (int y = (int)(_offset.y * Scale); y < gridHeight; y += Scale)
        //    {
        //        for (int z = (int)(_offset.z * Scale); z < gridDepth; z += Scale)
        //        {
        //            Vector3Int gridPos = new Vector3Int(x, y, z);
        //            Vector3 worldPos = Vector3.zero;

        //            if (hasGroundCollider)
        //            {
        //                worldPos.x = (x / Scale) - (_offset.x * 2);
        //                worldPos.y = (y / Scale) - (_offset.y * 2);
        //                worldPos.z = (z / Scale) - (_offset.z * 2);
        //            }
        //            else
        //            {
        //                worldPos.x = (x / Scale) - _offset.x;
        //                worldPos.y = (y / Scale) - _offset.y;
        //                worldPos.z = (z / Scale) - _offset.z;
        //            }

        //            Voxel_Base newVoxel = Voxels[x, y, z] = new Voxel_Base();

        //            newVoxel.SetVoxelProperties(gridPosition: gridPos, worldPosition: worldPos, size: Vector3.one, g: double.PositiveInfinity, rhs: double.PositiveInfinity, isObstacle: false);

        //            Voxels[x, y, z].UpdateMovementCost(1);
        //        }
        //    }
        //}

        ////Debug.Log($"Grid Size: {new Vector3(gridWidth, gridHeight, gridDepth)}");

        //Initialised = true;
    }

    public static Voxel_Base AddSubvoxelToVoxelGrid(Vector3 position, bool isObstacle = false)
    {
        Collider collider = Physics.OverlapSphere(position, 0.1f).FirstOrDefault(); // hitCollider => hitCollider.gameObject.layer == "Wall");

        if (collider == null)
        {
            Debug.Log($"No cells at Pos: {position}.");
            return null;
        }

        Vector3 center = collider.bounds.center;
        Vector3 extents = collider.bounds.extents;

        Vector3 minBounds = center - extents;
        Vector3 maxBounds = center + extents;

        //Vector3Int minGridPosition = WorldToGridPosition(minBounds);
        //Vector3Int maxGridPosition = WorldToGridPosition(maxBounds);

        for (int x = (int)minBounds.x; x < maxBounds.x; x += Scale)
        {
            for (int y = (int)minBounds.y; y < maxBounds.y; y += Scale)
            {
                for (int z = (int)minBounds.z; z < maxBounds.z; z += Scale)
                {
                    Vector3Int positionToRemove = Vector3Int.zero; //= new Vector3Int(WorldToGridPosition(x, y, z));
                    Voxel_Base voxelToRemove = Voxels[positionToRemove.x, positionToRemove.y, positionToRemove.z];

                    Debug.Log($"Trying to remove Voxel: {voxelToRemove} WorldPos: {position} GridPos: {positionToRemove}");

                    if (voxelToRemove != null && !voxelToRemove.IsObstacle)
                    {
                        Debug.Log($"Removing voxel at {positionToRemove}");
                        RemoveVoxelAtPosition(positionToRemove);
                    }
                }
            }
        }

        Vector3[] vertices = new Vector3[8];
        vertices[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        vertices[1] = center + new Vector3(extents.x, -extents.y, -extents.z);
        vertices[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
        vertices[3] = center + new Vector3(extents.x, extents.y, -extents.z);
        vertices[4] = center + new Vector3(-extents.x, -extents.y, extents.z);
        vertices[5] = center + new Vector3(extents.x, -extents.y, extents.z);
        vertices[6] = center + new Vector3(-extents.x, extents.y, extents.z);
        vertices[7] = center + new Vector3(extents.x, extents.y, extents.z);

        Vector3 minExtents = vertices.Select(vertex => vertex - position).Aggregate(Vector3.Min);
        Vector3 maxExtents = vertices.Select(vertex => vertex - position).Aggregate(Vector3.Max);

        Vector3 subVoxelSize = maxExtents - minExtents;
        Vector3 subVoxelCenter = position + (minExtents + maxExtents) / 2;

        Vector3Int gridPosition = WorldToGridPosition(subVoxelCenter);
        Voxel_Base existingVoxel = Voxels[gridPosition.x, gridPosition.y, gridPosition.z];

        if (existingVoxel != null && existingVoxel.Size == subVoxelSize)
        {
            Debug.Log($"Voxel already exists at Pos: {position} GridPos: {gridPosition} WorldPos: {existingVoxel.WorldPosition} with same scale: {subVoxelSize}.");
            return null;
        }

        Voxel_Base newVoxel = Voxels[gridPosition.x, gridPosition.y, gridPosition.z] = new Voxel_Base();

        newVoxel.SetVoxelProperties(gridPosition: gridPosition, worldPosition: subVoxelCenter, size: subVoxelSize, g: double.PositiveInfinity, rhs: double.PositiveInfinity, isObstacle: isObstacle);

        if (isObstacle) newVoxel.UpdateMovementCost(double.PositiveInfinity);

        _testShowPathfinding.Add(newVoxel);

        return newVoxel;
    }

    public static void RemoveVoxelAtPosition(Vector3? position)
    {
        if (!position.HasValue) return;

        Voxel_Base voxel = GetVoxelAtPosition(position.Value);

        if (voxel != null)
        {
            voxel.UpdateMovementCost(1);
            _testShowPathfinding.Remove(voxel);
            VoxelGrid.Voxels[voxel.GridPosition.x, voxel.GridPosition.y, voxel.GridPosition.z] = null;
            if (VoxelGrid.Voxels?[voxel.GridPosition.x, voxel.GridPosition.y, voxel.GridPosition.z] == null) Debug.Log($"Voxel removed at {position}");
            return;
        }

        TestHideAllVoxels();
        TestShowAllVoxels();

        return;
    }

    public static Voxel_Base GetVoxelAtPosition(Vector3 position)
    {
        //Vector3Int gridPosition = WorldToGridPosition(position);

        //if (gridPosition.x < 0 && gridPosition.x >= Voxels.GetLength(0) &&
        //    gridPosition.y < 0 && gridPosition.y >= Voxels.GetLength(1) &&
        //    gridPosition.z < 0 && gridPosition.z >= Voxels.GetLength(2))
        //{
        //    Debug.Log($"Converted position WorldPos: {position} GridPos: {gridPosition} is out of Voxel Grid bounds: {Voxels.GetLength(0)}_{Voxels.GetLength(1)}_{Voxels.GetLength(2)}");
        //    return null;
        //}

        ////Debug.Log($"WorldPos: {position} GridPos: {new Vector3(gridPosition.x,gridPosition.y, gridPosition.z)}");

        //Voxel_Base voxel = Voxels[gridPosition.x, gridPosition.y, gridPosition.z];

        //if (voxel == null)
        //{
        //    Vector3 scaledPosition = new Vector3(
        //        (int)(((int)(gridPosition.x / Scale) + _offset.x) * Scale),
        //        (int)(((int)(gridPosition.y / Scale) + _offset.y) * Scale),
        //        (int)(((int)(gridPosition.z / Scale) + _offset.z) * Scale)
        //        );

        //    voxel = Voxels[(int)scaledPosition.x, (int)scaledPosition.y, (int)scaledPosition.z];

        //    if (voxel == null)
        //    {
        //        Debug.Log($"Voxel and Subvoxel does not exist at: GridPos: {gridPosition} WorldPos: {position} ScaledPosition: {scaledPosition}");
        //        return null;
        //    }

        //    return voxel;
        //}

        //return voxel;

        return null;
    }

    public static Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        return Vector3Int.zero;
        //return new Vector3Int(
        //    (int)((worldPosition.x + _offset.x) * Scale),
        //    (int)((worldPosition.y + _offset.y) * Scale),
        //    (int)((worldPosition.z + _offset.z) * Scale)
        //);
    }

    public static void TestShowAllVoxels(bool showVoxels = true, bool showSubvoxels = true)
    {
        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        Material green = Resources.Load<Material>("Meshes/Material_Green");
        Material red = Resources.Load<Material>("Meshes/Material_Red");
        Material blue = Resources.Load<Material>("Meshes/Material_Blue");

        foreach (Voxel_Base voxel in Voxels)
        {
            if (voxel == null) continue;

            if (!voxel.IsObstacle && showVoxels)
            {
                GameObject voxelGO = voxel.TestShowVoxel(GameObject.Find("!Obstacle").transform, mesh, green);
                _testShowVoxels.Add(voxelGO);
            }
            else if (voxel.IsObstacle && showSubvoxels)
            {
                GameObject voxelGO = voxel.TestShowVoxel(GameObject.Find("Obstacle").transform, mesh, red);
                _testShowSubVoxels.Add(voxelGO);
            }
        }

        VoxelsTestShown = true;
    }

    public static void TestHideAllVoxels()
    {
        for (int i = 0; i < _testShowVoxels.Count; i++)
        {
            Manager_Game.Destroy(_testShowVoxels[i]);
        }

        VoxelsTestShown = false;
    }
}

public enum VoxelType { Open, Ground, Obstacle, Air, Water }

public class Voxel_Base
{
    public Vector3Int GridPosition { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public Vector3 Size { get; private set; }
    public VoxelType VoxelType { get; private set; }
    public double G;
    public double RHS;
    public Voxel_Base Predecessor { get; private set; }

    public double MovementCost;

    public bool IsObstacle { get; private set; }
    GameObject _pathfindingVoxelGO;

    public Voxel_Base SetVoxelProperties(
        Vector3Int? gridPosition = null, 
        Vector3? worldPosition = null, 
        Vector3? size = null, 
        VoxelType? voxelType = null, 
        double? g = null, 
        double? rhs = null, 
        bool? isObstacle = null
        )
    {
        if (gridPosition != null) GridPosition = gridPosition.Value;
        if (worldPosition != null) WorldPosition = worldPosition.Value;
        if (size != null) Size = size.Value;
        if (voxelType != null) VoxelType = voxelType.Value;
        if (g != null) G = g.Value;
        if (rhs != null) rhs = rhs.Value;
        if (isObstacle != null) IsObstacle = isObstacle.Value;

        return this;
    }

    public GameObject TestShowVoxel(Transform transform, Mesh mesh, Material material)
    {
        GameObject voxelGO = new GameObject($"{WorldPosition}");
        _pathfindingVoxelGO = voxelGO;
        voxelGO.AddComponent<MeshFilter>().mesh = mesh;
        voxelGO.AddComponent<MeshRenderer>().material = material;
        voxelGO.transform.SetParent(transform);
        voxelGO.transform.localPosition = WorldPosition;
        voxelGO.transform.localScale = Size;
        return voxelGO;
    }
    public void TestHideVoxel()
    {
        _pathfindingVoxelGO.SetActive(false);
    }

    public void SetPredecessor(Voxel_Base predecessor)
    {
        Predecessor = predecessor;
    }

    public void UpdateMovementCostTest()
    {
        switch(VoxelType)
        {
            case VoxelType.Open:
                MovementCost = 1;
                break;
            case VoxelType.Ground:
                MovementCost = 1.5;
                break;
            case VoxelType.Obstacle:
                MovementCost = double.PositiveInfinity;
                break;
            case VoxelType.Air:
                MovementCost = 0.75;
                break;
            case VoxelType.Water:
                MovementCost = 1.5;
                break;
            default:
                MovementCost = double.PositiveInfinity;
                break;
        }
    }

    public void UpdateMovementCost(double cost)
    {
        if (cost == double.PositiveInfinity) IsObstacle = true;

        MovementCost = cost;
    }

    public double GetMovementCostTo(Voxel_Base successor, PuzzleSet puzzleSet)
    {
        return successor.MovementCost;

        throw new InvalidOperationException($"{puzzleSet} not valid.");
    }

    public bool Equals(Voxel_Base that)
    {
        if (GridPosition == that.GridPosition) return true;
        return false;
    }

    public LinkedList<Voxel_Base> GetSuccessors()
    {
        LinkedList<Voxel_Base> successors = new LinkedList<Voxel_Base>();
        int scale = (int)VoxelGrid.Scale;

        TryAddDirectionalSuccessors(1, 0, 0);
        TryAddDirectionalSuccessors(-1, 0, 0);
        TryAddDirectionalSuccessors(0, 1, 0);
        TryAddDirectionalSuccessors(0, -1, 0);
        TryAddDirectionalSuccessors(0, 0, 1);
        TryAddDirectionalSuccessors(0, 0, -1);

        void TryAddDirectionalSuccessors(int x, int y, int z)
        {
            bool subVoxelExists = false;
            bool subVoxelIsObstacle = false;

            for (int i = 1; i < 10; i++)
            {
                (subVoxelExists, subVoxelIsObstacle) = TryAddSuccessor(GridPosition.x + x * i, GridPosition.y + y * i, GridPosition.z + z * i);
                if (subVoxelExists && subVoxelIsObstacle) break;
            }

            if (!subVoxelIsObstacle) TryAddSuccessor(GridPosition.x + x * scale, GridPosition.y + y * scale, GridPosition.z + z * scale);
        }

        (bool subVoxelExists, bool subVoxelIsObstacle) TryAddSuccessor(int x, int y, int z)
        {
            if (x >= 0 && x < VoxelGrid.Voxels.GetLength(0) &&
                y >= 0 && y < VoxelGrid.Voxels.GetLength(1) &&
                z >= 0 && z < VoxelGrid.Voxels.GetLength(2))
            {
                Voxel_Base voxel = VoxelGrid.Voxels[x, y, z];

                if (voxel == null) return (false, false);

                successors.AddFirst(voxel);

                if (!voxel.IsObstacle) return (true, false);

                else return (true, true);
            }

            return (false, false);
        }

        return successors;
    }

    public LinkedList<Voxel_Base> GetPredecessors()
    {
        LinkedList<Voxel_Base> predecessors = new();
        int scale = (int)VoxelGrid.Scale;

        if (!TryAddPredecessor(GridPosition.x + 1, GridPosition.y, GridPosition.z))
            TryAddPredecessor(GridPosition.x + scale, GridPosition.y, GridPosition.z);

        if (!TryAddPredecessor(GridPosition.x - 1, GridPosition.y, GridPosition.z))
            TryAddPredecessor(GridPosition.x - scale, GridPosition.y, GridPosition.z);

        if (!TryAddPredecessor(GridPosition.x, GridPosition.y + 1, GridPosition.z))
            TryAddPredecessor(GridPosition.x, GridPosition.y + scale, GridPosition.z);

        if (!TryAddPredecessor(GridPosition.x, GridPosition.y - 1, GridPosition.z))
            TryAddPredecessor(GridPosition.x, GridPosition.y - scale, GridPosition.z);

        if (!TryAddPredecessor(GridPosition.x, GridPosition.y, GridPosition.z + 1))
            TryAddPredecessor(GridPosition.x, GridPosition.y, GridPosition.z + scale);

        if (!TryAddPredecessor(GridPosition.x, GridPosition.y, GridPosition.z - 1))
            TryAddPredecessor(GridPosition.x, GridPosition.y, GridPosition.z - scale);

        bool TryAddPredecessor(int x, int y, int z)
        {
            if (x >= 0 && x < VoxelGrid.Voxels.GetLength(0) &&
                y >= 0 && y < VoxelGrid.Voxels.GetLength(1) &&
                z >= 0 && z < VoxelGrid.Voxels.GetLength(2))
            {
                Voxel_Base voxel = VoxelGrid.Voxels[x, y, z];

                if (voxel != null && !voxel.IsObstacle)
                {
                    predecessors.AddFirst(voxel);
                    return true;
                }
            }

            return false;
        }

        return predecessors;
    }
}

public class Priority_3D
{
    public double PrimaryPriority;
    public double SecondaryPriority;

    public Priority_3D(double primaryPriority, double secondaryPriority)
    {
        PrimaryPriority = primaryPriority;
        SecondaryPriority = secondaryPriority;
    }
    public int CompareTo(Priority_3D that)
    {
        if (PrimaryPriority < that.PrimaryPriority) return -1;
        else if (PrimaryPriority > that.PrimaryPriority) return 1;
        if (SecondaryPriority > that.SecondaryPriority) return 1;
        else if (SecondaryPriority < that.SecondaryPriority) return -1;
        return 0;
    }
}

public class NodeQueue_3D
{
    public Voxel_Base Node;
    public Priority_3D Priority;

    public NodeQueue_3D(Voxel_Base node, Priority_3D priority)
    {
        Node = node;
        Priority = priority;
    }
}

public class PriorityQueue_3D
{
    int _currentPosition;
    NodeQueue_3D[] _nodeQueue;
    Dictionary<Voxel_Base, int> _priorityQueue;

    public PriorityQueue_3D(int maxNodes)
    {
        _currentPosition = 0;
        _nodeQueue = new NodeQueue_3D[maxNodes];
        _priorityQueue = new Dictionary<Voxel_Base, int>();
    }

    public Priority_3D Peek()
    {
        if (_currentPosition == 0) return new Priority_3D(Double.PositiveInfinity, Double.PositiveInfinity);

        return _nodeQueue[1].Priority;
    }

    public Voxel_Base Dequeue()
    {
        if (_currentPosition == 0) return null;

        Voxel_Base node = _nodeQueue[1].Node;
        _nodeQueue[1] = _nodeQueue[_currentPosition];
        _priorityQueue[_nodeQueue[1].Node] = 1;
        _priorityQueue[node] = 0;
        _currentPosition--;
        _moveDown(1);
        return node;
    }

    public void Enqueue(Voxel_Base node, Priority_3D priority)
    {
        NodeQueue_3D nodeQueue = new NodeQueue_3D(node, priority);
        _currentPosition++;
        _priorityQueue[node] = _currentPosition;
        if (_currentPosition == _nodeQueue.Length) Array.Resize<NodeQueue_3D>(ref _nodeQueue, _nodeQueue.Length * 2);
        _nodeQueue[_currentPosition] = nodeQueue;
        _moveUp(_currentPosition);
    }

    public void Update(Voxel_Base node, Priority_3D priority)
    {
        int index = _priorityQueue[node];
        if (index == 0) return;
        Priority_3D priorityOld = _nodeQueue[index].Priority;
        _nodeQueue[index].Priority = priority;
        if (priorityOld.CompareTo(priority) < 0)
        {
            _moveDown(index);
        }
        else
        {
            _moveUp(index);
        }
    }

    public void Remove(Voxel_Base node)
    {
        int index = _priorityQueue[node];

        if (index == 0) return;

        _priorityQueue[node] = 0;
        _nodeQueue[index] = _nodeQueue[_currentPosition];
        _priorityQueue[_nodeQueue[index].Node] = index;
        _currentPosition--;
        _moveDown(index);
    }

    public bool Contains(Voxel_Base node)
    {
        int index;
        if (!_priorityQueue.TryGetValue(node, out index))
        {
            return false;
        }
        return index != 0;
    }

    void _moveDown(int index)
    {
        int childL = index * 2;
        if (childL > _currentPosition) return;
        int childR = index * 2 + 1;
        int smallerChild;
        if (childR > _currentPosition)
        {
            smallerChild = childL;
        }
        else if (_nodeQueue[childL].Priority.CompareTo(_nodeQueue[childR].Priority) < 0)
        {
            smallerChild = childL;
        }
        else
        {
            smallerChild = childR;
        }
        if (_nodeQueue[index].Priority.CompareTo(_nodeQueue[smallerChild].Priority) > 0)
        {
            _swap(index, smallerChild);
            _moveDown(smallerChild);
        }
    }

    void _moveUp(int index)
    {
        if (index == 1) return;
        int parent = index / 2;
        if (_nodeQueue[parent].Priority.CompareTo(_nodeQueue[index].Priority) > 0)
        {
            _swap(parent, index);
            _moveUp(parent);
        }
    }

    void _swap(int indexA, int indexB)
    {
        NodeQueue_3D tempQueue = _nodeQueue[indexA];
        _nodeQueue[indexA] = _nodeQueue[indexB];
        _priorityQueue[_nodeQueue[indexB].Node] = indexA;
        _nodeQueue[indexB] = tempQueue;
        _priorityQueue[tempQueue.Node] = indexB;
    }
}
public enum MoverType { Ground, Fly, Dig, Swim }

public interface PathfinderMover_3D
{
    List<MoverType> MoverType { get; set; }
    bool CanGetNewPath { get; set; }
    void MoveTo(Voxel_Base target);
    void StartPathfindingCoroutine(IEnumerator coroutine);
    void StopPathfindingCoroutine();
    List<Vector3> GetObstaclesInVision();
}