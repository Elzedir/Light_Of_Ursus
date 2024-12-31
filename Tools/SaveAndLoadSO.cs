using Ability;
using Actor;
using Careers;
using City;
using DataPersistence;
using DateAndTime;
using Faction;
using Items;
using Jobs;
using JobSite;
using Recipes;
using Region;
using Station;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public class SaveAndLoadSO : EditorWindow
    {
        [MenuItem("Tools/Save And Load SO")]
        public static void ShowWindow()
        {
            GetWindow<SaveAndLoadSO>("Save And Load SO");
        }

        void OnGUI()
        {
            if (GUILayout.Button("Save All SOs"))
            {
                _saveAllSOs();
            }
            
            if (GUILayout.Button("Refresh All SOs"))
            {
                ClearAllSOs();
            }

            if (GUILayout.Button("Recreate All SOs"))
            {
                RecreateAllSOs();
            }

            if (GUILayout.Button("Delete Test Save File"))
            {
                _deleteTestSaveFile();
            }
        }

        static void _saveAllSOs()
        {
            DataPersistenceManager.DataPersistence_SO.SaveGame("");
        }

        public static void ClearAllSOs()
        {
            Ability_Manager.ClearSOData();
            Actor_Manager.ClearSOData();
            ActorPreset_Manager.ClearSOData();
            Career_Manager.ClearSOData();
            City_Manager.ClearSOData();
            Faction_Manager.ClearSOData();
            Item_Manager.ClearSOData();
            Job_Manager.ClearSOData();
            JobSite_Manager.ClearSOData();
            Recipe_Manager.ClearSOData();
            Region_Manager.ClearSOData();
            Station_Manager.ClearSOData();
        }

        public static void RecreateAllSOs()
        {
            var scriptableObjectsToDestroy = Resources.FindObjectsOfTypeAll<ScriptableObject>();

            foreach (var scriptableObject in scriptableObjectsToDestroy)
            {
                var assetPath = AssetDatabase.GetAssetPath(scriptableObject);

                if (!assetPath.Contains("Resources/ScriptableObjects")) continue;
                
                DestroyImmediate(scriptableObject, true);
            }

            const string folderPath = "Assets/Resources/ScriptableObjects";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "ScriptableObjects");
            }

            _saveScriptableObject<Ability_SO>("Ability_SO", folderPath);
            _saveScriptableObject<Actor_SO>("Actor_SO", folderPath);
            _saveScriptableObject<ActorPreset_SO>("ActorDataPreset_SO", folderPath);
            _saveScriptableObject<Career_SO>("Career_SO", folderPath);
            _saveScriptableObject<City_SO>("City_SO", folderPath);
            _saveScriptableObject<DataPersistence_SO>("DataPersistence_SO", folderPath);
            _saveScriptableObject<DateAndTime_SO>("DateAndTime_SO", folderPath);
            _saveScriptableObject<Faction_SO>("Faction_SO", folderPath);
            _saveScriptableObject<Item_SO>("Item_SO", folderPath);
            _saveScriptableObject<Job_SO>("Job_SO", folderPath);
            _saveScriptableObject<JobSite_SO>("JobSite_SO", folderPath);
            _saveScriptableObject<Recipe_SO>("Recipe_SO", folderPath);
            _saveScriptableObject<Region_SO>("Region_SO", folderPath);
            _saveScriptableObject<Station_SO>("Station_SO", folderPath);
            
            AssetDatabase.Refresh();

            Debug.Log("All SOs refreshed.");
        }
        
        static void _saveScriptableObject<T>(string fileName, string folderPath) where T : ScriptableObject
        {
            var      instance  = CreateInstance<T>();
            var assetPath = $"{folderPath}/{fileName}.asset";

            AssetDatabase.CreateAsset(instance, assetPath);
        }

        void _deleteTestSaveFile()
        {
            DataPersistenceManager.DataPersistence_SO.DeleteTestSaveFile();
        }
    }
}
