using System;
using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Recipe
{
    [CreateAssetMenu(fileName = "Recipe_SO", menuName = "SOList/Recipe_SO")]
    [Serializable]
    public class Recipe_SO : Data_SO<Recipe_Data>
    {
        public Data_Object<Recipe_Data>[] Recipes => DataObjects;
        public Data_Object<Recipe_Data>   GetRecipe_Master(RecipeName recipeName) => GetDataObject_Master((uint)recipeName);
        
        public override uint GetDataObjectID(int id) => (uint)Recipes[id].DataObject.RecipeName;
        
        public void PopulateDefaultRecipes()
        {
            if (_defaultRecipes.Count == 0)
            {
                Debug.Log("No Default Recipes Found");
            }
        }

        protected override Dictionary<uint, Data_Object<Recipe_Data>> _populateDefaultDataObjects()
        {
            return _convertDictionaryToDataObject(Recipe_List.GetAllDefaultRecipes());
        }

        Dictionary<uint, Data_Object<Recipe_Data>> _defaultRecipes => DefaultDataObjects;

        protected override Data_Object<Recipe_Data> _convertToDataObject(Recipe_Data data)
        {
            return new Data_Object<Recipe_Data>(
                dataObjectID: (uint)data.RecipeName, 
                dataObject: data,
                dataObjectTitle: $"{(uint)data.RecipeName}: {data.RecipeName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(Recipe_SO))]
    public class AllRecipes_SOEditor : Data_SOEditor<Recipe_Data>
    {
        public override Data_SO<Recipe_Data> SO => _so ??= (Recipe_SO)target;
    }
}