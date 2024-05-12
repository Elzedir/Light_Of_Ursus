using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class Pathfinder_Base_3D
{
    Coroutine _pathfinderCoroutine;
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
        if( _pathfinderCoroutine != null )
        {
            Manager_Game.Instance.StopCoroutine(_pathfinderCoroutine);
            _pathfinderCoroutine = null;
        }
        
        _puzzleSet = puzzleSet;
        _mover = mover;
        _startVoxel = VoxelGrid.GetVoxelAtPosition(start);
        _targetVoxel = VoxelGrid.GetVoxelAtPosition(target);

        mover.StartPathfindingCoroutine(_runPathfinder());
    }

    public void UpdatePath(Vector3 start, Vector3 target)
    {
        _startVoxel = VoxelGrid.GetVoxelAtPosition(start);
        _targetVoxel = VoxelGrid.GetVoxelAtPosition(target);
    }

    IEnumerator _runPathfinder()
    {
        if (_startVoxel == null) Debug.Log("StartVoxel is null");
        if (_targetVoxel == null) Debug.Log("TargetVoxel is null");

        if (_startVoxel.Equals(_targetVoxel)) yield break;

        Voxel_Base currentVoxel = _startVoxel;

        _initialise();

        

        int infinity = 0;

        while (!currentVoxel.Equals(_targetVoxel) && infinity < 1000)
        {
            bool changesInEnvironment = false;
            LinkedList<Vector3> obstacleCoordinates = _mover.GetObstaclesInVision();

            foreach (Vector3 position in obstacleCoordinates)
            {
                Voxel_Base obstacleVoxel = VoxelGrid.GetVoxelAtPosition(position);

                if (obstacleVoxel.IsObstacle) continue;

                obstacleVoxel = VoxelGrid.AddSubvoxelToVoxelGrid(position, true);
                changesInEnvironment = true;

                foreach (Voxel_Base p in obstacleVoxel.GetPredecessors())
                {
                    _updateVertex(p);
                }
            }

            Voxel_Base nextVoxel = _minimumSuccessorVoxel(currentVoxel);

            double previousPriorityModifier = _priorityModifier;
            currentVoxel = nextVoxel;

            _priorityModifier += _manhattanDistance(currentVoxel, nextVoxel);

            if (!changesInEnvironment)
            {
                _priorityModifier = previousPriorityModifier;
            }
            else
            {
                //_mover.MoveTo(_targetVoxel);
            }

            _computeShortestPath();
            infinity++;
        }
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

        Debug.Log($"WorldPos: {voxel.WorldPosition} Cost: {voxel.MovementCost}");

        foreach (Voxel_Base successor in voxel.GetSuccessors())
        {
            Debug.Log($"WorldPos: {successor.WorldPosition} Cost: {successor.MovementCost}");

            double costToMove = voxel.GetMovementCostTo(successor, _puzzleSet) + successor.G;

            if (costToMove <= minimumCostToMove && !successor.IsObstacle)
            {
                minimumCostToMove = costToMove;
                bestSucessor = successor;
            }
        }

        bestSucessor.SetPredecessor(voxel);

        Debug.Log($"Best Succ WorldPos: {bestSucessor.WorldPosition} Cost: {bestSucessor.MovementCost}");

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

        while (currentVoxel != null && !currentVoxel.Equals(startVoxel))
        {
            if (currentVoxel == null || currentVoxel.Equals(startVoxel)) break;

            Debug.Log($"CurrentVoxelPos: {currentVoxel.WorldPosition}");
            Debug.Log($"Predecessor: {currentVoxel.Predecessor}");
            Debug.Log($"PredecessorPos: {currentVoxel.Predecessor.WorldPosition}");

            path.Add(currentVoxel.WorldPosition);
            currentVoxel = currentVoxel.Predecessor;
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
    public static Voxel_Base[,,] Voxels;
    public static float Scale { get; private set; }
    static Vector3 _offset;

    public static void InitializeVoxelGrid(float width, float height, float depth, float scale, Vector3 offset)
    {
        Scale = scale;
        _offset = offset;

        int gridWidth = (int)((width + 1) * scale);
        int gridHeight = (int)((height + 1) * scale);
        int gridDepth = (int)((depth + 1) * scale);

        Pathfinder_Base_3D.SetGrid(gridWidth, gridHeight, gridDepth);

        Voxels = new Voxel_Base[gridWidth, gridHeight, gridDepth];

        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        Material material = Resources.Load<Material>("Meshes/Material_Green");

        for (int x = 0; x < gridWidth; x += (int)Scale)
        {
            for (int y = 0; y < gridHeight; y += (int)Scale)
            {
                for (int z = 0; z < gridDepth; z += (int)Scale)
                {
                    if (y < 1 * scale || y > 2 * Scale) continue;

                    float worldX = ((float)x / scale);// - _offset.x;
                    float worldY = ((float)y / scale);// - _offset.y;
                    float worldZ = ((float)z / scale);// - _offset.z;

                    Voxels[x, y, z] = new Voxel_Base
                    {
                        GridPosition = new Vector3Int(x, y, z),
                        WorldPosition = new Vector3(worldX, worldY, worldZ),
                        Size = Vector3.one,
                        G = double.PositiveInfinity,
                        RHS = double.PositiveInfinity
                    };

                    Voxels[x, y, z].UpdateMovementCost(1);

                    // Voxels[x, y, z].TestShowVoxel(GameObject.Find("TestTransform").transform, mesh, material);
                }
            }
        }
    }

    public static Voxel_Base AddSubvoxelToVoxelGrid(Vector3 position, bool isObstacle)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f);
        Collider collider = null;

        if (hitColliders.Length > 0)
        {
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.name.Contains("Wall"))
                {
                    collider = hitCollider;
                }
            }
        }
        else
        {
            Debug.Log($"No cells at this position {position}.");
            return null;
        }

        Vector3 center = collider.bounds.center;
        Vector3 extents = collider.bounds.extents;
        Vector3 voxelCenter = position;

        Vector3[] vertices = new Vector3[8];
        vertices[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        vertices[1] = center + new Vector3(extents.x, -extents.y, -extents.z);
        vertices[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
        vertices[3] = center + new Vector3(extents.x, extents.y, -extents.z);
        vertices[4] = center + new Vector3(-extents.x, -extents.y, extents.z);
        vertices[5] = center + new Vector3(extents.x, -extents.y, extents.z);
        vertices[6] = center + new Vector3(-extents.x, extents.y, extents.z);
        vertices[7] = center + new Vector3(extents.x, extents.y, extents.z);

        Vector3 minExtents = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 maxExtents = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (Vector3 vertex in vertices)
        {
            Vector3 relativePosition = vertex - voxelCenter;
            minExtents = Vector3.Min(minExtents, relativePosition);
            maxExtents = Vector3.Max(maxExtents, relativePosition);
        }

        Vector3 subVoxelSize = maxExtents - minExtents;
        Vector3 subVoxelCenter = voxelCenter + (minExtents + maxExtents) / 2;

        Vector3Int gridPosition = WorldToGridPosition(subVoxelCenter);

        if (Voxels[gridPosition.x, gridPosition.y, gridPosition.z] != null && Voxels[gridPosition.x, gridPosition.y, gridPosition.z].Size == subVoxelSize)
        {
            Debug.Log($"Voxel already exists at Pos: {position} GridPos: {gridPosition} WorldPos: {Voxels[gridPosition.x, gridPosition.y, gridPosition.z].WorldPosition} with same scale: {subVoxelSize}.");
            return null;
        }

        Voxels[gridPosition.x, gridPosition.y, gridPosition.z] = new Voxel_Base
        {
            GridPosition = new Vector3Int(gridPosition.x, gridPosition.y, gridPosition.z),
            WorldPosition = new Vector3(subVoxelCenter.x, subVoxelCenter.y, subVoxelCenter.z),
            Size = subVoxelSize,
            G = double.PositiveInfinity,
            RHS = double.PositiveInfinity
        };

        if (isObstacle) Voxels[gridPosition.x, gridPosition.y, gridPosition.z].UpdateMovementCost(double.PositiveInfinity);

        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        Material material = Resources.Load<Material>("Meshes/Material_Red");

        //Voxels[gridPosition.x, gridPosition.y, gridPosition.z].TestShowVoxel(GameObject.Find("TestTransform").transform, mesh, material);

        return Voxels[gridPosition.x, gridPosition.y, gridPosition.z];
    }

    public static Voxel_Base GetVoxelAtPosition(Vector3 position)
    {
        Vector3Int gridPosition = WorldToGridPosition(position);

        if (gridPosition.x >= 0 && gridPosition.x < Voxels.GetLength(0) &&
            gridPosition.y >= 0 && gridPosition.y < Voxels.GetLength(1) &&
            gridPosition.z >= 0 && gridPosition.z < Voxels.GetLength(2))
        {
            if (Voxels[gridPosition.x, gridPosition.y, gridPosition.z] == null)
            {
                if (Voxels[(int)(((int)(gridPosition.x / Scale)) * Scale), (int)(((int)(gridPosition.y / Scale)) * Scale), (int)(((int)(gridPosition.z / Scale)) * Scale)] == null)
                {
                    Debug.Log("Voxel does not exist at:");
                    Debug.Log($"WorldPos: {position}");
                    Debug.Log($"GridPos: {position * Scale}");
                    Debug.Log($"OffsetGridPos: {gridPosition}");
                    return null;
                }

                return Voxels[(int)(((int)(gridPosition.x / Scale)) * Scale), (int)(((int)(gridPosition.y / Scale)) * Scale), (int)(((int)(gridPosition.z / Scale)) * Scale)];
            }

            return Voxels[gridPosition.x, gridPosition.y, gridPosition.z];
        }
        else
        {
            Debug.Log($"WorldPos: {position}");
            Debug.Log($"GridPos: {position * Scale}");
            Debug.Log($"OffsetGridPos: {gridPosition}");
            Debug.Log($"Grid: {Voxels.GetLength(0)}_{Voxels.GetLength(1)}_{Voxels.GetLength(2)}");

            Debug.LogError("Converted position is out of voxel grid bounds.");
            return null;
        }
    }

    public static Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector3Int(
            (int)((worldPosition.x + _offset.x) * Scale),
            (int)((worldPosition.y + _offset.y) * Scale),
            (int)((worldPosition.z + _offset.z) * Scale)
        );
    }
}

public class Voxel_Base
{
    public Vector3Int GridPosition;
    public Vector3 WorldPosition;
    public Vector3 Size;
    public double G;
    public double RHS;
    public Voxel_Base Predecessor { get; private set; }

    public double MovementCost;

    public bool IsObstacle { get; private set; }

    public void TestShowVoxel(Transform transform, Mesh mesh, Material material)
    {
        GameObject voxelGO = new GameObject($"{WorldPosition}");
        voxelGO.AddComponent<MeshFilter>().mesh = mesh;
        voxelGO.AddComponent<MeshRenderer>().material = material;
        voxelGO.transform.SetParent(transform);
        voxelGO.transform.localPosition = WorldPosition;
        voxelGO.transform.localScale = Size;
    }

    public void SetPredecessor(Voxel_Base predecessor)
    {
        Predecessor = predecessor;
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
        // Find out why it's only returning three of the possible six sucessor directions.

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

public interface PathfinderMover_3D
{
    void StartPathfindingCoroutine(IEnumerator coroutine);
    void StopPathfindingCoroutine();
    void MoveTo(Voxel_Base target);
    LinkedList<Vector3> GetObstaclesInVision();
}