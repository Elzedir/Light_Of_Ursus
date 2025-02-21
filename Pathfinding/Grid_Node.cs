using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    public class Grid_Node
    {
        Dictionary<ulong, Node_3D> _nodes;
        Dictionary<ulong, Node_3D> Nodes => _nodes ??= _initialiseNodes();
        
        const int _gridWidth = 10, _gridHeight = 5, _gridDepth = 10;
        
        const float _voxelSpacing = 1f;

        public Dictionary<ulong, Node_3D> _initialiseNodes()
        {
            var nodes = new Dictionary<ulong, Node_3D>();
            
            for (var x = 0; x < _gridWidth; x++)
            {
                for (var y = 0; y < _gridHeight; y++)
                {
                    for (var z = 0; z < _gridDepth; z++)
                    {
                        var position = new Vector3(x * _voxelSpacing, y * _voxelSpacing, z * _voxelSpacing);

                        var node = new Node_3D(position);
                        _nodes.Add(node.ID, node);
                    }
                }
            }
            
            foreach (var node in _nodes.Values)
            {
                var neighbors = _getNeighbors(node);
                
                foreach (var neighbor in neighbors)
                {
                    node.Neighbors.Add(neighbor);
                }
            }

            return nodes;
        }
        
        List<Node_3D> _getNeighbors(Node_3D nodeGrid)
        {
            List<Node_3D> neighbors = new();

            foreach (var direction in AllDirections)
            {
                var neighborPosition = nodeGrid.Position + direction;
                if (_isWithinGrid(neighborPosition))
                    neighbors.Add(_getNode(neighborPosition));
            }

            return neighbors;
        }

        static bool _isWithinGrid(Vector3 position) =>
            position.x is >= 0 and < _gridWidth && 
            position.y is >= 0 and < _gridHeight && 
            position.z is >= 0 and < _gridDepth;
        
        Node_3D _getNode(Vector3 position)
        {
            var nodeId = Node_3D.GetNodeIDFromPosition(position);

            if (_nodes.TryGetValue(nodeId, out var node)) return node;

            throw new System.Exception("Node not found");
            
            //* Old system that creates a new node if it doesn't exist
            // node = new Node_3D(position);
            // _nodes[nodeId] = node;
            // return node;
        }

        public List<Vector3> FindShortestPath(Vector3 start, Vector3 end)
        {
            Debug.Log("Using 3D grid pathfinding");
            var startNode = _getNode(start);
            var endNode = _getNode(end);
            
            return AStar_Node.RunAStar(startNode, endNode);
        }
        
        List<Vector3> _allDirections;
        List<Vector3> AllDirections => _allDirections ??= _getAllDirections(); 
        
        static List<Vector3> _getAllDirections()
        {
            var cardinal_Horizontal = new List<Vector3>
            {
                Vector3.left, Vector3.right, Vector3.forward, Vector3.back
            };

            var diagonal_Horizontal = new List<Vector3>
            {
                Vector3.left + Vector3.forward, Vector3.left + Vector3.back, 
                Vector3.right + Vector3.forward, Vector3.right + Vector3.back
            };

            var cardinal_Vertical = new List<Vector3>
            {
                Vector3.up, Vector3.down, 
                Vector3.left + Vector3.up, Vector3.left + Vector3.down,
                Vector3.right + Vector3.up, Vector3.right + Vector3.down, 
                Vector3.forward + Vector3.up, Vector3.forward + Vector3.down, 
                Vector3.back + Vector3.up, Vector3.back + Vector3.down
            };

            var diagonal_Vertical = new List<Vector3>
            {
                Vector3.left + Vector3.forward + Vector3.up, Vector3.left + Vector3.back + Vector3.up,
                Vector3.right + Vector3.forward + Vector3.up, Vector3.right + Vector3.back + Vector3.up,
                Vector3.left + Vector3.forward + Vector3.down, Vector3.left + Vector3.back + Vector3.down,
                Vector3.right + Vector3.forward + Vector3.down, Vector3.right + Vector3.back + Vector3.down
            };

            var horizontal = cardinal_Horizontal.Concat(diagonal_Horizontal).ToList();
            var vertical = cardinal_Vertical.Concat(diagonal_Vertical).ToList();
            return horizontal.Concat(vertical).ToList();
        }
    }
}