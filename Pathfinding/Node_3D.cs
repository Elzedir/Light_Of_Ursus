using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Node_3D
    {
        ulong _id;
        public ulong ID => _id != 0 
            ? _id
            : _id = GetNodeIDFromPosition(Position);
        
        public Vector3 Position;
        public readonly List<Node_3D> Neighbors;

        public Node_3D(Vector3 position)
        {
            Position = position;
            Neighbors = new List<Node_3D>();
        }

        public static ulong GetNodeIDFromPosition(Vector3 position)
        {
            var x = Mathf.RoundToInt(position.x * 1000f);
            var y = Mathf.RoundToInt(position.y * 1000f);
            var z = Mathf.RoundToInt(position.z * 1000f);

            var hash = 14695981039346656037UL;
            
            hash = _fnv1aHashComponent(x, hash);
            hash = _fnv1aHashComponent(y, hash);
            hash = _fnv1aHashComponent(z, hash);

            return hash;
        }
        
        static ulong _fnv1aHashComponent(float component, ulong hash)
        {
            var bytes = System.BitConverter.GetBytes(component);
            
            foreach (var byt in bytes)
            {
                hash ^= byt;
                hash *= 1099511628211UL;
            }
            
            return hash;
        }
    }
}