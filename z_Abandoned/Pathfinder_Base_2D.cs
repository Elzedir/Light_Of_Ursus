using System;
using System.Collections.Generic;
using Priorities.Priority_Queues;
using UnityEngine;
using static System.Double;

namespace z_Abandoned
{
    public abstract class Pathfinder_Base_2D
    {
        //Priority_Queue_MaxHeap _mainPriorityQueueMaxHeap;
        double _priorityModifier;
        Node_Base_2D _targetNode;
        Node_Base_2D _startNode;
        PuzzleSet _puzzleSet;
    
        public void RunPathfinderOpenWorld(Coordinates start, Coordinates target, PathfinderMover_2D mover)
        {
            RunPathfinder(Manager_Grid.Rows, Manager_Grid.Columns, new Coordinates(start.X + Manager_Grid.XOffset, start.Y + Manager_Grid.YOffset), new Coordinates(target.X + Manager_Grid.XOffset, target.Y + Manager_Grid.YOffset), mover, PuzzleSet.None);
        }
        public void RunPathfinder(int rows, int columns, Coordinates start, Coordinates target, PathfinderMover_2D mover, PuzzleSet puzzleSet)
        {
            if (start.Equals(target)) return;

            _puzzleSet = puzzleSet;
            _startNode = new Node_Base_2D { X = start.X, Y = start.Y };
            _targetNode = new Node_Base_2D { X = target.X, Y = target.Y };

            var lastNode = _startNode;

            _initialise(rows, columns);

            _computeShortestPath();

            var iterations = 0;

            while (!_startNode.Equals(_targetNode) && iterations < 1000)
            {
                _startNode = _minimumSuccessorNode(_startNode);
                var obstacleCoordinates = mover.GetObstaclesInVision();
                var oldPriorityModifier = _priorityModifier;
                var oldLastNode = lastNode;
                _priorityModifier += _manhattanDistance(_startNode, lastNode);
                lastNode = _startNode;
                var change = false;

                foreach (var coordinate in obstacleCoordinates)
                {
                    var node = NodeArray_2D.S_Nodes[coordinate.X, coordinate.Y];
                    if (node.IsObstacle) continue;
                    change = true;
                    node.IsObstacle = true;
                    foreach (var p in node.GetPredecessors(NodeArray_2D.S_Nodes))
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

                iterations++;
            }

            mover.MoveTo(_targetNode);
        }

        void _initialise(int rows, int columns)
        {
            foreach (Node_Base_2D node in NodeArray_2D.S_Nodes){ node.RHSCost = PositiveInfinity; node.GCost = PositiveInfinity; }
            //_mainPriorityQueueMaxHeap = new Priority_Queue_MaxHeap(rows * columns);
            _priorityModifier = 0;
            _targetNode = NodeArray_2D.S_Nodes[_targetNode.X, _targetNode.Y];
            _startNode = NodeArray_2D.S_Nodes[_startNode.X, _startNode.Y];
            _targetNode.RHSCost = 0;
            //_mainPriorityQueueMaxHeap.Update(_targetNode, _calculatePriority(_targetNode));
        }

        Priority_2D _calculatePriority(Node_Base_2D node)
        {
            return new Priority_2D(Math.Min(node.GCost, node.RHSCost) + _manhattanDistance(node, _startNode) + _priorityModifier, Math.Min(node.GCost, node.RHSCost));
        }
        double _manhattanDistance(Node_Base_2D a, Node_Base_2D b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    
        void _updateVertex(Node_Base_2D node)
        {
            if (!node.Equals(_targetNode))
            {
                node.RHSCost = _minimumSuccessorCost(node);
            }
            // if (_mainPriorityQueueMaxHeap.Contains(node))
            // {
            //     _mainPriorityQueueMaxHeap.Remove(node);
            // }
            // if (node.GCost != node.RHSCost)
            // {
            //     _mainPriorityQueueMaxHeap.Update(node, _calculatePriority(node));
            // }
        }
        Node_Base_2D _minimumSuccessorNode(Node_Base_2D node)
        {
            double minimumCostToMove = PositiveInfinity;
            Node_Base_2D newNode = null;
            foreach (Node_Base_2D successor in node.GetSuccessors(NodeArray_2D.S_Nodes))
            {
                double costToMove = node.GetMovementCostTo(successor, _puzzleSet) + successor.GCost;

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
            double minimumCost = PositiveInfinity;
            foreach (Node_Base_2D successor in node.GetSuccessors(NodeArray_2D.S_Nodes))
            {
                double costToMove = node.GetMovementCostTo(successor, _puzzleSet) + successor.GCost;
                if (costToMove < minimumCost && !successor.IsObstacle) minimumCost = costToMove;
            }
            return minimumCost;
        }
        void _computeShortestPath()
        {
            // while (_mainPriorityQueueMaxHeap.Peek().CompareTo(_calculatePriority(_startNode)) < 0 || _startNode.RHSCost != _startNode.GCost)
            // {
            //     Priority_2D highestPriority = _mainPriorityQueueMaxHeap.Peek();
            //     Node_Base_2D node = _mainPriorityQueueMaxHeap.Dequeue();
            //     if (node == null) break;
            //
            //     if (highestPriority.CompareTo(_calculatePriority(node)) < 0)
            //     {
            //         _mainPriorityQueueMaxHeap.Update(node, _calculatePriority(node));
            //     }
            //     else if (node.GCost > node.RHSCost)
            //     {
            //         node.GCost = node.RHSCost;
            //         foreach (Node_Base_2D neighbour in node.GetPredecessors(NodeArray_2D.S_Nodes))
            //         {
            //             _updateVertex(neighbour);
            //         }
            //     }
            //     else
            //     {
            //         node.GCost = PositiveInfinity;
            //         _updateVertex(node);
            //         foreach (Node_Base_2D neighbour in node.GetPredecessors(NodeArray_2D.S_Nodes))
            //         {
            //             _updateVertex(neighbour);
            //         }
            //     }
            // }
        }

        public void UpdateWallState(Node_Base_2D node, Direction direction, bool wallExists)
        {
            double newCost = wallExists ? PositiveInfinity : 1;

            node.UpdateMovementCost(direction, newCost);

            Node_Base_2D neighborNode = GetNeighbor(node, direction);

            Direction oppositeDirection = GetOppositeDirection(direction);
            neighborNode.UpdateMovementCost(oppositeDirection, newCost);
        }

        Node_Base_2D GetNeighbor(Node_Base_2D node, Direction direction)
        {
            switch (direction)
            {
                case Direction.Top: return node.Y + 1 < NodeArray_2D.S_Nodes.GetLength(1) ? NodeArray_2D.S_Nodes[node.X, node.Y + 1] : null;
                case Direction.Bottom: return node.Y - 1 >= 0 ? NodeArray_2D.S_Nodes[node.X, node.Y - 1] : null;
                case Direction.Left: return node.X - 1 >= 0 ? NodeArray_2D.S_Nodes[node.X - 1, node.Y] : null;
                case Direction.Right: return node.X + 1 < NodeArray_2D.S_Nodes.GetLength(0) ? NodeArray_2D.S_Nodes[node.X + 1, node.Y] : null;
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
            if (NodeArray_2D.S_Nodes[x, y] != null) return NodeArray_2D.S_Nodes[x, y];
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
        public static Node_Base_2D[,] S_Nodes;

        public static Node_Base_2D[,] InitializeArray(int rows, int columns)
        {
            S_Nodes = new Node_Base_2D[rows, columns];

            for (var row = 0; row < S_Nodes.GetLength(0); row++)
            {
                for (var column = 0; column < S_Nodes.GetLength(1); column++)
                {
                    S_Nodes[row, column] = new Node_Base_2D
                    {
                        X = row,
                        Y = column,
                        GCost = PositiveInfinity,
                        RHSCost = PositiveInfinity
                    };
                }
            }

            return S_Nodes;
        }
    }

    public enum Direction { None, Top, Bottom, Left, Right }

    public class Node_Base_2D
    {
        public int X;
        public int Y;
        public double GCost, RHSCost;
        Node_Base_2D _predecessor;
        public Node_Base_2D Predecessor { get; set; }

        public double MovementCost;
        public Dictionary<Direction, double> MovementCosts { get; private set; }

        public bool IsObstacle;

        public Node_Base_2D()
        {
            MovementCosts = new Dictionary<Direction, double>
            {
                { Direction.Top, PositiveInfinity },
                { Direction.Bottom, PositiveInfinity },
                { Direction.Left, PositiveInfinity },
                { Direction.Right, PositiveInfinity }
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
}