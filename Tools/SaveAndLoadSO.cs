using Ability;
using Actor;
using Career;
using City;
using DateAndTime;
using EmployeePosition;
using Faction;
using Items;
using Jobs;
using JobSite;
using Recipe;
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
                _refreshAllSOs();
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

        static void _refreshAllSOs()
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
            _saveScriptableObject<ActorDataPreset_SO>("ActorDataPreset_SO", folderPath);
            _saveScriptableObject<Career_SO>("Career_SO", folderPath);
            _saveScriptableObject<City_SO>("City_SO", folderPath);
            _saveScriptableObject<DataPersistence_SO>("DataPersistence_SO", folderPath);
            _saveScriptableObject<DateAndTime_SO>("DateAndTime_SO", folderPath);
            _saveScriptableObject<EmployeePosition_SO>("EmployeePosition_SO", folderPath);
            _saveScriptableObject<Faction_SO>("Faction_SO", folderPath);
            _saveScriptableObject<Item_SO>("Item_SO", folderPath);
            _saveScriptableObject<Job_SO>("Job_SO", folderPath);
            _saveScriptableObject<JobSite_SO>("JobSite_SO", folderPath);
            _saveScriptableObject<Recipe_SO>("Recipe_SO", folderPath);
            _saveScriptableObject<Region_SO>("Region_SO", folderPath);
            _saveScriptableObject<Station_SO>("Station_SO", folderPath);

            // Refresh the AssetDatabase to show the changes
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
