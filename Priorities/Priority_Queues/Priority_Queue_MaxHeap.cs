using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;

namespace Priorities.Priority_Queues
{
    public class Priority_Queue_MaxHeap<T> : Priority_Queue<T> where T : class
    {
        public Priority_Queue_MaxHeap(int maxSize = 10) : base(maxSize) { }
        
        protected override void _moveDown(int index)
        {
            {
                while (true)
                {
                    var childL = index * 2;

                    if (childL > _currentPosition) return;

                    var childR = index * 2 + 1;
                    int largerChild;

                    if (childR > _currentPosition)
                    {
                        largerChild = childL;
                    }
                    else if (_priorityArray[childL].PriorityValue >= _priorityArray[childR].PriorityValue)
                    {
                        largerChild = childL;
                    }
                    else
                    {
                        largerChild = childR;
                    }

                    if (_priorityArray[index].PriorityValue >= _priorityArray[largerChild].PriorityValue) return;

                    _swap(index, largerChild);
                    index = largerChild;
                }
            }
        }

        protected override void _moveUp(int index)
        {
            while (true)
            {
                if (index == 1) return;

                var parent = index / 2;

                if (_priorityArray[parent].PriorityValue >= _priorityArray[index].PriorityValue) return;

                _swap(parent, index);
                index = parent;
            }
        }
    }
}