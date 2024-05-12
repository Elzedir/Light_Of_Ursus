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

        _pathfinderCoroutine = Manager_Game.Instance.StartVirtualCoroutine(_runPathfinder());
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

        Voxel_Base lastNode = _startVoxel;

        _initialise();

        _computeShortestPath();

        while (!_startVoxel.Equals(_targetVoxel))
        {
            _startVoxel = _minimumSuccessorNode(_startVoxel);
            LinkedList<Vector3> obstacleCoordinates = _mover.GetObstaclesInVision();
            double oldPriorityModifier = _priorityModifier;
            Voxel_Base oldLastNode = lastNode;
            _priorityModifier += _manhattanDistance(_startVoxel, lastNode);
            lastNode = _startVoxel;
            bool change = false;

            foreach (Vector3 position in obstacleCoordinates)
            {
                Voxel_Base voxel = VoxelGrid.GetVoxelAtPosition(position);

                if (voxel != null) continue;
                
                voxel = VoxelGrid.AddSubvoxelToVoxelGrid(position, true);
                change = true;

                foreach (Voxel_Base p in voxel.GetPredecessors(VoxelGrid.Voxels))
                {
                    _updateVertex(p);
                }
            }

            if (!change)
            {
                _priorityModifier = oldPriorityModifier;
                lastNode = oldLastNode;
            }
            else
            {
                _mover.MoveTo(_targetVoxel);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void _initialise()
    {
        foreach (Voxel_Base node in VoxelGrid.Voxels) { node.RHS = double.PositiveInfinity; node.G = double.PositiveInfinity; }
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

    void _updateVertex(Voxel_Base node)
    {
        if (!node.Equals(_targetVoxel))
        {
            node.RHS = _minimumSuccessorCost(node);
        }
        if (_mainPriorityQueue.Contains(node))
        {
            _mainPriorityQueue.Remove(node);
        }
        if (node.G != node.RHS)
        {
            _mainPriorityQueue.Enqueue(node, _calculatePriority(node));
        }
    }
    Voxel_Base _minimumSuccessorNode(Voxel_Base node)
    {
        double minimumCostToMove = Double.PositiveInfinity;
        Voxel_Base newNode = null;
        foreach (Voxel_Base successor in node.GetSuccessors(VoxelGrid.Voxels))
        {
            double costToMove = node.GetMovementCostTo(successor, _puzzleSet) + successor.G;

            if (costToMove <= minimumCostToMove && !successor.IsObstacle)
            {
                minimumCostToMove = costToMove;
                newNode = successor;
            }
        }

        newNode.Predecessor = node;

        return newNode;
    }
    double _minimumSuccessorCost(Voxel_Base node)
    {
        double minimumCost = Double.PositiveInfinity;
        foreach (Voxel_Base successor in node.GetSuccessors(VoxelGrid.Voxels))
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
                foreach (Voxel_Base neighbour in node.GetPredecessors(VoxelGrid.Voxels))
                {
                    _updateVertex(neighbour);
                }
            }
            else
            {
                node.G = Double.PositiveInfinity;
                _updateVertex(node);
                foreach (Voxel_Base neighbour in node.GetPredecessors(VoxelGrid.Voxels))
                {
                    _updateVertex(neighbour);
                }
            }
        }
    }
    #endregion

    public List<Vector3> RetrievePath(Voxel_Base startNode, Voxel_Base targetNode)
    {
        List<Vector3> path = new List<Vector3>();
        Voxel_Base currentNode = targetNode;

        while (currentNode != null && !currentNode.Equals(startNode))
        {
            if (currentNode == null || currentNode.Equals(startNode)) break;

            path.Add(new Vector3(currentNode.WorldPosition.x, currentNode.WorldPosition.y, currentNode.WorldPosition.z));
            currentNode = currentNode.Predecessor;
        }

        if (currentNode != null)
        {
            path.Add(new Vector3(startNode.WorldPosition.x, startNode.WorldPosition.y, startNode.WorldPosition.z));
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
    static float _scale;
    static Vector3 _offset;

    public static void InitializeVoxelGrid(float width, float height, float depth, float scale, Vector3 offset)
    {
        _scale = scale;
        _offset = offset;

        int gridWidth = (int)((width + 1) * scale);
        int gridHeight = (int)((height + 1) * scale);
        int gridDepth = (int)((depth + 1) * scale);

        Pathfinder_Base_3D.SetGrid(gridWidth, gridHeight, gridDepth);

        Voxels = new Voxel_Base[gridWidth, gridHeight, gridDepth];

        for (int x = 0; x < gridWidth; x += (int)_scale)
        {
            for (int y = 0; y < gridHeight; y += (int)_scale)
            {
                for (int z = 0; z < gridDepth; z += (int)_scale)
                {
                    if (y < 1 * scale || y > 2 * _scale) continue;

                    float worldX = ((float)x / scale) - offset.x;
                    float worldY = ((float)y / scale) - offset.y;
                    float worldZ = ((float)z / scale) - offset.z;

                    Voxels[x, y, z] = new Voxel_Base
                    {
                        GridPosition = new Vector3Int(x, y, z),
                        WorldPosition = new Vector3(worldX, worldY, worldZ),
                        Size = Vector3.one,
                        G = double.PositiveInfinity,
                        RHS = double.PositiveInfinity
                    };

                    Debug.Log(Voxels[x, y, z].WorldPosition);
                }
            }
        }

        foreach (Voxel_Base voxel in Voxels)
        {
            Debug.Log(voxel.WorldPosition);
        }
    }

    public static Voxel_Base AddSubvoxelToVoxelGrid(Vector3 position, bool isObstacle)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.5f);
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
            Debug.Log("Voxel with same scale already exists at this position.");
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

        Voxels[gridPosition.x, gridPosition.y, gridPosition.z].TestShowVoxel(GameObject.Find("TestTransform").transform);

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
                if (Voxels[(int)(((int)(gridPosition.x / _scale)) * _scale), (int)(((int)(gridPosition.y / _scale)) * _scale), (int)(((int)(gridPosition.z / _scale)) * _scale)] == null)
                {
                    Debug.Log("Voxel does not exist at:");
                    Debug.Log($"WorldPos: {position}");
                    Debug.Log($"GridPos: {position * _scale}");
                    Debug.Log($"OffsetGridPos: {gridPosition}");
                    return null;
                }

                Debug.Log(Voxels[(int)(((int)(gridPosition.x / _scale)) * _scale), (int)(((int)(gridPosition.y / _scale)) * _scale), (int)(((int)(gridPosition.z / _scale)) * _scale)].GridPosition);
                Debug.Log(Voxels[(int)(((int)(gridPosition.x / _scale)) * _scale), (int)(((int)(gridPosition.y / _scale)) * _scale), (int)(((int)(gridPosition.z / _scale)) * _scale)].WorldPosition);
                return Voxels[(int)(((int)(gridPosition.x / _scale)) * _scale), (int)(((int)(gridPosition.y / _scale)) * _scale), (int)(((int)(gridPosition.z / _scale)) * _scale)];
            }

            return Voxels[gridPosition.x, gridPosition.y, gridPosition.z];
        }
        else
        {
            Debug.Log($"WorldPos: {position}");
            Debug.Log($"GridPos: {position * _scale}");
            Debug.Log($"OffsetGridPos: {gridPosition}");
            Debug.Log($"Grid: {Voxels.GetLength(0)}_{Voxels.GetLength(1)}_{Voxels.GetLength(2)}");

            Debug.LogError("Converted position is out of voxel grid bounds.");
            return null;
        }
    }

    public static Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector3Int(
            (int)((worldPosition.x + _offset.x) * _scale),
            (int)((worldPosition.y + _offset.y) * _scale),
            (int)((worldPosition.z + _offset.z) * _scale)
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
    private Voxel_Base _predecessor;
    public Voxel_Base Predecessor { get; set; }

    public double MovementCost;

    public bool IsObstacle { get; private set; }

    public void TestShowVoxel(Transform transform)
    {
        GameObject voxelGO = new GameObject($"{WorldPosition}");
        voxelGO.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        voxelGO.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Meshes/Material_White");
        voxelGO.transform.SetParent(transform);
        voxelGO.transform.localPosition = WorldPosition;
        voxelGO.transform.localScale = Size;
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

    public LinkedList<Voxel_Base> GetSuccessors(Voxel_Base[,,] nodes)
    {
        LinkedList<Voxel_Base> successors = new LinkedList<Voxel_Base>();

        int width = nodes.GetLength(0);
        int height = nodes.GetLength(1);
        int depth = nodes.GetLength(2);

        if (GridPosition.x + 1 < width)
            successors.AddFirst(nodes[GridPosition.x + 1, GridPosition.y, GridPosition.z]);
        if (GridPosition.x - 1 >= 0)
            successors.AddFirst(nodes[GridPosition.x - 1, GridPosition.y, GridPosition.z]);

        if (GridPosition.y + 1 < height)
            successors.AddFirst(nodes[GridPosition.x, GridPosition.y + 1, GridPosition.z]);
        if (GridPosition.y - 1 >= 0)
            successors.AddFirst(nodes[GridPosition.x, GridPosition.y - 1, GridPosition.z]);

        if (GridPosition.z + 1 < depth)
            successors.AddFirst(nodes[GridPosition.x, GridPosition.y, GridPosition.z + 1]);
        if (GridPosition.z - 1 >= 0)
            successors.AddFirst(nodes[GridPosition.x, GridPosition.y, GridPosition.z - 1]);

        return successors;
    }

    public LinkedList<Voxel_Base> GetPredecessors(Voxel_Base[,,] nodes)
    {
        LinkedList<Voxel_Base> neighbours = new();

        int width = nodes.GetLength(0);
        int height = nodes.GetLength(1);
        int depth = nodes.GetLength(2);

        Voxel_Base tempNode;

        if (GridPosition.x + 1 < width)
        {
            tempNode = nodes[GridPosition.x + 1, GridPosition.y, GridPosition.z];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }
        if (GridPosition.x - 1 >= 0)
        {
            tempNode = nodes[GridPosition.x - 1, GridPosition.y, GridPosition.z];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }

        if (GridPosition.y + 1 < height)
        {
            tempNode = nodes[GridPosition.x, GridPosition.y + 1, GridPosition.z];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }
        if (GridPosition.y - 1 >= 0)
        {
            tempNode = nodes[GridPosition.x, GridPosition.y - 1, GridPosition.z];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }

        if (GridPosition.z + 1 < depth)
        {
            tempNode = nodes[GridPosition.x, GridPosition.y, GridPosition.z + 1];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }
        if (GridPosition.z - 1 >= 0)
        {
            tempNode = nodes[GridPosition.x, GridPosition.y, GridPosition.z - 1];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }

        return neighbours;
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
    void MoveTo(Voxel_Base target);
    LinkedList<Vector3> GetObstaclesInVision();
}