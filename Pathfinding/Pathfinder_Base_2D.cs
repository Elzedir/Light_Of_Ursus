using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder_Base_2D
{
    PriorityQueue_2D _mainPriorityQueue;
    double _priorityModifier;
    Node_Base_2D _targetNode;
    Node_Base_2D _startNode;
    PuzzleSet _puzzleSet;

    #region Initialisation
    public void RunPathfinderOpenWorld(Coordinates start, Coordinates target, PathfinderMover_2D mover)
    {
        RunPathfinder(Manager_Grid.Rows, Manager_Grid.Columns, new Coordinates(start.X + Manager_Grid.XOffset, start.Y + Manager_Grid.YOffset), new Coordinates(target.X + Manager_Grid.XOffset, target.Y + Manager_Grid.YOffset), mover, PuzzleSet.None);
    }
    public void RunPathfinder(int rows, int columns, Coordinates start, Coordinates target, PathfinderMover_2D mover, PuzzleSet puzzleSet)
    {
        if (start.Equals(target)) return;

        _puzzleSet = puzzleSet;
        _startNode = new Node_Base_2D();
        _startNode.X = start.X;
        _startNode.Y = start.Y;
        _targetNode = new Node_Base_2D();
        _targetNode.X = target.X;
        _targetNode.Y = target.Y;

        Node_Base_2D lastNode = _startNode;

        _initialise(rows, columns);

        _computeShortestPath();

        int infinity = 0;

        while (!_startNode.Equals(_targetNode) && infinity < 1000)
        {
            _startNode = _minimumSuccessorNode(_startNode);
            LinkedList<Coordinates> obstacleCoordinates = mover.GetObstaclesInVision();
            double oldPriorityModifier = _priorityModifier;
            Node_Base_2D oldLastNode = lastNode;
            _priorityModifier += _manhattanDistance(_startNode, lastNode);
            lastNode = _startNode;
            bool change = false;

            foreach (Coordinates coordinate in obstacleCoordinates)
            {
                Node_Base_2D node = NodeArray_2D.Nodes[coordinate.X, coordinate.Y];
                if (node.IsObstacle) continue;
                change = true;
                node.IsObstacle = true;
                foreach (Node_Base_2D p in node.GetPredecessors(NodeArray_2D.Nodes))
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

            infinity++;
        }

        mover.MoveTo(_targetNode);
    }

    void _initialise(int rows, int columns)
    {
        foreach (Node_Base_2D node in NodeArray_2D.Nodes){ node.RHS = double.PositiveInfinity; node.G = double.PositiveInfinity; }
        _mainPriorityQueue = new PriorityQueue_2D(rows * columns);
        _priorityModifier = 0;
        _targetNode = NodeArray_2D.Nodes[_targetNode.X, _targetNode.Y];
        _startNode = NodeArray_2D.Nodes[_startNode.X, _startNode.Y];
        _targetNode.RHS = 0;
        _mainPriorityQueue.Enqueue(_targetNode, _calculatePriority(_targetNode));
    }

    Priority_2D _calculatePriority(Node_Base_2D node)
    {
        return new Priority_2D(Math.Min(node.G, node.RHS) + _manhattanDistance(node, _startNode) + _priorityModifier, Math.Min(node.G, node.RHS));
    }
    double _manhattanDistance(Node_Base_2D a, Node_Base_2D b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
    
    void _updateVertex(Node_Base_2D node)
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
    Node_Base_2D _minimumSuccessorNode(Node_Base_2D node)
    {
        double minimumCostToMove = Double.PositiveInfinity;
        Node_Base_2D newNode = null;
        foreach (Node_Base_2D successor in node.GetSuccessors(NodeArray_2D.Nodes))
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
    double _minimumSuccessorCost(Node_Base_2D node)
    {
        double minimumCost = Double.PositiveInfinity;
        foreach (Node_Base_2D successor in node.GetSuccessors(NodeArray_2D.Nodes))
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
            Priority_2D highestPriority = _mainPriorityQueue.Peek();
            Node_Base_2D node = _mainPriorityQueue.Dequeue();
            if (node == null) break;

            if (highestPriority.CompareTo(_calculatePriority(node)) < 0)
            {
                _mainPriorityQueue.Enqueue(node, _calculatePriority(node));
            }
            else if (node.G > node.RHS)
            {
                node.G = node.RHS;
                foreach (Node_Base_2D neighbour in node.GetPredecessors(NodeArray_2D.Nodes))
                {
                    _updateVertex(neighbour);
                }
            }
            else
            {
                node.G = Double.PositiveInfinity;
                _updateVertex(node);
                foreach (Node_Base_2D neighbour in node.GetPredecessors(NodeArray_2D.Nodes))
                {
                    _updateVertex(neighbour);
                }
            }
        }
    }
    #endregion

    public void UpdateWallState(Node_Base_2D node, Direction direction, bool wallExists)
    {
        double newCost = wallExists ? Double.PositiveInfinity : 1;

        node.UpdateMovementCost(direction, newCost);

        Node_Base_2D neighborNode = GetNeighbor(node, direction);

        Direction oppositeDirection = GetOppositeDirection(direction);
        neighborNode.UpdateMovementCost(oppositeDirection, newCost);
    }

    Node_Base_2D GetNeighbor(Node_Base_2D node, Direction direction)
    {
        switch (direction)
        {
            case Direction.Top: return node.Y + 1 < NodeArray_2D.Nodes.GetLength(1) ? NodeArray_2D.Nodes[node.X, node.Y + 1] : null;
            case Direction.Bottom: return node.Y - 1 >= 0 ? NodeArray_2D.Nodes[node.X, node.Y - 1] : null;
            case Direction.Left: return node.X - 1 >= 0 ? NodeArray_2D.Nodes[node.X - 1, node.Y] : null;
            case Direction.Right: return node.X + 1 < NodeArray_2D.Nodes.GetLength(0) ? NodeArray_2D.Nodes[node.X + 1, node.Y] : null;
            default: return null;
        }
    }

    Direction GetOppositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Top: return Direction.Bottom;
            case Direction.Bottom: return Direction.Top;
            case Direction.Left: return Direction.Right;
            case Direction.Right: return Direction.Left;
            default: throw new InvalidOperationException("Invalid wall direction.");
        }
    }

    public static Node_Base_2D GetNodeAtPosition(int x, int y)
    {
        if (NodeArray_2D.Nodes[x, y] != null) return NodeArray_2D.Nodes[x, y];
        return null;
    }

    public List<Coordinates> RetrievePath(Node_Base_2D startNode, Node_Base_2D targetNode)
    {
        List<Coordinates> path = new List<Coordinates>();
        Node_Base_2D currentNode = targetNode;

        while (currentNode != null && !currentNode.Equals(startNode))
        {
            if (currentNode == null || currentNode.Equals(startNode)) break;

            path.Add(new Coordinates(currentNode.X, currentNode.Y));
            currentNode = currentNode.Predecessor;
        }

        if (currentNode != null)
        {
            path.Add(new Coordinates(startNode.X, startNode.Y));
        }

        path.Reverse();

        return path;
    }

    public static void FindAllPredecessors(Node_Base_2D node, int infiniteEnd, int infinityStart = 0)
    {
        infinityStart++;
        if (infinityStart > infiniteEnd) return;
        if (node.Predecessor == null) { Debug.Log($"{node.X}_{node.Y} predecessor is null"); return; }

        Debug.Log($"{node.X}_{node.Y} -> {node.Predecessor.X}_{node.Predecessor.Y}");
        FindAllPredecessors(node.Predecessor, infinityStart, infiniteEnd);
    }
}

