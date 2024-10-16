using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

public enum StationType
{
    None,
    Resource,
    Crafter,
    Storage
}

[RequireComponent(typeof(BoxCollider))]
public abstract class StationComponent : MonoBehaviour, IInteractable, ITickable
{
    bool _initialised = false;

    public StationData StationData;
    public abstract StationName StationName { get; }
    public abstract StationType StationType { get; }
    public bool IsStationBeingOperated { get { return AllOperatingAreasInStation.Any(oa => oa.OperatingAreaData.CurrentOperatorID != 0); } }

    public float InteractRange { get; protected set; }

    public List<EmployeePosition> AllowedEmployeePositions;
    public abstract EmployeePosition CoreEmployeePosition { get; }
    public abstract RecipeName DefaultProduct { get; }
    public abstract  List<RecipeName> AllowedRecipes { get; }
    public abstract List<uint> AllowedStoredItemIDs { get; }
    public abstract uint OperatingAreaCount { get; }
    public List<OperatingAreaComponent> AllOperatingAreasInStation = new();

    float _baseProgressRatePerHour = 1;
    List<Item> _currentProductsCrafted = new();

    BoxCollider _boxCollider;
    public BoxCollider BoxCollider { get { return _boxCollider ??= gameObject.GetComponent<BoxCollider>(); } }

    // Temporary
    public Transform CollectionPoint;

    public Dictionary<(uint ActorID, int OrderID), Order_Base> CurrentOrders = new();
    public Dictionary<OrderType, Order_Request> OrderRequests = new();
    public Order_Request GetOrderRequest(OrderType orderType) => OrderRequests.ContainsKey(orderType) ? OrderRequests[orderType] : null;
    public void AddOrderRequest(OrderType orderType, Order_Request orderRequest)
    {
        if (OrderRequests.ContainsKey(orderType))
        {
            OrderRequests[orderType] = orderRequest;
        }
        else
        {
            OrderRequests.Add(orderType, orderRequest);
        }
    }
    public void RemoveOrderRequest(OrderType orderType) => OrderRequests.Remove(orderType);

    public void AddOperatorToArea(uint operatorData)
    {
        var openOperatingArea = AllOperatingAreasInStation.FirstOrDefault(area => !area.OperatingAreaData.HasOperator());
        
        if (openOperatingArea != null)
        {
            openOperatingArea.OperatingAreaData.AddOperatorToOperatingArea(operatorData);
        }
        else
        {
            Debug.Log($"No open operating areas found in all operating areas.");
        }
    }

    public void RemoveOperatorFromArea(uint operatorID)
    {
        if (AllOperatingAreasInStation.Any(area => area.OperatingAreaData.CurrentOperatorID == operatorID))
        {
            AllOperatingAreasInStation.FirstOrDefault(area => area.OperatingAreaData.CurrentOperatorID == operatorID).OperatingAreaData.RemoveOperatorFromOperatingArea();
        }
        else
        {
            Debug.Log($"Operator {operatorID} not found in operating areas.");
        }
    }

    public void RemoveAllOperators()
    {
        foreach(var operatingArea in AllOperatingAreasInStation)
        {
            operatingArea.OperatingAreaData.RemoveOperatorFromOperatingArea();
        }
    }

    public TickRate GetTickRate()
    {
        return TickRate.OneSecond;
    }

