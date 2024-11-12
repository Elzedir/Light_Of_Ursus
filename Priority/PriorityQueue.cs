using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Priority
{
    public class PriorityQueue
    {
        int                            _currentPosition;
        PriorityValue[]                _priorityArray;
        readonly Dictionary<uint, int> _priorityQueue;

        public PriorityQueue(int maxPriorities)
        {
            _currentPosition = 0;
            _priorityArray   = new PriorityValue[maxPriorities];
            _priorityQueue   = new Dictionary<uint, int>();
        }

        public PriorityValue Peek(uint priorityID = 1)
        {
            if (priorityID == 1)
            {
                if (_currentPosition == 0) return null;

                return _priorityArray[1];
            }
            else
            {
                if (!_priorityQueue.TryGetValue(priorityID, out var index)) return null;

                return index == 0 ? null : _priorityArray[index];
            }
        }

        public PriorityValue[] PeekAll()
        {
            if (_currentPosition == 0) return null;

            return _priorityArray.Skip(1).ToArray();
        }

        public PriorityValue Dequeue(uint priorityID = 1)
        {
            if (priorityID == 1)
            {
                if (_currentPosition == 0) return null;

                PriorityValue priorityValue = _priorityArray[1];
                _priorityArray[1]                            = _priorityArray[_currentPosition];
                _priorityQueue[_priorityArray[1].PriorityID] = 1;
                _priorityQueue[priorityValue.PriorityID]     = 0;
                _currentPosition--;
                _moveDown(1);

                return priorityValue;
            }
            else
            {
                if (!_priorityQueue.TryGetValue(priorityID, out var index)) return null;
            
                if (index == 0) return null;

                PriorityValue priorityValue = _priorityArray[index];
                _priorityQueue[priorityID]                                  = 0;
                _priorityArray[index]                                       = _priorityArray[_currentPosition];
                _priorityQueue[_priorityArray[_currentPosition].PriorityID] = index;
                _currentPosition--;
                _moveDown(index);

                return priorityValue;
            }
        }

        bool _enqueue(uint priorityID, List<float> priorities)
        {
            if (_priorityQueue.TryGetValue(priorityID, out int index) && index != 0)
            {
                Debug.Log($"PriorityID: {priorityID} already exists in PriorityQueue.");

                if (!Update(priorityID, priorities))
                {
                    Debug.LogError($"PriorityID: {priorityID} unable to be updated.");
                    return false;
                }

                return true;
            }

            PriorityValue priorityValue = new PriorityValue(priorityID, priorities);
            _currentPosition++;
            _priorityQueue[priorityID] = _currentPosition;
            if (_currentPosition == _priorityArray.Length) Array.Resize(ref _priorityArray, _priorityArray.Length * 2);
            _priorityArray[_currentPosition] = priorityValue;
            _moveUp(_currentPosition);

            return true;
        }

        public bool Update(uint priorityID, List<float> newPriorities)
        {
            if (newPriorities.Count == 0)
            {
                if (!Remove(priorityID))
                {
                    Debug.LogError($"PriorityID: {priorityID} not found in PriorityQueue.");
                    return false;
                }
            
                return true;
            }

            if (!_priorityQueue.TryGetValue(priorityID, out int index) || index == 0)
            {
                if (!_enqueue(priorityID, newPriorities))
                {
                    Debug.LogError($"PriorityID: {priorityID} unable to be enqueued.");
                    return false;
                }

                return true;
            }
        
            PriorityValue priorityValueNew = new PriorityValue(priorityID, newPriorities);
            PriorityValue priorityValueOld = _priorityArray[index];

            _priorityArray[index] = priorityValueNew;

            if (priorityValueOld.CompareTo(priorityValueNew) < 0)
            {
                _moveDown(index);
            }
            else
            {
                _moveUp(index);
            }

            return true;
        }

        public bool Remove(uint priorityID)
        {
            if (!_priorityQueue.TryGetValue(priorityID, out int index) || index == 0)
            {
                Debug.LogError($"PriorityID: {priorityID} not found in PriorityQueue.");
                return false;
            }

            _priorityQueue[priorityID]                       = 0;
            _priorityArray[index]                            = _priorityArray[_currentPosition];
            _priorityQueue[_priorityArray[index].PriorityID] = index;
            _currentPosition--;
            _moveDown(index);

            return true;
        }

        void _moveDown(int index)
        {
            int childL = index * 2;

            if (childL > _currentPosition) return;

            int childR = index * 2 + 1;
            int largerChild;

            if (childR > _currentPosition)
            {
                largerChild = childL;
            }
            else if (_priorityArray[childL].CompareTo(_priorityArray[childR]) > 0)
            {
                largerChild = childL;
            }
            else
            {
                largerChild = childR;
            }

            if (_priorityArray[index].CompareTo(_priorityArray[largerChild]) >= 0) return;

            _swap(index, largerChild);
            _moveDown(largerChild);

        }

        void _moveUp(int index)
        {
            if (index == 1) return;
            int parent = index / 2;

            if (_priorityArray[parent].CompareTo(_priorityArray[index]) < 0)
            {
                _swap(parent, index);
                _moveUp(parent);
            }
        }

        void _swap(int indexA, int indexB)
        {
            PriorityValue tempPriorityValueA = _priorityArray[indexA];
            _priorityArray[indexA]                            = _priorityArray[indexB];
            _priorityQueue[_priorityArray[indexB].PriorityID] = indexA;
            _priorityArray[indexB]                            = tempPriorityValueA;
            _priorityQueue[tempPriorityValueA.PriorityID]     = indexB;
        }
    }

    public class PriorityValue
    {
        public readonly uint        PriorityID;
        public readonly List<float> AllPriorities;

        public PriorityValue(uint priorityID, List<float> priorities)
        {
            PriorityID    = priorityID;
            AllPriorities = new List<float>(priorities);
        }

        public int CompareTo(PriorityValue that)
        {
            for (int i = 0; i < Math.Min(AllPriorities.Count, that.AllPriorities.Count); i++)
            {
                if (AllPriorities[i]      < that.AllPriorities[i]) return -1;
                else if (AllPriorities[i] > that.AllPriorities[i]) return 1;
            }

            if (AllPriorities.Count      > that.AllPriorities.Count) return 1;
            else if (AllPriorities.Count < that.AllPriorities.Count) return -1;

            return 0;
        }
    }
}