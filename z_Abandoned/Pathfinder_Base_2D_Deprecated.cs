using System;
using System.Collections.Generic;
using Priorities.Priority_Queues;
using UnityEngine;
using static System.Double;

namespace z_Abandoned
{
    public abstract class Pathfinder_Base_2D_Deprecated
    {
        //Priority_Queue_MaxHeap _mainPriorityQueueMaxHeap;
        double _priorityModifier;
        Node_Base_2D_Deprecated _targetNode;
        Node_Base_2D_Deprecated _startNode;
        PuzzleSet _puzzleSet;
    
        public void RunPathfinderOpenWorld(Coordinates_Deprecated start, Coordinates_Deprecated target, PathfinderMover_2D_Deprecated mover)
        {
            RunPathfinder(Manager_Grid.Rows, Manager_Grid.Columns, new Coordinates_Deprecated(start.X + Manager_Grid.XOffset, start.Y + Manager_Grid.YOffset), new Coordinates_Deprecated(target.X + Manager_Grid.XOffset, target.Y + Manager_Grid.YOffset), mover, PuzzleSet.None);
        }
        public void RunPathfinder(int rows, int columns, Coordinates_Deprecated start, Coordinates_Deprecated target, PathfinderMover_2D_Deprecated mover, PuzzleSet puzzleSet)
        {
            if (start.Equals(target)) return;

            _puzzleSet = puzzleSet;
            _startNode = new Node_Base_2D_Deprecated { X = start.X, Y = start.Y };
            _targetNode = new Node_Base_2D_Deprecated { X = target.X, Y = target.Y };

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
                    var node = NodeArray_2D_Deprecated.S_Nodes[coordinate.X, coordinate.Y];
                    if (node.IsObstacle) continue;
                    change = true;
                    node.IsObstacle = true;
                    foreach (var p in node.GetPredecessors(NodeArray_2D_Deprecated.S_Nodes))
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
            foreach (Node_Base_2D_Deprecated node in NodeArray_2D_Deprecated.S_Nodes){ node.RHSCost = PositiveInfinity; node.GCost = PositiveInfinity; }
            //_mainPriorityQueueMaxHeap = new Priority_Queue_MaxHeap(rows * columns);
            _priorityModifier = 0;
            _targetNode = NodeArray_2D_Deprecated.S_Nodes[_targetNode.X, _targetNode.Y];
            _startNode = NodeArray_2D_Deprecated.S_Nodes[_startNode.X, _startNode.Y];
            _targetNode.RHSCost = 0;
            //_mainPriorityQueueMaxHeap.Update(_targetNode, _calculatePriority(_targetNode));
        }

