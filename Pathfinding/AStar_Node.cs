using System.Collections.Generic;
using Priorities.Priority_Queues;
using UnityEngine;

namespace Pathfinding
{
    public abstract class AStar_Node
    {
        public static List<Vector3> RunAStar(Node_3D startNode, Node_3D endNode)
        {
            var openList = new Priority_Queue_MinHeap<Node_3D>();
            var closedSet = new HashSet<ulong>();
            var cameFrom = new Dictionary<Node_3D, Node_3D>();

            var initialCost = new Dictionary<ulong, float>();
            var totalCost = new Dictionary<ulong, float>();
            
            initialCost[startNode.ID] = 0;
            totalCost[startNode.ID] = _getDistance(startNode, endNode);

            openList.Update(new Priority_Element<Node_3D>(startNode.ID, totalCost[startNode.ID], startNode));

            while (openList.Count() > 0)
            {
                var currentNode = openList.Dequeue().PriorityObject;

                if (currentNode.ID == endNode.ID) return _getShortestPath(cameFrom, currentNode);

                closedSet.Add(currentNode.ID);

                foreach (var neighbor in currentNode.Neighbors)
                {
                    if (closedSet.Contains(neighbor.ID)) continue;

                    var newCost = initialCost[currentNode.ID] + _getDistance(currentNode, neighbor);

                    if (initialCost.ContainsKey(neighbor.ID) && !(newCost < initialCost[neighbor.ID])) continue;
                    
                    cameFrom[neighbor] = currentNode;
                    initialCost[neighbor.ID] = newCost;
                    totalCost[neighbor.ID] = newCost + _getDistance(neighbor, endNode);

                    openList.Update(new Priority_Element<Node_3D>(neighbor.ID, totalCost[neighbor.ID], neighbor));
                }
            }

            return null;
        }

        static float _getDistance(Node_3D a, Node_3D b) => Vector3.Distance(a.Position, b.Position);

        static List<Vector3> _getShortestPath(Dictionary<Node_3D, Node_3D> cameFrom, Node_3D startNode)
        {
            var path = new List<Vector3> { startNode.Position };
            
            while (cameFrom.ContainsKey(startNode))
            {
                startNode = cameFrom[startNode];
                path.Add(startNode.Position);
            }

            path.Reverse();
            return path;
        }
    }
}
