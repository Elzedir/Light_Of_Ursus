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
        
        public Object_Data<WorkPost_Data>[]         WorkPosts                            => Objects_Data;
        public Object_Data<WorkPost_Data>           GetWorkPost_Data(uint      workPostID) => GetObject_Data(workPostID);
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

        public override uint GetDataObjectID(int id) => WorkPosts[id].DataObject.WorkPostID;

        public void UpdateWorkPost(uint workPostID, WorkPost_Data workPost_Data) => UpdateDataObject(workPostID, workPost_Data);
        public void UpdateAllWorkPosts(Dictionary<uint, WorkPost_Data> allWorkPosts) => UpdateAllDataObjects(allWorkPosts);

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
                if (workPost?.DataObject is null) continue;
                
                if (existingWorkPosts.TryGetValue(workPost.DataObject.WorkPostID, out var existingWorkPost))
                {
                    WorkPostComponents[workPost.DataObject.WorkPostID] = existingWorkPost;
                    existingWorkPost.SetWorkPostData(workPost.DataObject);
                    existingWorkPosts.Remove(workPost.DataObject.WorkPostID);
                    continue;
                }
                
                Debug.LogWarning($"WorkPost with ID {workPost.DataObject.WorkPostID} not found in the scene.");
            }
            
            foreach (var workPost in existingWorkPosts)
            {
                if (DataObjectIndexLookup.ContainsKey(workPost.Key))
                {
                    Debug.LogWarning($"WorkPost with ID {workPost.Key} wasn't removed from existingWorkPosts.");
                    continue;
                }
                
                Debug.LogWarning($"WorkPost with ID {workPost.Key} does not have DataObject in WorkPost_SO.");
            }
        }

        protected override Dictionary<uint, Object_Data<WorkPost_Data>> _getDefaultDataObjects(bool initialisation = false)
        {
            // No default work stations for now.
            
            var defaultWorkPosts = new Dictionary<uint, WorkPost_Data>();

            // foreach (var defaultWorkPost in WorkPost_List.DefaultWorkPosts)
            // {
            //     defaultWorkPosts.Add(defaultWorkPost.Key, defaultWorkPost.Value);
            // }

            return _convertDictionaryToDataObject(defaultWorkPosts);
        }

        static uint _lastUnusedWorkPostID = 1;

        public uint GetUnusedWorkPostID()
        {
            while (DataObjectIndexLookup.ContainsKey(_lastUnusedWorkPostID))
            {
                _lastUnusedWorkPostID++;
            }

            return _lastUnusedWorkPostID;
        }
        
        Dictionary<uint, Object_Data<WorkPost_Data>> _defaultWorkPosts => DefaultDataObjects;
         
        protected override Object_Data<WorkPost_Data> _convertToDataObject(WorkPost_Data dataObject)
        {
            return new Object_Data<WorkPost_Data>(
                dataObjectID: dataObject.WorkPostID, 
                dataObject: dataObject,
                dataObjectTitle: $"WorkPost: {dataObject.WorkPostID}",
                getData_Display: dataObject.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(WorkPost_SO))]
    public class WorkPost_SOEditor : Data_SOEditor<WorkPost_Data>
    {
        public override Data_SO<WorkPost_Data> SO => _so ??= (WorkPost_SO)target;
    }
}