using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Career
{
    [CreateAssetMenu(fileName = "Career_SO", menuName = "SOList/Career_SO")]
    [Serializable]
    public class Career_SO : Data_SO<Career_Master>
    {
        public Data_Object<Career_Master>[] Careers                           => DataObjects;
        public Data_Object<Career_Master>   GetCareer_Master(CareerName careerName) => GetDataObject_Master((uint)careerName);

        public override uint GetDataObjectID(int id) => (uint)Careers[id].DataObject.CareerName;

        public void PopulateDefaultCareers()
        {
            if (_defaultCareers.Count == 0)
            {
                Debug.Log("No Default Careers Found");
            }
        }
        protected override Dictionary<uint, Data_Object<Career_Master>> _populateDefaultDataObjects()
        {
            var defaultCareers = new Dictionary<uint, Career_Master>();

            foreach (var item in Career_List.GetAllDefaultCareers())
            {
                defaultCareers.Add(item.Key, item.Value);
            }

            return _convertDictionaryToDataObject(defaultCareers);
        }
        
        Dictionary<uint, Data_Object<Career_Master>> _defaultCareers => DefaultDataObjects;
        
        protected override Data_Object<Career_Master> _convertToDataObject(Career_Master data)
        {
            return new Data_Object<Career_Master>(
                dataObjectID: (uint)data.CareerName,
                dataObject: data,
                dataObjectTitle: $"{(uint)data.CareerName}{data.CareerName}",
                dataSO_Object: data.DataSO_Object);
        }
    }
    
    [CustomEditor(typeof(Career_SO))]
    public class AllCareers_SOEditor : Editor
    {
        
    }
}