    public virtual void OnTick()
    {
        if (!_initialised) return;

        // Change the actor check so that an actor with no knowledge of the recipe cannot operate the station if they are AI.
        // Change the has materials to instead also include delivering materials, so instead make it part of the operation process.
        // Change material check to only happen when operator is set or leaves, or when material is used.

        if (!StationData.StationIsActive)
        {
            Debug.Log($"StationIsActive: {StationData.StationIsActive}");
            return;
        }

        if (!IsStationBeingOperated)
        {
            Debug.Log($"IsStationBeingOperated: {IsStationBeingOperated}");

            // Trying a new job system instead

            // if (GetOrderRequest(OrderType.Hire) == null)
            // {
            //     //  This won't do anything for now, but we'll use it later on.
            //     OrderRequests.Add(OrderType.Hire, new Order_Request_Hire(StationData.StationID, new List<EmployeePosition> { CoreEmployeePosition }));
            // }
            // else
            // {
            //     var orderRequest = GetOrderRequest(OrderType.Hire) as Order_Request_Hire;

            //     if (!orderRequest.DesiredEmployeePositions.Contains(CoreEmployeePosition))
            //     {
            //         orderRequest.DesiredEmployeePositions.Add(CoreEmployeePosition);
            //     }
            //     else
            //     {
            //         Debug.Log($"DesiredEmployeePositions already contains CoreEmployeePosition: {CoreEmployeePosition}");
            //     }
            // }

            return;
        }

        if (!StationData.InventoryData.InventoryContainsAllItems(StationData.StationProgressData.CurrentProduct.RequiredIngredients))
        {
            Debug.Log($"InventoryContainsAllItems: false for station {StationData.StationID}");

            // Trying a new job system instead
            
            // if (GetOrderRequest(OrderType.Haul_Deliver) == null)
            // {
            //     OrderRequests.Add(OrderType.Haul_Deliver, new Order_Request_Haul_Deliver(StationData.StationID, GetInventoryItemsToHaul()));
            // }
            // else
            // {
            //     var orderRequest = GetOrderRequest(OrderType.Haul_Deliver) as Order_Request_Haul_Deliver;
                
            //     var existingItemIDs = orderRequest.Items.Select(i => i.ItemID).ToList();
            //     var missingItems = StationData.InventoryData.InventoryMissingItems(StationData.StationProgressData.CurrentProduct.RequiredIngredients);

            //     foreach (var missingItem in missingItems)
            //     {
            //         if (!existingItemIDs.Contains(missingItem.ItemID))
            //         {
            //             orderRequest.Items.Add(missingItem);
            //         }
            //         else
            //         {
            //             var existingItem = orderRequest.Items.FirstOrDefault(i => i.ItemID == missingItem.ItemID);

            //             if (existingItem.ItemAmount < missingItem.ItemAmount)
            //             {
            //                 existingItem.ItemAmount += missingItem.ItemAmount;
            //             }
            //             else
            //             {
            //                 Debug.Log($"Items already contains enough of missing item {missingItem.ItemID}. Waiting for hauler to deliver.");
            //             }
            //         }
            //     }
            // }
            return;
        }

        _operateStation();
    }

    protected virtual void _operateStation()
    {
        if (StationData.StationProgressData.CurrentProduct.RecipeName == RecipeName.None && DefaultProduct != RecipeName.None)
        {
            Debug.Log($"CurrentProduct is None: {StationData.StationProgressData.CurrentProduct}");

            if (Manager_Jobsite.GetJobsiteData(StationData.JobsiteID).JobsiteFactionID != 1)
            {
                Debug.Log($"Setting CurrentProduct to DefaultProduct: {DefaultProduct}");

                StationData.StationProgressData.CurrentProduct = Manager_Recipe.GetRecipe(DefaultProduct);
            }
        }

        foreach (var operatingArea in AllOperatingAreasInStation)
        {
            var progressMade = operatingArea.Operate(_baseProgressRatePerHour, StationData.StationProgressData.CurrentProduct);
            var itemCrafted = StationData.StationProgressData.Progress(progressMade);

            if (!itemCrafted) continue;

            // For now is the final person who adds the last progress, but change to a cumulative system later.
            CraftItem(
                StationData.StationProgressData.CurrentProduct.RecipeName, 
                Manager_Actor.GetActor(operatingArea.OperatingAreaData.CurrentOperatorID)
                );
        }
    }

