using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Graph_World
    {
        Dictionary<ulong, Node_3D> _nodes;
        Dictionary<ulong, Node_3D> Nodes => _nodes ??= _initialiseNodes();

        static Dictionary<ulong, Node_3D> _initialiseNodes()
        {
            //* Eventually replace with actual in-game data.
            var nodes = new Dictionary<ulong, Node_3D>();
            
            var cityA = new Node_3D(position: new Vector3(0, 0, 0));
            var cityB = new Node_3D(position: new Vector3(100, 0, 0));
            var cityC = new Node_3D(position: new Vector3(150, 0, 100));
            
            cityA.Neighbors.Add(cityB);
            cityB.Neighbors.Add(cityA);
            cityB.Neighbors.Add(cityC);
            cityC.Neighbors.Add(cityB);
            
            nodes[cityA.ID] = cityA;
            nodes[cityB.ID] = cityB;
            nodes[cityC.ID] = cityC;   
            
            return nodes;
        }
        
        Node_3D _getOrCreateNearestNode(Vector3 position)
        {
            var nodeId = Node_3D.GetNodeIDFromPosition(position);

            if (Nodes.TryGetValue(nodeId, out var node)) return node;

            //* Definitely not a good system, definitely replace soon.
            Node_3D closestNode = null;
            var closestDistance = float.PositiveInfinity;
            
            foreach (var closeNode in Nodes.Values)
            {
                var distance = Vector3.Distance(position, closeNode.Position);
                if (distance >= closestDistance) continue;
                
                closestNode = closeNode;
                closestDistance = distance;
            }

            if (closestNode != null) return closestNode;
            
            Debug.LogWarning($"Node not found at {position}. Creating new node.");

            node = new Node_3D(position);
            Nodes[nodeId] = node;
            return node;
        }

        public List<Vector3> FindShortestPath(Vector3 start, Vector3 end)
        {
            var startNode = _getOrCreateNearestNode(start);
            var endNode = _getOrCreateNearestNode(end);
            
            return startNode != endNode 
                ? AStar_Node.RunAStar(startNode, endNode) 
                : new List<Vector3> { end };
        }
    }
}