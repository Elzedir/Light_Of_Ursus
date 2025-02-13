using System;

namespace Priorities.Priority_Queues
{
    [Serializable]
    public class Priority_Element<T>
    {
        public long PriorityID;
        public float PriorityValue;
        public T PriorityObject;

        public Priority_Element(long priorityID, float priorityValue)
        {
            PriorityID = priorityID;
            PriorityValue = priorityValue;
        }

        public void UpdatePriorityValue(float value) => PriorityValue = value;
    }
}