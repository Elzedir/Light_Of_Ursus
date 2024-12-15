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
        public Data_Object<Career_Data>[] Careers                           => DataObjects;
        public Data_Object<Career_Data>   GetCareer_Master(CareerName careerName) => GetDataObject_Master((uint)careerName);

        public override uint GetDataObjectID(int id) => (uint)Careers[id].DataObject.CareerName;

        public void PopulateDefaultCareers()
        {
            if (_defaultCareers.Count == 0)
            {
                Debug.Log("No Default Careers Found");
            }
        }
        protected override Dictionary<uint, Data_Object<Career_Data>> _populateDefaultDataObjects()
        {
            var defaultCareers = new Dictionary<uint, Career_Data>();

            foreach (var item in Career_List.GetAllDefaultCareers())
            {
                defaultCareers.Add(item.Key, item.Value);
            }

            return _convertDictionaryToDataObject(defaultCareers);
        }
        
        Dictionary<uint, Data_Object<Career_Data>> _defaultCareers => DefaultDataObjects;
        
        protected override Data_Object<Career_Data> _convertToDataObject(Career_Data data)
        {
            return new Data_Object<Career_Data>(
                dataObjectID: (uint)data.CareerName,
                dataObject: data,
                dataObjectTitle: $"{(uint)data.CareerName}: {data.CareerName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }
    }
    
    [CustomEditor(typeof(Career_SO))]
    public class AllCareers_SOEditor : Data_SOEditor<Career_Data>
    {
        public override Data_SO<Career_Data> SO => _so ??= (Career_SO)target;
    }
}
