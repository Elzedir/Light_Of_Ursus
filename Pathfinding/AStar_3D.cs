using System.Collections.Generic;
using Priorities.Priority_Queues;
using UnityEngine;

namespace Pathfinding
{
    public class AStar_3D
    {
        readonly long[,,] _grid;
        readonly long _gridWidth, _gridHeight, _gridDepth;
        readonly Dictionary<ulong, Node_3D> _nodes = new();

        public AStar_3D(long[,,] grid)
        {
            _grid = grid;
            _gridWidth = grid.GetLength(0);
            _gridHeight = grid.GetLength(1);
            _gridDepth = grid.GetLength(2);
        }

        public List<Vector3> RunAStar(Vector3 start, Vector3 end)
        {
            var openList = new Priority_Queue_MinHeap<Node_3D>();
            var closedList = new HashSet<ulong>();

            var startNode = _getOrCreateNode(start);
            var endNode = _getOrCreateNode(end);

            startNode.GCost = 0;
            startNode.HeuristicCost = _getDistance(startNode, endNode);

            openList.Update(new Priority_Element<Node_3D>(startNode.NodeID, startNode.TotalCost, startNode));

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
                    openList.Update(new Priority_Element<Node_3D>(neighbor.NodeID, neighbor.TotalCost, neighbor));
                }
            }

            return null; // No path found
        }

        Node_3D _getOrCreateNode(Vector3 position)
        {
            var nodeId = Node_3D.GetNodeIDFromPosition(position);

            if (_nodes.TryGetValue(nodeId, out var node)) return node;

            node = new Node_3D(position);
            _nodes[nodeId] = node;
            return node;
        }

        List<Node_3D> _getNeighbors(Node_3D node3D)
        {
            List<Node_3D> neighbors = new();
            Vector3[] directions = 
            {
                Vector3.up, Vector3.down, Vector3.left, Vector3.right, 
                Vector3.forward, Vector3.back, new Vector3(1, 1, 0), new Vector3(-1, -1, 0),
                new Vector3(1, -1, 1), new Vector3(-1, 1, -1) // Example for diagonal neighbors in 3D
            };

            foreach (var direction in directions)
            {
                var neighborPosition = node3D.Position + direction;
                if (_isWithinGrid(neighborPosition))
                    neighbors.Add(_getOrCreateNode(neighborPosition));
            }

            return neighbors;
        }

        bool _isWithinGrid(Vector3 position) =>
            position.x >= 0 && position.x < _gridWidth && 
            position.y >= 0 && position.y < _gridHeight && 
            position.z >= 0 && position.z < _gridDepth;

        bool _isUnwalkable(Vector3 position) =>
            _grid[(int)position.x, (int)position.y, (int)position.z] != 0; // Assumes 0 is walkable, non-zero is blocked.

        static float _getDistance(Node_3D a, Node_3D b)
        {
            return Vector3.Distance(a.Position, b.Position);
        }

        static List<Vector3> _getShortestPath(Node_3D startNode3D, Node_3D endNode3D)
        {
            var path = new List<Vector3>();
            var currentNode = endNode3D;

            while (currentNode.NodeID != startNode3D.NodeID)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}
