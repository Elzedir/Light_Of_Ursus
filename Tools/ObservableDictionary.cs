using System;
using System.Collections.Generic;

namespace Tools
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public Action<TKey> DictionaryChanged;

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            DictionaryChanged?.Invoke(key);
        }

        public new bool Remove(TKey key)
        {
            var result = base.Remove(key);
            
            if (result)
            {
                DictionaryChanged?.Invoke(key);
            }
            return result;
        }

        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                base[key] = value;
                DictionaryChanged?.Invoke(key);
            }
        }

        public new void Clear()
        {
            base.Clear();
            DictionaryChanged?.Invoke(default);
        }
    }
}
