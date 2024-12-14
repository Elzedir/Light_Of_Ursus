using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools
{
    [Serializable]
    public abstract class Data_SO<T> : ScriptableObject where T : class
    {
        int                               _baseObjectLength;
        [SerializeField] Data_Object<T>[] _baseObjects;

        public Data_Object<T>[] DataObjects
        {
            get
            {
                if (_baseObjects is not null && _baseObjects.Length != 0 && _baseObjects.Length == _baseObjectLength) return _baseObjects;
                
                _baseObjectLength = _baseObjects?.Length ?? 0;
                var baseObjects = InitialiseAllBaseObjects();
                return baseObjects; 
            }
        }

        public void RefreshBaseObjects() => _baseObjectLength = 0;

        public void LoadSO(T[] baseObjects) => _baseObjects = baseObjects.Select(_convertToBaseObject).ToArray();

        Dictionary<uint, int>        _baseObjectIndexLookup;
        public Dictionary<uint, int> BaseObjectIndexLookup => _baseObjectIndexLookup ??= _buildIndexLookup();
        int                          _currentIndex;

        public Data_Object<T>[] InitialiseAllBaseObjects()
        {
            _baseObjects = new Data_Object<T>[DefaultBaseObjects.Count * 2];
            Array.Copy(DefaultBaseObjects.Values.ToArray(), DataObjects, DefaultBaseObjects.Count);
            _currentIndex = DefaultBaseObjects.Count;
            _buildIndexLookup();
            
            return DataObjects ?? throw new NullReferenceException("BaseObjects is null.");
        }

        protected Dictionary<uint, int> _buildIndexLookup()
        {
            var newIndexLookup = new Dictionary<uint, int>();

            for (var i = 0; i < DataObjects.Length; i++)
            {
                if (DataObjects[i] is null) continue;

                newIndexLookup[GetBaseObjectID(i)] = i;
            }

            return newIndexLookup;
        }

        public abstract uint GetBaseObjectID(int id);

        public Data_Object<T> GetBaseObject_Master(uint baseObjectID)
        {
            if (DataObjects is null || DataObjects.Length is 0) InitialiseAllBaseObjects();

            if (BaseObjectIndexLookup.TryGetValue(baseObjectID, out var index))
            {
                return DataObjects?[index];
            }

            Debug.LogWarning($"BaseObject {baseObjectID} does not exist in BaseObjects.");
            return null;
        }

        public void AddBaseObject(uint baseObjectID, Data_Object<T> dataObject)
        {
            if (BaseObjectIndexLookup.ContainsKey(baseObjectID))
            {
                Debug.LogWarning($"BaseObject {baseObjectID} already exists in BaseObjects.");
                return;
            }

            if (_currentIndex >= DataObjects.Length)
            {
                _compactAndResizeArray();
            }

            DataObjects[_currentIndex]          = dataObject;
            BaseObjectIndexLookup[baseObjectID] = _currentIndex;
            _currentIndex++;
        }

        public void RemoveBaseObjects(uint baseObjectID)
        {
            if (!BaseObjectIndexLookup.TryGetValue(baseObjectID, out var index))
            {
                Debug.LogWarning($"BaseObject {baseObjectID} does not exist in BaseObjects.");
                return;
            }

            DataObjects[index] = null;
            BaseObjectIndexLookup.Remove(baseObjectID);

            if (BaseObjectIndexLookup.Count < DataObjects.Length / 4)
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
                BaseObjectIndexLookup[GetBaseObjectID(i)] = newSize;
                newSize++;
            }

            Array.Resize(ref _baseObjects, Math.Max(newSize * 2, DataObjects.Length));
            _currentIndex = newSize;
        }

        public void UpdateAllBaseObjects(Dictionary<uint, T> newBaseObjects, bool clearDataFirst = false)
        {
            if (clearDataFirst) ClearBaseObjectData();

            foreach (var (key, value) in newBaseObjects)
            {
                UpdateBaseObject(key, value);
            }
        }

        public void UpdateBaseObject(uint baseObjectID, T baseObject)
        {
            if (BaseObjectIndexLookup.TryGetValue(baseObjectID, out var index))
            {
                DataObjects[index] = _convertToBaseObject(baseObject);
            }
            else
            {
                AddBaseObject(baseObjectID, _convertToBaseObject(baseObject));
            }
        }
        
        public             Vector2                         BaseScrollPosition;
        protected abstract Data_Object<T>                  _convertToBaseObject(T data);

        protected Dictionary<uint, Data_Object<T>> _convertDictionaryToBaseObject(
            Dictionary<uint, T> dictionary)
        {
            return dictionary.ToDictionary(key => key.Key, value => _convertToBaseObject(value.Value));
        }

        public void ClearBaseObjectData()
        {
            _baseObjects = Array.Empty<Data_Object<T>>();
            BaseObjectIndexLookup.Clear();
            _currentIndex = 0;
        }

        protected abstract Dictionary<uint, Data_Object<T>> _populateDefaultBaseObjects();

        Dictionary<uint, Data_Object<T>> _defaultBaseObjects;

        public Dictionary<uint, Data_Object<T>> DefaultBaseObjects =>
            _defaultBaseObjects ??= _populateDefaultBaseObjects();
    }

    [Serializable]
    public class Data_Object<T> where T : class
    {
        public readonly uint                            DataObjectID;
        public readonly string                          DataObjectTitle;

        public T      DataObject;
        public Action DisplayData;

        public Data_Object(uint   dataObjectID,    T      dataObject,
                           string dataObjectTitle, Action displayData)
        {
            DataObjectID     = dataObjectID;
            DataObject       = dataObject;
            DataObjectTitle  = dataObjectTitle;
            DisplayData = displayData;
        }
    }

    public class DataSO_Object
    {
        public          int             SelectedIndex = -1;
        public          bool            ShowData;
        public          Vector2         ScrollPosition;

        public readonly string               Title;
        public readonly List<DataSO_Category> Data;

        public DataSO_Object(string title, List<DataSO_Category> data)
        {
            Title     = title;
            Data = data;
        }
    }

    public class DataSO_Category
    {
        public int     SelectedIndex = -1;
        public bool    ShowData;
        public Vector2 ScrollPosition;

        public readonly string                   CategoryTitle;
        public readonly DataDisplayType          DataDisplayType;
        public readonly Action DisplayData;

        public DataSO_Category(string categoryTitle, DataDisplayType dataDisplayType, Action displayData)
        {
            CategoryTitle           = categoryTitle;
            DataDisplayType = dataDisplayType;
            DisplayData     = displayData;
        }
    }

    public enum DataDisplayType
    {
        Item,
        List,
        SelectableList
    }
}