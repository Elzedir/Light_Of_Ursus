using System;
using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Recipes
{
    [CreateAssetMenu(fileName = "Recipe_SO", menuName = "SOList/Recipe_SO")]
    [Serializable]
    public class Recipe_SO : Data_SO<Recipe_Data>
    {
        public Data<Recipe_Data>[] Recipes => Data;
        public Data<Recipe_Data>   GetRecipe_Master(RecipeName recipeName) => GetData((uint)recipeName);
        
        public override uint GetDataID(int id) => (uint)Recipes[id].Data_Object.RecipeName;

        protected override Dictionary<uint, Data<Recipe_Data>> _getDefaultData() => 
            _convertDictionaryToData(Recipe_List.DefaultRecipes);

        protected override Data<Recipe_Data> _convertToData(Recipe_Data data)
        {
            return new Data<Recipe_Data>(
                dataID: (uint)data.RecipeName, 
                data_Object: data,
                dataTitle: $"{(uint)data.RecipeName}: {data.RecipeName}",
                getDataToDisplay: data.GetData_Display);
        }
    }

    [CustomEditor(typeof(Recipe_SO))]
    public class AllRecipes_SOEditor : Data_SOEditor<Recipe_Data>
    {
        public override Data_SO<Recipe_Data> SO => _so ??= (Recipe_SO)target;
    }
}