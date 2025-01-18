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
        public Data<Job_Data>[] Jobs                           => Data;
        public Data<Job_Data>   GetJob_Master(JobName jobName) => GetData((uint)jobName);

        public override uint GetDataID(int id) => (uint)Jobs[id].Data_Object.JobName;
        
        protected override Dictionary<uint, Data<Job_Data>> _getDefaultData() => 
            _convertDictionaryToData(Job_List.DefaultJobs);
        
        protected override Data<Job_Data> _convertToData(Job_Data data)
        {
            return new Data<Job_Data>(
                dataID: (uint)data.JobName,
                data_Object: data,
                dataTitle: $"{(uint)data.JobName}: {data.JobName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
    }

    [CustomEditor(typeof(Job_SO))]
    public class AllJobs_SOEditor : Data_SOEditor<Job_Data>
    {
        public override Data_SO<Job_Data> SO => _so ??= (Job_SO)target;
    }
}