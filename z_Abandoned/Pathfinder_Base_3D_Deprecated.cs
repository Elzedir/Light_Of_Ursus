using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;

namespace z_Abandoned
{
    public class Pathfinder_Base_3D_Deprecated
    {
        //Priority_Queue_MaxHeap _mainPriorityQueueMaxHeap;
        double _priorityModifier;
        Voxel_Base_Deprecated _targetVoxel;
        Voxel_Base_Deprecated _startVoxel;
        PuzzleSet _puzzleSet;
        PathfinderMover_3D_Deprecated _mover;
        
        static int s_gridWidth;
        static int s_gridHeight;
        static int s_gridDepth;

        public static void SetGrid(int gridWidth, int gridHeight, int gridDepth)
        {
            s_gridWidth = gridWidth;
            s_gridHeight = gridHeight;
            s_gridDepth = gridDepth;
        }

        public void SetPath(Vector3 start, Vector3 target, PathfinderMover_3D_Deprecated mover, PuzzleSet puzzleSet)
        {
            if (!VoxelGrid_Deprecated.Initialised) VoxelGrid_Deprecated.InitializeVoxelGrid();

            _puzzleSet = puzzleSet;
            _mover = mover;

            _startVoxel = VoxelGrid_Deprecated.GetVoxelAtPosition(start);
            _targetVoxel = VoxelGrid_Deprecated.GetVoxelAtPosition(target);

            mover.StartPathfindingCoroutine(_runPathfinder(mover));
        }

        public void UpdatePath(PathfinderMover_3D_Deprecated mover, Vector3 start, Vector3 target)
        {
            _startVoxel = VoxelGrid_Deprecated.GetVoxelAtPosition(start);
            _targetVoxel = VoxelGrid_Deprecated.GetVoxelAtPosition(target);

            if (mover.CanGetNewPath)
            {
                Debug.Log("Ran pathfinder again");
                mover.StartPathfindingCoroutine(_runPathfinder(mover));
            }
        }

        IEnumerator _runPathfinder(PathfinderMover_3D_Deprecated mover)
        {
            if (_startVoxel == null) yield break;
            if (_startVoxel != null) yield break;

            if (_startVoxel == null || _targetVoxel == null || _startVoxel.Equals(_targetVoxel))
            {
                Debug.Log($"StartVoxel: {_startVoxel} or TargetVoxel: {_targetVoxel} is null or equal.");
                yield break;
            }

            var currentVoxel = _startVoxel;
            List<Voxel_Base_Deprecated> currentPath = new();
            List<Voxel_Base_Deprecated> previousPath = null;
            List<Vector3> currentObstacles = new();
            List<Vector3> previousObstacles = null;
            var pathIsComplete = false;

            _initialise();

            var iterationCount = 0;

            while (true)
            {
                var changesInEnvironment = false;
                currentObstacles = _mover.GetObstaclesInVision();

                if (previousObstacles == null || !_areObstaclesEqual(previousObstacles, currentObstacles))
                {
                    changesInEnvironment = true;

                    foreach (var position in currentObstacles)
                    {
                        var obstacleVoxel = VoxelGrid_Deprecated.GetVoxelAtPosition(position);

                        if (obstacleVoxel.IsObstacle) continue;

                        obstacleVoxel = VoxelGrid_Deprecated.AddSubvoxelToVoxelGrid(position, true);

                        foreach (Voxel_Base_Deprecated predecessor in obstacleVoxel.GetPredecessors())
                        {
                            _updateVertex(predecessor);
                        }
                    }

                    previousObstacles ??= new List<Vector3>(currentObstacles);
                }

                if (previousObstacles != null)
                {
                    foreach (var position in previousObstacles)
                    {
                        if (currentObstacles.Contains(position))
                        {
                            continue;
                        }

                        var removedObstacleVoxel = VoxelGrid_Deprecated.GetVoxelAtPosition(position);

                        if (removedObstacleVoxel == null)
                        {
                            Debug.Log($"{removedObstacleVoxel.WorldPosition} exists in obstacle lists but does not exist in VoxelGrid.");
                            continue;
                        }

                        removedObstacleVoxel.UpdateMovementCost(1);

                        foreach (var predecessor in removedObstacleVoxel.GetPredecessors())
                        {
                            _updateVertex(predecessor);
                        }

                        VoxelGrid_Deprecated.RemoveVoxelAtPosition(position);
                    }

                    previousObstacles = new List<Vector3>(currentObstacles);
                }

                _computeShortestPath();

                if (pathIsComplete)
                {
                    currentVoxel = _startVoxel;
                }

                var nextVoxel = _minimumSuccessorVoxel(currentVoxel);
                currentPath.Add(nextVoxel);
                currentVoxel = nextVoxel;

                var previousPriorityModifier = _priorityModifier;
                _priorityModifier += _manhattanDistance(currentVoxel, nextVoxel);

                if (!changesInEnvironment)
                {
                    _priorityModifier = previousPriorityModifier;
                }

                if (currentVoxel.Equals(_targetVoxel))
                {
                    pathIsComplete = true;
                }

                if (pathIsComplete && (previousPath == null || !_isPathEqual(previousPath, currentPath)))
                {
                    if (!VoxelGrid_Deprecated.VoxelsTestShown)
                    {
                        VoxelGrid_Deprecated.TestShowAllVoxels();
                    }
                
                    _mover.MoveTo(_targetVoxel);
                    currentVoxel = _startVoxel;

                    previousPath = new List<Voxel_Base_Deprecated>(currentPath);
                    currentPath.Clear();
                    pathIsComplete = false;

                    iterationCount = 0;
                    yield return null;
                }

                iterationCount++;

                if (iterationCount >= 1000) break;
            }

            Debug.Log($"IterationCount: {iterationCount}");
        }