    public void Initialise()
    {
        AllOperatingAreasInStation = GetAllOperatingAreasInStation();

        var employeeOperatingAreaPairs = from operatingArea in AllOperatingAreasInStation
                                         from employeeID in StationData.CurrentOperatorIDs
                                         let actorData = Manager_Actor.GetActorData(employeeID)
                                         where actorData.CareerAndJobs.OperatingAreaID == operatingArea.OperatingAreaData.OperatingAreaID
                                         select new { operatingArea, employeeID };

        foreach (var pair in employeeOperatingAreaPairs)
        {
            pair.operatingArea.OperatingAreaData.AddOperatorToOperatingArea(pair.employeeID);
        }

        SetInteractRange();
        InitialiseAllowedEmployeePositions();
        InitialiseStartingInventory();

        Manager_TickRate.RegisterTickable(this);

        _initialised = true;
    }

    public OperatingAreaComponent GetOperatingArea(int operatingAreaID)
    {
        return AllOperatingAreasInStation.FirstOrDefault(oa => oa.OperatingAreaData.OperatingAreaID == operatingAreaID);
    }

    public List<OperatingAreaComponent> GetAllOperatingAreasInStation()
    {
        foreach(Transform child in transform)
        {
            if (child.name.Contains("OperatingArea") || child.name.Contains("CollectionPoint"))
            {
                Destroy(child);
            }
        }

        var operatingAreas = new List<OperatingAreaComponent>();

        for (uint i = 1; i <= OperatingAreaCount; i++)
        {
            operatingAreas.Add(_createOperatingArea(i));
        }

        CollectionPoint = new GameObject("CollectionPoint").transform;
        CollectionPoint.SetParent(transform);
        CollectionPoint.localPosition = new Vector3(0, 0, -2);

        return operatingAreas;
    }

    protected abstract OperatingAreaComponent _createOperatingArea(uint operatingAreaID);

    public abstract void InitialiseAllowedEmployeePositions();

    public abstract void InitialiseStartingInventory();
    public abstract List<Item> GetInventoryItemsToHaul();

    public void SetStationData(StationData stationData) => StationData = stationData;

    public void SetInteractRange(float interactRange = 2)
    {
        InteractRange = interactRange;
    }

    public bool WithinInteractRange(ActorComponent interactor)
    {
        return Vector3.Distance(interactor.transform.position, transform.position) < InteractRange;
    }

    public abstract IEnumerator Interact(ActorComponent actor);

    public abstract void CraftItem(RecipeName recipeName, ActorComponent actor);

    public abstract List<Item> GetItemsToDropOff(IInventoryOwner inventoryOwner);

    protected abstract List<Item> _getCost(List<Item> ingredients, ActorComponent actor);

    protected abstract List<Item> _getYield(List<Item> ingredients, ActorComponent actor);

    public virtual bool AddItemsToStation(List<Item> items)
    {
        var added = StationData.InventoryData.AddToInventory(items);
        return added;
    }

    public virtual bool RemoveItemsFromStation(List<Item> items)
    {
        var removed = StationData.InventoryData.RemoveFromInventory(items);
        return removed;
    }

    protected virtual void _onCraftItem(List<Item> craftedItems)
    {
        _currentProductsCrafted.AddRange(craftedItems);
    }

    public virtual List<Item> GetActualProductionRatePerHour()
    {
        var currentProductsCrafted = new List<Item>(_currentProductsCrafted);
        return currentProductsCrafted;
    }

    public virtual List<Item> GetEstimatedProductionRatePerHour()
    {
        float totalProductionRate = 0;
        // Then modify production rate by any area modifiers (Land type, events, etc.)

        foreach (var currentOperatorID in StationData.CurrentOperatorIDs)
        {
            var individualProductionRate = _baseProgressRatePerHour;

            foreach(var vocation in StationData.StationProgressData.CurrentProduct.RequiredVocations)
            {
                individualProductionRate *= Manager_Actor.GetActorData(currentOperatorID).VocationData.GetProgress(vocation);
            }

            totalProductionRate += individualProductionRate;
            // Don't forget to add in estimations for travel time.
        }

        float requiredProgress = StationData.StationProgressData.CurrentProduct.RequiredProgress;
        float estimatedProductionCount = totalProductionRate > 0 ? totalProductionRate / requiredProgress : 0;

        var estimatedProductionItems = new List<Item>();

        for (int i = 0; i < Mathf.FloorToInt(estimatedProductionCount); i++)
        {
            foreach(var item in StationData.StationProgressData.CurrentProduct.RecipeProducts)
            {
                estimatedProductionItems.Add(new Item(item));
            }
        }

        return estimatedProductionItems;
    }

