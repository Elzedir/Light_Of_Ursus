using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataPersistence;
using UnityEngine;

namespace Tools
{
    public abstract class Data_Component_SO<TD, TC> : Data_SO<TD>
        where TD : class
        where TC : MonoBehaviour
    {
        //public             HashSet<uint>              ComponentIDsToChange = new();
        public Dictionary<uint, Data<TD>> SavedData => _getSavedData();   
        
        protected abstract Dictionary<uint, Data<TD>> _getSavedData();
        public             Dictionary<uint, TC>       SceneComponents => _getSceneComponents();
        public             Dictionary<uint, Data<TD>> SceneData       => _getSceneData();
        protected abstract Dictionary<uint, Data<TD>> _getSceneData();
        
        protected Dictionary<uint, TC> _getSceneComponents(bool repopulate = false)
        {
            // foreach (var componentID in ComponentIDsToChange)
            // {
            //     if (SceneComponents.TryGetValue(componentID, out var component))
            //     {
            //         SceneComponents[componentID] = component;
            //     }
            // }
            
            // foreach(var dataID in SceneData.Keys)
            // {
            //     if (SceneComponents.TryGetValue(dataID, out var component)) continue;
            //     
            //     
            // }
            
            //= \d: Matches any single digit (0–9).
            //= +: Matches one or more of the preceding element (in this case, digits).
            
            var regex = new Regex(@"\d+");

            return FindObjectsByType<TC>(FindObjectsSortMode.None)
                   .Select(component =>
                   {
                       var match = regex.Match(component.name);
                       return match.Success && uint.TryParse(match.Value, out var id)
                           ? new { Id = id, Component = component }
                           : null;
                   })
                   .Where(entry => entry != null)
                   .ToDictionary(entry => entry.Id, entry => entry.Component);
        }
        
        protected override Dictionary<uint, Data<TD>> _getAllData()
        {
            var allData = new Dictionary<uint, Data<TD>>();
            
            foreach (var (key, value) in DefaultData)
            {
                allData[key] = value;
            }
            
            foreach(var (key, value) in SavedData)
            {
                allData[key] = value;
            }
            
            foreach(var (key, value) in SceneData)
            {
                allData[key] = value;
            }
            
            return allData;
        }
        
        public abstract void SaveData(SaveData saveData);
    }
}