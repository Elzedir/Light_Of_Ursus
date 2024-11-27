using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Priority
{
    public class PriorityQueue
    {
        int                            _currentPosition;
        PriorityElement[]              _priorityArray;
        readonly Dictionary<uint, int> _priorityQueue;

        public Action<uint> OnPriorityRemoved;

        public PriorityQueue(int maxPriorities)
        {
            _currentPosition = 0;
            _priorityArray   = new PriorityElement[maxPriorities];
            _priorityQueue   = new Dictionary<uint, int>();
        }

        public PriorityElement Peek(uint priorityID = 1)
        {
            if (priorityID == 1)
            {
                return _currentPosition == 0 ? null : _priorityArray[1];
            }
            else
            {
                if (!_priorityQueue.TryGetValue(priorityID, out var index)) return null;

                return index == 0 ? null : _priorityArray[index];
            }
        }

        public PriorityElement[] PeekAll()
        {
            return _currentPosition == 0 ? null : _priorityArray.Skip(1).ToArray();
        }

        public PriorityElement Dequeue(uint priorityID = 1)
        {
            if (_currentPosition == 0) return null;

            var index = priorityID == 1 ? 1 : _priorityQueue.GetValueOrDefault(priorityID, 0);
            if (index == 0) return null;

            var priorityValue = _priorityArray[index];
            _priorityQueue[priorityValue.PriorityID] = 0;

            if (index != _currentPosition)
            {
                _priorityArray[index]                            = _priorityArray[_currentPosition];
                _priorityQueue[_priorityArray[index].PriorityID] = index;
            }

            _currentPosition--;
            _moveDown(index);

            OnPriorityRemoved?.Invoke(priorityID);

            return priorityValue;
        }

        bool _enqueue(uint priorityID, Dictionary<PriorityParameterName, object> priorities)
        {
            if (_priorityQueue.TryGetValue(priorityID, out var index) && index != 0)
            {
                Debug.Log($"PriorityID: {priorityID} already exists in PriorityQueue.");

                if (Update(priorityID, priorities))
                    return true;

                Debug.LogError($"PriorityID: {priorityID} unable to be updated.");
                return false;

            }

            var priorityValue = new PriorityElement(priorityID, priorities);
            _currentPosition++;
            _priorityQueue[priorityID] = _currentPosition;
            if (_currentPosition == _priorityArray.Length)
                Array.Resize(ref _priorityArray, _priorityArray.Length * 2);
            _priorityArray[_currentPosition] = priorityValue;
            _moveUp(_currentPosition);

            return true;
        }

        public bool UpdateAll(Dictionary<PriorityParameterName, object> newPriorities)
        {
            foreach(var priority in _priorityQueue)
            {
                if (Update(priority.Key, newPriorities)) continue;
                
                Debug.LogError($"PriorityID: {priority.Key} unable to be updated.");
                return false;
            }
            
            return true;
        }

        public bool Update(uint priorityID, Dictionary<PriorityParameterName, object> newPriorities)
        {
            if (newPriorities.Count == 0)
                return Remove(priorityID);

            if (!_priorityQueue.TryGetValue(priorityID, out var index) || index == 0)
                return _enqueue(priorityID, newPriorities);

            foreach (var priority in newPriorities)
            {
                _priorityArray[index].UpdatePriority(priority.Key, (float)priority.Value);
            }

            if (index == _currentPosition) return true;
            
            if (_priorityArray[index].PriorityValue >= _priorityArray[index / 2].PriorityValue)
            {
                _moveDown(index);
            }
            else
            {
                _moveUp(index);
            }

            return true;
        }

        public bool Replace(uint priorityID, Dictionary<PriorityParameterName, object> newPriorities)
        {
            if (newPriorities.Count == 0)
                return Remove(priorityID);

            if (!_priorityQueue.TryGetValue(priorityID, out var index) || index == 0)
                return _enqueue(priorityID, newPriorities);

            var priorityValueNew = new PriorityElement(priorityID, newPriorities);
            var priorityValueOld = _priorityArray[index];

            _priorityArray[index] = priorityValueNew;

            if (priorityValueOld.PriorityValue >= priorityValueNew.PriorityValue)
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
            if (!_priorityQueue.TryGetValue(priorityID, out var index) || index == 0)
                return false;

            if (index != _currentPosition)
            {
                _priorityArray[index]                            = _priorityArray[_currentPosition];
                _priorityQueue[_priorityArray[index].PriorityID] = index;
            }

            _currentPosition--;
            _moveDown(index);

            OnPriorityRemoved?.Invoke(priorityID);

            return true;
        }

        void _moveDown(int index)
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
            _moveDown(largerChild);

        }

        void _moveUp(int index)
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

        void _swap(int indexA, int indexB)
        {
            var tempPriorityValueA = _priorityArray[indexA];
            _priorityArray[indexA]                            = _priorityArray[indexB];
            _priorityQueue[_priorityArray[indexB].PriorityID] = indexA;
            _priorityArray[indexB]                            = tempPriorityValueA;
            _priorityQueue[tempPriorityValueA.PriorityID]     = indexB;
        }
    }

    public class PriorityElement
    {
        public readonly uint PriorityID;
        bool _allPrioritiesChanged;

        float _priorityValue;
        public float PriorityValue
        {
            get
            {
                if (!_allPrioritiesChanged) return _priorityValue;
                
                _allPrioritiesChanged = false;
                return _priorityValue  = AllPriorities.Values.Sum(x => (float)x);
            }
        }
        public readonly Dictionary<PriorityParameterName, object> AllPriorities;

        public PriorityElement(uint priorityID, Dictionary<PriorityParameterName, object> allPriorities)
        {
            PriorityID    = priorityID;
            AllPriorities = new Dictionary<PriorityParameterName, object>(allPriorities);
        }

        public void UpdatePriority(PriorityParameterName priorityParameterName, float value)
        {
            AllPriorities[priorityParameterName] = value;
            _allPrioritiesChanged                = true;
        }
    }
}