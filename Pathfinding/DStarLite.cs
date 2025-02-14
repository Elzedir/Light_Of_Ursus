using System;
using System.Collections.Generic;
using System.Linq;
using Priorities.Priority_Queues;
using UnityEngine;

namespace Pathfinding
{
    public class DStarLite
    {
        readonly Dictionary<long, Voxel_Decision> _nodes;
        readonly Priority_Queue_MaxHeap<Voxel_Decision> _openList;
        readonly Voxel_Walkable _start, _end;

        public DStarLite(Octree_Path allPath, Vector3 startPosition, Vector3 endPosition)
        {
            _nodes = new Dictionary<long, Voxel_Decision>();
            _openList = new Priority_Queue_MaxHeap<Voxel_Decision>();

            _start = allPath.GetClosestVoxel(startPosition);
            _end = allPath.GetClosestVoxel(endPosition);

            Initialize(allPath);
        }

        void Initialize(Octree_Path path)
        {
            foreach (var voxel in path.AllWalkableVoxels.Values)
            {
                _nodes[voxel.VoxelID] = new Voxel_Decision(
                        voxelWalkable: voxel, 
                        gCost: float.MaxValue, 
                        rhsCost: float.MaxValue, 
                        heuristic: _getHeuristic(voxel, _end));
            }

            var endNode = _nodes[_end.VoxelID];
            endNode.RHSCost = 0;
            UpdateVertex(endNode);
        }

        static float _getHeuristic(Voxel_Walkable a, Voxel_Walkable b) => Vector3.Distance(a.Position, b.Position);
        
        void UpdateVertex(Voxel_Decision node)
        {
            if (node.VoxelID != _end.VoxelID)
            {
                node.RHSCost = node.Voxel_Walkable.Neighbors
                    .Where(n => _nodes.ContainsKey(n.VoxelID))
                    .Min(n => _nodes[n.VoxelID].GCost + Vector3.Distance(node.Voxel_Walkable.Position, n.Position));
            }

            _openList.Remove(node.VoxelID);

            if (Mathf.Approximately(node.GCost, node.RHSCost)) return;
            
            var key1 = Math.Min(node.GCost, node.RHSCost) + _getHeuristic(_start, node.Voxel_Walkable);
            var key2 = Math.Min(node.GCost, node.RHSCost);
            //_openList.Update(node.VoxelID, (key1, key2));
        }
        
        public void PathChanged(Vector3 changedNodePosition)
        {
            // if (!_nodes.TryGetValue(GetNodeID(changedNodePosition), out var changedNode)) return;
            //
            // UpdateVertex(changedNode);

            RunDStarLite();
        }
        
        public void UpdatePath(Vector3 start, Vector3 end)
        {
            
            // _start = _nodes[GetNodeID(start)].Voxel_Walkable;
            // _end = _nodes[GetNodeID(end)].Voxel_Walkable;

            RunDStarLite();
        }
        
        public List<Vector3> RunDStarLite()
        {
            while (_openList.Count() > 0)
            {
                var currentNode = _openList.Dequeue().PriorityObject;
                
                if (currentNode.GCost <= currentNode.RHSCost) continue;

                currentNode.GCost = currentNode.RHSCost;

                foreach (var neighbor in currentNode.Voxel_Walkable.Neighbors)
                {
                    if (!_nodes.TryGetValue(neighbor.VoxelID, out var node)) continue;

                    UpdateVertex(node);
                }
            }

            return GetShortestPath();
        }

        List<Vector3> GetShortestPath()
        {
            var path = new List<Vector3>();
            var current = _start;

            while (current.VoxelID != _end.VoxelID)
            {
                path.Add(current.Position);

                if (!current.Neighbors.Any(n => _nodes.ContainsKey(n.VoxelID))) 
                    break;

                current = current.Neighbors
                    .Where(n => _nodes.ContainsKey(n.VoxelID))
                    .OrderBy(n => _nodes[n.VoxelID].GCost)
                    .FirstOrDefault();
                
                if (current == null) break;

                if (Mathf.Approximately(_nodes[current.VoxelID].GCost, float.MaxValue)) break;
            }

            path.Add(_end.Position);
            return path;
        }
    }

    public class ExampleUseClassFlowPath
    {
        Octree_Map _map;
        Octree_Path _path;
        DStarLite _pathfinder;
        List<Vector3> _shortestPath;
        Vector3 _startPos, _endPos;

        public void OnActionPerformed(Vector3 treePos)
        {
            _map ??= new Octree_Map(100);
            
            //_path = new Octree_Path(_map);
            _pathfinder = new DStarLite(_path, _startPos, _endPos);
            _shortestPath = _pathfinder.RunDStarLite();
        }
    }
}