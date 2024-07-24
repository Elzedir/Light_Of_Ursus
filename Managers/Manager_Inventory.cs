using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Inventory : MonoBehaviour
{
    
}

public class CharacterInventoryManager
{
    public Actor_Base Actor;
}

public class ActorInventory
{
    public int Gold = 0;
    public List<Item> Inventory = new();

    public void UpdateInventory(List<Item> inventory, int gold)
    {
        Inventory = inventory;
        Gold = gold;
    }
}
