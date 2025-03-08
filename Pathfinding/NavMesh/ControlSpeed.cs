using UnityEngine;

namespace Pathfinding.NavMesh
{
    public class ControlSpeed : MonoBehaviour
    {
        [Range(0.01f, 1f)] public float Speed = 1f;
    }
}