using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools
{
    [Serializable]
    public abstract class Data_SO<TD> : ScriptableObject where TD : class
    {
        //* Maybe write a second array to save, so that we don't autoupdate the data to Save on SceneChange.
        [SerializeField] Data<TD>[] _data;

        public Data<TD>[] Data
        {
            get
            {
                if (_data is not null && _data.Length != 0) return _data;

                var data = InitialiseAllData();
                return data.Length != 0 ? data : null;
            }
        }

        public virtual void RefreshData(bool reinitialise = false)
        {
            if (reinitialise) InitialiseAllData();
        }

        Dictionary<uint, int> _dataIndexLookup;

        public Dictionary<uint, int> DataIndexLookup => _dataIndexLookup ??= _buildIndexLookup();
        
        int _currentIndex;

        public bool ToggleMissingDataDebugs;

        public Data<TD>[] InitialiseAllData()
        {
            var allData = _getAllInitialisationData();

            _data = new Data<TD>[allData.Count * 2];
            Array.Copy(allData.Values.ToArray(), Data, allData.Count);
            _currentIndex = allData.Count;
            _buildIndexLookup();

            return Data ?? throw new NullReferenceException("Data is null.");
        }

        protected Dictionary<uint, int> _buildIndexLookup()
        {
            var newIndexLookup = new Dictionary<uint, int>();

            for (var i = 0; i < Data.Length; i++)
            {
                if (Data[i]?.Data_Object is null)
                    continue;

                newIndexLookup[GetDataID(i)] = i;
            }

            return newIndexLookup;
        }

        public abstract uint GetDataID(int id);

        public Data<TD> GetData(uint dataID)
        {
            try
            {
                return Data[DataIndexLookup[dataID]];
            }
            catch
            {
                if (DataIndexLookup.TryGetValue(dataID, out var index))
                {
                    Debug.LogWarning($"DataID: {dataID} is null at index {index}.");
                    return null;
                }

                Debug.LogWarning($"DataID: {dataID} does not exist in DataIndexLookup.");
                return null;
            }
        }

        void _addData(uint dataID, Data<TD> data)
        {
            if (DataIndexLookup.ContainsKey(dataID))
            {
                Debug.LogError($"DataID: {dataID} already exists in Data. But since Add is only called in Update, there is a problem.");
                return;
            }

            if (_currentIndex >= Data.Length)
            {
                _compactAndResizeArray();
            }

            Data[_currentIndex] = data;
            DataIndexLookup[dataID] = _currentIndex;
            _currentIndex++;
        }

        public void RemoveData(uint dataID)
        {
            if (!DataIndexLookup.TryGetValue(dataID, out var index))
            {
                Debug.LogWarning($"DataID: {dataID} does not exist in Data.");
                return;
            }

            Data[index] = null;
            DataIndexLookup.Remove(dataID);

            if (DataIndexLookup.Count < Data.Length / 4)
            {
                _compactAndResizeArray();
            }
        }

        void _compactAndResizeArray()
        {
            var newSize = 0;

            for (var i = 0; i < Data.Length; i++)
            {
                if (Data[i] is null) continue;

                Data[newSize] = Data[i];
                DataIndexLookup[GetDataID(i)] = newSize;
                newSize++;
            }

            Array.Resize(ref _data, Math.Max(newSize * 2, Data.Length));
            _currentIndex = newSize;
        }

        public void UpdateAllData(Dictionary<uint, TD> newData, bool clearDataFirst = false)
        {
            if (clearDataFirst) ClearSOData();

            foreach (var (key, value) in newData)
            {
                UpdateData(key, value);
            }
        }

        public void UpdateData(uint dataID, TD data)
        {
            if (DataIndexLookup.TryGetValue(dataID, out var index))
            {
                Data[index] = _convertToData(data);
            }
            else
            {
                _addData(dataID, _convertToData(data));
            }
        }

        public Vector2 ScrollPosition;
        protected abstract Data<TD> _convertToData(TD data);

        protected Dictionary<uint, Data<TD>> _convertDictionaryToData(
            Dictionary<uint, TD> dictionary)
        {
            return dictionary.ToDictionary(key => key.Key, value => _convertToData(value.Value));
        }

        public void ClearSOData()
        {
            _data = Array.Empty<Data<TD>>();
            _dataIndexLookup?.Clear();
            _currentIndex = 0;
        }

        protected Dictionary<uint, Data<TD>> _defaultData;
        public Dictionary<uint, Data<TD>> DefaultData => _defaultData ??= _getDefaultData();
        protected abstract Dictionary<uint, Data<TD>> _getDefaultData();

        protected virtual Dictionary<uint, Data<TD>> _getAllInitialisationData()
        {
            var allData = new Dictionary<uint, Data<TD>>();

            foreach (var (key, value) in DefaultData)
            {
                allData[key] = value;
            }

            return allData;
        }
    }

    [Serializable]
    public class Data<T> where T : class
    {
        public readonly uint DataID;
        public readonly string DataTitle;

        public T Data_Object;
        public Func<bool, DataToDisplay> GetDataToDisplay;

        public Data(uint dataID, T data_Object,
            string dataTitle, Func<bool, DataToDisplay> getDataToDisplay)
        {
            DataID = dataID;
            Data_Object = data_Object;
            DataTitle = dataTitle;
            GetDataToDisplay = getDataToDisplay;
        }
    }
}