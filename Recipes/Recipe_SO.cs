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
        
        public override void PopulateSceneData()
        {
            if (_defaultRecipes.Count == 0)
            {
                Debug.Log("No Default Recipes Found");
            }
        }

        protected override Dictionary<uint, Data<Recipe_Data>> _getDefaultData(bool initialisation = false)
        {
            return _convertDictionaryToData(Recipe_List.DefaultRecipes);
        }

        Dictionary<uint, Data<Recipe_Data>> _defaultRecipes => DefaultData;

        protected override Data<Recipe_Data> _convertToData(Recipe_Data data)
        {
            return new Data<Recipe_Data>(
                dataID: (uint)data.RecipeName, 
                data_Object: data,
                dataTitle: $"{(uint)data.RecipeName}: {data.RecipeName}",
                getData_Display: data.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(Recipe_SO))]
    public class AllRecipes_SOEditor : Data_SOEditor<Recipe_Data>
    {
        public override Data_SO<Recipe_Data> SO => _so ??= (Recipe_SO)target;
    }
}