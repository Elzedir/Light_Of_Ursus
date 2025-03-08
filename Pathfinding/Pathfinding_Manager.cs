using System.Collections.Generic;
using System.Linq;
using Pathfinding.NavMesh;
using UnityEngine;

namespace Pathfinding
{
    public abstract class Pathfinding_Manager
    {
        static readonly Graph_World _graph_World = new();
        static readonly Grid_Node _grid_Node = new();
        static readonly Graph_NavMesh _graph_NavMesh = new();

        public static List<Vector3> GetPath(Vector3 start, Vector3 end, HashSet<MoverType> moverTypes)
        {
            var worldPath = _graph_World.FindShortestPath(start, end);
            
            if (worldPath == null || worldPath.Count == 0)
                return null;
            
            if (worldPath.Count != 1) //* && if (!withinPlayerRenderRange) 
                return worldPath;
            
            var localStart = worldPath.Last();

            var localPath = moverTypes.Contains(MoverType.Air) || moverTypes.Contains(MoverType.Dig)
                ? _grid_Node.FindShortestPath(localStart, end)
                : _graph_NavMesh.FindShortestPath(localStart, end);

            return localPath;

            //* Instead of running DStarLite from start to end, instead run it from individual node to node, so it's limited
            //* in size per character. Also, pass this path through to each character, and their individual DStarLte pathfinders
            //* will navigate their small circles around them.
        }
    }
}