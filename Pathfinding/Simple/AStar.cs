using System.Collections.Generic;
using Priorities.Priority_Queues;
using UnityEngine;

namespace Pathfinding
{
    public class AStar
    {
        readonly long[,] _grid;
        readonly long _gridWidth, _gridHeight;

        public AStar(long[,] grid)
        {
            _grid = grid;
            _gridWidth = grid.GetLength(0);
            _gridHeight = grid.GetLength(1);
        }

        public List<Vector2Int> RunAStar(Vector2Int start, Vector2Int end)
        {
            var startNode = new Node_Base(start);
            var endNode = new Node_Base(end);

            var openList = new Priority_Queue_MinHeap<Node_Base>();
            var closedList = new HashSet<long>();
            
            openList.Update(startNode.NodeID, 0);

            while (openList.Count() > 0)
            {
                var currentNode = openList.Dequeue().PriorityObject;

                if (currentNode.NodeID == endNode.NodeID) return GetShortestPath(startNode, currentNode);
                
                closedList.Add(currentNode.NodeID);

                foreach (var neighbor in _getNeighbors(currentNode))
                {
                    if (closedList.Contains(neighbor.NodeID) || _isUnwalkable(neighbor.Position)) continue;

                    var newMovementCostToNeighbor = currentNode.GCost + _getDistance(currentNode, neighbor);
                    var isBetterPath = newMovementCostToNeighbor < neighbor.GCost || !openList.Contains(neighbor.NodeID);

                    if (!isBetterPath) continue;
                    
                    neighbor.GCost = newMovementCostToNeighbor;
                    neighbor.HeuristicCost = _getDistance(neighbor, endNode);
                    neighbor.Parent = currentNode;

                    if (!openList.Contains(neighbor.NodeID))
                        openList.Update(neighbor.NodeID, neighbor.TotalCost);
                }
            }

            return null;
        }

        List<Node_Base> _getNeighbors(Node_Base nodeBase)
        {
            var neighbors = new List<Node_Base>();
            Vector2Int[] directions = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };

            foreach (var direction in directions)
            {
                var neighborPosition = nodeBase.Position + direction;
                
                if (_isWithinGrid(neighborPosition))
                    neighbors.Add(new Node_Base(neighborPosition));
            }

            return neighbors;
        }
        
        bool _isWithinGrid(Vector2Int position)
        {
            return position.x >= 0 && position.x < (int)_gridWidth && position.y >= 0 && position.y < (int)_gridHeight;
        }

        bool _isUnwalkable(Vector2Int position) => _grid[position.x, position.y] != 0;

        static float _getDistance(Node_Base a, Node_Base b)
        {
            var x_Distance = Mathf.Abs(a.Position.x - b.Position.x);
            var y_Distance = Mathf.Abs(a.Position.y - b.Position.y);
            
            return _getManhattanDistance(x_Distance, y_Distance);
        }

        static float _getManhattanDistance(int a, int b) => a + b;

        static List<Vector2Int> GetShortestPath(Node_Base startNodeBase, Node_Base endNodeBase)
        {
            var path = new List<Vector2Int>();
            var currentNode = endNodeBase;

            while (currentNode.NodeID != startNodeBase.NodeID)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}