public class NodeArray_2D
{
    public static Node_Base_2D[,] Nodes;

    public static Node_Base_2D[,] InitializeArray(int rows, int columns)
    {
        Nodes = new Node_Base_2D[rows, columns];

        for (int row = 0; row < Nodes.GetLength(0); row++)
        {
            for (int column = 0; column < Nodes.GetLength(1); column++)
            {
                Nodes[row, column] = new Node_Base_2D();
                Nodes[row, column].X = row;
                Nodes[row, column].Y = column;
                Nodes[row, column].G = Double.PositiveInfinity;
                Nodes[row, column].RHS = Double.PositiveInfinity;
            }
        }

        return Nodes;
    }
}

public enum Direction { None, Top, Bottom, Left, Right }

public class Node_Base_2D
{
    public int X;
    public int Y;
    public double G;
    public double RHS;
    private Node_Base_2D _predecessor;
    public Node_Base_2D Predecessor { get; set; }

    public double MovementCost;
    public Dictionary<Direction, double> MovementCosts { get; private set; }

    public bool IsObstacle;

    public Node_Base_2D()
    {
        MovementCosts = new Dictionary<Direction, double>
        {
            { Direction.Top, Double.PositiveInfinity },
            { Direction.Bottom, Double.PositiveInfinity },
            { Direction.Left, Double.PositiveInfinity },
            { Direction.Right, Double.PositiveInfinity }
        };
    }

