using System.Collections.Generic;
using Priorities.Priority_Queues;
using UnityEngine;

namespace Pathfinding
{
    public abstract class AStar_Triangle
    {
        public static List<Vector3> RunAStar(Node_Triangle startNodeTriangle, Node_Triangle endNodeTriangle, Dictionary<ulong, Node_Triangle> allTriangles)
        {
            var openList = new Priority_Queue_MinHeap<Node_Triangle>();
            var closedSet = new HashSet<ulong>();
            var cameFrom = new Dictionary<Node_Triangle, Node_Triangle>();

            var initialCost = new Dictionary<ulong, float>();
            var totalCost = new Dictionary<ulong, float>();
            
            initialCost[startNodeTriangle.ID] = 0;
            totalCost[startNodeTriangle.ID] = _getDistance(startNodeTriangle, endNodeTriangle);

            openList.Update(new Priority_Element<Node_Triangle>(startNodeTriangle.ID, totalCost[startNodeTriangle.ID], startNodeTriangle));

            while (openList.Count() > 0)
            {
                var currentNode = openList.Dequeue().PriorityObject;

                if (currentNode.ID == endNodeTriangle.ID) return _getShortestPath(cameFrom, currentNode);

                closedSet.Add(currentNode.ID);

                foreach (var neighbor in currentNode.GetAdjacentTriangles(allTriangles))
                {
                    if (closedSet.Contains(neighbor.ID)) continue;

                    var newCost = initialCost[currentNode.ID] + _getDistance(currentNode, neighbor);

                    if (initialCost.ContainsKey(neighbor.ID) && !(newCost < initialCost[neighbor.ID])) continue;
                    
                    cameFrom[neighbor] = currentNode;
                    initialCost[neighbor.ID] = newCost;
                    totalCost[neighbor.ID] = newCost + _getDistance(neighbor, endNodeTriangle);

                    openList.Update(new Priority_Element<Node_Triangle>(neighbor.ID, totalCost[neighbor.ID], neighbor));
                }
            }

            return null;
        }

        static float _getDistance(Node_Triangle a, Node_Triangle b) => Vector3.Distance(a.Centroid, b.Centroid);

        static List<Vector3> _getShortestPath(Dictionary<Node_Triangle, Node_Triangle> cameFrom, Node_Triangle startNode)
        {
            var path = new List<Vector3> { startNode.Centroid };
            
            while (cameFrom.ContainsKey(startNode))
            {
                startNode = cameFrom[startNode];
                path.Add(startNode.Centroid);
            }

            path.Reverse();
            return path;
        }
    }
}