        bool _areObstaclesEqual(List<Vector3> previousObstacles, List<Vector3> currentObstacles)
        {
            if (previousObstacles == null)
            {
                return false;
            }

            if (previousObstacles.Count != currentObstacles.Count)
            {
                return false;
            }

            var previousEnumerator = previousObstacles.GetEnumerator();
            var currentEnumerator = currentObstacles.GetEnumerator();

            while (previousEnumerator.MoveNext() && currentEnumerator.MoveNext())
            {
                if (!previousEnumerator.Current.Equals(currentEnumerator.Current))
                {
                    return false;
                }
            }

            return true;
        }

        bool _isPathEqual(List<Voxel_Base_Deprecated> path1, List<Voxel_Base_Deprecated> path2)
        {
            if (path1.Count != path2.Count)
            {
                return false;
            }

            for (int i = 0; i < path1.Count; i++)
            {
                if (!path1[i].Equals(path2[i]))
                {
                    return false;
                }
            }

            return true;
        }


        void _initialise()
        {
            foreach (Voxel_Base_Deprecated voxel in VoxelGrid_Deprecated.Voxels) { if (voxel != null) { voxel.RHS = double.PositiveInfinity; voxel.G = double.PositiveInfinity; } }
            //_mainPriorityQueueMaxHeap = new Priority_Queue_MaxHeap(s_gridWidth * s_gridHeight * s_gridDepth);
            _priorityModifier = 0;
            _targetVoxel.RHS = 0;
            //_mainPriorityQueueMaxHeap.Update(_targetVoxel, _calculatePriority(_targetVoxel));
        }

        Priority_Old_Deprecated _calculatePriority(Voxel_Base_Deprecated node)
        {
            return new Priority_Old_Deprecated(Math.Min(node.G, node.RHS) + _manhattanDistance(node, _startVoxel) + _priorityModifier, Math.Min(node.G, node.RHS));
        }
        double _manhattanDistance(Voxel_Base_Deprecated a, Voxel_Base_Deprecated b)
        {
            Vector3 distance = a.WorldPosition - b.WorldPosition;
            return Math.Abs(distance.x) + Math.Abs(distance.y) + Math.Abs(distance.z);
        }

