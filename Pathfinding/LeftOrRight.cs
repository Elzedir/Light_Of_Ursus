using UnityEngine;

namespace Pathfinding
{
    public class LeftOrRight
    {

        public Transform youTrans;
        public Transform wayPointTrans;

        void Update()
        {
            //The direction you are facing
            Vector3 youDir = youTrans.forward;

            //The direction from you to the waypoint
            Vector3 waypointDir = wayPointTrans.position - youTrans.position;

            //The cross product between these vectors
            Vector3 crossProduct = Vector3.Cross(youDir, waypointDir);

            //The dot product between the your up vector and the cross product
            //This can be said to be a volume that can be negative
            float dotProduct = Vector3.Dot(crossProduct, youTrans.up);

            //Now we can decide if we should turn left or right
            if (dotProduct > 0f)
            {
                Debug.Log("Turn right");
            }
            else
            {
                Debug.Log("Turn left");
            }

        }
    }
}