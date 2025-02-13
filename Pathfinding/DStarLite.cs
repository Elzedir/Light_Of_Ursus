using System.Collections.Generic;
using System.Linq;
using Pathfinding.FlowPath;
using Priorities.Priority_Queues;
using UnityEngine;

namespace Pathfinding
{
    public class DStarLite
    {
        readonly Dictionary<long, Voxel_Decision> _nodes;
        Priority_Queue_MaxHeap<Voxel_Decision> _openList;
        readonly Voxel_Path _start, _end;

        public DStarLite(Octree_Paths allPaths, Vector3 startPosition, Vector3 endPosition)
        {
            _nodes = new Dictionary<long, Voxel_Decision>();
            _openList = new Priority_Queue_MaxHeap<Voxel_Decision>();

            _start = allPaths.GetClosestNode(startPosition);
            _end = allPaths.GetClosestNode(endPosition);

            Initialize(allPaths);
        }

        void Initialize(Octree_Paths paths)
        {
            foreach (var voxel in paths.PathNodes.Values)
            {
                _nodes[voxel.NodeID] = new Voxel_Decision(
                        voxelPath: voxel, 
                        gCost: float.MaxValue, 
                        rhsCost: float.MaxValue, 
                        heuristic: _getHeuristic(voxel, _end));
            }

            var endNode = _nodes[_end.NodeID];
            endNode.RHSCost = 0;
            _openList.Update(endNode.NodeID, ?);
        }

        static float _getHeuristic(Voxel_Path a, Voxel_Path b) => Vector3.Distance(a.Position, b.Position);

        public List<Vector3> RunDStarLite()
        {
            while (_openList.Count() > 0)
            {
                var currentNode = _openList.Dequeue().PriorityObject;
                
                if (currentNode.GCost <= currentNode.RHSCost) continue;

                currentNode.GCost = currentNode.RHSCost;

                foreach (var neighbor in currentNode.Voxel_Path.Neighbors)
                {
                    var neighborNode = _nodes[neighbor.Position];
                    var newRHS = currentNode.GCost + Vector3.Distance(currentNode.Voxel_Path.Position, neighbor.Position);

                    if (!(newRHS < neighborNode.RHSCost)) continue;

                    neighborNode.RHSCost = newRHS;
                    _openList.Update(neighborNode);
                }
            }

            return GetShortestPath();
        }

        List<Vector3> GetShortestPath()
        {
            var path = new List<Vector3>();
            var current = _start;

            while (current.NodeID != _end.NodeID)
            {
                path.Add(current.Position);
                current = current.Neighbors.OrderBy(n => _nodes[n.Position].GCost).First();
            }

            path.Add(_end.Position);
            return path;
        }
    }

    public class ExampleUseClassFlowPath
    {
        Octree_Map _tempOctreeMap;
        Octree_Paths _tempOctreePaths;
        DStarLite _dStarLite;
        List<Vector3> _haulerPath;
        Vector3 _haulerPos, _logPilePos;

        public void OnActionPerformed(Vector3 treePos)
        {
            _tempOctreeMap ??= new Octree_Map(100);

            _tempOctreeMap.RemoveObstacle(treePos);
            _tempOctreePaths = new Octree_Paths(_tempOctreeMap);
            _dStarLite = new DStarLite(_tempOctreePaths, _haulerPos, _logPilePos);
            _haulerPath = _dStarLite.RunDStarLite();
        }
    }
}