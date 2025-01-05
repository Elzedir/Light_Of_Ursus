using System;
using System.Collections.Generic;
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
        public Data<Career_Data>   GetCareer_Master(CareerName careerName) => GetData((uint)careerName);

        public override uint GetDataID(int id) => (uint)Careers[id].Data_Object.CareerName;

        public override void PopulateSceneData()
        {
            if (_defaultCareers.Count == 0)
            {
                Debug.Log("No Default Careers Found");
            }
        }
        protected override Dictionary<uint, Data<Career_Data>> _getDefaultData(bool initialisation = false)
        {
            return _convertDictionaryToData(Career_List.DefaultCareers);
        }
        
        Dictionary<uint, Data<Career_Data>> _defaultCareers => DefaultData;
        
        protected override Data<Career_Data> _convertToData(Career_Data data)
        {
            return new Data<Career_Data>(
                dataID: (uint)dataObject.CareerName,
                data_Object: dataObject,
                dataTitle: $"{(uint)dataObject.CareerName}: {dataObject.CareerName}",
                getData_Display: dataObject.GetDataSO_Object);
        }
    }
    
    [CustomEditor(typeof(Career_SO))]
    public class AllCareers_SOEditor : Data_SOEditor<Career_Data>
    {
        public override Data_SO<Career_Data> SO => _so ??= (Career_SO)target;
    }
}
