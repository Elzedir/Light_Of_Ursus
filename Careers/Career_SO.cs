using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Careers
{
    [CreateAssetMenu(fileName = "Career_SO", menuName = "SOList/Career_SO")]
    [Serializable]
    public class Career_SO : Data_SO<Career_Data>
    {
        public Data<Career_Data>[] Careers                           => Data;
        public Data<Career_Data>   GetCareer_Data(CareerName careerName) => GetData((ulong)careerName);
        
        protected override Dictionary<ulong, Data<Career_Data>> _getDefaultData() => 
            _convertDictionaryToData(Career_List.DefaultCareers);
        
        protected override Data<Career_Data> _convertToData(Career_Data data)
        {
            return new Data<Career_Data>(
                dataID: (ulong)data.CareerName,
                data_Object: data,
                dataTitle: $"{(ulong)data.CareerName}: {data.CareerName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
    }
    
    [CustomEditor(typeof(Career_SO))]
    public class AllCareers_SOEditor : Data_SOEditor<Career_Data>
    {
        public override Data_SO<Career_Data> SO => _so ??= (Career_SO)target;
    }
}