    protected List<StationComponent> _getAllStationsToHaulTo(ActorComponent actor)
    {
        var stationsToHaulTo = new List<StationComponent>();

        var jobsite = Manager_Jobsite.GetJobsite(StationData.JobsiteID);
        var actorInventory = actor.ActorData.InventoryAndEquipment.InventoryData;

        foreach (var station in jobsite.AllStationsInJobsite)
        {
            if (station.AllowedStoredItemIDs.Count == 0) continue;

            if (actorInventory.InventoryContainsAnyItems(station.AllowedStoredItemIDs))
            {
                stationsToHaulTo.Add(station);
                break;
            }
        }

        return stationsToHaulTo;
    }

    protected List<StationComponent> _getAllStationsToHaulFrom()
    {
        var stationsToHaulFrom = new List<StationComponent>();
        
        var jobsite = Manager_Jobsite.GetJobsite(StationData.JobsiteID);

        var PriorityQueue_Station = new PriorityQueue_Station(jobsite.AllStationsInJobsite.Count);

        foreach (var station in jobsite.AllStationsInJobsite)
        {
            if (station.GetInventoryItemsToHaul().Count <= 0)
            {
                continue;
            }

            stationsToHaulFrom.Add(station);
        }

        return stationsToHaulFrom;
    }

    protected List<StationComponent> _prioritiseStations(List<StationComponent> stations, List<uint> itemIDs)
    {
        var stationPriorities = new List<StationPriority>();

        foreach (var station in stations)
        {
            foreach (var itemID in itemIDs)
            {
                if (!station.AllowedStoredItemIDs.Contains(itemID)) continue;

                var priorityValue = 0.0;
                var storagePriorities = Manager_Item.GetMasterItem(itemID).PriorityStats_Item.Priority_StationForStorage;

                if (!storagePriorities.ContainsKey(station.StationName))
                {
                    priorityValue = 0;
                }
                else
                {
                    priorityValue += storagePriorities[station.StationName];
                }

                stationPriorities.Add(new StationPriority (station, new Priority(priorityValue)));
            }
        }

        stationPriorities.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        return stationPriorities.Select(sp => sp.Station).ToList();
    }
}

[CustomEditor(typeof(StationComponent), true)]
public class StationComponent_Editor : Editor
{
    bool _showBasicInfo = false;
    bool _showProductionItems = false;
    bool _showInventory = false;
    bool _showOperators = false;
    bool _showProgress = false;
    bool _showRecipe = false;
    Vector2 _productionItemScrollPos;
    Vector2 _inventoryItemScrollPos;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspectorExcept(nameof(StationComponent.StationData), nameof(StationComponent.AllowedEmployeePositions), nameof(StationComponent.AllowedRecipes));

        StationComponent stationComponent = (StationComponent)target;
        StationData stationData = stationComponent.StationData;

        if (stationData == null) return;

        EditorGUILayout.LabelField("Station Data", EditorStyles.boldLabel);

        _showBasicInfo = EditorGUILayout.Toggle("Basic Info", _showBasicInfo);

        if (_showBasicInfo)
        {
            EditorGUILayout.LabelField("StationID", stationData.StationID.ToString());
            //EditorGUILayout.LabelField("StationType", stationData.StationType.ToString());
            //EditorGUILayout.LabelField("StationName", stationData.StationName.ToString());
            EditorGUILayout.LabelField("JobsiteID", stationData.JobsiteID.ToString());
            EditorGUILayout.LabelField("StationIsActive", stationData.StationIsActive.ToString());
            EditorGUILayout.LabelField("StationDescription", stationData.StationDescription);
        }

