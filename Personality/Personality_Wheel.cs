using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Personality
{
    public class Personality_Wheel : MonoBehaviour
    {
        PersonalityRadar radar;
        TextMeshProUGUI data;

        [Range(-1f, 1f)] public float Bravery;
        [Range(-1f, 1f)] public float Humility;
        [Range(-1f, 1f)] public float Generosity;
        [Range(-1f, 1f)] public float Logic;
        [Range(-1f, 1f)] public float Loyalty;
        [Range(-1f, 1f)] public float Confidence;
    
        void Update()
        {
            if (data == null)
            {
                data = GameObject.Find("Personality_Data").GetComponent<TextMeshProUGUI>();
            }
        
            if (radar == null)
            {
                radar = GameObject.Find("PersonalityRadar").GetComponent<PersonalityRadar>();
            }
        
            data.text = $"Bravery: {Bravery}\n" +
                        $"Humility: {Humility}\n" +
                        $"Generosity: {Generosity}\n" +
                        $"Logic: {Logic}\n" +
                        $"Loyalty: {Loyalty}\n" +
                        $"Confidence: {Confidence}";
        
            radar.personality.traits = new List<Trait>
            {
                new() { name = "Humility", value = Humility },
                new() { name = "Generosity", value = Generosity },
                new() { name = "Loyalty", value = Loyalty },
                new() { name = "Bravery", value = Bravery },
                new() { name = "Logic", value = Logic },
                new() { name = "Confidence", value = Confidence }
            };
            radar.SetVerticesDirty();
        }
    }

    public class PersonalityRadarSetup : MonoBehaviour
    {
        public PersonalityRadar radar;

    
    }

    [System.Serializable]
    public class Personality_Test
    {
        public List<Trait> traits = new();
    }
}