using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    public class Pathfinder_Simple_AStar
    {
        readonly int[,] _grid;
        readonly int _gridWidth, _gridHeight;

        public Pathfinder_Simple_AStar(int[,] grid)
        {
            _grid = grid;
            _gridWidth = grid.GetLength(0);
            _gridHeight = grid.GetLength(1);
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            var startNode = new Node(start);
            var endNode = new Node(end);

            var openList = new List<Node> { startNode };
            var closedList = new HashSet<Node>();

            while (openList.Count > 0)
            {
                Node currentNode = openList.OrderBy(node => node.FCost).ThenBy(node => node.HCost).First();

                if (currentNode.Position == endNode.Position)
                    return RetracePath(startNode, currentNode);

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (Node neighbor in GetNeighbors(currentNode))
                {
                    if (closedList.Any(n => n.Position == neighbor.Position) || IsUnwalkable(neighbor.Position))
                        continue;

                    float newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                    bool isBetterPath = newMovementCostToNeighbor < neighbor.GCost || !openList.Contains(neighbor);

                    if (isBetterPath)
                    {
                        neighbor.GCost = newMovementCostToNeighbor;
                        neighbor.HCost = GetDistance(neighbor, endNode);
                        neighbor.Parent = currentNode;

                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>();
            Vector2Int[] directions = {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0)
            };

            foreach (var direction in directions)
            {
                Vector2Int neighborPos = node.Position + direction;
                if (IsWithinGrid(neighborPos))
                    neighbors.Add(new Node(neighborPos));
            }

            return neighbors;
        }

        private bool IsUnwalkable(Vector2Int position)
        {
            return _grid[position.x, position.y] != 0;
        }

        private bool IsWithinGrid(Vector2Int position)
        {
            return position.x >= 0 && position.x < _gridWidth && position.y >= 0 && position.y < _gridHeight;
        }

        private float GetDistance(Node a, Node b)
        {
            int dstX = Mathf.Abs(a.Position.x - b.Position.x);
            int dstY = Mathf.Abs(a.Position.y - b.Position.y);
            return dstX + dstY;  // Manhattan distance
        }

        private List<Vector2Int> RetracePath(Node startNode, Node endNode)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }
    }

    public class Node
    {
        public Vector2Int Position;
        public Node Parent;
        public float GCost;  // Cost from start to this node
        public float HCost;  // Heuristic cost from this node to the end
        public float FCost => GCost + HCost;  // Total cost

        public Node(Vector2Int position, Node parent = null)
        {
            Position = position;
            Parent = parent;
            GCost = 0;
            HCost = 0;
        }
    }
}