    public void UpdateMovementCost(Direction direction, double cost)
    {
        if (direction == Direction.None) MovementCost = cost;
        else MovementCosts[direction] = cost;
    }

    public double GetMovementCostTo(Node_Base_2D successor, PuzzleSet puzzleSet)
    {
        if (puzzleSet == PuzzleSet.MouseMaze)
        {
            Direction directionToSuccessor = Direction.None;

            if (X == successor.X)
            {
                if (Y == successor.Y - 1) directionToSuccessor = Direction.Top;
                else if (Y == successor.Y + 1) directionToSuccessor = Direction.Bottom;
                else throw new InvalidOperationException("Nodes are not X adjacent.");
            }
            else if (Y == successor.Y)
            {
                if (X == successor.X - 1) directionToSuccessor = Direction.Right;
                else if (X == successor.X + 1) directionToSuccessor = Direction.Left;
                else throw new InvalidOperationException("Nodes are not Y adjacent.");
            }
            else throw new InvalidOperationException("Nodes are not adjacent.");

            if (!MovementCosts.TryGetValue(directionToSuccessor, out double cost) || directionToSuccessor == Direction.None) throw new InvalidOperationException("Invalid direction.");

            return cost;
        }
        else if (puzzleSet == PuzzleSet.IceWall || puzzleSet == PuzzleSet.None)
        {
            return successor.MovementCost;
        }

        throw new InvalidOperationException($"{puzzleSet} not valid.");
    }

    public bool Equals(Node_Base_2D that)
    {
        if (X == that.X && Y == that.Y) return true;
        return false;
    }

    public LinkedList<Node_Base_2D> GetSuccessors(Node_Base_2D[,] nodes)
    {
        LinkedList<Node_Base_2D> successors = new LinkedList<Node_Base_2D>();
        if (X + 1 < nodes.GetLength(0)) successors.AddFirst(nodes[X + 1, Y]);
        if (Y + 1 < nodes.GetLength(1)) successors.AddFirst(nodes[X, Y + 1]);
        if (X - 1 >= 0) successors.AddFirst(nodes[X - 1, Y]);
        if (Y - 1 >= 0) successors.AddFirst(nodes[X, Y - 1]);
        return successors;
    }

    public LinkedList<Node_Base_2D> GetPredecessors(Node_Base_2D[,] nodes)
    {
        LinkedList<Node_Base_2D> neighbours = new LinkedList<Node_Base_2D>();
        Node_Base_2D tempNode;
        if (X + 1 < nodes.GetLength(0))
        {
            tempNode = nodes[X + 1, Y];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }
        if (Y + 1 < nodes.GetLength(1))
        {
            tempNode = nodes[X, Y + 1];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }
        if (X - 1 >= 0)
        {
            tempNode = nodes[X - 1, Y];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }
        if (Y - 1 >= 0)
        {
            tempNode = nodes[X, Y - 1];
            if (!tempNode.IsObstacle) neighbours.AddFirst(tempNode);
        }
        return neighbours;
    }
}

