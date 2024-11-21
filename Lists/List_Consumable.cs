using System.Collections.Generic;

namespace Lists
{
    public abstract class List_Consumable : Manager_Item
    {
        public static Dictionary<uint, Item_Master> GetAllConsumables()
        {
            var allConsumables = new Dictionary<uint, Item_Master>();
            
            foreach (var consumable in _potions)
            {
                allConsumables.Add(consumable.Key, consumable.Value);
            }
            
            return allConsumables;
        }

        static readonly Dictionary<uint, Item_Master> _potions = new()
        {
            {
                202,
                new Item_Master(
                    new CommonStats_Item(
                        itemID: 202,
                        itemType: ItemType.Consumable,
                        itemName: "Small Health Potion",
                        itemEquippable: true,
                        maxStackSize: 99,
                        itemValue: 1
                    ),

                    null,
                    null,
                    null,

                    new FixedModifiers_Item(
                        healthRecovery: 5
                    ),
                    null,
                    null
                )
            }
        };
    }
}
