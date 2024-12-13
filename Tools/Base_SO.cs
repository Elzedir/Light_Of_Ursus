using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools
{
    [Serializable]
    public abstract class Base_SO_Test<T> : ScriptableObject where T : class
    {
        int                               _baseObjectLength;
        [SerializeField] Base_Object<T>[] _baseObjects;

        public Base_Object<T>[] BaseObjects
        {
            get
            {
                if (_baseObjects is not null && _baseObjects.Length != 0 && _baseObjects.Length == _baseObjectLength) return _baseObjects;
                
                _baseObjectLength = _baseObjects?.Length ?? 0;
                return InitialiseAllBaseObjects();
            }
        }

        public void RefreshBaseObjects() => _baseObjectLength = 0;

        public void LoadSO(T[] baseObjects) => _baseObjects = baseObjects.Select(_convertToBaseObject).ToArray();

        Dictionary<uint, int>        _baseObjectIndexLookup;
        public Dictionary<uint, int> BaseObjectIndexLookup => _baseObjectIndexLookup ??= _buildIndexLookup();
        int                          _currentIndex;

        public Base_Object<T>[] InitialiseAllBaseObjects()
        {
            _baseObjects = new Base_Object<T>[DefaultBaseObjects.Count * 2];
            Array.Copy(DefaultBaseObjects.Values.ToArray(), BaseObjects, DefaultBaseObjects.Count);
            _currentIndex = DefaultBaseObjects.Count;
            _buildIndexLookup();
            return BaseObjects ?? throw new NullReferenceException("BaseObjects is null.");
        }

        protected Dictionary<uint, int> _buildIndexLookup()
        {
            var newIndexLookup = new Dictionary<uint, int>();

            for (var i = 0; i < BaseObjects.Length; i++)
            {
                if (BaseObjects[i] is null) continue;

                newIndexLookup[GetBaseObjectID(i)] = i;
            }

            return newIndexLookup;
        }

        public abstract uint GetBaseObjectID(int id);

        public Base_Object<T> GetBaseObject_Master(uint baseObjectID)
        {
            if (BaseObjects is null || BaseObjects.Length is 0) InitialiseAllBaseObjects();

            if (BaseObjectIndexLookup.TryGetValue(baseObjectID, out var index))
            {
                return BaseObjects?[index];
            }

            Debug.LogWarning($"BaseObject {baseObjectID} does not exist in BaseObjects.");
            return null;
        }

        public void AddBaseObject(uint baseObjectID, Base_Object<T> baseObject)
        {
            if (BaseObjectIndexLookup.ContainsKey(baseObjectID))
            {
                Debug.LogWarning($"BaseObject {baseObjectID} already exists in BaseObjects.");
                return;
            }

            if (_currentIndex >= BaseObjects.Length)
            {
                _compactAndResizeArray();
            }

            BaseObjects[_currentIndex]          = baseObject;
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

            BaseObjects[index] = null;
            BaseObjectIndexLookup.Remove(baseObjectID);

            if (BaseObjectIndexLookup.Count < BaseObjects.Length / 4)
            {
                _compactAndResizeArray();
            }
        }

        void _compactAndResizeArray()
        {
            var newSize = 0;

            for (var i = 0; i < BaseObjects.Length; i++)
            {
                if (BaseObjects[i] is null) continue;

                BaseObjects[newSize]                      = BaseObjects[i];
                BaseObjectIndexLookup[GetBaseObjectID(i)] = newSize;
                newSize++;
            }

            Array.Resize(ref _baseObjects, Math.Max(newSize * 2, BaseObjects.Length));
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
                BaseObjects[index] = _convertToBaseObject(baseObject);
            }
            else
            {
                AddBaseObject(baseObjectID, _convertToBaseObject(baseObject));
            }
        }

        public             Vector2                         BaseScrollPosition;
        public abstract    Dictionary<uint, DataToDisplay> GetDataToDisplay(T     actor_Data);
        protected abstract Base_Object<T>                  _convertToBaseObject(T ability_Master);

        protected abstract Dictionary<uint, Base_Object<T>> _convertDictionaryToBaseObject(
            Dictionary<uint, T> ability_Masters);

        public void ClearBaseObjectData()
        {
            _baseObjects = Array.Empty<Base_Object<T>>();
            BaseObjectIndexLookup.Clear();
            _currentIndex = 0;
        }

        protected abstract Dictionary<uint, Base_Object<T>> _populateDefaultBaseObjects();

        Dictionary<uint, Base_Object<T>> _defaultBaseObjects;

        public Dictionary<uint, Base_Object<T>> DefaultBaseObjects =>
            _defaultBaseObjects ??= _populateDefaultBaseObjects();
    }

    [Serializable]
    public class Base_Object<T> where T : class
    {
        public readonly uint                            BaseObjectID;
        public readonly string                          BaseObjectTitle;
        public readonly Dictionary<uint, DataToDisplay> AllDataToDisplay;

        public T DataObject;

        public Base_Object(uint   baseObjectID, Dictionary<uint, DataToDisplay> allDataToDisplay, T dataObject,
                           string baseObjectTitle)
        {
            BaseObjectID     = baseObjectID;
            AllDataToDisplay = allDataToDisplay;
            DataObject       = dataObject;
            BaseObjectTitle  = baseObjectTitle;
        }
    }

    public class DataToDisplay
    {
        public          int             SelectedIndex = -1;
        public readonly DataDisplayType DataDisplayType;
        public          Vector2         ScrollPosition;

        public readonly List<string> Data;

        public DataToDisplay(List<string> data, DataDisplayType dataDisplayType)
        {
            Data            = data;
            DataDisplayType = dataDisplayType;
        }
    }

    public enum DataDisplayType
    {
        Item,
        List,
        SelectableList
    }
}