using System;

namespace Tools
{
    [Serializable]
    public class ObservableDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue>
    {
        public Action<TKey> DictionaryChanged;
        
        public void SetDictionaryChanged(Action<TKey> dictionaryChanged) => DictionaryChanged = dictionaryChanged;
        public void Cleanup() => SetDictionaryChanged(null);

        public override void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            DictionaryChanged?.Invoke(key);
        }

        public override bool Remove(TKey key)
        {
            var result = base.Remove(key);
            
            if (result)
            {
                DictionaryChanged?.Invoke(key);
            }
            
            return result;
        }

        public override TValue this[TKey key]
        {
            get => base[key];
            set
            {
                base[key] = value;
                DictionaryChanged?.Invoke(key);
            }
        }

        public override  void Clear()
        {
            base.Clear();
            DictionaryChanged?.Invoke(default);
        }
    }
}
