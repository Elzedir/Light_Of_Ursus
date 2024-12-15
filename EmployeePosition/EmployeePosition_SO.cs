using System;
using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;

namespace EmployeePosition
{
    [CreateAssetMenu(fileName = "EmployeePosition_SO", menuName = "SOList/EmployeePosition_SO")]
    [Serializable]
    public class EmployeePosition_SO : Data_SO<EmployeePosition_Data>
    {
        public Data_Object<EmployeePosition_Data>[] EmployeePositions                           => DataObjects;

        public Data_Object<EmployeePosition_Data> GetEmployeePosition_Master(EmployeePositionName employeePositionName) =>
            GetDataObject_Master((uint)employeePositionName);
        
        public override uint GetDataObjectID(int id) => (uint)EmployeePositions[id].DataObject.EmployeePositionName;

        public void PopulateDefaultEmployeePositions()
        {
            if (_defaultEmployeePositions.Count == 0)
            {
                Debug.Log("No Default EmployeePosition Positions Found");
            }
        }
        protected override Dictionary<uint, Data_Object<EmployeePosition_Data>> _populateDefaultDataObjects()
        {
            var defaultEmployeePositions = new Dictionary<uint, EmployeePosition_Data>();

            foreach (var defaultEmployeePosition in EmployeePosition_List.GetAllDefaultEmployeePositions())
            {
                defaultEmployeePositions.Add(defaultEmployeePosition.Key, defaultEmployeePosition.Value);
            }

            return _convertDictionaryToDataObject(defaultEmployeePositions);
        }
        
        Dictionary<uint, Data_Object<EmployeePosition_Data>> _defaultEmployeePositions => DefaultDataObjects;
        
        protected override Data_Object<EmployeePosition_Data> _convertToDataObject(EmployeePosition_Data data)
        {
            return new Data_Object<EmployeePosition_Data>(
                dataObjectID: (uint)data.EmployeePositionName,
                dataObject: data,
                dataObjectTitle: $"{(uint)data.EmployeePositionName}: {data.EmployeePositionName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }
    }
    
    [CustomEditor(typeof(EmployeePosition_SO))]
    public class AllEmployeePositions_SOEditor : Data_SOEditor<EmployeePosition_Data>
    {
        public override Data_SO<EmployeePosition_Data> SO => _so ??= (EmployeePosition_SO)target;
    }
}