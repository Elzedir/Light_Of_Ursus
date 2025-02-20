using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    public class NavMesh_Manager
    {
    }

    public class NavMeshNode
    {
        public Vector3 Position;
        public List<NavMeshNode> Neighbors;
        public TerrainType TerrainType;
        public float Cost;

        public NavMeshNode(Vector3 position, TerrainType terrainType)
        {
            Position = position;
            TerrainType = terrainType;

            Neighbors = new List<NavMeshNode>();
            Cost = float.PositiveInfinity;
        }
    }

    public class NavMeshGraph
    {
        public List<NavMeshNode> Nodes = new();
        int size = 10;
        float spacing = 5f;
        int heightLevels = 5;

        public void GenerateNavMesh()
        {
            var size = 10;
            var spacing = 5f;

            Nodes.Clear();

            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < heightLevels; y++)
                {
                    for (var z = 0; z < size; z++)
                    {
                        var position = new Vector3(x * spacing, y * spacing, z * spacing);
                        bool isGround = (y == 0);
                        bool isAir = (y > 0);
                        bool isUnderground = (y < 0);
                        var node = new NavMeshNode(position, isGround, isAir, isUnderground);
                        Nodes.Add(node);
                    }
                }
            }

            foreach (var node in Nodes)
            {
                foreach (var neighbor in Nodes)
                {
                    if (Vector3.Distance(node.Position, neighbor.Position) < spacing * 1.5f)
                    {
                        node.Neighbors.Add(neighbor);
                    }
                }
            }
        }
    }

    public class AStarNavMesh
    {
        public static List<Vector3> FindPath(NavMeshNode start, NavMeshNode goal)
        {
            var openSet = new HashSet<NavMeshNode> { start };
            var cameFrom = new Dictionary<NavMeshNode, NavMeshNode>();
            var gScore = new Dictionary<NavMeshNode, float> { [start] = 0 };
            var fScore = new Dictionary<NavMeshNode, float>
                { [start] = Vector3.Distance(start.Position, goal.Position) };

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : float.MaxValue).First();

                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(current);

                foreach (var neighbor in current.Neighbors)
                {
                    float tentativeGScore = gScore[current] + Vector3.Distance(current.Position, neighbor.Position);
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + Vector3.Distance(neighbor.Position, goal.Position);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null; // No path found
        }

        static List<Vector3> ReconstructPath(Dictionary<NavMeshNode, NavMeshNode> cameFrom, NavMeshNode current)
        {
            var path = new List<Vector3> { current.Position };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current.Position);
            }

            path.Reverse();
            return path;
        }

        public class HybridPathfinding
        {
            NavMeshGraph _navMesh;
            DStarLite _dStarLite;

            public HybridPathfinding()
            {
                _navMesh = new NavMeshGraph();
                _navMesh.GenerateNavMesh();
            }

            public List<Vector3> GetPath(Vector3 start, Vector3 goal, List<MoverType> moverTypes)
            {
                var startNode = _findClosestNavMeshNode(start);
                var goalNode = _findClosestNavMeshNode(goal);

                var navMeshPath = AStarNavMesh.FindPath(startNode, goalNode);

                if (navMeshPath == null || navMeshPath.Count == 0)
                    return null;

                _dStarLite = new DStarLite(moverTypes, start, goal);
                return _dStarLite.ShortestPath;
            }

            NavMeshNode _findClosestNavMeshNode(Vector3 position)
            {
                return _navMesh.Nodes.OrderBy(n => Vector3.Distance(n.Position, position)).FirstOrDefault();
            }
        }
    }

    public class WorldNode
    {
        public string Name;
        public Vector3 Position;
        public List<WorldNode> Neighbors;
        public float Cost; // Travel cost (distance, time, etc.)

        public WorldNode(string name, Vector3 position)
        {
            Name = name;
            Position = position;
            Neighbors = new List<WorldNode>();
            Cost = float.PositiveInfinity;
        }
    }

    public class WorldGraph
    {
        public List<WorldNode> Nodes = new List<WorldNode>();
        public List<Vector3> Path { get; private set; }

        public WorldGraph()
        {
            Path = new List<Vector3>();
        }

        public void GenerateWorldGraph()
        {
            // Generate nodes for cities, regions, etc.
            Nodes.Clear();

            // Add nodes (cities or regions)
            // Example: (Can be based on your world map structure)
            var cityA = new WorldNode("City A", new Vector3(0, 0, 0));
            var cityB = new WorldNode("City B", new Vector3(100, 0, 0));
            var cityC = new WorldNode("City C", new Vector3(150, 0, 100));

            Nodes.AddRange(new[] { cityA, cityB, cityC });

            // Define the connections (edges) between them
            cityA.Neighbors.Add(cityB);
            cityB.Neighbors.Add(cityA);
            cityB.Neighbors.Add(cityC);
            cityC.Neighbors.Add(cityB);
        }

        public List<WorldNode> FindPath(WorldNode start, WorldNode goal)
        {
            // Simple A* or Dijkstra pathfinding for world graph
            var openSet = new HashSet<WorldNode> { start };
            var cameFrom = new Dictionary<WorldNode, WorldNode>();
            var gScore = new Dictionary<WorldNode, float> { [start] = 0 };
            var fScore = new Dictionary<WorldNode, float> { [start] = Vector3.Distance(start.Position, goal.Position) };

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : float.MaxValue).First();

                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(current);

                foreach (var neighbor in current.Neighbors)
                {
                    float tentativeGScore = gScore[current] + Vector3.Distance(current.Position, neighbor.Position);
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + Vector3.Distance(neighbor.Position, goal.Position);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null; // No path found
        }

        private List<Vector3> ReconstructPath(Dictionary<WorldNode, WorldNode> cameFrom, WorldNode current)
        {
            var path = new List<Vector3> { current.Position };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current.Position);
            }

            path.Reverse();
            return path;
        }
    }

    public class VoxelNode
    {
        public Vector3 Position;
        public TerrainType TerrainType;
        public float Cost; // Movement cost (e.g., flying, digging, or walking)
        public List<VoxelNode> Neighbors;

        public VoxelNode(Vector3 position, TerrainType terrainType)
        {
            Position = position;
            TerrainType = terrainType;
            Neighbors = new List<VoxelNode>();
            Cost = float.PositiveInfinity;
        }
    }

    public class VoxelGrid
    {
        public List<VoxelNode> Nodes;
        public int GridSizeX = 10;
        public int GridSizeY = 5; // Height of grid, adjust for air and underground levels
        public int GridSizeZ = 10;
        public float VoxelSpacing = 5f;

        public VoxelGrid()
        {
            Nodes = new List<VoxelNode>();
        }

        public void GenerateVoxelGrid()
        {
            Nodes.Clear();

            // Generate voxels across the grid
            for (var x = 0; x < GridSizeX; x++)
            {
                for (var y = 0; y < GridSizeY; y++)
                {
                    for (var z = 0; z < GridSizeZ; z++)
                    {
                        var position = new Vector3(x * VoxelSpacing, y * VoxelSpacing, z * VoxelSpacing);

                        TerrainType terrainType = TerrainType.Ground;
                        if (y == 0)
                            terrainType = TerrainType.Ground; // Ground level
                        else if (y > 0)
                            terrainType = TerrainType.Air; // Air above ground
                        else
                            terrainType = TerrainType.Underground; // Underground

                        var node = new VoxelNode(position, terrainType);
                        Nodes.Add(node);
                    }
                }
            }

            // Connect neighbors within the grid (adjacent nodes)
            foreach (var node in Nodes)
            {
                var neighbors = GetNeighbors(node);
                foreach (var neighbor in neighbors)
                {
                    node.Neighbors.Add(neighbor);
                }
            }
        }

        public List<VoxelNode> GetNeighbors(VoxelNode node)
        {
            var neighbors = new List<VoxelNode>();
            Vector3[] directions = new Vector3[]
            {
                Vector3.left, Vector3.right, Vector3.forward, Vector3.back, Vector3.up, Vector3.down
            };

            foreach (var direction in directions)
            {
                var neighborPosition = node.Position + direction * VoxelSpacing;
                var neighbor = Nodes.FirstOrDefault(n => n.Position == neighborPosition);
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }
    }



    public enum TerrainType
    {
        Air,
        Underground,
        Ground,
        Water
    }

    public class HybridPathfinding
    {
        NavMeshGraph _navMesh;
        WorldGraph _worldGraph;
        VoxelGrid _voxelGrid;
        DStarLite _dStarLite;

        public HybridPathfinding()
        {
            _navMesh = new NavMeshGraph();
            _navMesh.GenerateNavMesh();
            _worldGraph = new WorldGraph();
            _worldGraph.GenerateWorldGraph();
            _voxelGrid = new VoxelGrid();
            _voxelGrid.GenerateVoxelGrid();
        }

        public List<Vector3> GetPath(Vector3 start, Vector3 goal, List<MoverType> moverTypes)
        {
            // Step 1: Use WorldGraph for high-level pathfinding
            var startNodeWorld = _findClosestWorldNode(start);
            var goalNodeWorld = _findClosestWorldNode(goal);

            var worldPath = _worldGraph.FindPath(startNodeWorld, goalNodeWorld);

            if (worldPath == null || worldPath.Count == 0)
                return null;

            // Step 2: Once you reach a specific city or region, use the VoxelGrid system for detailed 3D movement (air or underground)
            var localStart = worldPath.Last();
            var localGoal = goal;
            
            //* If ground, navMesh

            if (ground)
            {
                var startNode = _findClosestNavMeshNode(localStart);
                var goalNode = _findClosestNavMeshNode(localGoal);

                var navMeshPath = AStarNavMesh.FindPath(startNode, goalNode);
            
                if (navMeshPath == null || navMeshPath.Count == 0)
                    return null;

                // Optionally, use D* Lite if dynamic obstacles or real-time updates are required
                _dStarLite = new DStarLite(moverTypes, start, goal);
                return _dStarLite.ShortestPath;   
            }
            else if (air || underground || water)
            {
                var startNodeVoxel = _findClosestNavMeshNode(localStart);
                var goalNodeVoxel = _findClosestNavMeshNode(localGoal);
            
                var voxelPath = AStarVoxel.FindPath(startNodeVoxel, goalNodeVoxel);

                if (voxelPath == null || voxelPath.Count == 0)
                    return null;

                return voxelPath;
            }

            return null;
        }

        WorldNode _findClosestWorldNode(Vector3 position)
        {
            return _worldGraph.Nodes.OrderBy(n => Vector3.Distance(n.Position, position)).FirstOrDefault();
        }
        
        NavMeshNode _findClosestNavMeshNode(Vector3 position)
        {
            return _navMesh.Nodes.OrderBy(n => Vector3.Distance(n.Position, position)).FirstOrDefault();
        }

        VoxelNode _findClosestVoxelNode(Vector3 position)
        {
            return _voxelGrid.Nodes.OrderBy(n => Vector3.Distance(n.Position, position)).FirstOrDefault();
        }
    }
}