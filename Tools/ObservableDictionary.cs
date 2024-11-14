using System.Collections.Generic;

namespace Tools
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public delegate void                  DictionaryChangedHandler();
        public event DictionaryChangedHandler DictionaryChanged;

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            DictionaryChanged?.Invoke();
        }

        public new bool Remove(TKey key)
        {
            bool result = base.Remove(key);
            if (result)
            {
                DictionaryChanged?.Invoke();
            }
            return result;
        }

        public new void Clear()
        {
            base.Clear();
            DictionaryChanged?.Invoke();
        }
    }
}
