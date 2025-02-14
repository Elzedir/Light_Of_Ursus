using System;

namespace Priorities.Priority_Queues
{
    [Serializable]
    public class Priority_Element<T>
    {
        public ulong PriorityID;
        public float PriorityValue;
        public T PriorityObject;

        public Priority_Element(ulong priorityID, float priorityValue, T priorityObject)
        {
            PriorityID = priorityID;
            PriorityValue = priorityValue;
            PriorityObject = priorityObject;
        }

        public Priority_Element(Priority_Element<T> priority_Element)
        {
            PriorityID = priority_Element.PriorityID;
            PriorityValue = priority_Element.PriorityValue;
            PriorityObject = priority_Element.PriorityObject;
        }

        public void UpdatePriorityValue(float value) => PriorityValue = value;
    }
}