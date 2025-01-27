using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace WorkPosts
{
    [CreateAssetMenu(fileName = "WorkPost_SO", menuName = "SOList/WorkPost_SO")]
    [Serializable]
    public class WorkPost_SO : Data_Component_SO<WorkPost_Data, WorkPost_Component>
    {
        // Not really a point to this class?
        
        public Data<WorkPost_Data>[]         WorkPosts                            => Data;
        public Data<WorkPost_Data>           GetWorkPost_Data(ulong      workPostID) => GetData(workPostID);
        public Dictionary<ulong, WorkPost_Component> WorkPostComponents => _getSceneComponents();

        public WorkPost_Component GetWorkPost_Component(ulong workPostID)
        {
            if (WorkPostComponents.TryGetValue(workPostID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"WorkPost with ID {workPostID} not found in WorkPost_SO.");
            return null;
        }

        public void UpdateWorkPost(ulong workPostID, WorkPost_Data workPost_Data) => UpdateData(workPostID, workPost_Data);
        public void UpdateAllWorkPosts(Dictionary<ulong, WorkPost_Data> allWorkPosts) => UpdateAllData(allWorkPosts);

        protected override Dictionary<ulong, Data<WorkPost_Data>> _getDefaultData() => 
            _convertDictionaryToData(new Dictionary<ulong, WorkPost_Data>());

        protected override Dictionary<ulong, Data<WorkPost_Data>> _getSavedData() =>
            _convertDictionaryToData(new Dictionary<ulong, WorkPost_Data>());

        protected override Dictionary<ulong, Data<WorkPost_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.WorkPostData));
         
        protected override Data<WorkPost_Data> _convertToData(WorkPost_Data data)
        {
            return new Data<WorkPost_Data>(
                dataID: data.WorkPostID, 
                data_Object: data,
                dataTitle: $"WorkPost: {data.WorkPostID}",
                getDataToDisplay: data.GetDataToDisplay);
        }

        public override void SaveData(Save_Data saveData) { }
    }

    [CustomEditor(typeof(WorkPost_SO))]
    public class WorkPost_SOEditor : Data_SOEditor<WorkPost_Data>
    {
        public override Data_SO<WorkPost_Data> SO => _so ??= (WorkPost_SO)target;
    }
}