using System.Collections.Generic;
using UnityEngine;

public enum ProcessedMaterialName
{
    None,
    Plank
}

public class List_ProcessedMaterial : Manager_Item
{
    public static void InitializeProcessedMaterialData()
    {
        _metals();
        _woods();
        _stones();
        _gems();
        _herbs();
        _fibers();
        _leathers();
        _ores();
        _fuels();
        _flora();
        _animalProducts();
        _liquids();
    }

    static void _metals()
    {
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2200, itemType: ItemType.Processed_Material, itemName: "Iron Ingot", maxStackSize: 100, itemWeight: 5, itemValue: 15)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2201, itemType: ItemType.Processed_Material, itemName: "Copper Ingot", maxStackSize: 100, itemWeight: 5, itemValue: 10)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2202, itemType: ItemType.Processed_Material, itemName: "Silver Ingot", maxStackSize: 100, itemWeight: 5, itemValue: 25)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2203, itemType: ItemType.Processed_Material, itemName: "Gold Ingot", maxStackSize: 100, itemWeight: 5, itemValue: 50)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2204, itemType: ItemType.Processed_Material, itemName: "Steel Ingot", maxStackSize: 100, itemWeight: 5, itemValue: 20)
            , null, null, null, null, null, null));
    }

    static void _woods()
    {
        AddToList(new Item_Master(
            new CommonStats_Item(itemID: 2300, itemType: ItemType.Processed_Material, itemName: "Plank", maxStackSize: 100, itemWeight: 3, itemValue: 5),
            new VisualStats_Item(itemMesh: GameObject.Find("TestPlank").GetComponent<MeshFilter>().mesh, itemMaterial: Resources.Load<Material>("Materials/Material_Yellow"), itemScale: new Vector3(0.1f, 1, 0.2f)),
            null, null, null, null,
            new PriorityStats_Item(
                new Dictionary<StationName, uint>
                {
                    { StationName.Log_Pile, 2 },
                    { StationName.Sawmill, 1 }
                }
            )));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2301, itemType: ItemType.Processed_Material, itemName: "Timber", maxStackSize: 100, itemWeight: 3, itemValue: 4)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2302, itemType: ItemType.Processed_Material, itemName: "Board", maxStackSize: 100, itemWeight: 3, itemValue: 6)
            , null, null, null, null, null, null));
    }

    static void _stones()
    {
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2400, itemType: ItemType.Processed_Material, itemName: "Cut Granite", maxStackSize: 100, itemWeight: 10, itemValue: 8)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2401, itemType: ItemType.Processed_Material, itemName: "Cut Marble", maxStackSize: 100, itemWeight: 10, itemValue: 15)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2402, itemType: ItemType.Processed_Material, itemName: "Cut Limestone", maxStackSize: 100, itemWeight: 10, itemValue: 5)
            , null, null, null, null, null, null));
    }

    static void _gems()
    {
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2500, itemType: ItemType.Processed_Material, itemName: "Cut Diamond", maxStackSize: 100, itemWeight: 1, itemValue: 100)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2501, itemType: ItemType.Processed_Material, itemName: "Cut Ruby", maxStackSize: 100, itemWeight: 1, itemValue: 80)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2502, itemType: ItemType.Processed_Material, itemName: "Cut Sapphire", maxStackSize: 100, itemWeight: 1, itemValue: 75)
            , null, null, null, null, null, null));
    }

    static void _herbs()
    {
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2600, itemType: ItemType.Processed_Material, itemName: "Dried Lavender", maxStackSize: 100, itemWeight: 0.1f, itemValue: 5)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2601, itemType: ItemType.Processed_Material, itemName: "Dried Mint", maxStackSize: 100, itemWeight: 0.1f, itemValue: 4)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2602, itemType: ItemType.Processed_Material, itemName: "Dried Basil", maxStackSize: 100, itemWeight: 0.1f, itemValue: 6)
            , null, null, null, null, null, null));
    }

    static void _fibers()
    {
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2700, itemType: ItemType.Processed_Material, itemName: "Spun Cotton", maxStackSize: 100, itemWeight: 0.2f, itemValue: 2)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2701, itemType: ItemType.Processed_Material, itemName: "Spun Wool", maxStackSize: 100, itemWeight: 0.5f, itemValue: 3)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2702, itemType: ItemType.Processed_Material, itemName: "Spun Silk", maxStackSize: 100, itemWeight: 0.1f, itemValue: 10)
            , null, null, null, null, null, null));
    }

    static void _leathers()
    {
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2800, itemType: ItemType.Processed_Material, itemName: "Tanned Cowhide", maxStackSize: 100, itemWeight: 2, itemValue: 15)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2801, itemType: ItemType.Processed_Material, itemName: "Tanned Deerhide", maxStackSize: 100, itemWeight: 2, itemValue: 20)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2802, itemType: ItemType.Processed_Material, itemName: "Tanned Snakehide", maxStackSize: 100, itemWeight: 2, itemValue: 25)
            , null, null, null, null, null, null));
    }

    static void _ores()
    {
        // Assuming processed ores are refined forms like powders or purified materials
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2900, itemType: ItemType.Processed_Material, itemName: "Iron Powder", maxStackSize: 100, itemWeight: 5, itemValue: 10)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2901, itemType: ItemType.Processed_Material, itemName: "Copper Powder", maxStackSize: 100, itemWeight: 5, itemValue: 8)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 2902, itemType: ItemType.Processed_Material, itemName: "Gold Powder", maxStackSize: 100, itemWeight: 5, itemValue: 20)
            , null, null, null, null, null, null));
    }

    static void _fuels()
    {
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3000, itemType: ItemType.Processed_Material, itemName: "Refined Coal", maxStackSize: 100, itemWeight: 10, itemValue: 5)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3001, itemType: ItemType.Processed_Material, itemName: "Refined Oil", maxStackSize: 100, itemWeight: 10, itemValue: 8)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3002, itemType: ItemType.Processed_Material, itemName: "Charcoal Briquettes", maxStackSize: 100, itemWeight: 10, itemValue: 4)
            , null, null, null, null, null, null));
    }

    static void _flora()
    {
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3100, itemType: ItemType.Processed_Material, itemName: "Processed Flower Petals", maxStackSize: 100, itemWeight: 0.1f, itemValue: 3)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3101, itemType: ItemType.Processed_Material, itemName: "Processed Roots", maxStackSize: 100, itemWeight: 0.2f, itemValue: 4)
            , null, null, null, null, null, null));
    }

    static void _animalProducts()
    {
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3200, itemType: ItemType.Processed_Material, itemName: "Bone Meal", maxStackSize: 100, itemWeight: 2, itemValue: 5),
            null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3201, itemType: ItemType.Processed_Material, itemName: "Feather Quills", maxStackSize: 100, itemWeight: 0.1f, itemValue: 2)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3202, itemType: ItemType.Processed_Material, itemName: "Claw Powder", maxStackSize: 100, itemWeight: 0.5f, itemValue: 3)
            , null, null, null, null, null, null));
    }

    static void _liquids()
    {
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3300, itemType: ItemType.Processed_Material, itemName: "Purified Water", maxStackSize: 100, itemWeight: 1, itemValue: 1)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3301, itemType: ItemType.Processed_Material, itemName: "Refined Oil", maxStackSize: 100, itemWeight: 1, itemValue: 2)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3302, itemType: ItemType.Processed_Material, itemName: "Potion Base", maxStackSize: 100, itemWeight: 1, itemValue: 5)
            , null, null, null, null, null, null));
        AddToList(new Item_Master(new CommonStats_Item(itemID: 3303, itemType: ItemType.Processed_Material, itemName: "Concentrated Reagent", maxStackSize: 100, itemWeight: 1, itemValue: 10)
            , null, null, null, null, null, null));
    }
}
