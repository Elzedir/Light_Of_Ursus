using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    [Serializable]
    public class DataToDisplay
    {
        public int SelectedIndex = -1;
        public bool ShowData;
        public Vector2 ScrollPosition = Vector2.zero;

        public string Title;
        Dictionary<string, Dictionary<string, string>> _allStringData;
        public Dictionary<string, Dictionary<string, string>> AllStringData => _allStringData ??= new Dictionary<string, Dictionary<string, string>>();
        
        Dictionary<string, DataToDisplay> _allSubData;
        public Dictionary<string, DataToDisplay> AllSubData => _allSubData ??= new Dictionary<string, DataToDisplay>();

        Dictionary<string, DataToDisplay> _allInteractableData;
        public Dictionary<string, DataToDisplay> AllInteractableData => _allInteractableData ??= new Dictionary<string, DataToDisplay>();

        public DataToDisplay(string title)
        {
            Title = title;
        }
        
        public DataToDisplay(DataToDisplay dataToDisplay, 
            string newTitle = null, 
            Dictionary<string, Dictionary<string, string>> newStringData = null,
            Dictionary<string, DataToDisplay> newSubData = null,
            Dictionary<string, DataToDisplay> newInteractableData = null)
        {
            Title = newTitle ?? dataToDisplay.Title;
            _allStringData = newStringData ?? dataToDisplay.AllStringData;
            _allSubData = newSubData ?? dataToDisplay.AllSubData;
            _allInteractableData = newInteractableData ?? dataToDisplay.AllInteractableData;
        }
    }
}