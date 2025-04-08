
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        Dictionary<TKey, TValue> _dictionary = new();
        public int Count => _dictionary.Count;
        
        [SerializeField] List<TKey> _keys = new();
        [SerializeField] List<TValue> _values = new();
        
        public virtual void Add(TKey key, TValue value) => _dictionary.Add(key, value);
        public virtual bool Remove(TKey key) => _dictionary.Remove(key);
        public virtual void Clear() => _dictionary.Clear();
        public virtual bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
        public virtual bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);
        public virtual bool TryAdd(TKey key, TValue value) => _dictionary.TryAdd(key, value);
        public TValue GetValueOrDefault(TKey key, TValue defaultValue = default)
            => _dictionary.GetValueOrDefault(key, defaultValue);
        
        public virtual TValue this[TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;
        public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;
        
        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            
            foreach (var kvp in _dictionary)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            _dictionary.Clear();
            
            if (_keys.Count != _values.Count) { Debug.LogError($"Key count: {_keys.Count} does not match value count: {_values.Count}"); }
            
            for (var i = 0; i < _keys.Count; i++)
            {
                if (i < _values.Count)
                {
                    _dictionary[_keys[i]] = _values[i];
                }
            }
        }
        
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // LINQ Support (optional, but convenient)
        public IEnumerable<KeyValuePair<TKey, TValue>> Where(Func<KeyValuePair<TKey, TValue>, bool> predicate)
            => _dictionary.Where(predicate);

        public IEnumerable<TResult> Select<TResult>(Func<KeyValuePair<TKey, TValue>, TResult> selector)
            => _dictionary.Select(selector);
    }
    
    public static class SerializableDictionaryExtensions
    {
        public static SerializableDictionary<TKey, TValue> ToSerializedDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            var result = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in source)
                result.Add(kvp.Key, kvp.Value);
            return result;
        }

        public static SerializableDictionary<TKey, TValue> ToSerializedDictionary<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> valueSelector)
        {
            var result = new SerializableDictionary<TKey, TValue>();
            foreach (var item in source)
                result.Add(keySelector(item), valueSelector(item));
            return result;
        }
    }
}