        Priority_2D_Deprecated _calculatePriority(Node_Base_2D_Deprecated node)
        {
            return new Priority_2D_Deprecated(Math.Min(node.GCost, node.RHSCost) + _manhattanDistance(node, _startNode) + _priorityModifier, Math.Min(node.GCost, node.RHSCost));
        }
        double _manhattanDistance(Node_Base_2D_Deprecated a, Node_Base_2D_Deprecated b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    
        void _updateVertex(Node_Base_2D_Deprecated node)
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
        Node_Base_2D_Deprecated _minimumSuccessorNode(Node_Base_2D_Deprecated node)
        {
            double minimumCostToMove = PositiveInfinity;
            Node_Base_2D_Deprecated newNode = null;
            foreach (Node_Base_2D_Deprecated successor in node.GetSuccessors(NodeArray_2D_Deprecated.S_Nodes))
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
        double _minimumSuccessorCost(Node_Base_2D_Deprecated node)
        {
            double minimumCost = PositiveInfinity;
            foreach (Node_Base_2D_Deprecated successor in node.GetSuccessors(NodeArray_2D_Deprecated.S_Nodes))
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

        public void UpdateWallState(Node_Base_2D_Deprecated node, Direction_Deprecated directionDeprecated, bool wallExists)
        {
            double newCost = wallExists ? PositiveInfinity : 1;

            node.UpdateMovementCost(directionDeprecated, newCost);

            Node_Base_2D_Deprecated neighborNode = GetNeighbor(node, directionDeprecated);

            Direction_Deprecated oppositeDirectionDeprecated = GetOppositeDirection(directionDeprecated);
            neighborNode.UpdateMovementCost(oppositeDirectionDeprecated, newCost);
        }

        Node_Base_2D_Deprecated GetNeighbor(Node_Base_2D_Deprecated node, Direction_Deprecated directionDeprecated)
        {
            switch (directionDeprecated)
            {
                case Direction_Deprecated.Top: return node.Y + 1 < NodeArray_2D_Deprecated.S_Nodes.GetLength(1) ? NodeArray_2D_Deprecated.S_Nodes[node.X, node.Y + 1] : null;
                case Direction_Deprecated.Bottom: return node.Y - 1 >= 0 ? NodeArray_2D_Deprecated.S_Nodes[node.X, node.Y - 1] : null;
                case Direction_Deprecated.Left: return node.X - 1 >= 0 ? NodeArray_2D_Deprecated.S_Nodes[node.X - 1, node.Y] : null;
                case Direction_Deprecated.Right: return node.X + 1 < NodeArray_2D_Deprecated.S_Nodes.GetLength(0) ? NodeArray_2D_Deprecated.S_Nodes[node.X + 1, node.Y] : null;
                default: return null;
            }
        }

        Direction_Deprecated GetOppositeDirection(Direction_Deprecated directionDeprecated)
        {
            switch (directionDeprecated)
            {
                case Direction_Deprecated.Top: return Direction_Deprecated.Bottom;
                case Direction_Deprecated.Bottom: return Direction_Deprecated.Top;
                case Direction_Deprecated.Left: return Direction_Deprecated.Right;
                case Direction_Deprecated.Right: return Direction_Deprecated.Left;
                default: throw new InvalidOperationException("Invalid wall direction.");
            }
        }

        public static Node_Base_2D_Deprecated GetNodeAtPosition(int x, int y)
        {
            if (NodeArray_2D_Deprecated.S_Nodes[x, y] != null) return NodeArray_2D_Deprecated.S_Nodes[x, y];
            return null;
        }

        public List<Coordinates_Deprecated> RetrievePath(Node_Base_2D_Deprecated startNode, Node_Base_2D_Deprecated targetNode)
        {
            List<Coordinates_Deprecated> path = new List<Coordinates_Deprecated>();
            Node_Base_2D_Deprecated currentNode = targetNode;

            while (currentNode != null && !currentNode.Equals(startNode))
            {
                if (currentNode == null || currentNode.Equals(startNode)) break;

                path.Add(new Coordinates_Deprecated(currentNode.X, currentNode.Y));
                currentNode = currentNode.Predecessor;
            }

            if (currentNode != null)
            {
                path.Add(new Coordinates_Deprecated(startNode.X, startNode.Y));
            }

            path.Reverse();

            return path;
        }

        public static void FindAllPredecessors(Node_Base_2D_Deprecated node, int infiniteEnd, int infinityStart = 0)
        {
            infinityStart++;
            if (infinityStart > infiniteEnd) return;
            if (node.Predecessor == null) { Debug.Log($"{node.X}_{node.Y} predecessor is null"); return; }

            Debug.Log($"{node.X}_{node.Y} -> {node.Predecessor.X}_{node.Predecessor.Y}");
            FindAllPredecessors(node.Predecessor, infinityStart, infiniteEnd);
        }
    }

    public class NodeArray_2D_Deprecated
    {
        public static Node_Base_2D_Deprecated[,] S_Nodes;

        public static Node_Base_2D_Deprecated[,] InitializeArray(int rows, int columns)
        {
            S_Nodes = new Node_Base_2D_Deprecated[rows, columns];

            for (var row = 0; row < S_Nodes.GetLength(0); row++)
            {
                for (var column = 0; column < S_Nodes.GetLength(1); column++)
                {
                    S_Nodes[row, column] = new Node_Base_2D_Deprecated
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

    public enum Direction_Deprecated { None, Top, Bottom, Left, Right }

    public class Node_Base_2D_Deprecated
    {
        public int X;
        public int Y;
        public double GCost, RHSCost;
        Node_Base_2D_Deprecated _predecessor;
        public Node_Base_2D_Deprecated Predecessor { get; set; }

        public double MovementCost;
        public Dictionary<Direction_Deprecated, double> MovementCosts { get; private set; }

        public bool IsObstacle;

        public Node_Base_2D_Deprecated()
        {
            MovementCosts = new Dictionary<Direction_Deprecated, double>
            {
                { Direction_Deprecated.Top, PositiveInfinity },
                { Direction_Deprecated.Bottom, PositiveInfinity },
                { Direction_Deprecated.Left, PositiveInfinity },
                { Direction_Deprecated.Right, PositiveInfinity }
            };
        }

        public void UpdateMovementCost(Direction_Deprecated directionDeprecated, double cost)
        {
            if (directionDeprecated == Direction_Deprecated.None) MovementCost = cost;
            else MovementCosts[directionDeprecated] = cost;
        }

        public double GetMovementCostTo(Node_Base_2D_Deprecated successor, PuzzleSet puzzleSet)
        {
            if (puzzleSet == PuzzleSet.MouseMaze)
            {
                Direction_Deprecated directionDeprecatedToSuccessor = Direction_Deprecated.None;

                if (X == successor.X)
                {
                    if (Y == successor.Y - 1) directionDeprecatedToSuccessor = Direction_Deprecated.Top;
                    else if (Y == successor.Y + 1) directionDeprecatedToSuccessor = Direction_Deprecated.Bottom;
                    else throw new InvalidOperationException("Nodes are not X adjacent.");
                }
                else if (Y == successor.Y)
                {
                    if (X == successor.X - 1) directionDeprecatedToSuccessor = Direction_Deprecated.Right;
                    else if (X == successor.X + 1) directionDeprecatedToSuccessor = Direction_Deprecated.Left;
                    else throw new InvalidOperationException("Nodes are not Y adjacent.");
                }
                else throw new InvalidOperationException("Nodes are not adjacent.");

                if (!MovementCosts.TryGetValue(directionDeprecatedToSuccessor, out double cost) || directionDeprecatedToSuccessor == Direction_Deprecated.None) throw new InvalidOperationException("Invalid direction.");

                return cost;
            }
            else if (puzzleSet == PuzzleSet.IceWall || puzzleSet == PuzzleSet.None)
            {
                return successor.MovementCost;
            }

            throw new InvalidOperationException($"{puzzleSet} not valid.");
        }

        public bool Equals(Node_Base_2D_Deprecated that)
        {
            if (X == that.X && Y == that.Y) return true;
            return false;
        }

        public LinkedList<Node_Base_2D_Deprecated> GetSuccessors(Node_Base_2D_Deprecated[,] nodes)
        {
            LinkedList<Node_Base_2D_Deprecated> successors = new LinkedList<Node_Base_2D_Deprecated>();
            if (X + 1 < nodes.GetLength(0)) successors.AddFirst(nodes[X + 1, Y]);
            if (Y + 1 < nodes.GetLength(1)) successors.AddFirst(nodes[X, Y + 1]);
            if (X - 1 >= 0) successors.AddFirst(nodes[X - 1, Y]);
            if (Y - 1 >= 0) successors.AddFirst(nodes[X, Y - 1]);
            return successors;
        }

        public LinkedList<Node_Base_2D_Deprecated> GetPredecessors(Node_Base_2D_Deprecated[,] nodes)
        {
            LinkedList<Node_Base_2D_Deprecated> neighbours = new LinkedList<Node_Base_2D_Deprecated>();
            Node_Base_2D_Deprecated tempNode;
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

    public class Priority_2D_Deprecated
    {
        public double PrimaryPriority;
        public double SecondaryPriority;

        public Priority_2D_Deprecated(double primaryPriority, double secondaryPriority)
        {
            PrimaryPriority = primaryPriority;
            SecondaryPriority = secondaryPriority;
        }
        public int CompareTo(Priority_2D_Deprecated that)
        {
            if (PrimaryPriority < that.PrimaryPriority) return -1;
            else if (PrimaryPriority > that.PrimaryPriority) return 1;
            if (SecondaryPriority > that.SecondaryPriority) return 1;
            else if (SecondaryPriority < that.SecondaryPriority) return -1;
            return 0;
        }
    }

    public class NodeQueue_2D_Deprecated
    {
        public Node_Base_2D_Deprecated Node;
        public Priority_2D_Deprecated Priority;

        public NodeQueue_2D_Deprecated(Node_Base_2D_Deprecated node, Priority_2D_Deprecated priority)
        {
            Node = node;
            Priority = priority;
        }
    }

    public class Coordinates_Deprecated
    {
        public int X;
        public int Y;

        public Coordinates_Deprecated(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public interface PathfinderMover_2D_Deprecated
    {
        void MoveTo(Node_Base_2D_Deprecated target);
        LinkedList<Coordinates_Deprecated> GetObstaclesInVision();
    }
}