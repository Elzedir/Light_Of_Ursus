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
        public Data<Job_Data>   GetJob_Data(JobName jobName) => GetData((ulong)jobName);
        
        protected override Dictionary<ulong, Data<Job_Data>> _getDefaultData() => 
            _convertDictionaryToData(Job_List.DefaultJobs);
        
        protected override Data<Job_Data> _convertToData(Job_Data data)
        {
            return new Data<Job_Data>(
                dataID: (ulong)data.JobName,
                data_Object: data,
                dataTitle: $"{(ulong)data.JobName}: {data.JobName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
    }

    [CustomEditor(typeof(Job_SO))]
    public class AllJobs_SOEditor : Data_SOEditor<Job_Data>
    {
        public override Data_SO<Job_Data> SO => _so ??= (Job_SO)target;
    }
}