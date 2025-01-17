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
        public Data<WorkPost_Data>           GetWorkPost_Data(uint      workPostID) => GetData(workPostID);
        public Dictionary<uint, WorkPost_Component> WorkPostComponents => _getSceneComponents();

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

        protected override Dictionary<uint, Data<WorkPost_Data>> _getDefaultData() => 
            _convertDictionaryToData(new Dictionary<uint, WorkPost_Data>());

        protected override Dictionary<uint, Data<WorkPost_Data>> _getSavedData() =>
            _convertDictionaryToData(new Dictionary<uint, WorkPost_Data>());

        protected override Dictionary<uint, Data<WorkPost_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.WorkPostData));

        static uint _lastUnusedWorkPostID = 1;

        public uint GetUnusedWorkPostID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedWorkPostID))
            {
                _lastUnusedWorkPostID++;
            }

            return _lastUnusedWorkPostID;
        }
         
        protected override Data<WorkPost_Data> _convertToData(WorkPost_Data data)
        {
            return new Data<WorkPost_Data>(
                dataID: data.WorkPostID, 
                data_Object: data,
                dataTitle: $"WorkPost: {data.WorkPostID}",
                getDataToDisplay: data.GetData_Display);
        }

        public override void SaveData(SaveData saveData) { }
    }

    [CustomEditor(typeof(WorkPost_SO))]
    public class WorkPost_SOEditor : Data_SOEditor<WorkPost_Data>
    {
        public override Data_SO<WorkPost_Data> SO => _so ??= (WorkPost_SO)target;
    }
}