        void _updateVertex(Voxel_Base_Deprecated voxel)
        {
            if (!voxel.Equals(_targetVoxel))
            {
                voxel.RHS = _minimumSuccessorCost(voxel);
            }
            // if (_mainPriorityQueueMaxHeap.Contains(voxel))
            // {
            //     _mainPriorityQueueMaxHeap.Remove(voxel);
            // }
            if (voxel.G != voxel.RHS)
            {
                //_mainPriorityQueueMaxHeap.Update(voxel, _calculatePriority(voxel));
            }
        }
        Voxel_Base_Deprecated _minimumSuccessorVoxel(Voxel_Base_Deprecated voxel)
        {
            double minimumCostToMove = Double.PositiveInfinity;
            Voxel_Base_Deprecated bestSucessor = null;

            //Debug.Log($"Original: WorldPos: {voxel.WorldPosition} Cost: {voxel.MovementCost}");

            foreach (Voxel_Base_Deprecated successor in voxel.GetSuccessors())
            {
                //Debug.Log($"WorldPos: {successor.WorldPosition} Cost: {successor.MovementCost} G: {successor.G}");

                double costToMove = voxel.GetMovementCostTo(successor, _puzzleSet) + successor.G;

                if (costToMove <= minimumCostToMove && !successor.IsObstacle)
                {
                    minimumCostToMove = costToMove;
                    bestSucessor = successor;
                }
            }

            bestSucessor.SetPredecessor(voxel);

            //Debug.Log($"Predecessor: {bestSucessor.Predecessor.WorldPosition}");

            //Debug.Log($"Best Succ WorldPos: {bestSucessor.WorldPosition} Cost: {bestSucessor.MovementCost}");

            return bestSucessor;
        }
        double _minimumSuccessorCost(Voxel_Base_Deprecated node)
        {
            double minimumCost = Double.PositiveInfinity;
            foreach (Voxel_Base_Deprecated successor in node.GetSuccessors())
            {
                double costToMove = node.GetMovementCostTo(successor, _puzzleSet) + successor.G;
                if (costToMove < minimumCost && !successor.IsObstacle) minimumCost = costToMove;
            }
            return minimumCost;
        }
        void _computeShortestPath()
        {
            // while (_mainPriorityQueueMaxHeap.Peek().CompareTo(_calculatePriority(_startVoxel)) < 0 || _startVoxel.RHS != _startVoxel.G)
            // {
            //     Priority_Old highestPriority = _mainPriorityQueueMaxHeap.Peek();
            //     Voxel_Base node = _mainPriorityQueueMaxHeap.Dequeue();
            //     if (node == null) break;
            //
            //     if (highestPriority.CompareTo(_calculatePriority(node)) < 0)
            //     {
            //         _mainPriorityQueueMaxHeap.Enqueue(node, _calculatePriority(node));
            //     }
            //     else if (node.G > node.RHS)
            //     {
            //         node.G = node.RHS;
            //         foreach (Voxel_Base neighbour in node.GetPredecessors())
            //         {
            //             _updateVertex(neighbour);
            //         }
            //     }
            //     else
            //     {
            //         node.G = Double.PositiveInfinity;
            //         _updateVertex(node);
            //         foreach (Voxel_Base neighbour in node.GetPredecessors())
            //         {
            //             _updateVertex(neighbour);
            //         }
            //     }
            // }
        }

        public List<Vector3> RetrievePath(Voxel_Base_Deprecated startVoxel, Voxel_Base_Deprecated targetVoxel)
        {
            List<Vector3> path = new List<Vector3>();
            Voxel_Base_Deprecated currentVoxel = targetVoxel;

            int iterationCount = 0;

            while (currentVoxel != null && !currentVoxel.Equals(startVoxel) && iterationCount < 1000)
            {
                if (currentVoxel == null || currentVoxel.Equals(startVoxel)) break;
                path.Add(currentVoxel.WorldPosition);
                currentVoxel = currentVoxel.Predecessor;
                iterationCount++;
            }

            if (currentVoxel != null)
            {
                path.Add(startVoxel.WorldPosition);
            }

            path.Reverse();

            return path;
        }

        public static void FindAllPredecessors(Voxel_Base_Deprecated node, int infiniteEnd, int infinityStart = 0)
        {
            infinityStart++;
            if (infinityStart > infiniteEnd) return;
            if (node.Predecessor == null) { Debug.Log($"{node.WorldPosition}_{node.WorldPosition} predecessor is null"); return; }

            Debug.Log($"{node.WorldPosition}_{node.WorldPosition} -> {node.Predecessor.WorldPosition}_{node.Predecessor.WorldPosition}");
            FindAllPredecessors(node.Predecessor, infinityStart, infiniteEnd);
        }
    }

    public class VoxelGrid_Deprecated
    {
        public static bool Initialised { get; private set; }
        public static Voxel_Base_Deprecated[,,] Voxels;
        public static int Scale { get; private set; }
        //static Vector3 _offset;
        public static bool VoxelsTestShown { get; private set; } = false;
        static List<Voxel_Base_Deprecated> _testShowPathfinding = new();
        static List<GameObject> _testShowVoxels = new();
        static List<GameObject> _testShowSubVoxels = new();
        static Vector3 _defaultOffset = new Vector3(0.5f, 0, 0.5f);

        public static List<Voxel_Base_Deprecated> VoxelsTest = new();
        public static List<Vector3> NavigationList = new();

