using System.Collections.Generic;

namespace Items
{
    public abstract class List_Consumable
    {
        static Dictionary<ulong, Item_Data> _defaultConsumables;
        public static Dictionary<ulong, Item_Data> DefaultConsumables => _defaultConsumables ??= _initialiseDefaultConsumables();
        
        static Dictionary<ulong, Item_Data> _initialiseDefaultConsumables()
        {
            var defaultConsumables = new Dictionary<ulong, Item_Data>();

            foreach (var item in _potions())
            {
                defaultConsumables.Add(item.Key, item.Value);
            }

            return defaultConsumables;
        }

        static Dictionary<ulong, Item_Data> _potions()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    202,
                    new Item_Data(
                        new Item_CommonStats(
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

                        new Item_FixedModifiers(
                            healthRecovery: 5
                        ),
                        null,
                        null
                    )
                }
            };
        }
    }
}
