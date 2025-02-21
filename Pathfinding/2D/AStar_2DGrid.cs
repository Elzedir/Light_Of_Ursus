using System.Collections.Generic;
using Priorities.Priority_Queues;
using UnityEngine;

namespace Pathfinding._2D
{
    public class AStar_2DGrid
    {
        readonly long[,] _grid;
        readonly long _gridWidth, _gridHeight;
        readonly Dictionary<ulong, Node_2D> _nodes = new();
        
        public AStar_2DGrid(long[,] grid)
        {
            _grid = grid;
            _gridWidth = grid.GetLength(0);
            _gridHeight = grid.GetLength(1);
        }

        public List<Vector2Int> RunAStar(Vector2Int start, Vector2Int end)
        {
            var openList = new Priority_Queue_MinHeap<Node_2D>();
            var closedList = new HashSet<ulong>();
            
            var startNode = _getOrCreateNode(start);
            var endNode = _getOrCreateNode(end);
            
            startNode.GCost = 0;
            startNode.HeuristicCost = _getDistance(startNode, endNode);
            
            openList.Update(new Priority_Element<Node_2D>(startNode.NodeID, startNode.TotalCost, startNode));
            
            while (openList.Count() > 0)
            {
                var currentNode = openList.Dequeue().PriorityObject;

                if (currentNode.NodeID == endNode.NodeID) return _getShortestPath(startNode, currentNode);
                
                closedList.Add(currentNode.NodeID);

                foreach (var neighbor in _getNeighbors(currentNode))
                {
                    if (closedList.Contains(neighbor.NodeID) || _isUnwalkable(neighbor.Position)) continue;

                    var newCost = currentNode.GCost + _getDistance(currentNode, neighbor);

                    if (newCost >= neighbor.GCost) continue;
                    
                    neighbor.GCost = newCost;
                    neighbor.HeuristicCost = _getDistance(neighbor, endNode);
                    neighbor.Parent = currentNode;
                    openList.Update(new Priority_Element<Node_2D>(neighbor.NodeID, neighbor.TotalCost, neighbor));
                }
            }

            return null;
        }
        
        Node_2D _getOrCreateNode(Vector2Int position)
        {
            var nodeId = Node_2D.GetNodeIDFromPosition(position);

            if (_nodes.TryGetValue(nodeId, out var node)) return node;
            
            node = new Node_2D(position);
            _nodes[nodeId] = node;
            return node;
        }

        List<Node_2D> _getNeighbors(Node_2D node2D)
        {
            List<Node_2D> neighbors = new();
            Vector2Int[] directions = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };

            foreach (var direction in directions)
            {
                var neighborPosition = node2D.Position + direction;
                
                if (_isWithinGrid(neighborPosition))
                    neighbors.Add(_getOrCreateNode(neighborPosition));
            }

            return neighbors;
        }
        
        bool _isWithinGrid(Vector2Int position) =>
            position.x >= 0 && position.x < (int)_gridWidth && position.y >= 0 && position.y < (int)_gridHeight;

        bool _isUnwalkable(Vector2Int position) => _grid[position.x, position.y] != 0;

        static float _getDistance(Node_2D a, Node_2D b)
        {
            var x_Distance = Mathf.Abs(a.Position.x - b.Position.x);
            var y_Distance = Mathf.Abs(a.Position.y - b.Position.y);
            
            return _getManhattanDistance(x_Distance, y_Distance);
        }

        static float _getManhattanDistance(int a, int b) => a + b;

        static List<Vector2Int> _getShortestPath(Node_2D startNode2D, Node_2D endNode2D)
        {
            var path = new List<Vector2Int>();
            var currentNode = endNode2D;

            while (currentNode.NodeID != startNode2D.NodeID)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}