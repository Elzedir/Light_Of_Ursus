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
public class StationComponent : MonoBehaviour, IInteractable, ITickable
{
    bool _initialised = false;

    public StationData StationData;
    public BoxCollider StationArea;

    public bool IsStationBeingOperated { get { return AllOperatingAreasInStation.Any(oa => oa.OperatingAreaData.CurrentOperator.ActorID != 0); } }

    public float InteractRange { get; protected set; }

    public List<EmployeePosition> AllAllowedEmployeePositions;
    public EmployeePosition NecessaryEmployeePosition;

    public List<RecipeName> AllowedRecipes;

    public List<OperatingAreaComponent> AllOperatingAreasInStation = new();

    public float BaseProgressRatePerHour = 60;
    List<Item> _currentProductsCrafted = new();

    protected void Awake()
    {
        StationArea = gameObject.GetComponent<BoxCollider>();

        if (!StationArea.isTrigger)
        {
            Debug.Log($"Set IsTrigger to true for {name}");
            StationArea.isTrigger = true;
        }
    }

    public void AddOperatorToArea(ActorData operatorData)
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

    public void RemoveOperatorFromArea(ActorData operatorData)
    {
        if (AllOperatingAreasInStation.Any(area => area.OperatingAreaData.CurrentOperator.ActorID == operatorData.ActorID))
        {
            AllOperatingAreasInStation.FirstOrDefault(area => area.OperatingAreaData.CurrentOperator.ActorID == operatorData.ActorID).OperatingAreaData.RemoveOperatorFromOperatingArea();
        }
        else
        {
            Debug.Log($"Operator {operatorData.ActorID}: {operatorData.ActorName.GetName()} not found in operating areas.");
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

    public void OnTick()
    {
        if (!_initialised) return;

        // Change the has materials to instead also include delivering materials, so instead make it part of the operation process.
        // Change material check to only happen when operator is set or leaves, or when material is used.

        Debug.Log($"Tick {name}");

        if (
            StationData.StationIsActive && 
            IsStationBeingOperated && 
            StationData.InventoryData.InventoryContainsAllItems(StationData.StationProgressData.CurrentProduct.RequiredIngredients))
        {
            _operateStation();
        }
    }

    protected void _operateStation()
    {
        foreach(var operatingArea in AllOperatingAreasInStation)
        {
            StationData.StationProgressData.Progress(operatingArea.Operate(BaseProgressRatePerHour, StationData.StationProgressData.CurrentProduct));
        }
    }

    public void Initialise()
    {
        InitialiseStationName();
        GetAllOperatingAreasInStation();
        SetInteractRange();
        InitialiseAllowedEmployeePositions();
        InitialiseAllowedRecipes();
        InitialiseStartingInventory();

        _initialised = true;
    }

    public virtual void InitialiseStationName()
    {
        throw new ArgumentException("Cannot use base class.");
    }

    public void GetAllOperatingAreasInStation()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out OperatingAreaComponent operatingArea))
            {
                operatingArea.Initialise(StationData.StationID);
                AllOperatingAreasInStation.Add(operatingArea);
            }
        }
    }

    public virtual void InitialiseAllowedEmployeePositions()
    {
        throw new ArgumentException("Cannot use base class.");
    }

    public virtual void InitialiseAllowedRecipes()
    {
        throw new ArgumentException("Cannot use base class.");
    }

    public virtual void InitialiseStartingInventory()
    {
        
    }

    public void SetStationData(StationData stationData)
    {
        StationData = stationData;
    }

    public void SetInteractRange(float interactRange = 2)
    {
        InteractRange = interactRange;
    }

    public bool WithinInteractRange(Actor_Base interactor)
    {
        return Vector3.Distance(interactor.transform.position, transform.position) < InteractRange;
    }

    public virtual IEnumerator Interact(Actor_Base actor)
    {
        throw new ArgumentException("Cannot use base class.");
    }

    public virtual void CraftItem(RecipeName recipeName, Actor_Base actor)
    {
        throw new ArgumentException("Cannot use base class.");
    }

    public virtual List<Item> GetItemsToDropOff(IInventoryOwner inventoryOwner)
    {
        throw new ArgumentException("Cannot use base class.");
    }

    protected virtual List<Item> _getCost(List<Item> ingredients, Actor_Base actor)
    {
        throw new ArgumentException("Cannot use base class.");
    }

    protected virtual List<Item> _getYield(List<Item> ingredients, Actor_Base actor)
    {
        throw new ArgumentException("Cannot use base class.");
    }

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

        foreach (var currentOperator in StationData.CurrentOperators)
        {
            var individualProductionRate = BaseProgressRatePerHour;

            foreach(var vocation in StationData.StationProgressData.CurrentProduct.RequiredVocations)
            {
                individualProductionRate *= currentOperator.VocationData.GetProgress(vocation);
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
                estimatedProductionItems.Add(Manager_Item.GetItem(item.CommonStats_Item.ItemID, item.CommonStats_Item.CurrentStackSize));
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
        DrawDefaultInspectorExcept("StationData", "AllowedEmployeePositions", "AllowedRecipes");

        StationComponent stationComponent = (StationComponent)target;
        StationData stationData = stationComponent.StationData;

        if (stationData == null) return;

        _showBasicInfo = EditorGUILayout.Toggle("Basic Info", _showBasicInfo);

        if (_showBasicInfo)
        {
            stationData.StationID = EditorGUILayout.IntField("StationID", stationData.StationID);
            stationData.StationType = (StationType)EditorGUILayout.EnumPopup("StationType", stationData.StationType);
            stationData.StationName = (StationName)EditorGUILayout.EnumPopup("StationName", stationData.StationName);
            stationData.JobsiteID = EditorGUILayout.IntField("JobsiteID", stationData.JobsiteID);
            stationData.StationIsActive = EditorGUILayout.Toggle("StationIsActive", stationData.StationIsActive);
            stationData.StationDescription = EditorGUILayout.TextField("StationDescription", stationData.StationDescription);

            EditorUtility.SetDirty(stationComponent);
        }

        if (stationData.ProductionData != null)
        {
            _showProductionItems = EditorGUILayout.Toggle("All Produced Items", _showProductionItems);

            if (_showProductionItems)
            {
                _productionItemScrollPos = EditorGUILayout.BeginScrollView(_productionItemScrollPos);

                foreach (var item in stationData.ProductionData.AllProducedItems)
                {
                    EditorGUILayout.LabelField(item.CommonStats_Item.ItemName.ToString());
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

                foreach (var item in stationData.InventoryData.InventoryItems)
                {
                    EditorGUILayout.LabelField($"{item.CommonStats_Item.ItemID}: {item.CommonStats_Item.ItemName} Qty: {item.CommonStats_Item.CurrentStackSize}");
                }

                EditorGUILayout.EndScrollView();
            }
        }

        if (stationData.CurrentOperators != null)
        {
            _showOperators = EditorGUILayout.Toggle("Current Operators", _showOperators);
            if (_showOperators)
            {
                foreach (var operatorData in stationData.CurrentOperators)
                {
                    EditorGUILayout.LabelField($"Operator: {operatorData.ActorID}: {operatorData.ActorName.GetName()} Pos: {operatorData.CareerAndJobs.EmployeePosition}");
                }
            }

        }

        if (stationData.StationProgressData != null)
        {
            if (_showProgress)
            {
                EditorGUILayout.LabelField("CurrentProgress", stationData.StationProgressData.CurrentProgress.ToString());
                EditorGUILayout.LabelField("CurrentQuality", stationData.StationProgressData.CurrentQuality.ToString());

                _showRecipe = EditorGUILayout.Toggle("Current Recipe", _showRecipe);

                if (_showRecipe)
                {
                    EditorGUILayout.LabelField("RecipeName", stationData.StationProgressData.CurrentProduct.RecipeName.ToString());
                    EditorGUILayout.LabelField("RequiredProgress", stationData.StationProgressData.CurrentProduct.RequiredProgress.ToString());
                    EditorGUILayout.LabelField("RequiredIngredients", stationData.StationProgressData.CurrentProduct.RequiredIngredients.Count.ToString());
                    EditorGUILayout.LabelField("RecipeProducts", stationData.StationProgressData.CurrentProduct.RecipeProducts.Count.ToString());
                    EditorGUILayout.LabelField("RequiredVocations", stationData.StationProgressData.CurrentProduct.RequiredVocations.Count.ToString());
                }
            }
        }
    }

    private void DrawDefaultInspectorExcept(params string[] propertyNames)
    {
        SerializedObject obj = serializedObject;
        SerializedProperty prop = obj.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (Array.IndexOf(propertyNames, prop.name) == -1)
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }
        obj.ApplyModifiedProperties();
    }
}

[Serializable]
public class StationProgressData
{
    public float CurrentProgress;
    public float CurrentQuality;
    public Recipe CurrentProduct;

    public StationProgressData(Recipe currentProduct)
    {
        CurrentProgress = 0;
        CurrentQuality = 0;

        CurrentProduct = currentProduct;
    }

    public void Progress(float progress)
    {
        CurrentProgress += progress;
        CurrentQuality += progress;

        if (CurrentProgress >= CurrentProduct.RequiredProgress)
        {
            // Create the item using total quality.
            return;
        }
    }
}
