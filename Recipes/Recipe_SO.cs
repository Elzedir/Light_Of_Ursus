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
        public Object_Data<Recipe_Data>[] Recipes => Objects_Data;
        public Object_Data<Recipe_Data>   GetRecipe_Master(RecipeName recipeName) => GetObject_Data((uint)recipeName);
        
        public override uint GetDataObjectID(int id) => (uint)Recipes[id].DataObject.RecipeName;
        
        public override void PopulateSceneData()
        {
            if (_defaultRecipes.Count == 0)
            {
                Debug.Log("No Default Recipes Found");
            }
        }

        protected override Dictionary<uint, Object_Data<Recipe_Data>> _getDefaultDataObjects(bool initialisation = false)
        {
            return _convertDictionaryToDataObject(Recipe_List.DefaultRecipes);
        }

        Dictionary<uint, Object_Data<Recipe_Data>> _defaultRecipes => DefaultDataObjects;

        protected override Object_Data<Recipe_Data> _convertToDataObject(Recipe_Data dataObject)
        {
            return new Object_Data<Recipe_Data>(
                dataObjectID: (uint)dataObject.RecipeName, 
                dataObject: dataObject,
                dataObjectTitle: $"{(uint)dataObject.RecipeName}: {dataObject.RecipeName}",
                data_Display: dataObject.GetDataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(Recipe_SO))]
    public class AllRecipes_SOEditor : Data_SOEditor<Recipe_Data>
    {
        public override Data_SO<Recipe_Data> SO => _so ??= (Recipe_SO)target;
    }
}