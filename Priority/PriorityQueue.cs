using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PriorityQueue
{
    int _currentPosition;
    Priority[] _priorityArray;
    Dictionary<uint, int> _priorityQueue;

    public PriorityQueue(int maxPriorities)
    {
        _currentPosition = 0;
        _priorityArray = new Priority[maxPriorities];
        _priorityQueue = new Dictionary<uint, int>();
    }

    public Priority Peek(uint priorityID = 1)
    {
        if (priorityID == 1)
        {
            if (_currentPosition == 0) return null;

            return _priorityArray[1];
        }
        else
        {
            int index;

            if (!_priorityQueue.TryGetValue(priorityID, out index)) return null;

            return index == 0 ? null : _priorityArray[index];
        }
    }

    public Priority[] PeekAll()
    {
        if (_currentPosition == 0) return null;

        return _priorityArray.Skip(1).ToArray();
    }

    public Priority Dequeue(uint priorityID = 1)
    {
        if (priorityID == 1)
        {
            if (_currentPosition == 0) return null;

            Priority priority = _priorityArray[1];
            _priorityArray[1] = _priorityArray[_currentPosition];
            _priorityQueue[_priorityArray[1].PriorityID] = 1;
            _priorityQueue[priority.PriorityID] = 0;
            _currentPosition--;
            _moveDown(1);

            return priority;
        }
        else
        {
            int index;

            if (!_priorityQueue.TryGetValue(priorityID, out index)) return null;
            
            if (index == 0) return null;

            Priority priority = _priorityArray[index];
            _priorityQueue[priorityID] = 0;
            _priorityArray[index] = _priorityArray[_currentPosition];
            _priorityQueue[_priorityArray[_currentPosition].PriorityID] = index;
            _currentPosition--;
            _moveDown(index);

            return priority;
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

        Priority priority = new Priority(priorityID, priorities);
        _currentPosition++;
        _priorityQueue[priorityID] = _currentPosition;
        if (_currentPosition == _priorityArray.Length) Array.Resize(ref _priorityArray, _priorityArray.Length * 2);
        _priorityArray[_currentPosition] = priority;
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
        
        Priority priority_New = new Priority(priorityID, newPriorities);
        Priority priority_Old = _priorityArray[index];

        _priorityArray[index] = priority_New;

        if (priority_Old.CompareTo(priority_New) < 0)
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

        _priorityQueue[priorityID] = 0;
        _priorityArray[index] = _priorityArray[_currentPosition];
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
        Priority tempPriorityA = _priorityArray[indexA];
        _priorityArray[indexA] = _priorityArray[indexB];
        _priorityQueue[_priorityArray[indexB].PriorityID] = indexA;
        _priorityArray[indexB] = tempPriorityA;
        _priorityQueue[tempPriorityA.PriorityID] = indexB;
    }
}

public class Priority
{
    public uint PriorityID;
    public List<float> AllPriorities;

    public Priority(uint priorityID, List<float> priorities)
    {
        PriorityID = priorityID;
        AllPriorities = new List<float>(priorities);
    }

    public int CompareTo(Priority that)
    {
        for (int i = 0; i < Math.Min(AllPriorities.Count, that.AllPriorities.Count); i++)
        {
            if (AllPriorities[i] < that.AllPriorities[i]) return -1;
            else if (AllPriorities[i] > that.AllPriorities[i]) return 1;
        }

        if (AllPriorities.Count > that.AllPriorities.Count) return 1;
        else if (AllPriorities.Count < that.AllPriorities.Count) return -1;

        return 0;
    }
}