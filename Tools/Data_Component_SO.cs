using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataPersistence;
using Station;
using UnityEngine;

namespace Tools
{
    public abstract class Data_Component_SO<TD, TC> : Data_SO<TD>
        where TD : class
        where TC : MonoBehaviour
    {
        //public             HashSet<ulong>              ComponentIDsToChange = new();
        public Dictionary<ulong, Data<TD>> SavedData => _getSavedData();
        
        //* Maybe see if we can make a common function here for _getSavedData(); 
        protected abstract Dictionary<ulong, Data<TD>> _getSavedData();
        public             Dictionary<ulong, TC>       SceneComponents => _getSceneComponents();
        public             Dictionary<ulong, Data<TD>> SceneData       => _getSceneData();
        protected abstract Dictionary<ulong, Data<TD>> _getSceneData();
        
        protected Dictionary<ulong, TC> _getSceneComponents(bool repopulate = false)
        {
            // if (repopulate)
            // {
            //     // Clear all   
            // }
            
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
                       return match.Success && ulong.TryParse(match.Value, out var id)
                           ? new { Id = id, Component = component }
                           : null;
                   })
                   .Where(entry => entry != null)
                   .ToDictionary(entry => entry.Id, entry => entry.Component);
        }

        public Data<TD> GetDataFromName(string componentName)
        {
            var regex = new Regex(@"\d+");

            var dataID = componentName;

            if (regex.IsMatch(componentName))
            {
                dataID = regex.Match(componentName).Value;
            }

            if (ulong.TryParse(dataID, out var id))
            {
                return GetData(id);
            }

            Debug.LogError($"Data_Component with name {componentName} not found in Data_SO.");
            return null;
        }

        public override void RefreshData(bool reinitialise = false)
        {
            if (reinitialise)
            {
                InitialiseAllData();
            }
            
            if (!Application.isPlaying) return;
            
            _initialiseSceneData();
        }

        void _initialiseSceneData()
        {
            foreach(var data in SceneData.Values)
            {
                UpdateData(data.DataID, data.Data_Object);
            }
        }
        
        protected override Dictionary<ulong, Data<TD>> _getAllInitialisationData()
        {
            var allData = new Dictionary<ulong, Data<TD>>();
            
            foreach (var (key, value) in DefaultData)
            {
                allData[key] = value;
            }
            
            foreach(var (key, value) in SavedData)
            {
                allData[key] = value;
            }
            
            return allData;
        }
        
        public abstract void SaveData(Save_Data saveData);
    }
}