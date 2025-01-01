using System;
using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Jobs
{
    [CreateAssetMenu(fileName = "Job_SO", menuName = "SOList/Job_SO")]
    [Serializable]
    public class Job_SO : Data_SO<Job_Data>
    {
        public Object_Data<Job_Data>[] Jobs                           => Objects_Data;
        public Object_Data<Job_Data>   GetJob_Master(JobName jobName) => GetObject_Data((uint)jobName);

        public override uint GetDataObjectID(int id) => (uint)Jobs[id].DataObject.JobName;

        public override void PopulateSceneData()
        {
            if (_defaultJobs.Count == 0)
            {
                Debug.Log("No Default Jobs Found");
            }
        }
        
        protected override Dictionary<uint, Object_Data<Job_Data>> _getDefaultDataObjects()
        {
            var defaultJobs = new Dictionary<uint, Job_Data>();

            foreach (var item in Job_List.GetAllDefaultJobs())
            {
                defaultJobs.Add(item.Key, item.Value);
            }

            return _convertDictionaryToDataObject(defaultJobs);
        }
        
        Dictionary<uint, Object_Data<Job_Data>> _defaultJobs => DefaultDataObjects;
        
        protected override Object_Data<Job_Data> _convertToDataObject(Job_Data data)
        {
            return new Object_Data<Job_Data>(
                dataObjectID: (uint)data.JobName,
                dataObject: data,
                dataObjectTitle: $"{(uint)data.JobName}: {data.JobName}",
                data_Display: data.GetDataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(Job_SO))]
    public class AllJobs_SOEditor : Data_SOEditor<Job_Data>
    {
        public override Data_SO<Job_Data> SO => _so ??= (Job_SO)target;
    }
}