using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tools
{
    [Serializable]
    public abstract class Data_SO<T> : ScriptableObject where T : class
    {
        int                                                                      _dataObjectLength;
        [SerializeField] Data_Object<T>[] _dataObjects;

        public Data_Object<T>[] DataObjects
        {
            get
            {
                if (_dataObjects is not null && _dataObjects.Length != 0 && _dataObjects.Length == _dataObjectLength) return _dataObjects;
                
                _dataObjectLength = _dataObjects?.Length ?? 0;
                var dataObjects = InitialiseAllDataObjects();
                return dataObjects; 
            }
        }

        public void RefreshDataObjects() => _dataObjectLength = 0;

        public void LoadSO(T[] dataObjects) => _dataObjects = dataObjects.Select(_convertToDataObject).ToArray();

        Dictionary<uint, int>        _dataObjectIndexLookup;
        public Dictionary<uint, int> DataObjectIndexLookup => _dataObjectIndexLookup ??= _buildIndexLookup();
        int                          _currentIndex;

        public Data_Object<T>[] InitialiseAllDataObjects()
        {
            _dataObjects = new Data_Object<T>[DefaultDataObjects.Count * 2];
            Array.Copy(DefaultDataObjects.Values.ToArray(), DataObjects, DefaultDataObjects.Count);
            _currentIndex = DefaultDataObjects.Count;
            _buildIndexLookup();
            
            return DataObjects ?? throw new NullReferenceException("DataObjects is null.");
        }

        protected Dictionary<uint, int> _buildIndexLookup()
        {
            var newIndexLookup = new Dictionary<uint, int>();

            for (var i = 0; i < DataObjects.Length; i++)
            {
                if (DataObjects[i] is null) continue;

                newIndexLookup[GetDataObjectID(i)] = i;
            }

            return newIndexLookup;
        }

        public abstract uint GetDataObjectID(int id);

        public Data_Object<T> GetDataObject_Master(uint dataObjectID)
        {
            if (DataObjects is null || DataObjects.Length is 0) InitialiseAllDataObjects();

            if (DataObjectIndexLookup.TryGetValue(dataObjectID, out var index))
            {
                return DataObjects?[index];
            }

            Debug.LogWarning($"DataObject {dataObjectID} does not exist in DataObjects.");
            return null;
        }

        public void AddDataObject(uint dataObjectID, Data_Object<T> dataObject)
        {
            if (DataObjectIndexLookup.ContainsKey(dataObjectID))
            {
                Debug.LogWarning($"DataObject {dataObjectID} already exists in DataObjects.");
                return;
            }

            if (_currentIndex >= DataObjects.Length)
            {
                _compactAndResizeArray();
            }

            DataObjects[_currentIndex]          = dataObject;
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

            DataObjects[index] = null;
            DataObjectIndexLookup.Remove(dataObjectID);

            if (DataObjectIndexLookup.Count < DataObjects.Length / 4)
            {
                _compactAndResizeArray();
            }
        }

        void _compactAndResizeArray()
        {
            var newSize = 0;

            for (var i = 0; i < DataObjects.Length; i++)
            {
                if (DataObjects[i] is null) continue;

                DataObjects[newSize]                      = DataObjects[i];
                DataObjectIndexLookup[GetDataObjectID(i)] = newSize;
                newSize++;
            }

            Array.Resize(ref _dataObjects, Math.Max(newSize * 2, DataObjects.Length));
            _currentIndex = newSize;
        }

        public void UpdateAllDataObjects(Dictionary<uint, T> newDataObjects, bool clearDataFirst = false)
        {
            if (clearDataFirst) ClearDataObjectData();

            foreach (var (key, value) in newDataObjects)
            {
                UpdateDataObject(key, value);
            }
        }

        public void UpdateDataObject(uint dataObjectID, T dataObject)
        {
            if (DataObjectIndexLookup.TryGetValue(dataObjectID, out var index))
            {
                DataObjects[index] = _convertToDataObject(dataObject);
            }
            else
            {
                AddDataObject(dataObjectID, _convertToDataObject(dataObject));
            }
        }
        
        [FormerlySerializedAs("DataScrollPosition")] public Vector2        ScrollPosition;
        protected abstract                                  Data_Object<T> _convertToDataObject(T data);

        protected Dictionary<uint, Data_Object<T>> _convertDictionaryToDataObject(
            Dictionary<uint, T> dictionary)
        {
            return dictionary.ToDictionary(key => key.Key, value => _convertToDataObject(value.Value));
        }

        public void ClearDataObjectData()
        {
            _dataObjects = Array.Empty<Data_Object<T>>();
            DataObjectIndexLookup.Clear();
            _currentIndex = 0;
        }

        protected abstract Dictionary<uint, Data_Object<T>> _populateDefaultDataObjects();

        Dictionary<uint, Data_Object<T>> _defaultDataObjects;

        public Dictionary<uint, Data_Object<T>> DefaultDataObjects =>
            _defaultDataObjects ??= _populateDefaultDataObjects();
    }

    [Serializable]

    public abstract class Data_Class
    {
        DataSO_Object        _dataSO_Object;
        public DataSO_Object DataSO_Object => _dataSO_Object ??= _getDataSO_Object();

        protected abstract DataSO_Object _getDataSO_Object();
    }

    [Serializable]
    public class Data_Object<T> where T : class
    {
        public readonly uint                            DataObjectID;
        public readonly string                          DataObjectTitle;

        public T             DataObject;
        public DataSO_Object DataSO_Object;

        public Data_Object(uint   dataObjectID,    T      dataObject,
                           string dataObjectTitle, DataSO_Object dataSO_Object)
        {
            DataObjectID       = dataObjectID;
            DataObject         = dataObject;
            DataObjectTitle    = dataObjectTitle;
            DataSO_Object = dataSO_Object;
        }
    }

    [Serializable]
    public class DataSO_Object
    {
        public int     SelectedIndex = -1;
        public bool    ShowData;
        public Vector2 ScrollPosition;

        public readonly string              Title;
        public readonly DataDisplayType     DataDisplayType;
        public readonly List<string>        Data;
        public readonly List<DataSO_Object> SubData;
        
        public DataSO_Object(string title, DataDisplayType dataDisplayType, List<string> data = null, List<DataSO_Object> subData = null)
        {
            switch (dataDisplayType)
            {
                case DataDisplayType.List when data is null && subData is null:
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
        List,
        SelectableList
    }
}