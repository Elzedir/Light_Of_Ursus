using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;

namespace Priority
{
    public class Priority_Queue : Data_Class
    {
        int                            _currentPosition;
        PriorityElement[]              _priorityArray;
        readonly Dictionary<uint, int> _priorityQueue;

        public Action<uint> OnPriorityRemoved;

        public Priority_Queue(int maxPriorities)
        {
            _currentPosition = 0;
            _priorityArray   = new PriorityElement[maxPriorities];
            _priorityQueue   = new Dictionary<uint, int>();
        }

        public PriorityElement Peek(uint priorityID = 1)
        {
            if (_currentPosition == 0)
                return null;

            var index = priorityID == 1 ? 1 : _priorityQueue.GetValueOrDefault(priorityID, 0);

            return index == 0 ? null : _priorityArray[index];
        }

        public PriorityElement[] PeekAll()
        {
            return _currentPosition == 0 ? null : _priorityArray.Skip(1).ToArray();
        }

        public PriorityElement Dequeue(uint priorityID = 1)
        {
            if (_currentPosition == 0)
            {
                Debug.Log($"Priority Queue should be empty: {_priorityQueue.Count} since _currentPosition: {_currentPosition}.");
                return null;
            }

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

        bool _enqueue(uint priorityID, float priority)
        {
            if (_priorityQueue.TryGetValue(priorityID, out var index) && index != 0)
            {
                Debug.Log($"PriorityID: {priorityID} already exists in PriorityQueue.");

                if (Update(priorityID, priority))
                    return true;

                Debug.LogError($"PriorityID: {priorityID} unable to be updated.");
                return false;

            }

            var priorityValue = new PriorityElement(priorityID, priority);
            _currentPosition++;
            _priorityQueue[priorityID] = _currentPosition;
            if (_currentPosition == _priorityArray.Length)
                Array.Resize(ref _priorityArray, _priorityArray.Length * 2);
            _priorityArray[_currentPosition] = priorityValue;
            _moveUp(_currentPosition);

            return true;
        }

        public bool Update(uint priorityID, float newPriority)
        {
            if (!_priorityQueue.TryGetValue(priorityID, out var index) || index == 0)
                return _enqueue(priorityID, newPriority);

            _priorityArray[index].UpdatePriority(newPriority);

            if (index == _currentPosition || index == 1) return true;

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

        public override Dictionary<string, string> GetStringData()
        {
            var stringData = new Dictionary<string, string>();
            
            foreach(var priority in _priorityArray)
            {
                if (priority is null) continue;

                var iteration = 0;
                
                while (stringData.ContainsKey($"PriorityID({iteration}) - {priority.PriorityID}") && iteration < 4)
                {
                    Debug.LogError($"PriorityID({iteration}) - {priority.PriorityID} already exists.");
                    iteration++;
                }
                
                if (iteration >= 4)
                {
                    Debug.LogError("PriorityID iteration limit reached.");
                    continue;
                }
                
                stringData.Add($"PriorityID({iteration}) - {priority.PriorityID}", $"PriorityValue - {priority.PriorityValue}");
            }
            
            return stringData;
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Priority Queue",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }
    }

    public class PriorityElement
    {
        public readonly uint         PriorityID;
        public float PriorityValue { get; private set; }

        // We'll save this in case we need to exclude any values. Or Debug anything.

        public PriorityElement(uint priorityID, float priorityValue)
        {
            PriorityID       = priorityID;
            PriorityValue    = priorityValue;
        }

        public void UpdatePriority(float value)
        {
            PriorityValue = value;
        }
    }
}