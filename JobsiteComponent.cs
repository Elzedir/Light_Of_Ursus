using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public abstract class JobsiteComponent : MonoBehaviour, ITickable
{
    public JobsiteData JobsiteData;
    public void SetJobsiteData(JobsiteData jobsiteData) => JobsiteData = jobsiteData;
    public void SetCityID(uint cityID) => JobsiteData.CityID = cityID;

    public List<StationComponent> AllStationsInJobsite;
    public List<EmployeePosition> AllCoreEmployeePositions;

    public PriorityQueue PriorityQueue;

    public bool JobsiteOpen = true;

    public float IdealRatio;
    public void SetIdealRatio(float idealRatio) => IdealRatio = idealRatio;
    public int PermittedProductionInequality = 10;
    public bool CustomerPresent = false;

    public void Initialise()
    {
        AllStationsInJobsite = GetAllStationsInJobsite();

        AllStationsInJobsite.ForEach(station => station.StationData.JobsiteID = JobsiteData.JobsiteID);

        Manager_TickRate.RegisterTickable(OnTick, TickRate.OneGameHour);
    }

    public virtual void OnTick()
    {
        foreach (var product in AllStationsInJobsite.Select(s => s.StationData.ProductionData))
        {
            product.GetEstimatedProductionRatePerHour();
            product.GetActualProductionRatePerHour();
        }

        _compareProductionOutput();
    }

    protected abstract bool _compareProductionOutput();

    protected abstract void _adjustProduction(float idealRatio);
    protected abstract VocationName _getRelevantVocation(EmployeePosition position);

    public void RefreshJobsite()
    {
        // Refresh all stations in jobsitedata
    }

    public List<StationComponent> GetAllStationsInJobsite() => GetComponentsInChildren<StationComponent>().ToList();

    public StationComponent GetNearestStationInJobsite(Vector3 position, StationName stationName)
    => AllStationsInJobsite
    .Where(station => station.StationName == stationName)
    .OrderBy(station => Vector3.Distance(position, station.transform.position))
    .FirstOrDefault();


    public List<EmployeePosition> GetMinimumEmployeePositions()
    {
        HashSet<EmployeePosition> employeePositions = new();

        foreach (var station in AllStationsInJobsite)
        {
            foreach (var position in station.AllowedEmployeePositions)
            {
                employeePositions.Add(position);
            }
        }

        return employeePositions.ToList();
    }

    protected virtual void _assignEmployeesToStations(List<uint> employeeIDs)
    {
        foreach (var station in AllStationsInJobsite)
        {
            station.RemoveAllOperators();
        }

        var tempEmployees = employeeIDs.Select(employeeID => Manager_Actor.GetActorData(employeeID)).ToList();

        foreach (var station in AllStationsInJobsite)
        {
            var allowedPositions = station.AllowedEmployeePositions;
            var employeesForStation = tempEmployees
                .Where(e => allowedPositions.Contains(e.CareerAndJobs.EmployeePosition))
                .OrderByDescending(e => e.CareerAndJobs.EmployeePosition)
                .ThenByDescending(e => e.VocationData.GetVocationExperience(_getRelevantVocation(e.CareerAndJobs.EmployeePosition)))
                .ToList();

            foreach (var employee in employeesForStation)
            {
                JobsiteData.AddEmployeeToStation(employee.ActorID, station.StationData.StationID);
                tempEmployees.Remove(employee);
            }

            if (tempEmployees.Count > 0)
            {
                Debug.Log($"Not all employees were assigned to stations. {tempEmployees.Count} employees left.");
            }
        }
    }

    protected virtual List<List<uint>> _getAllCombinations(List<uint> employees)
    {
        var result = new List<List<uint>>();
        int combinationCount = (int)Mathf.Pow(2, employees.Count);

        for (int i = 1; i < combinationCount; i++)
        {
            var combination = new List<uint>();
            for (int j = 0; j < employees.Count; j++)
            {
                if ((i & (1 << j)) != 0)
                {
                    combination.Add(employees[j]);
                }
            }
            result.Add(combination);
        }

        return result;
    }

    public StationComponent GetStationToFetch()
    {
        if (PriorityQueue == null) PriorityQueue = new PriorityQueue(AllStationsInJobsite.Count);

        foreach (var station in AllStationsInJobsite)
        {
            List<float> priorityValues = new();
            var itemsToHaul = station.GetInventoryItemsToHaul();

            var allItemPriorities = 0f;

            foreach (var item in itemsToHaul)
            {
                if (station.DesiredStoredItemIDs.Contains(item.ItemID))
                {
                    Debug.LogWarning($"Item: {item.ItemID} is allowed to be stored in {station.StationName} but is being hauled.");
                }

                var storagePriorities = Manager_Item.GetMasterItem(item.ItemID).PriorityStats_Item.Priority_StationForStorage;

                if (!storagePriorities.ContainsKey(station.StationName))
                {
                    // Debug.Log($"Station {station.StationName} not found in storage priorities.");
                    allItemPriorities += 0;
                }
                else
                {
                    var itemPriority = storagePriorities[station.StationName] * item.ItemAmount;
                    // Debug.Log($"Station {station.StationName} found in storage priorities.");
                    allItemPriorities += itemPriority;
                }

                // Debug.Log($"Priority value: {priorityValue}");
            }

            priorityValues.Add(allItemPriorities);

            // Debug.Log($"Adding station {station.StationName} to stationPriorities.");

            PriorityQueue.Enqueue(station.StationID, priorityValues);
        }

        // foreach(var stationPriority in stationPriorities)
        // {
        //     Debug.Log($"Station: {stationPriority.Station.StationName} Priority: {stationPriority.Priority.AllPriorities[0]}");
        // }

        return Manager_Station.GetStation(PriorityQueue.Dequeue().PriorityID);
    }

    public StationComponent GetStationToDeliver()
    {
        if (PriorityQueue == null) PriorityQueue = new PriorityQueue(AllStationsInJobsite.Count);

        foreach (var station in AllStationsInJobsite)
        {
            
        }

        return null;
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

public class PriorityQueue
{
    int _currentPosition;
    Priority[] _allPriorities;
    Dictionary<uint, int> _priorityQueue;

    public PriorityQueue(int maxPriorities)
    {
        _currentPosition = 0;
        _allPriorities = new Priority[maxPriorities];
        _priorityQueue = new Dictionary<uint, int>();
    }

    public Priority Peek(uint priorityID = 1)
    {
        if (priorityID == 1)
        {
            if (_currentPosition == 0) return null;

            return _allPriorities[1];
        }
        else
        {
            int index;

            if (!_priorityQueue.TryGetValue(priorityID, out index)) return null;

            return index == 0 ? null : _allPriorities[index];
        }
    }

    public Priority Dequeue(uint priorityID = 1)
    {
        if (priorityID == 1)
        {
            if (_currentPosition == 0) return null;

            Priority priority = _allPriorities[1];
            _allPriorities[1] = _allPriorities[_currentPosition];
            _priorityQueue[_allPriorities[1].PriorityID] = 1;
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

            Priority priority = _allPriorities[index];
            _priorityQueue[priorityID] = 0;
            _allPriorities[index] = _allPriorities[_currentPosition];
            _priorityQueue[_allPriorities[_currentPosition].PriorityID] = index;
            _currentPosition--;
            _moveDown(index);

            return priority;
        }
    }

    public bool Enqueue(uint priorityID, List<float> priorities)
    {
        if (_priorityQueue.TryGetValue(priorityID, out _))
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
        if (_currentPosition == _allPriorities.Length) Array.Resize<Priority>(ref _allPriorities, _allPriorities.Length * 2);
        _allPriorities[_currentPosition] = priority;
        _moveUp(_currentPosition);

        return true;
    }

    public bool Update(uint priorityID, List<float> priorities)
    {
        if (priorities.Count == 0)
        {
            if (!Remove(priorityID))
            {
                Debug.LogError($"PriorityID: {priorityID} not found in PriorityQueue.");
                return false;
            }
            
            return true;
        }

        int index;

        if (!_priorityQueue.TryGetValue(priorityID, out index))
        {
            if (!Enqueue(priorityID, priorities))
            {
                Debug.LogError($"PriorityID: {priorityID} unable to be enqueued.");
                return false;
            }

            return true;
        }

        if (index == 0)
        {
            Debug.LogError($"PriorityID: {priorityID} not found in PriorityQueue.");
            return false;
        }
        
        Priority priority_New = new Priority(priorityID, priorities);
        Priority priority_Old = _allPriorities[index];

        _allPriorities[index] = priority_New;

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
        int index;

        if (!_priorityQueue.TryGetValue(priorityID, out index))
        {
            return false;
        }

        if (index == 0)
        {
            Debug.LogError($"PriorityID: {priorityID} not found in PriorityQueue.");
            return false;
        }

        _priorityQueue[priorityID] = 0;
        _allPriorities[index] = _allPriorities[_currentPosition];
        _priorityQueue[_allPriorities[index].PriorityID] = index;
        _currentPosition--;
        _moveDown(index);

        return true;
    }

    void _moveDown(int index)
    {
        int childL = index * 2;

        if (childL > _currentPosition) return;

        int childR = index * 2 + 1;
        int smallerChild;

        if (childR > _currentPosition)
        {
            smallerChild = childL;
        }
        else if (_allPriorities[childL].CompareTo(_allPriorities[childR]) < 0)
        {
            smallerChild = childL;
        }
        else
        {
            smallerChild = childR;
        }

        if (_allPriorities[index].CompareTo(_allPriorities[smallerChild]) > 0)
        {
            _swap(index, smallerChild);
            _moveDown(smallerChild);
        }
    }

    void _moveUp(int index)
    {
        if (index == 1) return;
        int parent = index / 2;

        if (_allPriorities[parent].CompareTo(_allPriorities[index]) > 0)
        {
            _swap(parent, index);
            _moveUp(parent);
        }
    }

    void _swap(int indexA, int indexB)
    {
        Priority tempPriorityA = _allPriorities[indexA];
        _allPriorities[indexA] = _allPriorities[indexB];
        _priorityQueue[_allPriorities[indexB].PriorityID] = indexA;
        _allPriorities[indexB] = tempPriorityA;
        _priorityQueue[tempPriorityA.PriorityID] = indexB;
    }
}