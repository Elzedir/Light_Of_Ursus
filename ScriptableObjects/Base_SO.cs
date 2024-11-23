using System;
using System.Collections.Generic;
using System.Linq;
using Recipes;
using UnityEngine;

namespace ScriptableObjects
{
    [Serializable]
    public abstract class Base_SO<T> : ScriptableObject where T : class
    {
        [SerializeField] T[]   _objects;
        public           T[]   Objects => _objects ??= InitialiseAllObjects();
        Dictionary<uint, int>        _objectIndexLookup;
        public Dictionary<uint, int> ObjectIndexLookup => _objectIndexLookup ??= _buildIndexLookup();
        int                                _currentIndex;
        
        public T[] InitialiseAllObjects()
        {
            _objects = new T[DefaultObjects.Count * 2];
            Array.Copy(DefaultObjects.Values.ToArray(), Objects, DefaultObjects.Count);
            _currentIndex = DefaultObjects.Count;
            _buildIndexLookup();
            return Objects ?? throw new NullReferenceException("Objects is null.");
        }

        protected Dictionary<uint, int> _buildIndexLookup()
        {
            var newIndexLookup = new Dictionary<uint, int>();

            for (var i = 0; i < Objects.Length; i++)
            {
                if (Objects[i] is null) continue;

                newIndexLookup[GetObjectID(i)] = i;
            }

            return newIndexLookup;
        }

        public abstract uint GetObjectID(int id);

        public T GetObject_Master(uint objectID)
        {
            if (Objects is null || Objects.Length is 0) InitialiseAllObjects();
                
            if (ObjectIndexLookup.TryGetValue(objectID, out var index))
            {
                return Objects?[index];
            }

            Debug.LogWarning($"Object {objectID} does not exist in Objects.");
            return null;
        }

        public void AddObject(uint objectID, T @object)
        {
            if (ObjectIndexLookup.ContainsKey(objectID))
            {
                Debug.LogWarning($"Object {objectID} already exists in Objects.");
                return;
            }

            if (_currentIndex >= Objects.Length)
            {
                _compactAndResizeArray();
            }

            Objects[_currentIndex]        = @object;
            ObjectIndexLookup[objectID] = _currentIndex;
            _currentIndex++;
        }

        public void RemoveObjects(uint objectID)
        {
            if (!ObjectIndexLookup.TryGetValue(objectID, out var index))
            {
                Debug.LogWarning($"Object {objectID} does not exist in Objects.");
                return;
            }

            Objects[index] = null;
            ObjectIndexLookup.Remove(objectID);
            
            if (ObjectIndexLookup.Count < Objects.Length / 4)
            {
                _compactAndResizeArray();
            }
        }
        
        void _compactAndResizeArray()
        {
            var newSize = 0;
            
            for (var i = 0; i < Objects.Length; i++)
            {
                if (Objects[i] is null) continue;
                
                Objects[newSize]                         = Objects[i];
                ObjectIndexLookup[GetObjectID(i)] = newSize;
                newSize++;
            }

            Array.Resize(ref _objects, Math.Max(newSize * 2, Objects.Length));
            _currentIndex = newSize;
        }

        public void UpdateObject(uint objectID, T @object)
        {
            if (ObjectIndexLookup.TryGetValue(objectID, out var index))
            {
                Objects[index] = @object;
            }
            else
            {
                AddObject(objectID, @object);
            }
        }

        public void ClearObjectData()
        {
            _objects = Array.Empty<T>();
            ObjectIndexLookup.Clear();
            _currentIndex = 0;
        }

        public abstract Dictionary<uint, T> PopulateDefaultObjects();

        Dictionary<uint, T> _defaultRecipes;
        protected Dictionary<uint, T> DefaultObjects => _defaultRecipes ??= PopulateDefaultObjects();
    }
}
