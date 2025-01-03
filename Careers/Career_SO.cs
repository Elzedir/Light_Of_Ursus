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
        public Object_Data<Career_Data>[] Careers                           => Objects_Data;
        public Object_Data<Career_Data>   GetCareer_Master(CareerName careerName) => GetObject_Data((uint)careerName);

        public override uint GetDataObjectID(int id) => (uint)Careers[id].DataObject.CareerName;

        public override void PopulateSceneData()
        {
            if (_defaultCareers.Count == 0)
            {
                Debug.Log("No Default Careers Found");
            }
        }
        protected override Dictionary<uint, Object_Data<Career_Data>> _getDefaultDataObjects(bool initialisation = false)
        {
            return _convertDictionaryToDataObject(Career_List.DefaultCareers);
        }
        
        Dictionary<uint, Object_Data<Career_Data>> _defaultCareers => DefaultDataObjects;
        
        protected override Object_Data<Career_Data> _convertToDataObject(Career_Data dataObject)
        {
            return new Object_Data<Career_Data>(
                dataObjectID: (uint)dataObject.CareerName,
                dataObject: dataObject,
                dataObjectTitle: $"{(uint)dataObject.CareerName}: {dataObject.CareerName}",
                getData_Display: dataObject.GetDataSO_Object);
        }
    }
    
    [CustomEditor(typeof(Career_SO))]
    public class AllCareers_SOEditor : Data_SOEditor<Career_Data>
    {
        public override Data_SO<Career_Data> SO => _so ??= (Career_SO)target;
    }
}
