using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Inventory;
using Items;
using Jobs;
using Jobsite;
using Managers;
using OperatingArea;
using Priority;
using Recipes;
using Station;
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
public abstract class StationComponent : MonoBehaviour, IInteractable
{
    public uint             StationID => StationData.StationID;
    public JobsiteComponent Jobsite   => StationData.Jobsite;
    bool                    _initialised;

    public StationData StationData;
    public abstract StationName StationName { get; }
    public abstract StationType StationType { get; }
    public bool IsStationBeingOperated => AllOperatingAreasInStation.Any(oa => oa.OperatingAreaData.CurrentOperatorID != 0);

    public float InteractRange { get; protected set; }
    
    public abstract         EmployeePosition             CoreEmployeePosition     { get; }
    public abstract         RecipeName                   DefaultProduct           { get; }
    public abstract         List<RecipeName>             AllowedRecipes           { get; }
    public abstract         List<uint>                   AllowedStoredItemIDs     { get; }
    public abstract         List<uint>                   DesiredStoredItemIDs     { get; }
    public abstract         uint                         OperatingAreaCount       { get; }
    public abstract         List<EmployeePosition>       AllowedEmployeePositions { get; }
    public         abstract List<JobName>                AllowedJobs              { get; }
    
    List<OperatingAreaComponent>     _allOperatingAreasInStation;
    public List<OperatingAreaComponent> AllOperatingAreasInStation =>
        _allOperatingAreasInStation ??= _getAllOperatingAreasInStation();

    const    float      _baseProgressRatePerHour = 5;
    readonly List<Item> _currentProductsCrafted  = new();
    
    PriorityComponent_Station _priorityComponent;
    public PriorityComponent_Station PriorityComponent => _priorityComponent ??= new PriorityComponent_Station(); 

    BoxCollider _boxCollider;
    public BoxCollider BoxCollider => _boxCollider ??= gameObject.GetComponent<BoxCollider>();

    // Temporary
    public Transform CollectionPoint;
    
    public void Initialise()
    {
        var employeeOperatingAreaPairs = from operatingArea in AllOperatingAreasInStation
                                         from employeeID in StationData.CurrentOperatorIDs
                                         let actorData = Manager_Actor.GetActorData(employeeID)
                                         where actorData.CareerData.CurrentJob.OperatingAreaID == operatingArea.OperatingAreaData.OperatingAreaID
                                         select new { operatingArea, employeeID };

        foreach (var pair in employeeOperatingAreaPairs)
        {
            pair.operatingArea.OperatingAreaData.AddOperatorToOperatingArea(pair.employeeID);
        }

        SetInteractRange();
        InitialiseStartingInventory();

        _setTickRate(TickRate.OneSecond);

        _initialised = true;
    }

    public OperatingAreaComponent GetRelevantOperatingArea(ActorComponent actor)
    {
        var relevantOperatingArea = AllOperatingAreasInStation.FirstOrDefault(oa => !oa.HasOperator());

        if (relevantOperatingArea is not null)
        {
            return relevantOperatingArea;
        }

        Debug.Log("No relevant operating area found for actor.");
        return null;
    }

    public void AddOperatorToArea(uint operatorData)
    {
        var openOperatingArea = AllOperatingAreasInStation.FirstOrDefault(area => !area.OperatingAreaData.HasOperator());
        
        if (openOperatingArea is not null)
        {
            openOperatingArea.OperatingAreaData.AddOperatorToOperatingArea(operatorData);
        }
        else
        {
            Debug.Log("No open operating areas found in all operating areas.");
        }
    }

    public void RemoveOperatorFromArea(uint operatorID)
    {
        if (AllOperatingAreasInStation.Any(area => area.OperatingAreaData.CurrentOperatorID == operatorID))
        {
            AllOperatingAreasInStation.FirstOrDefault(area => area.OperatingAreaData.CurrentOperatorID == operatorID)?
                .OperatingAreaData.RemoveOperatorFromOperatingArea();
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
    
    TickRate _currentTickRate;

    void _setTickRate(TickRate tickRate)
    {
        if (_currentTickRate == tickRate) return;
            
        Manager_TickRate.UnregisterTicker(TickerType.Station, _currentTickRate, StationID);
        Manager_TickRate.RegisterTicker(TickerType.Station, tickRate, StationID, _onTick);
        _currentTickRate = tickRate;
    }

    void _onTick()
    {
        if (!_initialised) return;

        _operateStation();
    }

    protected virtual void _operateStation()
    {
        if (!_passesStationChecks()) return;

        foreach (var operatingArea in AllOperatingAreasInStation)
        {
            var progressMade = operatingArea.Operate(_baseProgressRatePerHour,
                StationData.StationProgressData.CurrentProduct);
            var itemCrafted = StationData.StationProgressData.Progress(progressMade);

            if (!itemCrafted) continue;

            // For now is the final person who adds the last progress, but change to a cumulative system later.
            CraftItem(
                StationData.StationProgressData.CurrentProduct.RecipeName,
                Manager_Actor.GetActor(operatingArea.OperatingAreaData.CurrentOperatorID)
            );
        }
    }

    bool _passesStationChecks()
    {
        if (!StationData.StationIsActive) 
            return false;
        
        if (!IsStationBeingOperated) 
            return false;

        var success = false;

        while (!success)
        {
            try
            {
                success = StationData.InventoryData.InventoryContainsAllItems(
                    StationData.StationProgressData.CurrentProduct.RequiredIngredients);
                break;
            }
            catch (Exception e)
            {
                StationData.StationProgressData.CurrentProduct ??= Manager_Recipe.GetRecipe_Master(DefaultProduct);

                if (StationData.StationProgressData.CurrentProduct.RecipeName != RecipeName.None ||
                    DefaultProduct                                            == RecipeName.None)
                {
                    break;
                }
                
                //Console.WriteLine(e);
            }    
        }
        
        return success;
    }

    public OperatingAreaComponent GetOperatingArea(int operatingAreaID)
    {
        return AllOperatingAreasInStation.FirstOrDefault(oa => oa.OperatingAreaData.OperatingAreaID == operatingAreaID);
    }

    List<OperatingAreaComponent> _getAllOperatingAreasInStation()
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

    public abstract void InitialiseStartingInventory();
    public List<Item> GetInventoryItemsToFetch()
    {
        return StationData.InventoryData.GetInventoryItemsToFetch();
    }

    public List<Item> GetInventoryItemsToDeliver(InventoryData inventory)
    {
        return StationData.InventoryData.GetInventoryItemsToDeliver(inventory);
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

    protected abstract List<Item> _getCost(List<Item> ingredients, ActorComponent actor);

    protected abstract List<Item> _getYield(List<Item> ingredients, ActorComponent actor);

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
    bool    _showBasicInfo;
    bool    _showProductionItems;
    bool    _showInventory;
    bool    _showOperators;
    bool    _showProgress;
    bool    _showRecipe;
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
                    EditorGUILayout.LabelField(item.ItemName);
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

                foreach (var item in stationData.InventoryData.AllInventoryItems.Values)
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
    public int              OperatorID;
    public string           OperatorName;
    public EmployeePosition OperatorPosition;
    public int              OperatingAreaID;

    public Operator(int actorID, EmployeePosition careerAndJobs, int operatingAreaID)
    {
        OperatorID       = actorID;
        OperatorPosition = careerAndJobs;
        OperatingAreaID  = operatingAreaID;
    }

    public Operator(Operator other)
    {
        OperatorID       = other.OperatorID;
        OperatorPosition = other.OperatorPosition;
        OperatingAreaID  = other.OperatingAreaID;
    }
}