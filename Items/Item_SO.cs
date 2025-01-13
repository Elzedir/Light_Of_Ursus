using System;
using System.Collections.Generic;
using Items;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "Item_SO", menuName = "SOList/Item_SO")]
    [Serializable]
    public class Item_SO : Data_SO<Item_Data>
    {
        public Data<Item_Data>[] Items                       => Data;
        public Data<Item_Data>   GetItem_Master(uint itemID) => GetData(itemID);
        
        public override uint GetDataID(int id) => Items[id].Data_Object.ItemID;
        
        protected override Dictionary<uint, Data<Item_Data>> _getDefaultData() => 
            _convertDictionaryToData(DefaultItems);
        
        static        Dictionary<uint, Item_Data> _defaultItems;
        public static Dictionary<uint, Item_Data> DefaultItems => _defaultItems ??= _initialiseDefaultItems();
        
        static Dictionary<uint, Item_Data> _initialiseDefaultItems()
        {
            var defaultItems = new Dictionary<uint, Item_Data>();

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
        
        protected override Data<Item_Data> _convertToData(Item_Data data)
        {
            return new Data<Item_Data>(
                dataID: data.ItemID, 
                data_Object: data,
                dataTitle: $"{data.ItemID}: {data.ItemName}",
                getData_Display: data.GetData_Display);
        }
    }

    [CustomEditor(typeof(Item_SO))]
    public class AllItems_SOEditor : Data_SOEditor<Item_Data>
    {
        public override Data_SO<Item_Data> SO => _so ??= (Item_SO)target;
    }
}