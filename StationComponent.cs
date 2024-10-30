using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public uint StationID { get { return StationData.StationID; } }
    public JobsiteComponent Jobsite { get { return StationData.Jobsite; } }
    bool _initialised = false;

    public StationData StationData;
    public abstract StationName StationName { get; }
    public abstract StationType StationType { get; }
    public bool IsStationBeingOperated { get { return AllOperatingAreasInStation.Any(oa => oa.OperatingAreaData.CurrentOperatorID != 0); } }

    public PriorityComponent_Station PriorityComponent;

    public float InteractRange { get; protected set; }

    public List<EmployeePosition> AllowedEmployeePositions;
    public abstract EmployeePosition CoreEmployeePosition { get; }
    public abstract RecipeName DefaultProduct { get; }
    public abstract  List<RecipeName> AllowedRecipes { get; }
    public abstract List<uint> AllowedStoredItemIDs { get; }
    public abstract List<uint> DesiredStoredItemIDs { get; }
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
            //Debug.Log($"InventoryContainsAllItems: false for station {StationData.StationID}");

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

            if (Jobsite.JobsiteData.JobsiteFactionID != 1)
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

        Manager_TickRate.RegisterTickable(OnTick, TickRate.OneSecond);

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
    public List<Item> GetInventoryItemsToHaul()
    {
        return StationData.InventoryData.GetInventoryItemsToFetch();
    }

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

    public abstract List<Item> GetItemsToDeliver(InventoryData inventoryOwner);

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

public class PriorityComponent_Station : PriorityComponent
{
    public PriorityComponent_Station(uint stationID) 
    {
        _stationReferences = new ComponentReference_Station(stationID);
        PriorityQueue = new PriorityQueue(100);
    } 

    readonly ComponentReference_Station _stationReferences;

    public uint JobsiteID { get { return _stationReferences.StationID; } }
    protected StationComponent Jobsite { get { return _stationReferences.Station; } }

    protected override void _updateAllPriorities(List<object> allData)
    {
        List<StationComponent> allStations = allData.Cast<StationComponent>().ToList();
    }
}