public class Priority_2D
{
    public double PrimaryPriority;
    public double SecondaryPriority;

    public Priority_2D(double primaryPriority, double secondaryPriority)
    {
        PrimaryPriority = primaryPriority;
        SecondaryPriority = secondaryPriority;
    }
    public int CompareTo(Priority_2D that)
    {
        if (PrimaryPriority < that.PrimaryPriority) return -1;
        else if (PrimaryPriority > that.PrimaryPriority) return 1;
        if (SecondaryPriority > that.SecondaryPriority) return 1;
        else if (SecondaryPriority < that.SecondaryPriority) return -1;
        return 0;
    }
}

public class NodeQueue_2D
{
    public Node_Base_2D Node;
    public Priority_2D Priority;

    public NodeQueue_2D(Node_Base_2D node, Priority_2D priority)
    {
        Node = node;
        Priority = priority;
    }
}

public class PriorityQueue_2D
{
    int _currentPosition;
    NodeQueue_2D[] _nodeQueue;
    Dictionary<Node_Base_2D, int> _priorityQueue;

    public PriorityQueue_2D(int maxNodes)
    {
        _currentPosition = 0;
        _nodeQueue = new NodeQueue_2D[maxNodes];
        _priorityQueue = new Dictionary<Node_Base_2D, int>();
    }

    public Priority_2D Peek()
    {
        if (_currentPosition == 0) return new Priority_2D(Double.PositiveInfinity, Double.PositiveInfinity);

        return _nodeQueue[1].Priority;
    }

    public Node_Base_2D Dequeue()
    {
        if (_currentPosition == 0) return null;

        Node_Base_2D node = _nodeQueue[1].Node;
        _nodeQueue[1] = _nodeQueue[_currentPosition];
        _priorityQueue[_nodeQueue[1].Node] = 1;
        _priorityQueue[node] = 0;
        _currentPosition--;
        _moveDown(1);
        return node;
    }

    public void Enqueue(Node_Base_2D node, Priority_2D priority)
    {
        NodeQueue_2D nodeQueue = new NodeQueue_2D(node, priority);
        _currentPosition++;
        _priorityQueue[node] = _currentPosition;
        if (_currentPosition == _nodeQueue.Length) Array.Resize<NodeQueue_2D>(ref _nodeQueue, _nodeQueue.Length * 2);
        _nodeQueue[_currentPosition] = nodeQueue;
        _moveUp(_currentPosition);
    }

    public void Update(Node_Base_2D node, Priority_2D priority)
    {
        int index = _priorityQueue[node];
        if (index == 0) return;
        Priority_2D priorityOld = _nodeQueue[index].Priority;
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

    public void Remove(Node_Base_2D node)
    {
        int index = _priorityQueue[node];

        if (index == 0) return;

        _priorityQueue[node] = 0;
        _nodeQueue[index] = _nodeQueue[_currentPosition];
        _priorityQueue[_nodeQueue[index].Node] = index;
        _currentPosition--;
        _moveDown(index);
    }

    public bool Contains(Node_Base_2D node)
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
        NodeQueue_2D tempQueue = _nodeQueue[indexA];
        _nodeQueue[indexA] = _nodeQueue[indexB];
        _priorityQueue[_nodeQueue[indexB].Node] = indexA;
        _nodeQueue[indexB] = tempQueue;
        _priorityQueue[tempQueue.Node] = indexB;
    }
}

public class Coordinates
{
    public int X;
    public int Y;

    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public interface PathfinderMover_2D
{
    void MoveTo(Node_Base_2D target);
    LinkedList<Coordinates> GetObstaclesInVision();
}

