using System;
using System.Collections.Generic;
using System.Linq;
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
        public Data<Item_Data>   GetItem_Data(ulong itemID) => GetData(itemID);
        
        protected override Dictionary<ulong, Data<Item_Data>> _getDefaultData() => 
            _convertDictionaryToData(Item_List.DefaultItems);
        
        protected override Data<Item_Data> _convertToData(Item_Data data)
        {
            return new Data<Item_Data>(
                dataID: data.ItemID, 
                data_Object: data,
                dataTitle: $"{data.ItemID}: {data.ItemName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
    }

    [CustomEditor(typeof(Item_SO))]
    public class AllItems_SOEditor : Data_SOEditor<Item_Data>
    {
        public override Data_SO<Item_Data> SO => _so ??= (Item_SO)target;
    }
}