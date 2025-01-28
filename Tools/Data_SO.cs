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

        Dictionary<ulong, int> _dataIndexLookup;

        public Dictionary<ulong, int> DataIndexLookup => _dataIndexLookup ??= _buildIndexLookup();
        
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

        protected Dictionary<ulong, int> _buildIndexLookup()
        {
            var newIndexLookup = new Dictionary<ulong, int>();

            for (var i = 0; i < Data.Length; i++)
            {
                if (Data[i]?.DataID is null or 0)
                {
                    //Debug.Log($"DataID: {Data[i]?.DataID} is null or 0.");
                    continue;
                }
                
                //* Changed this from GetID() to DataID. Check that it doesn't break anything. (I think 25/01)

                newIndexLookup[Data[i].DataID] = i;
            }

            return newIndexLookup;
        }

        public List<ulong> GetAllDataIDs()
        {
            return Data
                .Where(data => data?.DataID != null)
                .Select(data => data.DataID).ToList();
        }

        public Data<TD> GetData(ulong dataID)
        {
            if (dataID == 0)
            {
                Debug.LogWarning($"DataID: {dataID} is not desired. Change expected ID to be non-zero.");
                return null;
            }
            
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

        void _addData(ulong dataID, Data<TD> data)
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

        public void RemoveData(ulong dataID)
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
                if (Data[i]?.DataID is null or 0)
                {
                    Debug.Log($"DataID: {Data[i]?.DataID} is null or 0.");
                    continue;
                }

                Data[newSize] = Data[i];
                DataIndexLookup[Data[i].DataID] = newSize;
                newSize++;
            }

            Array.Resize(ref _data, Math.Max(newSize * 2, Data.Length));
            _currentIndex = newSize;
        }

        public void UpdateAllData(Dictionary<ulong, TD> newData, bool clearDataFirst = false)
        {
            if (clearDataFirst) ClearSOData();

            foreach (var (key, value) in newData)
            {
                UpdateData(key, value);
            }
        }

        public void UpdateData(ulong dataID, TD data)
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

        protected Dictionary<ulong, Data<TD>> _convertDictionaryToData(
            Dictionary<ulong, TD> dictionary)
        {
            return dictionary.ToDictionary(key => key.Key, value => _convertToData(value.Value));
        }

        public void ClearSOData()
        {
            _data = Array.Empty<Data<TD>>();
            _dataIndexLookup?.Clear();
            _currentIndex = 0;
        }

        protected Dictionary<ulong, Data<TD>> _defaultData;
        public Dictionary<ulong, Data<TD>> DefaultData => _defaultData ??= _getDefaultData();
        protected abstract Dictionary<ulong, Data<TD>> _getDefaultData();

        protected virtual Dictionary<ulong, Data<TD>> _getAllInitialisationData()
        {
            var allData = new Dictionary<ulong, Data<TD>>();

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
        public readonly ulong DataID;
        public readonly string DataTitle;

        public T Data_Object;
        public Func<bool, DataToDisplay> GetDataToDisplay;

        public Data(ulong dataID, T data_Object,
            string dataTitle, Func<bool, DataToDisplay> getDataToDisplay)
        {
            DataID = dataID;
            Data_Object = data_Object;
            DataTitle = dataTitle;
            GetDataToDisplay = getDataToDisplay;
        }
    }
}