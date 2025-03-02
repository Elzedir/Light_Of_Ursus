using UnityEngine;

namespace Pathfinding
{
    public class ControlSpeed : MonoBehaviour
    {
        [Range(0.01f, 1f)] public float Speed = 1f;
    }
}