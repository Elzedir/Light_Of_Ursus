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
    static int _width;
    static int _height;
    static int _depth;

    public static void SetGrid(int width, int height, int depth)
    {
        _width = width;
        _height = height;
        _depth = depth;
    }

    public void SetPath(Vector3Int start, Vector3Int target, PathfinderMover_3D mover, PuzzleSet puzzleSet)
    {
        _puzzleSet = puzzleSet;
        _mover = mover;
        _startNode = new Voxel_Base();
        _startNode.Position = start;
        _targetNode = new Voxel_Base();
        _targetNode.Position = target;

        _pathfinderCoroutine = Manager_Game.Instance.StartVirtualCoroutine(RunPathfinder());
    }

    public IEnumerator RunPathfinder()
    {
        if (_startNode.Equals(_targetNode)) yield break;

        Voxel_Base lastNode = _startNode;

        _initialise();

        _computeShortestPath();

        while (!_startNode.Equals(_targetNode))
        {
            _startNode = _minimumSuccessorNode(_startNode);
            LinkedList<Vector3Int> obstacleCoordinates = _mover.GetObstaclesInVision();
            double oldPriorityModifier = _priorityModifier;
            Voxel_Base oldLastNode = lastNode;
            _priorityModifier += _manhattanDistance(_startNode, lastNode);
            lastNode = _startNode;
            bool change = false;

            foreach (Vector3Int coordinate in obstacleCoordinates)
            {
                Voxel_Base node = VoxelGrid.Voxels[(int)coordinate.x, (int)coordinate.y, (int)coordinate.z];
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
            _computeShortestPath();

            _mover.MoveTo(_targetNode);

            yield return new WaitForSeconds(1);
        }
    }

    void _initialise()
    {
        foreach (Voxel_Base node in VoxelGrid.Voxels) { node.RHS = double.PositiveInfinity; node.G = double.PositiveInfinity; }
        _mainPriorityQueue = new PriorityQueue_3D(_width * _height * _depth);
        _priorityModifier = 0;
        _targetNode = VoxelGrid.Voxels[_targetNode.Position.x, _targetNode.Position.y, _targetNode.Position.z];
        _startNode = VoxelGrid.Voxels[_startNode.Position.x, _startNode.Position.y, _startNode.Position.z];
        _targetNode.RHS = 0;
        _mainPriorityQueue.Enqueue(_targetNode, _calculatePriority(_targetNode));
    }

    Priority_3D _calculatePriority(Voxel_Base node)
    {
        return new Priority_3D(Math.Min(node.G, node.RHS) + _manhattanDistance(node, _startNode) + _priorityModifier, Math.Min(node.G, node.RHS));
    }
    double _manhattanDistance(Voxel_Base a, Voxel_Base b)
    {
        Vector3Int distance = a.Position - b.Position;
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

    public static Voxel_Base GetVoxelAtPosition(Vector3Int position)
    {
        if (VoxelGrid.Voxels[position.x, position.y, position.x] != null) return VoxelGrid.Voxels[position.x, position.y, position.x];
        return null;
    }

    public List<Vector3Int> RetrievePath(Voxel_Base startNode, Voxel_Base targetNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Voxel_Base currentNode = targetNode;

        while (currentNode != null && !currentNode.Equals(startNode))
        {
            if (currentNode == null || currentNode.Equals(startNode)) break;

            path.Add(new Vector3Int(currentNode.Position.x, currentNode.Position.y, currentNode.Position.z));
            currentNode = currentNode.Predecessor;
        }

        if (currentNode != null)
        {
            path.Add(new Vector3Int(startNode.Position.x, startNode.Position.y, startNode.Position.z));
        }

        path.Reverse();

        return path;
    }

    public static void FindAllPredecessors(Voxel_Base node, int infiniteEnd, int infinityStart = 0)
    {
        infinityStart++;
        if (infinityStart > infiniteEnd) return;
        if (node.Predecessor == null) { Debug.Log($"{node.Position}_{node.Position} predecessor is null"); return; }

        Debug.Log($"{node.Position}_{node.Position} -> {node.Predecessor.Position}_{node.Predecessor.Position}");
        FindAllPredecessors(node.Predecessor, infinityStart, infiniteEnd);
    }
}

public class VoxelGrid
{
    public static Voxel_Base[,,] Voxels;

    public static Voxel_Base[,,] InitializeVoxelGrid(int width, int height, int depth)
    {
        Pathfinder_Base_3D.SetGrid(width, height, depth);

        Voxels = new Voxel_Base[width, height, depth];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    Voxels[x, y, z] = new Voxel_Base();
                    Voxels[x, y, z].Position = new Vector3Int(x, y, z);
                    Voxels[x, y, z].G = double.PositiveInfinity;
                    Voxels[x, y, z].RHS = double.PositiveInfinity;
                }
            }
        }

        return Voxels;
    }
}

public class Voxel_Base
{
    public Vector3Int Position;
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
        if (Position == that.Position && Position == that.Position) return true;
        return false;
    }

    public LinkedList<Voxel_Base> GetSuccessors(Voxel_Base[,,] nodes)
    {
        LinkedList<Voxel_Base> successors = new LinkedList<Voxel_Base>();

        int width = nodes.GetLength(0);
        int height = nodes.GetLength(1);
        int depth = nodes.GetLength(2);

        if (Position.x + 1 < width)
            successors.AddFirst(nodes[(int)Position.x + 1, (int)Position.y, (int)Position.z]);
        if (Position.x - 1 >= 0)
            successors.AddFirst(nodes[(int)Position.x - 1, (int)Position.y, (int)Position.z]);

        if (Position.y + 1 < height)
            successors.AddFirst(nodes[(int)Position.x, (int)Position.y + 1, (int)Position.z]);
        if (Position.y - 1 >= 0)
            successors.AddFirst(nodes[(int)Position.x, (int)Position.y - 1, (int)Position.z]);

        if (Position.z + 1 < depth)
            successors.AddFirst(nodes[(int)Position.x, (int)Position.y, (int)Position.z + 1]);
        if (Position.z - 1 >= 0)
            successors.AddFirst(nodes[(int)Position.x, (int)Position.y, (int)Position.z - 1]);

        return successors;
    }

    public LinkedList<Voxel_Base> GetPredecessors(Voxel_Base[,,] nodes)
    {
        LinkedList<Voxel_Base> neighbours = new LinkedList<Voxel_Base>();

        int width = nodes.GetLength(0);
        int height = nodes.GetLength(1);
        int depth = nodes.GetLength(2);

        Voxel_Base tempNode;

        if (Position.x + 1 < width)
        {
            tempNode = nodes[(int)Position.x + 1, (int)Position.y, (int)Position.z];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }
        if (Position.x - 1 >= 0)
        {
            tempNode = nodes[(int)Position.x - 1, (int)Position.y, (int)Position.z];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }

        if (Position.y + 1 < height)
        {
            tempNode = nodes[(int)Position.x, (int)Position.y + 1, (int)Position.z];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }
        if (Position.y - 1 >= 0)
        {
            tempNode = nodes[(int)Position.x, (int)Position.y - 1, (int)Position.z];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }

        if (Position.z + 1 < depth)
        {
            tempNode = nodes[(int)Position.x, (int)Position.y, (int)Position.z + 1];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }
        if (Position.z - 1 >= 0)
        {
            tempNode = nodes[(int)Position.x, (int)Position.y, (int)Position.z - 1];
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
    LinkedList<Vector3Int> GetObstaclesInVision();
}

