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

        public readonly string Title;
        public Dictionary<string, string> StringData = new();
        public Dictionary<string, DataToDisplay> SubData = new();
        public Dictionary<string, DataToDisplay> InteractableData = new();

        public DataToDisplay(string title)
        {
            Title = title;
        }
    }
}