        if (stationData.ProductionData != null)
        {
            _showProductionItems = EditorGUILayout.Toggle("All Produced Items", _showProductionItems);

            if (_showProductionItems)
            {
                _productionItemScrollPos = EditorGUILayout.BeginScrollView(_productionItemScrollPos);

                foreach (var item in stationData.ProductionData.AllProducedItems)
                {
                    EditorGUILayout.LabelField(item.ItemName.ToString());
                }

                EditorGUILayout.EndScrollView();
            }
        }

        if (stationData.InventoryData != null)
        {
            _showInventory = EditorGUILayout.Toggle("Inventory", _showInventory);

            if (_showInventory)
            {
                _inventoryItemScrollPos = EditorGUILayout.BeginScrollView(_inventoryItemScrollPos);

                foreach (var item in stationData.InventoryData.AllInventoryItems)
                {
                    EditorGUILayout.LabelField($"{item.ItemID}: {item.ItemName} Qty: {item.ItemAmount}");
                }

                EditorGUILayout.EndScrollView();
            }
        }

        if (stationData.CurrentOperatorIDs != null)
        {
            _showOperators = EditorGUILayout.Toggle("Current Operators", _showOperators);
            if (_showOperators)
            {
                foreach (var operatorID in stationData.CurrentOperatorIDs)
                {
                    EditorGUILayout.LabelField($"Operator: {operatorID}");
                    //EditorGUILayout.LabelField($"Operator: {operatorID.ActorID}: {operatorID.ActorName.GetName()} Pos: {operatorID.CareerAndJobs.EmployeePosition}");
                }
            }

        }

        if (stationData.StationProgressData != null)
        {
            _showProgress = EditorGUILayout.Toggle("Progress", _showProgress);

            if (_showProgress)
            {
                EditorGUILayout.LabelField("CurrentProgress", stationData.StationProgressData.CurrentProgress.ToString());
                EditorGUILayout.LabelField("CurrentQuality", stationData.StationProgressData.CurrentQuality.ToString());

                _showRecipe = EditorGUILayout.Toggle("Current Recipe", _showRecipe);

                if (_showRecipe)
                {
                    EditorGUILayout.LabelField("RecipeName", stationData.StationProgressData.CurrentProduct.RecipeName.ToString());
                    EditorGUILayout.LabelField("RequiredProgress", stationData.StationProgressData.CurrentProduct.RequiredProgress.ToString());
                    foreach (var ingredient in stationData.StationProgressData.CurrentProduct.RequiredIngredients)
                    {
                        EditorGUILayout.LabelField($"Ingredient: {ingredient.ItemID}: {ingredient.ItemName} Qty: {ingredient.ItemAmount}");
                    }
                    foreach (var product in stationData.StationProgressData.CurrentProduct.RecipeProducts)
                    {
                        EditorGUILayout.LabelField($"Product: {product.ItemID}: {product.ItemName} Qty: {product.ItemAmount}");
                    }
                    foreach (var vocations in stationData.StationProgressData.CurrentProduct.RequiredVocations)
                    {
                        EditorGUILayout.LabelField($"Vocation: {vocations.VocationName}");
                    }
                }
            }
        }
    }

    private void DrawDefaultInspectorExcept(params string[] propertyNames)
    {
        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;
        while (property.NextVisible(enterChildren))
        {
            if (!propertyNames.Contains(property.name))
            {
                EditorGUILayout.PropertyField(property, true);
            }
            enterChildren = false;
        }
    }
}

[Serializable]
public class Operator
{
    public int OperatorID;
    public string OperatorName;
    public EmployeePosition OperatorPosition;
    public int OperatingAreaID;

    public Operator(int actorID, EmployeePosition careerAndJobs, int operatingAreaID)
    {
        OperatorID = actorID;
        OperatorPosition = careerAndJobs;
        OperatingAreaID = operatingAreaID;
    }

