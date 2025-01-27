using System.Collections.Generic;

namespace Items
{
    public abstract class Item_List
    {
        static        Dictionary<ulong, Item_Data> _defaultItems;
        public static Dictionary<ulong, Item_Data> DefaultItems => _defaultItems ??= _initialiseDefaultItems();
        
        static Dictionary<ulong, Item_Data> _initialiseDefaultItems()
        {
            var defaultItems = new Dictionary<ulong, Item_Data>();

            foreach (var item in List_Weapon.DefaultWeapons)
            {
                defaultItems.Add(item.Key, item.Value);
            }

            foreach (var item in List_Armour.DefaultArmour)
            {
                defaultItems.Add(item.Key, item.Value);
            }

            foreach (var item in List_Consumable.DefaultConsumables)
            {
                defaultItems.Add(item.Key, item.Value);
            }

            foreach (var rawMaterial in List_RawMaterial.DefaultRawMaterials)
            {
                defaultItems.Add(rawMaterial.Key, rawMaterial.Value);
            }

            foreach (var processedMaterial in List_ProcessedMaterial.DefaultProcessedMaterials)
            {
                defaultItems.Add(processedMaterial.Key, processedMaterial.Value);
            }

            return defaultItems;
        }
    }
}