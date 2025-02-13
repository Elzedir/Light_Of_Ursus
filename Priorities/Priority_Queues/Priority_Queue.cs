using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;

namespace Priorities.Priority_Queues
{
    public abstract class Priority_Queue<T> : Data_Class where T : class
    {
        protected int                            _currentPosition;
        protected Priority_Element<T>[]              _priorityArray;
        protected readonly Dictionary<long, int> _lookupTable;

        public Action<long> OnPriorityRemoved;

        public Priority_Queue(int maxPriorities)
        {
            _currentPosition = 0;
            _priorityArray   = new Priority_Element<T>[maxPriorities];
            _lookupTable   = new Dictionary<long, int>();
        }

        public Priority_Element<T> Peek(long priorityID = 1)
        {
            if (_currentPosition == 0)
                return null;

            var index = priorityID == 1 ? 1 : _lookupTable.GetValueOrDefault(priorityID, 0);

            return index != 0 ? _priorityArray[index] : null;
        }

        public Priority_Element<T>[] PeekAll()
        {
            if (_currentPosition == 0) return null;

            return _priorityArray
                .Skip(1)
                .Take(_currentPosition)
                .OrderByDescending(pe => pe.PriorityValue)
                .ToArray();
        }

        public Priority_Element<T> Dequeue(long priorityID = 1)
        {
            if (_currentPosition == 0)
            {
                Debug.Log($"Priority Queue should be empty: {_lookupTable.Count} since _currentPosition: {_currentPosition}.");
                return null;
            }

            var index = priorityID == 1 ? 1 : _lookupTable.GetValueOrDefault(priorityID, 0);
            if (index == 0) return null;

            var priorityValue = _priorityArray[index];
            _lookupTable[priorityValue.PriorityID] = 0;

            if (index != _currentPosition)
            {
                _priorityArray[index]                            = _priorityArray[_currentPosition];
                _lookupTable[_priorityArray[index].PriorityID] = index;
            }

            _currentPosition--;
            _moveDown(index);

            OnPriorityRemoved?.Invoke(priorityID);

            return priorityValue;
        }

        bool _enqueue(long priorityID, float priorityValue)
        {
            if (_lookupTable.TryGetValue(priorityID, out var index) && index != 0)
            {
                Debug.Log($"PriorityID: {priorityID} already exists in PriorityQueue.");

                if (Update(priorityID, priorityValue))
                    return true;

                Debug.LogError($"PriorityID: {priorityID} unable to be updated.");
                return false;

            }

            var priorityElement = new Priority_Element<T>(priorityID, priorityValue);
            _currentPosition++;
            _lookupTable[priorityID] = _currentPosition;
            
            if (_currentPosition == _priorityArray.Length)
                Array.Resize(ref _priorityArray, _priorityArray.Length * 2);
            
            _priorityArray[_currentPosition] = priorityElement;
            _moveUp(_currentPosition);

            return true;
        }

        public bool Update(long priorityID, float newPriority)
        {
            if (!_lookupTable.TryGetValue(priorityID, out var index) || index == 0)
                return _enqueue(priorityID, newPriority);

            _priorityArray[index].UpdatePriorityValue(newPriority);

            if (index == _currentPosition || index == 1) return true;
            
            if (_priorityArray[index].PriorityValue >= _priorityArray[index / 2].PriorityValue)
                _moveUp(index);
            else
                _moveDown(index);
            
            return true;
        }

        public bool Contains(long priorityID) => _lookupTable.ContainsKey(priorityID);
        public int Count() => _currentPosition;

        public bool Remove(long priorityID)
        {
            if (!_lookupTable.TryGetValue(priorityID, out var index) || index == 0)
                return false;

            if (index != _currentPosition)
            {
                _priorityArray[index]                            = _priorityArray[_currentPosition];
                _lookupTable[_priorityArray[index].PriorityID] = index;
            }

            _currentPosition--;
            _moveDown(index);

            OnPriorityRemoved?.Invoke(priorityID);

            return true;
        }

        protected abstract void _moveDown(int index);
        protected abstract void _moveUp(int index);

        protected void _swap(int indexA, int indexB)
        {
            var tempPriorityValueA = _priorityArray[indexA];
            _priorityArray[indexA]                            = _priorityArray[indexB];
            _lookupTable[_priorityArray[indexB].PriorityID] = indexA;
            _priorityArray[indexB]                            = tempPriorityValueA;
            _lookupTable[tempPriorityValueA.PriorityID]     = indexB;
        }

        public override Dictionary<string, string> GetStringData()
        {
            var stringData = new Dictionary<string, string>();
            
            foreach(var priority in PeekAll())
            {
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
}