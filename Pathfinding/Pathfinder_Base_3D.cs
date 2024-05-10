using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pathfinder_Base_3D
{
    Coroutine _pathfinderCoroutine;
    PriorityQueue_3D _mainPriorityQueue;
    double _priorityModifier;
    Voxel_Base _targetNode;
    Voxel_Base _startNode;
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
        _startNode = VoxelGrid.GetVoxelAtPosition(start);
        _targetNode = VoxelGrid.GetVoxelAtPosition(target);

        _pathfinderCoroutine = Manager_Game.Instance.StartVirtualCoroutine(_runPathfinder());
    }

    public void UpdatePath(Vector3 start, Vector3 target)
    {
        _startNode = VoxelGrid.GetVoxelAtPosition(start);
        _targetNode = VoxelGrid.GetVoxelAtPosition(target);
    }

    IEnumerator _runPathfinder()
    {
        Debug.Log(_startNode.WorldPosition);
        Debug.Log(_startNode.GridPosition);
        Debug.Log(_targetNode.WorldPosition);
        Debug.Log(_targetNode.GridPosition);

        if (_startNode.Equals(_targetNode)) yield break;

        Voxel_Base lastNode = _startNode;

        _initialise();

        _computeShortestPath();

        while (!_startNode.Equals(_targetNode))
        {
            _startNode = _minimumSuccessorNode(_startNode);
            LinkedList<Vector3> obstacleCoordinates = _mover.GetObstaclesInVision();
            double oldPriorityModifier = _priorityModifier;
            Voxel_Base oldLastNode = lastNode;
            _priorityModifier += _manhattanDistance(_startNode, lastNode);
            lastNode = _startNode;
            bool change = false;

            foreach (Vector3 coordinate in obstacleCoordinates)
            {
                Voxel_Base node = VoxelGrid.GetVoxelAtPosition(coordinate);
                if (node.IsObstacle) continue;
                change = true;
                node.IsObstacle = true;

                foreach (Voxel_Base p in node.GetPredecessors(VoxelGrid.Voxels))
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
                _mover.MoveTo(_targetNode);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void _initialise()
    {
        foreach (Voxel_Base node in VoxelGrid.Voxels) { node.RHS = double.PositiveInfinity; node.G = double.PositiveInfinity; }
        _mainPriorityQueue = new PriorityQueue_3D(_gridWidth * _gridHeight * _gridDepth);
        _priorityModifier = 0;
        _targetNode.RHS = 0;
        _mainPriorityQueue.Enqueue(_targetNode, _calculatePriority(_targetNode));
    }

    Priority_3D _calculatePriority(Voxel_Base node)
    {
        return new Priority_3D(Math.Min(node.G, node.RHS) + _manhattanDistance(node, _startNode) + _priorityModifier, Math.Min(node.G, node.RHS));
    }
    double _manhattanDistance(Voxel_Base a, Voxel_Base b)
    {
        Vector3 distance = a.WorldPosition - b.WorldPosition;
        return Math.Abs(distance.x) + Math.Abs(distance.y) + Math.Abs(distance.z);
    }

    void _updateVertex(Voxel_Base node)
    {
        if (!node.Equals(_targetNode))
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
        while (_mainPriorityQueue.Peek().CompareTo(_calculatePriority(_startNode)) < 0 || _startNode.RHS != _startNode.G)
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

    public static Voxel_Base[,,] InitializeVoxelGrid(float width, float height, float depth, float scale, Vector3 offset)
    {
        _scale = scale;
        _offset = offset;

        int gridWidth = (int)(width * scale);
        int gridHeight = (int)(height * scale);
        int gridDepth = (int)(depth * scale);

        Pathfinder_Base_3D.SetGrid(gridWidth, gridHeight, gridDepth);

        Voxels = new Voxel_Base[gridWidth, gridHeight, gridDepth];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    float worldX = (x / scale) - offset.x;
                    float worldY = (y / scale) - offset.y;
                    float worldZ = (z / scale) - offset.z;

                    Voxels[x, y, z] = new Voxel_Base
                    {
                        GridPosition = new Vector3Int(x, y, z),
                        WorldPosition = new Vector3(worldX, worldY, worldZ),
                        G = double.PositiveInfinity,
                        RHS = double.PositiveInfinity
                    };

                }
            }
        }

        return Voxels;
    }

    public static Voxel_Base GetVoxelAtPosition(Vector3 position)
    {
        Vector3Int gridPosition = WorldToGridPosition(position);

        Debug.Log(position);
        Debug.Log(position * _scale);

        if (gridPosition.x >= 0 && gridPosition.x < Voxels.GetLength(0) &&
            gridPosition.y >= 0 && gridPosition.y < Voxels.GetLength(1) &&
            gridPosition.z >= 0 && gridPosition.z < Voxels.GetLength(2))
        {
            return Voxels[gridPosition.x, gridPosition.y, gridPosition.z];
        }
        else
        {
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
    public double G;
    public double RHS;
    private Voxel_Base _predecessor;
    public Voxel_Base Predecessor { get; set; }

    public double MovementCost;

    public bool IsObstacle;

    public void UpdateMovementCost(double cost)
    {
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

