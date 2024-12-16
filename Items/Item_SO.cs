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
        public Object_Data<Item_Data>[] Items                       => Objects_Data;
        public Object_Data<Item_Data>   GetItem_Master(uint itemID) => GetObject_Data(itemID);
        
        public override uint GetDataObjectID(int id) => Items[id].DataObject.ItemID;

        public void PopulateDefaultItems()
        {
            if (_defaultItems.Count == 0)
            {
                Debug.Log("No Default Items Found");
            }
        }    
        
        protected override Dictionary<uint, Object_Data<Item_Data>> _populateDefaultDataObjects()
        {
            var defaultItems = new Dictionary<uint, Item_Data>();

            foreach (var item in List_Weapon.GetAllDefaultWeapons())
            {
                defaultItems.Add(item.Key, item.Value);
            }

            foreach (var item in List_Armour.GetAllDefaultArmour())
            {
                defaultItems.Add(item.Key, item.Value);
            }

            foreach (var item in List_Consumable.GetAllDefaultConsumables())
            {
                defaultItems.Add(item.Key, item.Value);
            }

            foreach (var rawMaterial in List_RawMaterial.GetAllDefaultRawMaterials())
            {
                defaultItems.Add(rawMaterial.Key, rawMaterial.Value);
            }

            foreach (var processedMaterial in List_ProcessedMaterial.GetAllDefaultProcessedMaterials())
            {
                defaultItems.Add(processedMaterial.Key, processedMaterial.Value);
            }

            return _convertDictionaryToDataObject(defaultItems);
        }

        Dictionary<uint, Object_Data<Item_Data>> _defaultItems => DefaultDataObjects;
        
        protected override Object_Data<Item_Data> _convertToDataObject(Item_Data data)
        {
            return new Object_Data<Item_Data>(
                dataObjectID: data.ItemID, 
                dataObject: data,
                dataObjectTitle: $"{data.ItemID}: {data.ItemName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(Item_SO))]
    public class AllItems_SOEditor : Data_SOEditor<Item_Data>
    {
        public override Data_SO<Item_Data> SO => _so ??= (Item_SO)target;
    }
}