using UnityEngine;

namespace Pathfinding
{
    public class PassedWaypoint
    {
        public Transform youTrans;
        public Transform waypointFromTrans;
        public Transform waypointToTrans;
        public Transform debugTrans;

        void Update()
        {
            //The vector between the character and the waypoint we are going from
            Vector3 a = youTrans.position - waypointFromTrans.position;

            //The vector between the waypoints
            Vector3 b = waypointToTrans.position - waypointFromTrans.position;

            //Vector projection from https://en.wikipedia.org/wiki/Vector_projection
            //To know if we have passed the upcoming waypoint we need to find out how much of b is a1
            //a1 = (a.b / |b|^2) * b
            //a1 = progress * b -> progress = a1 / b -> progress = (a.b / |b|^2)
            float progress = (a.x * b.x + a.y * b.y + a.z * b.z) / (b.x * b.x + b.y * b.y + b.z * b.z);

            //If progress is above 1 we know we have passed the waypoint
            if (progress > 1.0f)
            {
                Debug.Log("Change waypoint");
            }
            else
            {
                Debug.Log("Move on");
            }

            //Debug by adding a cube to where the closest point on the line between the waypoints from the character is
            debugTrans.position = progress * b + waypointFromTrans.position;
        }
    }
}