        public static void InitialiseVoxelGridTest(float width = 100, float height = 4, float depth = 100)
        {
            //Collider groundCollider = Manager_Game.S_Instance.GroundCollider;
            Collider groundCollider = GameObject.Find("Ground").GetComponent<Collider>(); // There is no ground collider
            bool hasGroundCollider = groundCollider != null;

            if (hasGroundCollider)
            {
                List<Collider> obstacles = new List<Collider>(UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None));
                obstacles.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

                for (int i = 0; i < obstacles.Count; i++)
                {
                    Collider collider = obstacles[i];
                    Voxel_Base_Deprecated newVoxel = new Voxel_Base_Deprecated();

                    newVoxel.SetVoxelProperties(
                        worldPosition: collider.transform.position,
                        size: collider.bounds.size,
                        voxelType: _getVoxelType(collider.gameObject),
                        g: double.PositiveInfinity,
                        rhs: double.PositiveInfinity
                    );

                    newVoxel.UpdateMovementCostTest();

                    VoxelsTest.Add(newVoxel);
                }
            }

            Initialised = true;
            //TestTestShowAllVoxels();
        }

        public static void TestTestShowAllVoxels(bool showOpen = true, bool showObstacles = true, bool showGround = true)
        {
            Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            Material green = Resources.Load<Material>("Materials/Material_Green");
            Material red = Resources.Load<Material>("Materials/Material_Red");
            Material blue = Resources.Load<Material>("Materials/Material_Blue");
            Material white = Resources.Load<Material>("Materials/Material_White");

            foreach (Voxel_Base_Deprecated voxel in VoxelsTest)
            {
                if (voxel == null) continue;

                if (voxel.VoxelTypeDeprecated == VoxelType_Deprecated.Open && showOpen)
                {
                    GameObject voxelGO = voxel.TestShowVoxel(GameObject.Find("OpenTest").transform, mesh, white);
                    _testShowVoxels.Add(voxelGO);
                }
                else if (voxel.VoxelTypeDeprecated == VoxelType_Deprecated.Obstacle && showObstacles)
                {
                    GameObject voxelGO = voxel.TestShowVoxel(GameObject.Find("ObstacleTest").transform, mesh, green);
                    _testShowVoxels.Add(voxelGO);
                }
                else if (voxel.VoxelTypeDeprecated == VoxelType_Deprecated.Ground && showGround)
                {
                    GameObject voxelGO = voxel.TestShowVoxel(GameObject.Find("GroundTest").transform, mesh, red);
                    _testShowSubVoxels.Add(voxelGO);
                }
            }

            VoxelsTestShown = true;
        }

        static VoxelType_Deprecated _getVoxelType(GameObject voxelGO)
        {
            if (voxelGO.name.Contains("Water"))
            {
                return VoxelType_Deprecated.Water;
            }
            if (voxelGO.name.Contains("Air"))
            {
                return VoxelType_Deprecated.Air;
            }
            if (voxelGO.name.Contains("Ground"))
            {
                return VoxelType_Deprecated.Ground;
            }
            if (voxelGO.name.Contains("Obstacle"))
            {
                return VoxelType_Deprecated.Obstacle;
            }
            else
            {
                return VoxelType_Deprecated.Open;
            }
        }

        public static void InitializeVoxelGrid(float width = 100, float height = 4, float depth = 100, int scale = 10, Vector3? offset = null)
        {
            //Scale = scale;

            //Collider groundCollider = Manager_Game.Instance.GroundCollider;
            //bool hasGroundCollider = false;

            //_offset = offset ?? getCollider();

            //Vector3 getCollider()
            //{
            //    if (groundCollider != null)
            //    {
            //        hasGroundCollider = true;
            //        return groundCollider.bounds.size / 2f;
            //    }

            //    return Vector3.zero;
            //}

            //_offset = new Vector3(_offset.x, 0, _offset.z);

            //int gridWidth = (int)((width + 1) * Scale);
            //int gridHeight = (int)((height + 1) * Scale);
            //int gridDepth = (int)((depth + 1) * Scale);

            //Pathfinder_Base_3D.SetGrid(gridWidth, gridHeight, gridDepth);

            //Voxels = new Voxel_Base[gridWidth, gridHeight, gridDepth];

            //for (int x = (int)(_offset.x * Scale); x < gridWidth; x += Scale)
            //{
            //    for (int y = (int)(_offset.y * Scale); y < gridHeight; y += Scale)
            //    {
            //        for (int z = (int)(_offset.z * Scale); z < gridDepth; z += Scale)
            //        {
            //            Vector3Int gridPos = new Vector3Int(x, y, z);
            //            Vector3 worldPos = Vector3.zero;

            //            if (hasGroundCollider)
            //            {
            //                worldPos.x = (x / Scale) - (_offset.x * 2);
            //                worldPos.y = (y / Scale) - (_offset.y * 2);
            //                worldPos.z = (z / Scale) - (_offset.z * 2);
            //            }
            //            else
            //            {
            //                worldPos.x = (x / Scale) - _offset.x;
            //                worldPos.y = (y / Scale) - _offset.y;
            //                worldPos.z = (z / Scale) - _offset.z;
            //            }

            //            Voxel_Base newVoxel = Voxels[x, y, z] = new Voxel_Base();

            //            newVoxel.SetVoxelProperties(gridPosition: gridPos, worldPosition: worldPos, size: Vector3.one, g: double.PositiveInfinity, rhs: double.PositiveInfinity, isObstacle: false);

            //            Voxels[x, y, z].UpdateMovementCost(1);
            //        }
            //    }
            //}

            ////Debug.Log($"Grid Size: {new Vector3(gridWidth, gridHeight, gridDepth)}");

            //Initialised = true;
        }

