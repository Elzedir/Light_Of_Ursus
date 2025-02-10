using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Tools;
using UnityEngine;

namespace Priorities
{
    public class Priority_Queue : Data_Class
    {
        int                            _currentPosition;
        Priority_Element[]              _priorityArray;
        readonly Dictionary<ulong, int> _lookupTable;

        public Action<ulong> OnPriorityRemoved;

        public Priority_Queue(int maxPriorities)
        {
            _currentPosition = 0;
            _priorityArray   = new Priority_Element[maxPriorities];
            _lookupTable   = new Dictionary<ulong, int>();
        }

        public Priority_Element Peek(ulong priorityID = 1)
        {
            if (_currentPosition == 0)
                return null;

            var index = priorityID == 1 ? 1 : _lookupTable.GetValueOrDefault(priorityID, 0);

            return index != 0 ? _priorityArray[index] : null;
        }

        public Priority_Element[] PeekAll()
        {
            if (_currentPosition == 0) return null;

            return _priorityArray
                .Skip(1)
                .Take(_currentPosition)
                .OrderByDescending(pe => pe.PriorityValue)
                .ToArray();
        }

        public Priority_Element Dequeue(ulong priorityID = 1)
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

        bool _enqueue(ulong priorityID, float priorityValue, Priority_Parameters priorityParameters)
        {
            if (_lookupTable.TryGetValue(priorityID, out var index) && index != 0)
            {
                Debug.Log($"PriorityID: {priorityID} already exists in PriorityQueue.");

                if (Update(priorityID, priorityValue, priorityParameters))
                    return true;

                Debug.LogError($"PriorityID: {priorityID} unable to be updated.");
                return false;

            }

            var priorityElement = new Priority_Element(priorityID, priorityValue, priorityParameters);
            _currentPosition++;
            _lookupTable[priorityID] = _currentPosition;
            
            if (_currentPosition == _priorityArray.Length)
                Array.Resize(ref _priorityArray, _priorityArray.Length * 2);
            
            _priorityArray[_currentPosition] = priorityElement;
            _moveUp(_currentPosition);

            return true;
        }

        public bool Update(ulong priorityID, float newPriority, Priority_Parameters priorityParameters)
        {
            if (!_lookupTable.TryGetValue(priorityID, out var index) || index == 0)
                return _enqueue(priorityID, newPriority, priorityParameters);

            _priorityArray[index].UpdatePriorityValue(newPriority, priorityParameters);

            if (index == _currentPosition || index == 1) return true;
            
            if (_priorityArray[index].PriorityValue >= _priorityArray[index / 2].PriorityValue)
                _moveUp(index);
            else
                _moveDown(index);
            
            return true;
        }

        public bool Remove(ulong priorityID)
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

    public class Priority_Element
    {
        public readonly ulong         PriorityID;
        public Priority_Parameters           PriorityParameters { get; private set; }
        public float PriorityValue { get; private set; }

        // We'll save this in case we need to exclude any values. Or Debug anything.

        public Priority_Element(ulong priorityID, float priorityValue, Priority_Parameters priorityParameters)
        {
            PriorityID       = priorityID;
            PriorityValue    = priorityValue;
            PriorityParameters = priorityParameters;
        }

        public void UpdatePriorityValue(float value, Priority_Parameters priorityParameters)
        {
            PriorityValue = value;
            PriorityParameters = priorityParameters;
        }
    }
}