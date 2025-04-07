using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.NavMesh
{
    public class NavRelevant : MonoBehaviour
    {
        BoxCollider _boxCollider;
        public BoxCollider BoxCollider => _boxCollider ??= GetComponent<BoxCollider>();

        public List<Vector3> GetNavRelevantPoints()
        {
            return GetCorners();
        }
        
        public List<Vector3> GetCorners()
        {
            var center = BoxCollider.center;
            var extents = BoxCollider.size / 2f;

            return new List<Vector3>
            {
                transform.TransformPoint(center + new Vector3(-extents.x, -extents.y, -extents.z)),
                transform.TransformPoint(center + new Vector3(-extents.x, -extents.y,  extents.z)),
                //transform.TransformPoint(center + new Vector3(-extents.x,  extents.y, -extents.z)),
                //transform.TransformPoint(center + new Vector3(-extents.x,  extents.y,  extents.z)),
                transform.TransformPoint(center + new Vector3( extents.x, -extents.y, -extents.z)),
                transform.TransformPoint(center + new Vector3( extents.x, -extents.y,  extents.z)),
                //transform.TransformPoint(center + new Vector3( extents.x,  extents.y, -extents.z)),
                //transform.TransformPoint(center + new Vector3( extents.x,  extents.y,  extents.z))
            };
            
            // Only 2D for now, can add once we add multi level movement.
        }
    }
}