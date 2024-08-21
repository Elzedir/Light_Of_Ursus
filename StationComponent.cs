using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StationType
{
    None,
    Resource,
    Crafter,
    Storage
}

[RequireComponent(typeof(BoxCollider))]
public class StationComponent : MonoBehaviour, IInteractable
{
    public StationData StationData;
    public BoxCollider StationArea;

    public Actor_Base CurrentOperator;
    public bool IsBeingOperated = false;
    public float CurrentProgress = 0;
    public float ProgressRequired = 100;
    public float ProgressPercentageChance = 80;
    public float ProgressCooldownTime = 1;
    public float ProgressCooldownCheck;

    public float InteractRange {  get; protected set; }

    public GameObject GameObject { get; protected set; }

    public List<EmployeePosition> AllowedEmployeePositions;
    public List<RecipeName> AllowedRecipes;

    public Dictionary<BoxCollider, bool> AllOperatingAreasInStation = new();

    bool _hasMaterials = false;

    public List<Item> ProducedItems = new();

    protected void Awake()
    {
        StationArea = gameObject.AddComponent<BoxCollider>();
        StationArea.isTrigger = true;
    }

    public void SetRecipe(RecipeName recipeName)
    {
        StationData.CurrentRecipe = Manager_Recipe.GetRecipe(recipeName);
        CheckMaterials();
    }

    public void SetOperator(Actor_Base actor)
    {
        CurrentOperator = actor;
        IsBeingOperated = true;
        CheckMaterials();
    }

    public void RemoveOperator()
    {
        CurrentOperator = null;
        IsBeingOperated = false;
        CheckMaterials();
    }

    public void CheckMaterials()
    {
        _hasMaterials = StationData.InventoryData.InventoryContainsAllItems(StationData.CurrentRecipe.RecipeIngredients);
    }

    protected void Update()
    {
        if (StationData.StationIsActive && IsBeingOperated && _hasMaterials)
        {
            _operateStation();
        }
    }

    protected void _operateStation()
    {
        // Use actor data to check how fast the process will be completed and the outcome of the process.
        // Use an accumulation to determine outcome. If one actor with 80 skill used it, then 80 * 100 = 800. 
        // If you have someone who did 40% of the work at 40 skill, then 40 * 40 + 80 * 60 = 160 + 480 = 640.

        ProgressCooldownCheck += UnityEngine.Time.deltaTime;

        if (ProgressCooldownCheck > ProgressCooldownTime)
        {
            if (UnityEngine.Random.Range(0, 100) < ProgressPercentageChance)
            {
                CurrentProgress += 1;
            }

            if (CurrentProgress >= ProgressRequired)
            {
                // Create the item.
                return;
            }

            ProgressCooldownCheck = 0;
        }
    }

    public void Initialise()
    {
        InitialiseStationName();
        _initialiseOperatingAreas();
        SetInteractRange();
        InitialiseAllowedEmployeePositions();
        InitialiseAllowedRecipes();
        InitialiseStartingInventory();
    }

    public virtual void InitialiseStationName()
    {
        throw new ArgumentException("Cannot use base class.");
    }

    void _initialiseOperatingAreas()
    {
        foreach(Transform child in transform)
        {
            if (!child.name.Contains("OperatingArea")) continue;

            if (!AllOperatingAreasInStation.ContainsKey(child.GetComponent<BoxCollider>()))AllOperatingAreasInStation.Add(child.GetComponent<BoxCollider>(), false);
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

    public virtual bool EmployeeCanUse(EmployeePosition employeePosition)
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
        CheckMaterials();
        return added;
    }

    public virtual bool RemoveItemsFromStation(List<Item> items)
    {
        var removed = StationData.InventoryData.RemoveFromInventory(items);
        CheckMaterials();
        return removed;
    }

    protected virtual void _onCraftItem(List<Item> craftedItems)
    {
        ProducedItems.AddRange(craftedItems);
    }

    public virtual List<Item> GetProductedItems()
    {
        var tempList = ProducedItems;
        ProducedItems.Clear();
        return tempList;
    }
}
