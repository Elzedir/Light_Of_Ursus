using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public class Component_Manager
    {
        // Central dictionary to cache components by ID
        private Dictionary<uint, MonoBehaviour> _sceneComponents    = new Dictionary<uint, MonoBehaviour>();
        private HashSet<uint>                   _componentsToUpdate = new HashSet<uint>();

        // Initialization (single search for efficiency)
        public void InitializeComponents()
        {
            var components = Object.FindObjectsOfType<MonoBehaviour>(); // Generic search
            foreach (var component in components)
            {
                uint id;
                if (TryExtractID(component.name, out id))
                {
                    _sceneComponents[id] = component;
                }
            }
        }

        // Update specific components
        public void UpdateChangedComponents()
        {
            foreach (var id in _componentsToUpdate)
            {
                if (_sceneComponents.ContainsKey(id))
                {
                    // Update logic for the existing component
                    var component = _sceneComponents[id];
                    // Reassign or refresh component reference
                    _sceneComponents[id] = component;
                }
                else
                {
                    // Handle newly added components
                    var component = FindComponentByID(id);
                    if (component != null)
                    {
                        _sceneComponents[id] = component;
                    }
                }
            }

            // Clear the update list after processing
            _componentsToUpdate.Clear();
        }

        // Helper to extract ID from name
        private bool TryExtractID(string name, out uint id)
        {
            var match = System.Text.RegularExpressions.Regex.Match(name, @"\d+");
            if (match.Success && uint.TryParse(match.Value, out id))
            {
                return true;
            }

            id = 0;
            return false;
        }

        // Helper to find a specific component by ID
        private MonoBehaviour FindComponentByID(uint id)
        {
            return FindObjectsOfType<MonoBehaviour>()
                .FirstOrDefault(component => TryExtractID(component.name, out var componentID) && componentID == id);
        }

        // Add/Remove IDs to the update set
        public void MarkComponentChanged(uint id)
        {
            _componentsToUpdate.Add(id);
        }

        public void RemoveComponent(uint id)
        {
            _sceneComponents.Remove(id);
            _componentsToUpdate.Remove(id);
        }
    }
}