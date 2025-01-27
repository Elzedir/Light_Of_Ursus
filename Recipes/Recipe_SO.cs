using System;
using System.Collections.Generic;
using System.Linq;
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
        public Data<Recipe_Data>   GetRecipe_Master(RecipeName recipeName) => GetData((ulong)recipeName);

        protected override Dictionary<ulong, Data<Recipe_Data>> _getDefaultData() => 
            _convertDictionaryToData(Recipe_List.DefaultRecipes);

        protected override Data<Recipe_Data> _convertToData(Recipe_Data data)
        {
            return new Data<Recipe_Data>(
                dataID: (ulong)data.RecipeName, 
                data_Object: data,
                dataTitle: $"{(ulong)data.RecipeName}: {data.RecipeName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
    }

    [CustomEditor(typeof(Recipe_SO))]
    public class AllRecipes_SOEditor : Data_SOEditor<Recipe_Data>
    {
        public override Data_SO<Recipe_Data> SO => _so ??= (Recipe_SO)target;
    }
}