    public Operator(Operator other)
    {
        OperatorID = other.OperatorID;
        OperatorPosition = other.OperatorPosition;
        OperatingAreaID = other.OperatingAreaID;
    }
}

public class Priority
{
    public List<double> AllPriorities;

    public Priority(params double[] priorities)
    {
        AllPriorities = new List<double>(priorities);
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

public class StationPriority
{
    public StationComponent Station;
    public Priority Priority;

    public StationPriority(StationComponent station, Priority priority)
    {
        Station = station;
        Priority = priority;
    }
}

public class PriorityQueue_Station
{
    int _currentPosition;
    StationPriority[] _allStationPriorities;
    Dictionary<StationComponent, int> _priorityQueue;

    public PriorityQueue_Station(int maxStations)
    {
        _currentPosition = 0;
        _allStationPriorities = new StationPriority[maxStations];
        _priorityQueue = new Dictionary<StationComponent, int>();
    }

    public Priority Peek()
    {
        if (_currentPosition == 0) return new Priority(-1);

        return _allStationPriorities[1].Priority;
    }

    public StationComponent Dequeue()
    {
        if (_currentPosition == 0) return null;

        StationComponent station = _allStationPriorities[1].Station;
        _allStationPriorities[1] = _allStationPriorities[_currentPosition];
        _priorityQueue[_allStationPriorities[1].Station] = 1;
        _priorityQueue[station] = 0;
        _currentPosition--;
        _moveDown(1);
        return station;
    }

    public void Enqueue(StationComponent station, Priority priority)
    {
        StationPriority queueStation = new StationPriority(station, priority);
        _currentPosition++;
        _priorityQueue[station] = _currentPosition;
        if (_currentPosition == _allStationPriorities.Length) Array.Resize<StationPriority>(ref _allStationPriorities, _allStationPriorities.Length * 2);
        _allStationPriorities[_currentPosition] = queueStation;
        _moveUp(_currentPosition);
    }

    public void Update(StationComponent station, Priority priority)
    {
        int index = _priorityQueue[station];
        if (index == 0) return;
        Priority priorityOld = _allStationPriorities[index].Priority;
        _allStationPriorities[index].Priority = priority;
        if (priorityOld.CompareTo(priority) < 0)
        {
            _moveDown(index);
        }
        else
        {
            _moveUp(index);
        }
    }

    public void Remove(StationComponent station)
    {
        int index = _priorityQueue[station];

        if (index == 0) return;

        _priorityQueue[station] = 0;
        _allStationPriorities[index] = _allStationPriorities[_currentPosition];
        _priorityQueue[_allStationPriorities[index].Station] = index;
        _currentPosition--;
        _moveDown(index);
    }

    public bool Contains(StationComponent station)
    {
        int index;
        if (!_priorityQueue.TryGetValue(station, out index))
        {
            return false;
        }
        return index != 0;
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
        else if (_allStationPriorities[childL].Priority.CompareTo(_allStationPriorities[childR].Priority) < 0)
        {
            smallerChild = childL;
        }
        else
        {
            smallerChild = childR;
        }
        if (_allStationPriorities[index].Priority.CompareTo(_allStationPriorities[smallerChild].Priority) > 0)
        {
            _swap(index, smallerChild);
            _moveDown(smallerChild);
        }
    }

    void _moveUp(int index)
    {
        if (index == 1) return;
        int parent = index / 2;

        if (_allStationPriorities[parent].Priority.CompareTo(_allStationPriorities[index].Priority) > 0)
        {
            _swap(parent, index);
            _moveUp(parent);
        }
    }

    void _swap(int indexA, int indexB)
    {
        StationPriority tempStation = _allStationPriorities[indexA];
        _allStationPriorities[indexA] = _allStationPriorities[indexB];
        _priorityQueue[_allStationPriorities[indexB].Station] = indexA;
        _allStationPriorities[indexB] = tempStation;
        _priorityQueue[tempStation.Station] = indexB;
    }
}