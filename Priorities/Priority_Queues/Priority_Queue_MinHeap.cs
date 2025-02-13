namespace Priorities.Priority_Queues
{
    public class Priority_Queue_MinHeap<T> : Priority_Queue<T> where T : class
    {
        public Priority_Queue_MinHeap(int maxSize = 10) : base(maxSize) { }
        
        protected override void _moveDown(int index)
        {
            {
                while (true)
                {
                    var childL = index * 2;

                    if (childL > _currentPosition) return;

                    var childR = index * 2 + 1;
                    int smallerChild;

                    if (childR > _currentPosition)
                    {
                        smallerChild = childL;
                    }
                    else if (_priorityArray[childL].PriorityValue <= _priorityArray[childR].PriorityValue)
                    {
                        smallerChild = childL;
                    }
                    else
                    {
                        smallerChild = childR;
                    }

                    if (_priorityArray[index].PriorityValue <= _priorityArray[smallerChild].PriorityValue) return;

                    _swap(index, smallerChild);
                    index = smallerChild;
                }
            }
        }

        protected override void _moveUp(int index)
        {
            while (true)
            {
                if (index == 1) return;

                var parent = index / 2;

                if (_priorityArray[parent].PriorityValue <= _priorityArray[index].PriorityValue) return;

                _swap(parent, index);
                index = parent;
            }
        }
    }
}