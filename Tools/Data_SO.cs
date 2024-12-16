using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools
{
    [Serializable]
    public abstract class Data_SO<T> : ScriptableObject where T : class
    {
        int                                                                       _dataObjects_CurrentLength;
        [SerializeField] Object_Data<T>[] _objects_Data;

        public Object_Data<T>[] Objects_Data
        {
            get
            {
                if (_objects_Data is not null && _objects_Data.Length != 0 && _objects_Data.Length == _dataObjects_CurrentLength) return _objects_Data;
                
                _dataObjects_CurrentLength = _objects_Data?.Length ?? 0;
                var dataObjects = InitialiseAllDataObjects();
                return dataObjects; 
            }
        }

        public void RefreshDataObjects() => _dataObjects_CurrentLength = 0;

        public void LoadSO(T[] dataObjects) => _objects_Data = dataObjects.Select(_convertToDataObject).ToArray();

        Dictionary<uint, int>        _dataObjectIndexLookup;
        public Dictionary<uint, int> DataObjectIndexLookup => _dataObjectIndexLookup ??= _buildIndexLookup();
        int                          _currentIndex;

        public bool ToggleMissingDataDebugs;

        public Object_Data<T>[] InitialiseAllDataObjects()
        {
            _objects_Data = new Object_Data<T>[DefaultDataObjects.Count * 2];
            Array.Copy(DefaultDataObjects.Values.ToArray(), Objects_Data, DefaultDataObjects.Count);
            _currentIndex = DefaultDataObjects.Count;
            _buildIndexLookup();
            
            return Objects_Data ?? throw new NullReferenceException("DataObjects is null.");
        }

        protected Dictionary<uint, int> _buildIndexLookup()
        {
            var newIndexLookup = new Dictionary<uint, int>();

            for (var i = 0; i < Objects_Data.Length; i++)
            {
                if (Objects_Data[i] is null) continue;

                newIndexLookup[GetDataObjectID(i)] = i;
            }

            return newIndexLookup;
        }

        public abstract uint GetDataObjectID(int id);

        public Object_Data<T> GetObject_Data(uint dataObjectID)
        {
            if (Objects_Data is null || Objects_Data.Length is 0) InitialiseAllDataObjects();

            if (DataObjectIndexLookup.TryGetValue(dataObjectID, out var index))
            {
                return Objects_Data?[index];
            }

            Debug.LogWarning($"DataObject {dataObjectID} does not exist in DataObjects.");
            return null;
        }

        public void AddDataObject(uint dataObjectID, Object_Data<T> objectData)
        {
            if (DataObjectIndexLookup.ContainsKey(dataObjectID))
            {
                Debug.LogWarning($"DataObject {dataObjectID} already exists in DataObjects.");
                return;
            }

            if (_currentIndex >= Objects_Data.Length)
            {
                _compactAndResizeArray();
            }

            Objects_Data[_currentIndex]          = objectData;
            DataObjectIndexLookup[dataObjectID] = _currentIndex;
            _currentIndex++;
        }

        public void RemoveDataObjects(uint dataObjectID)
        {
            if (!DataObjectIndexLookup.TryGetValue(dataObjectID, out var index))
            {
                Debug.LogWarning($"DataObject {dataObjectID} does not exist in DataObjects.");
                return;
            }

            Objects_Data[index] = null;
            DataObjectIndexLookup.Remove(dataObjectID);

            if (DataObjectIndexLookup.Count < Objects_Data.Length / 4)
            {
                _compactAndResizeArray();
            }
        }

        void _compactAndResizeArray()
        {
            var newSize = 0;

            for (var i = 0; i < Objects_Data.Length; i++)
            {
                if (Objects_Data[i] is null) continue;

                Objects_Data[newSize]                      = Objects_Data[i];
                DataObjectIndexLookup[GetDataObjectID(i)] = newSize;
                newSize++;
            }

            Array.Resize(ref _objects_Data, Math.Max(newSize * 2, Objects_Data.Length));
            _currentIndex = newSize;
        }

        public void UpdateAllDataObjects(Dictionary<uint, T> newDataObjects, bool clearDataFirst = false)
        {
            if (clearDataFirst) ClearSOData();

            foreach (var (key, value) in newDataObjects)
            {
                UpdateDataObject(key, value);
            }
        }

        public void UpdateDataObject(uint dataObjectID, T dataObject)
        {
            if (DataObjectIndexLookup.TryGetValue(dataObjectID, out var index))
            {
                Objects_Data[index] = _convertToDataObject(dataObject);
            }
            else
            {
                AddDataObject(dataObjectID, _convertToDataObject(dataObject));
            }
        }
        
        public             Vector2        ScrollPosition;
        protected abstract Object_Data<T> _convertToDataObject(T data);

        protected Dictionary<uint, Object_Data<T>> _convertDictionaryToDataObject(
            Dictionary<uint, T> dictionary)
        {
            return dictionary.ToDictionary(key => key.Key, value => _convertToDataObject(value.Value));
        }

        public void ClearSOData()
        {
            _objects_Data = Array.Empty<Object_Data<T>>();
            _dataObjectIndexLookup?.Clear();
            _currentIndex = 0;
        }

        protected abstract Dictionary<uint, Object_Data<T>> _populateDefaultDataObjects();

        Dictionary<uint, Object_Data<T>> _defaultDataObjects;

        public Dictionary<uint, Object_Data<T>> DefaultDataObjects =>
            _defaultDataObjects ??= _populateDefaultDataObjects();
    }

    [Serializable]
    public class Object_Data<T> where T : class
    {
        public readonly uint   DataObjectID;
        public readonly string DataObjectTitle;

        public T            DataObject;
        public Data_Display DataSO_Object;

        public Object_Data(uint   dataObjectID,    T      dataObject,
                           string dataObjectTitle, Data_Display data_Display)
        {
            DataObjectID       = dataObjectID;
            DataObject         = dataObject;
            DataObjectTitle    = dataObjectTitle;
            DataSO_Object = data_Display;
        }
    }

    [Serializable]
    public class Data_Display
    {
        public int     SelectedIndex = -1;
        public bool    ShowData;
        public Vector2 ScrollPosition;

        public readonly string              Title;
        public readonly DataDisplayType     DataDisplayType;
        public readonly List<string>        Data;
        public readonly List<Data_Display> SubData;
        
        public Data_Display(string title, DataDisplayType dataDisplayType, List<string> data = null, List<Data_Display> subData = null)
        {
            switch (dataDisplayType)
            {
                case DataDisplayType.CheckBoxList when data is null && subData is null:
                    throw new NullReferenceException("Data is null.");
                case DataDisplayType.Item when data is null:
                    throw new NullReferenceException("Data is null.");
                case DataDisplayType.SelectableList when subData is null:
                    throw new NullReferenceException("SubData is null.");
            }

            Title            = title;
            DataDisplayType  = dataDisplayType;
            Data             = data;
            SubData          = subData;
        }
    }

    public enum DataDisplayType
    {
        Item,
        CheckBoxList,
        SelectableList
    }
}