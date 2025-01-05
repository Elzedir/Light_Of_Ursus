using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Jobs
{
    [CreateAssetMenu(fileName = "Job_SO", menuName = "SOList/Job_SO")]
    [Serializable]
    public class Job_SO : Data_SO<Job_Data>
    {
        public Data<Job_Data>[] Jobs                           => Data;
        public Data<Job_Data>   GetJob_Master(JobName jobName) => GetData((uint)jobName);

        public override uint GetDataID(int id) => (uint)Jobs[id].Data_Object.JobName;

        public override void PopulateSceneData()
        {
            if (_defaultJobs.Count == 0)
            {
                Debug.Log("No Default Jobs Found");
            }
        }
        
        protected override Dictionary<uint, Data<Job_Data>> _getDefaultData(bool initialisation = false)
        {
            return _convertDictionaryToData(Job_List.DefaultJobs);
        }
        
        Dictionary<uint, Data<Job_Data>> _defaultJobs => DefaultData;
        
        protected override Data<Job_Data> _convertToData(Job_Data data)
        {
            return new Data<Job_Data>(
                dataID: (uint)data.JobName,
                data_Object: data,
                dataTitle: $"{(uint)data.JobName}: {data.JobName}",
                getData_Display: data.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(Job_SO))]
    public class AllJobs_SOEditor : Data_SOEditor<Job_Data>
    {
        public override Data_SO<Job_Data> SO => _so ??= (Job_SO)target;
    }
}