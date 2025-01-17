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
        public Dictionary<string, Dictionary<string, string>> AllStringData = new();
        public Dictionary<string, DataToDisplay> AllSubData = new();
        public Dictionary<string, DataToDisplay> AllInteractableData = new();

        public DataToDisplay(string title)
        {
            Title = title;
        }
    }
}