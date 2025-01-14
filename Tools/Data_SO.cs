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

        public virtual void RefreshData() { }

        Dictionary<uint, int>        _dataIndexLookup;
        public Dictionary<uint, int> DataIndexLookup => _dataIndexLookup ??= _buildIndexLookup();
        int                          _currentIndex;

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
                if (Data[i] is null) continue;

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

        public void AddData(uint dataID, Data<TD> data)
        {
            if (DataIndexLookup.ContainsKey(dataID))
            {
                Debug.LogWarning($"DataID: {dataID} already exists in Data.");
                return;
            }

            if (_currentIndex >= Data.Length)
            {
                _compactAndResizeArray();
            }

            Data[_currentIndex]         = data;
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

                Data[newSize]                     = Data[i];
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
                Debug.Log($"Updating Data {dataID} at index {index} with Array size {Data.Length}.");
                Data[index] = _convertToData(data);
            }
            else
            {
                Debug.Log($"Adding Data {dataID} with Array size {Data.Length}.");
                AddData(dataID, _convertToData(data));
            }
        }

        public             Vector2        ScrollPosition;
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
        public    Dictionary<uint, Data<TD>> DefaultData => _defaultData ??= _getDefaultData();
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
        public readonly uint   DataID;
        public readonly string DataTitle;

        public T                        Data_Object;
        public                                      Func<bool, Data_Display> GetData_Display;

        public Data(uint   dataID,    T                       data_Object,
                           string dataTitle, Func<bool, Data_Display> getData_Display)
        {
            DataID    = dataID;
            Data_Object      = data_Object;
            DataTitle = dataTitle;
            GetData_Display = getData_Display;
        }
    }

    [Serializable]
    public class Data_Display
    {
        public          int     SelectedIndex;
        public          bool    ShowData;
        public          Vector2 ScrollPosition;

        public readonly string                           Title;
        public readonly DataDisplayType                  DataDisplayType;
        public          Dictionary<string, string>       Data;
        public          Dictionary<string, Data_Display> SubData;

        public Data_Display(string                           title, DataDisplayType dataDisplayType, Dictionary<string, string> data = null,
                            Dictionary<string, Data_Display> subData = null)
        {
            switch (dataDisplayType)
            {
                case DataDisplayType.List_CheckBox when data is null && subData is null:
                    throw new NullReferenceException("Data is null.");
                case DataDisplayType.List_Item when data is null:
                    throw new NullReferenceException("Data is null.");
                case DataDisplayType.List_Selectable when subData is null:
                    throw new NullReferenceException("SubData is null.");
            }

            Title           = title;
            DataDisplayType = dataDisplayType;
            SelectedIndex   = -1;
            ShowData        = false;
            ScrollPosition  = Vector2.zero;
            Data            = data    ?? new Dictionary<string, string>();
            SubData         = subData ?? new Dictionary<string, Data_Display>();
        }
    }

    public enum DataDisplayType
    {
        List_Item,
        List_CheckBox,
        List_Selectable
    }
}