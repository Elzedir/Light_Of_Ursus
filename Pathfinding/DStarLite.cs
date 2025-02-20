using System;
using System.Collections.Generic;
using System.Linq;
using Priorities.Priority_Queues;
using UnityEngine;

namespace Pathfinding
{
    //* Current VoxelID and NodeID system can only work for worlds up to 1 048 576 units in size.
    //* Apparently, can try FNV-1a Hashing to avoid bit limits.
    
    public class DStarLite
    {
        Dictionary<ulong, GameObject> _visibleVoxels;
        
        readonly Dictionary<ulong, Voxel_Decision> _voxels;
        readonly Priority_Queue_MaxHeap<Voxel_Decision> _openList;
        readonly Octree_Path _path;

        Voxel_Walkable _start, _end;

        List<Vector3> _shortestPath;
        public List<Vector3> ShortestPath => _shortestPath ??= _runDStarLite();

        public DStarLite(List<MoverType> moverTypes, Vector3 startPosition, Vector3 endPosition)
        {
            _voxels = new Dictionary<ulong, Voxel_Decision>();
            _openList = new Priority_Queue_MaxHeap<Voxel_Decision>();
            _path = Octree_Map.GetOrCreatePath(moverTypes);

            _start = _path.GetClosestVoxel(startPosition);
            _end = _path.GetClosestVoxel(endPosition);

            Initialize();
        }

        void Initialize()
        {
            foreach (var voxel in _path.AllWalkableVoxels.Values)
            {
                _voxels[voxel.ID] = new Voxel_Decision(
                        voxelWalkable: voxel, 
                        gCost: float.PositiveInfinity, 
                        rhsCost: float.PositiveInfinity, 
                        heuristic: _getHeuristic(voxel, _end));
            }

            var endVoxel = _voxels[_end.ID];
            endVoxel.RHSCost = 0;
            UpdateVertex(endVoxel);
        }

        static float _getHeuristic(Voxel_Walkable a, Voxel_Walkable b) => Vector3.Distance(a.Position, b.Position);
        
        void UpdateVertex(Voxel_Decision voxel)
        {
            if (voxel.ID != _end.ID)
            {
                voxel.RHSCost = voxel.Voxel_Walkable.Neighbors
                    .Where(n => _voxels.ContainsKey(n.ID))
                    .Min(n => _voxels[n.ID].GCost + Vector3.Distance(voxel.Voxel_Walkable.Position, n.Position));
            }

            _openList.Remove(voxel.ID);
            
            if (voxel.IsApproximatelyEqualCost) return;

            _openList.Update(new Priority_Element<Voxel_Decision>(
                voxel.ID, Math.Min(voxel.GCost, voxel.RHSCost) + _getHeuristic(_start, voxel.Voxel_Walkable), voxel));
        }
        
        public void PathChanged(Vector3 changedVoxelPosition)
        {
            if (!_voxels.TryGetValue(Voxel_Base.GetVoxelIDFromPosition(changedVoxelPosition), out var changedVoxel)) return;
            
            UpdateVertex(changedVoxel);

            _shortestPath = _runDStarLite();
        }
        
        public void UpdatePath(Vector3 start, Vector3 end)
        {
            _visibleVoxels ??= _initialiseVisibleVoxels();
            
            if (!_voxels.TryGetValue(Voxel_Base.GetVoxelIDFromPosition(start), out var startVoxel))
            {
                Debug.LogError($"Start voxel not found at {start}");
                return;
            }

            if (!_voxels.TryGetValue(Voxel_Base.GetVoxelIDFromPosition(end), out var endVoxel))
            {
                Debug.LogError($"End voxel not found at {end}");
                return;
            }
            
            _start = startVoxel.Voxel_Walkable;
            _end = endVoxel.Voxel_Walkable;
            
            _shortestPath = _runDStarLite();
        }
        
        Dictionary<ulong, GameObject> _initialiseVisibleVoxels()
        {
            var mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var green = Resources.Load<Material>("Materials/Material_Green");
            var red = Resources.Load<Material>("Materials/Material_Red");
            var blue = Resources.Load<Material>("Materials/Material_Blue");
            
            var materials = new List<Material> { green, red };
            
            var visibleVoxels = new Dictionary<ulong, GameObject>();
            
            Debug.Log(_path.AllWalkableVoxels.Count);
            
            // foreach (var voxel in _path.AllWalkableVoxels.Values)
            // {
            //     visibleVoxels.Add(voxel.ID, _testShowVoxel(GameObject.Find("VisibleVoxels").transform, voxel.Position, mesh, blue, Vector3.one));
            // }

            Debug.Log(Octree_Map.S_RootMapVoxel.Children.Length);
            
            // foreach (var voxel in Octree_Map.S_RootMapVoxel.Children) // Assuming S_RootMapVoxel is your root voxel
            // {
            //     _showVoxelRecursive(GameObject.Find("VisibleVoxels").transform, voxel, mesh, materials, Vector3.one * voxel.Size, 0);
            // }

            return visibleVoxels;
        }

        static void _showVoxelRecursive(Transform parentTransform, Voxel_Base voxel, Mesh mesh, List<Material> materials, Vector3 size, int colorIndex)
        {
            var material = materials[colorIndex % materials.Count]; 
            
            var voxelGO = _testShowVoxel(parentTransform, voxel.Position, mesh, material, size);

            if (voxel.Children == null) return;
            
            foreach (var childVoxel in voxel.Children)
            {
                _showVoxelRecursive(parentTransform, childVoxel, mesh, materials, Vector3.one * childVoxel.Size, colorIndex + 1);
            }
        }

        static GameObject _testShowVoxel(Transform transform, Vector3 position ,Mesh mesh, Material material, Vector3 size)
        {
            var voxelGO = new GameObject($"{position}");
            voxelGO.AddComponent<MeshFilter>().mesh = mesh;
            voxelGO.AddComponent<MeshRenderer>().material = material;
            voxelGO.transform.SetParent(transform);
            voxelGO.transform.localPosition = position;
            voxelGO.transform.localScale = size;
            return voxelGO;
        }
        
        List<Vector3> _runDStarLite()
        {
            if (_start == null || _end == null || _start == _end || _voxels.Count == 0)
            {
                Debug.Log("Hit some problem here");
                return null;
            }
            
            while (_openList.Count() > 0)
            {
                var currentVoxel = _openList.Dequeue().PriorityObject;
                
                if (currentVoxel.GCost <= currentVoxel.RHSCost) continue;

                currentVoxel.GCost = currentVoxel.RHSCost;

                foreach (var neighbor in currentVoxel.Voxel_Walkable.Neighbors)
                {
                    if (!_voxels.TryGetValue(neighbor.ID, out var voxel_Decision)) continue;

                    UpdateVertex(voxel_Decision);
                }
            }

            return _getShortestPath();
        }

        List<Vector3> _getShortestPath()
        {
            var path = new List<Vector3>();
            var current = _start;

            while (current.ID != _end.ID)
            {
                path.Add(current.Position);

                if (!current.Neighbors.Any(n => _voxels.ContainsKey(n.ID))) 
                    break;

                current = current.Neighbors
                    .Where(n => _voxels.ContainsKey(n.ID))
                    .OrderBy(n => _voxels[n.ID].GCost)
                    .FirstOrDefault();
                
                if (current == null) break;

                if (Mathf.Approximately(_voxels[current.ID].GCost, float.PositiveInfinity)) break;
            }

            path.Add(_end.Position);
            return path;
        }
    }
}