using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    [Serializable]
    public class SerializableHashSet<T> : ISerializationCallbackReceiver, IEnumerable<T>
    {
        HashSet<T> _hashSet = new();
        [SerializeField] List<T> _items = new();
        
        public int Count => _hashSet.Count;
        
        public bool Add(T item) => _hashSet.Add(item);
        public bool Remove(T item) => _hashSet.Remove(item);
        public bool Contains(T item) => _hashSet.Contains(item);
        public void Clear() => _hashSet.Clear();
        
        public IEnumerator<T> GetEnumerator() => _hashSet.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void OnBeforeSerialize()
        {
            _items.Clear();
            _items.AddRange(_hashSet);
        }

        public void OnAfterDeserialize()
        {
            _hashSet.Clear();
            foreach (var item in _items)
            {
                _hashSet.Add(item);
            }
        }
    }
}