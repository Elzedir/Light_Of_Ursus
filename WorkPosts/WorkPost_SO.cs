using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tools;
using UnityEditor;
using UnityEngine;

namespace WorkPosts
{
    [CreateAssetMenu(fileName = "WorkPost_SO", menuName = "SOList/WorkPost_SO")]
    [Serializable]
    public class WorkPost_SO : Data_SO<WorkPost_Data>
    {
        // Not really a point to this class?
        
        public Data<WorkPost_Data>[]         WorkPosts                            => Data;
        public Data<WorkPost_Data>           GetWorkPost_Data(uint      workPostID) => GetData(workPostID);
        public Dictionary<uint, WorkPost_Component> WorkPostComponents = new();

        public WorkPost_Component GetWorkPost_Component(uint workPostID)
        {
            if (WorkPostComponents.TryGetValue(workPostID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"WorkPost with ID {workPostID} not found in WorkPost_SO.");
            return null;
        }

        public override uint GetDataID(int id) => WorkPosts[id].Data_Object.WorkPostID;

        public void UpdateWorkPost(uint workPostID, WorkPost_Data workPost_Data) => UpdateData(workPostID, workPost_Data);
        public void UpdateAllWorkPosts(Dictionary<uint, WorkPost_Data> allWorkPosts) => UpdateAllData(allWorkPosts);

        public override void PopulateSceneData()
        {
            if (_defaultWorkPosts.Count == 0)
            {
                Debug.Log("No Default WorkPosts Found");
            }
            
            var existingWorkPosts = FindObjectsByType<WorkPost_Component>(FindObjectsSortMode.None)
                                     .Where(station => Regex.IsMatch(station.name, @"\d+"))
                                     .ToDictionary(
                                         station => uint.Parse(new string(station.name.Where(char.IsDigit).ToArray())),
                                         station => station
                                     );
            
            foreach (var workPost in WorkPosts)
            {
                if (workPost?.Data_Object is null) continue;
                
                if (existingWorkPosts.TryGetValue(workPost.Data_Object.WorkPostID, out var existingWorkPost))
                {
                    WorkPostComponents[workPost.Data_Object.WorkPostID] = existingWorkPost;
                    existingWorkPost.SetWorkPostData(workPost.Data_Object);
                    existingWorkPosts.Remove(workPost.Data_Object.WorkPostID);
                    continue;
                }
                
                Debug.LogWarning($"WorkPost with ID {workPost.Data_Object.WorkPostID} not found in the scene.");
            }
            
            foreach (var workPost in existingWorkPosts)
            {
                if (DataIndexLookup.ContainsKey(workPost.Key))
                {
                    Debug.LogWarning($"WorkPost with ID {workPost.Key} wasn't removed from existingWorkPosts.");
                    continue;
                }
                
                Debug.LogWarning($"WorkPost with ID {workPost.Key} does not have DataObject in WorkPost_SO.");
            }
        }

        protected override Dictionary<uint, Data<WorkPost_Data>> _getDefaultData(bool initialisation = false)
        {
            // No default work stations for now.
            
            var defaultWorkPosts = new Dictionary<uint, WorkPost_Data>();

            // foreach (var defaultWorkPost in WorkPost_List.DefaultWorkPosts)
            // {
            //     defaultWorkPosts.Add(defaultWorkPost.Key, defaultWorkPost.Value);
            // }

            return _convertDictionaryToData(defaultWorkPosts);
        }

        static uint _lastUnusedWorkPostID = 1;

        public uint GetUnusedWorkPostID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedWorkPostID))
            {
                _lastUnusedWorkPostID++;
            }

            return _lastUnusedWorkPostID;
        }
        
        Dictionary<uint, Data<WorkPost_Data>> _defaultWorkPosts => DefaultData;
         
        protected override Data<WorkPost_Data> _convertToData(WorkPost_Data data)
        {
            return new Data<WorkPost_Data>(
                dataID: data.WorkPostID, 
                data_Object: data,
                dataTitle: $"WorkPost: {data.WorkPostID}",
                getData_Display: data.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(WorkPost_SO))]
    public class WorkPost_SOEditor : Data_SOEditor<WorkPost_Data>
    {
        public override Data_SO<WorkPost_Data> SO => _so ??= (WorkPost_SO)target;
    }
}