        public static Voxel_Base_Deprecated AddSubvoxelToVoxelGrid(Vector3 position, bool isObstacle = false)
        {
            Collider collider = Physics.OverlapSphere(position, 0.1f).FirstOrDefault(); // hitCollider => hitCollider.gameObject.layer == "Wall");

            if (collider == null)
            {
                Debug.Log($"No cells at Pos: {position}.");
                return null;
            }

            Vector3 center = collider.bounds.center;
            Vector3 extents = collider.bounds.extents;

            Vector3 minBounds = center - extents;
            Vector3 maxBounds = center + extents;

            //Vector3Int minGridPosition = WorldToGridPosition(minBounds);
            //Vector3Int maxGridPosition = WorldToGridPosition(maxBounds);

            for (int x = (int)minBounds.x; x < maxBounds.x; x += Scale)
            {
                for (int y = (int)minBounds.y; y < maxBounds.y; y += Scale)
                {
                    for (int z = (int)minBounds.z; z < maxBounds.z; z += Scale)
                    {
                        Vector3Int positionToRemove = Vector3Int.zero; //= new Vector3Int(WorldToGridPosition(x, y, z));
                        Voxel_Base_Deprecated voxelToRemove = Voxels[positionToRemove.x, positionToRemove.y, positionToRemove.z];

                        Debug.Log($"Trying to remove Voxel: {voxelToRemove} WorldPos: {position} GridPos: {positionToRemove}");

                        if (voxelToRemove != null && !voxelToRemove.IsObstacle)
                        {
                            Debug.Log($"Removing voxel at {positionToRemove}");
                            RemoveVoxelAtPosition(positionToRemove);
                        }
                    }
                }
            }

            Vector3[] vertices = new Vector3[8];
            vertices[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
            vertices[1] = center + new Vector3(extents.x, -extents.y, -extents.z);
            vertices[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
            vertices[3] = center + new Vector3(extents.x, extents.y, -extents.z);
            vertices[4] = center + new Vector3(-extents.x, -extents.y, extents.z);
            vertices[5] = center + new Vector3(extents.x, -extents.y, extents.z);
            vertices[6] = center + new Vector3(-extents.x, extents.y, extents.z);
            vertices[7] = center + new Vector3(extents.x, extents.y, extents.z);

            Vector3 minExtents = vertices.Select(vertex => vertex - position).Aggregate(Vector3.Min);
            Vector3 maxExtents = vertices.Select(vertex => vertex - position).Aggregate(Vector3.Max);

            Vector3 subVoxelSize = maxExtents - minExtents;
            Vector3 subVoxelCenter = position + (minExtents + maxExtents) / 2;

            Vector3Int gridPosition = WorldToGridPosition(subVoxelCenter);
            Voxel_Base_Deprecated existingVoxel = Voxels[gridPosition.x, gridPosition.y, gridPosition.z];

            if (existingVoxel != null && existingVoxel.Size == subVoxelSize)
            {
                Debug.Log($"Voxel already exists at Pos: {position} GridPos: {gridPosition} WorldPos: {existingVoxel.WorldPosition} with same scale: {subVoxelSize}.");
                return null;
            }

            Voxel_Base_Deprecated newVoxel = Voxels[gridPosition.x, gridPosition.y, gridPosition.z] = new Voxel_Base_Deprecated();

            newVoxel.SetVoxelProperties(gridPosition: gridPosition, worldPosition: subVoxelCenter, size: subVoxelSize, g: double.PositiveInfinity, rhs: double.PositiveInfinity, isObstacle: isObstacle);

            if (isObstacle) newVoxel.UpdateMovementCost(double.PositiveInfinity);

            _testShowPathfinding.Add(newVoxel);

            return newVoxel;
        }

        public static void RemoveVoxelAtPosition(Vector3? position)
        {
            if (!position.HasValue) return;

            Voxel_Base_Deprecated voxel = GetVoxelAtPosition(position.Value);

            if (voxel != null)
            {
                voxel.UpdateMovementCost(1);
                _testShowPathfinding.Remove(voxel);
                VoxelGrid_Deprecated.Voxels[voxel.GridPosition.x, voxel.GridPosition.y, voxel.GridPosition.z] = null;
                if (VoxelGrid_Deprecated.Voxels?[voxel.GridPosition.x, voxel.GridPosition.y, voxel.GridPosition.z] == null) Debug.Log($"Voxel removed at {position}");
                return;
            }

            TestHideAllVoxels();
            TestShowAllVoxels();

            return;
        }

        public static Voxel_Base_Deprecated GetVoxelAtPosition(Vector3 position)
        {
            //Vector3Int gridPosition = WorldToGridPosition(position);

            //if (gridPosition.x < 0 && gridPosition.x >= Voxels.GetLength(0) &&
            //    gridPosition.y < 0 && gridPosition.y >= Voxels.GetLength(1) &&
            //    gridPosition.z < 0 && gridPosition.z >= Voxels.GetLength(2))
            //{
            //    Debug.Log($"Converted position WorldPos: {position} GridPos: {gridPosition} is out of Voxel Grid bounds: {Voxels.GetLength(0)}_{Voxels.GetLength(1)}_{Voxels.GetLength(2)}");
            //    return null;
            //}

            ////Debug.Log($"WorldPos: {position} GridPos: {new Vector3(gridPosition.x,gridPosition.y, gridPosition.z)}");

            //Voxel_Base voxel = Voxels[gridPosition.x, gridPosition.y, gridPosition.z];

            //if (voxel == null)
            //{
            //    Vector3 scaledPosition = new Vector3(
            //        (int)(((int)(gridPosition.x / Scale) + _offset.x) * Scale),
            //        (int)(((int)(gridPosition.y / Scale) + _offset.y) * Scale),
            //        (int)(((int)(gridPosition.z / Scale) + _offset.z) * Scale)
            //        );

            //    voxel = Voxels[(int)scaledPosition.x, (int)scaledPosition.y, (int)scaledPosition.z];

            //    if (voxel == null)
            //    {
            //        Debug.Log($"Voxel and Subvoxel does not exist at: GridPos: {gridPosition} WorldPos: {position} ScaledPosition: {scaledPosition}");
            //        return null;
            //    }

            //    return voxel;
            //}

            //return voxel;

            return null;
        }

        public static Vector3Int WorldToGridPosition(Vector3 worldPosition)
        {
            return Vector3Int.zero;
            //return new Vector3Int(
            //    (int)((worldPosition.x + _offset.x) * Scale),
            //    (int)((worldPosition.y + _offset.y) * Scale),
            //    (int)((worldPosition.z + _offset.z) * Scale)
            //);
        }

        public static void TestShowAllVoxels(bool showVoxels = true, bool showSubvoxels = true)
        {
            Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            Material green = Resources.Load<Material>("Materials/Material_Green");
            Material red = Resources.Load<Material>("Materials/Material_Red");
            Material blue = Resources.Load<Material>("Materials/Material_Blue");

            foreach (Voxel_Base_Deprecated voxel in Voxels)
            {
                if (voxel == null) continue;

                if (!voxel.IsObstacle && showVoxels)
                {
                    GameObject voxelGO = voxel.TestShowVoxel(GameObject.Find("!Obstacle").transform, mesh, green);
                    _testShowVoxels.Add(voxelGO);
                }
                else if (voxel.IsObstacle && showSubvoxels)
                {
                    GameObject voxelGO = voxel.TestShowVoxel(GameObject.Find("Obstacle").transform, mesh, red);
                    _testShowSubVoxels.Add(voxelGO);
                }
            }

            VoxelsTestShown = true;
        }

        public static void TestHideAllVoxels()
        {
            for (int i = 0; i < _testShowVoxels.Count; i++)
            {
                Manager_Game.Destroy(_testShowVoxels[i]);
            }

            VoxelsTestShown = false;
        }
    }

    public enum VoxelType_Deprecated { None, Open, Ground, Obstacle, Air, Water }

    public class Voxel_Base_Deprecated
    {
        public Vector3Int GridPosition { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public Vector3 Size { get; private set; }
        public VoxelType_Deprecated VoxelTypeDeprecated { get; private set; }
        public double G;
        public double RHS;
        public Voxel_Base_Deprecated Predecessor { get; private set; }

        public double MovementCost;

        public bool IsObstacle { get; private set; }
        GameObject _pathfindingVoxelGO;

        public Voxel_Base_Deprecated SetVoxelProperties(
            Vector3Int? gridPosition = null, 
            Vector3? worldPosition = null, 
            Vector3? size = null, 
            VoxelType_Deprecated? voxelType = null, 
            double? g = null, 
            double? rhs = null, 
            bool? isObstacle = null
        )
        {
            if (gridPosition != null) GridPosition = gridPosition.Value;
            if (worldPosition != null) WorldPosition = worldPosition.Value;
            if (size != null) Size = size.Value;
            if (voxelType != null) VoxelTypeDeprecated = voxelType.Value;
            if (g != null) G = g.Value;
            if (rhs != null) rhs = rhs.Value;
            if (isObstacle != null) IsObstacle = isObstacle.Value;

            return this;
        }

        public GameObject TestShowVoxel(Transform transform, Mesh mesh, Material material)
        {
            GameObject voxelGO = new GameObject($"{WorldPosition}");
            _pathfindingVoxelGO = voxelGO;
            voxelGO.AddComponent<MeshFilter>().mesh = mesh;
            voxelGO.AddComponent<MeshRenderer>().material = material;
            voxelGO.transform.SetParent(transform);
            voxelGO.transform.localPosition = WorldPosition;
            voxelGO.transform.localScale = Size;
            return voxelGO;
        }
        public void TestHideVoxel()
        {
            _pathfindingVoxelGO.SetActive(false);
        }

        public void SetPredecessor(Voxel_Base_Deprecated predecessor)
        {
            Predecessor = predecessor;
        }

        public void UpdateMovementCostTest()
        {
            switch(VoxelTypeDeprecated)
            {
                case VoxelType_Deprecated.Open:
                    MovementCost = 1;
                    break;
                case VoxelType_Deprecated.Ground:
                    MovementCost = 1.5;
                    break;
                case VoxelType_Deprecated.Obstacle:
                    MovementCost = double.PositiveInfinity;
                    break;
                case VoxelType_Deprecated.Air:
                    MovementCost = 0.75;
                    break;
                case VoxelType_Deprecated.Water:
                    MovementCost = 1.5;
                    break;
                default:
                    MovementCost = double.PositiveInfinity;
                    break;
            }
        }

        public void UpdateMovementCost(double cost)
        {
            if (cost == double.PositiveInfinity) IsObstacle = true;

            MovementCost = cost;
        }

        public double GetMovementCostTo(Voxel_Base_Deprecated successor, PuzzleSet puzzleSet)
        {
            return successor.MovementCost;

            throw new InvalidOperationException($"{puzzleSet} not valid.");
        }

        public bool Equals(Voxel_Base_Deprecated that)
        {
            if (GridPosition == that.GridPosition) return true;
            return false;
        }

        public LinkedList<Voxel_Base_Deprecated> GetSuccessors()
        {
            LinkedList<Voxel_Base_Deprecated> successors = new LinkedList<Voxel_Base_Deprecated>();
            int scale = (int)VoxelGrid_Deprecated.Scale;

            TryAddDirectionalSuccessors(1, 0, 0);
            TryAddDirectionalSuccessors(-1, 0, 0);
            TryAddDirectionalSuccessors(0, 1, 0);
            TryAddDirectionalSuccessors(0, -1, 0);
            TryAddDirectionalSuccessors(0, 0, 1);
            TryAddDirectionalSuccessors(0, 0, -1);

            void TryAddDirectionalSuccessors(int x, int y, int z)
            {
                bool subVoxelExists = false;
                bool subVoxelIsObstacle = false;

                for (int i = 1; i < 10; i++)
                {
                    (subVoxelExists, subVoxelIsObstacle) = TryAddSuccessor(GridPosition.x + x * i, GridPosition.y + y * i, GridPosition.z + z * i);
                    if (subVoxelExists && subVoxelIsObstacle) break;
                }

                if (!subVoxelIsObstacle) TryAddSuccessor(GridPosition.x + x * scale, GridPosition.y + y * scale, GridPosition.z + z * scale);
            }

            (bool subVoxelExists, bool subVoxelIsObstacle) TryAddSuccessor(int x, int y, int z)
            {
                if (x >= 0 && x < VoxelGrid_Deprecated.Voxels.GetLength(0) &&
                    y >= 0 && y < VoxelGrid_Deprecated.Voxels.GetLength(1) &&
                    z >= 0 && z < VoxelGrid_Deprecated.Voxels.GetLength(2))
                {
                    Voxel_Base_Deprecated voxel = VoxelGrid_Deprecated.Voxels[x, y, z];

                    if (voxel == null) return (false, false);

                    successors.AddFirst(voxel);

                    if (!voxel.IsObstacle) return (true, false);

                    else return (true, true);
                }

                return (false, false);
            }

            return successors;
        }

        public LinkedList<Voxel_Base_Deprecated> GetPredecessors()
        {
            LinkedList<Voxel_Base_Deprecated> predecessors = new();
            int scale = (int)VoxelGrid_Deprecated.Scale;

            if (!TryAddPredecessor(GridPosition.x + 1, GridPosition.y, GridPosition.z))
                TryAddPredecessor(GridPosition.x + scale, GridPosition.y, GridPosition.z);

            if (!TryAddPredecessor(GridPosition.x - 1, GridPosition.y, GridPosition.z))
                TryAddPredecessor(GridPosition.x - scale, GridPosition.y, GridPosition.z);

            if (!TryAddPredecessor(GridPosition.x, GridPosition.y + 1, GridPosition.z))
                TryAddPredecessor(GridPosition.x, GridPosition.y + scale, GridPosition.z);

            if (!TryAddPredecessor(GridPosition.x, GridPosition.y - 1, GridPosition.z))
                TryAddPredecessor(GridPosition.x, GridPosition.y - scale, GridPosition.z);

            if (!TryAddPredecessor(GridPosition.x, GridPosition.y, GridPosition.z + 1))
                TryAddPredecessor(GridPosition.x, GridPosition.y, GridPosition.z + scale);

            if (!TryAddPredecessor(GridPosition.x, GridPosition.y, GridPosition.z - 1))
                TryAddPredecessor(GridPosition.x, GridPosition.y, GridPosition.z - scale);

            bool TryAddPredecessor(int x, int y, int z)
            {
                if (x >= 0 && x < VoxelGrid_Deprecated.Voxels.GetLength(0) &&
                    y >= 0 && y < VoxelGrid_Deprecated.Voxels.GetLength(1) &&
                    z >= 0 && z < VoxelGrid_Deprecated.Voxels.GetLength(2))
                {
                    Voxel_Base_Deprecated voxel = VoxelGrid_Deprecated.Voxels[x, y, z];

                    if (voxel != null && !voxel.IsObstacle)
                    {
                        predecessors.AddFirst(voxel);
                        return true;
                    }
                }

                return false;
            }

            return predecessors;
        }
    }

    public class Priority_Old_Deprecated
    {
        public double PrimaryPriority;
        public double SecondaryPriority;

        public Priority_Old_Deprecated(double primaryPriority, double secondaryPriority)
        {
            PrimaryPriority = primaryPriority;
            SecondaryPriority = secondaryPriority;
        }
        public int CompareTo(Priority_Old_Deprecated that)
        {
            if (PrimaryPriority < that.PrimaryPriority) return -1;
            else if (PrimaryPriority > that.PrimaryPriority) return 1;
            if (SecondaryPriority > that.SecondaryPriority) return 1;
            else if (SecondaryPriority < that.SecondaryPriority) return -1;
            return 0;
        }
    }

    public class Queue_Voxel_Deprecated
    {
        public Voxel_Base_Deprecated Voxel;
        public Priority_Old_Deprecated Priority;

        public Queue_Voxel_Deprecated(Voxel_Base_Deprecated voxel, Priority_Old_Deprecated priority)
        {
            Voxel = voxel;
            Priority = priority;
        }
    }
    
    public enum MoverType_Deprecated { None, Ground, Fly, Dig, Swim }

    public interface PathfinderMover_3D_Deprecated
    {
        List<MoverType_Deprecated> MoverTypes { get; set; }
        bool CanGetNewPath { get; set; }
        void MoveTo(Voxel_Base_Deprecated target);
        void StartPathfindingCoroutine(IEnumerator coroutine);
        void StopPathfindingCoroutine();
        List<Vector3> GetObstaclesInVision();
    }
}