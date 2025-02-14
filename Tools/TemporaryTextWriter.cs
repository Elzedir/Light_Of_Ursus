using System.Collections;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;

namespace Tools
{
    public abstract class TemporaryTextWriter
    {
        static TextMeshProUGUI s_textBox;
        static TextMeshProUGUI TextBox => s_textBox ??= Manager_Game.FindTransformRecursively(GameObject.Find("UI").transform, "TestText").GetComponent<TextMeshProUGUI>();
        //const float _delayBetweenCharacters = 0.05f;

        public static IEnumerator WriteTextWithDelay(string text, float _delayBetweenCharacters)
        {
            foreach (var character in text)
            {
                TextBox.text += character;
                var delay = Input.GetKey(KeyCode.Space) ? _delayBetweenCharacters / 10 : _delayBetweenCharacters;
                yield return new WaitForSeconds(delay);
                if (Manager_Dialogue.Instance.StopCurrentDialogue) break;
            }
        }

        public static IEnumerator RunTest(float delay)
        {
            yield return new WaitForSeconds(10);
            
            var audioListener = GameObject.Find("CameraHolder").AddComponent<AudioListener>();
            var audioSource = GameObject.Find("CameraHolder").AddComponent<AudioSource>();
            audioSource.volume = 0.75f;
            audioSource.clip = Resources.Load<AudioClip>("Music/Katawaredoki");
            audioSource.Play();
            
            TextBox.transform.parent.gameObject.SetActive(true);
            TextBox.transform.gameObject.SetActive(true);
            TextBox.text = "";
            
            foreach(var kvp in Testing)
            {
                yield return Manager_Dialogue.Instance.StartCoroutine(WriteTextWithDelay(kvp, delay));

                yield return new WaitForSeconds(1);
                
                TextBox.text = "";
            }
            
            TextBox.transform.parent.gameObject.SetActive(false);
            TextBox.transform.gameObject.SetActive(false);
        }
        
        static readonly List<string> Testing = new()
        {
            "Hey.",
            "You.",
            "My wife.",
            "My everything.", 
            "Don't turn around, whatever you do.", 
            "Or else you'll miss what I have to say.", 
            "I know you want to. So so much.", 
            "But just stay with me, here, just a little.",
            "One whole year, hey.", 
            "Wow. I could never have believed that a whole year could pass so quickly.", 
            "And what a year it's been.", 
            "A whole 365 days since I knelt down for the second time and asked the best question I've ever asked.",
            "I feel like I can't choose between wanting time to move faster so I get to experience even more things with you,",
            "or wanting time to move slower so that I can appreciate every second even more than I already do.",
            "Though, I suppose in the end,", 
            "the fact that I can have both is the greatest blessing I could have ever received.",
            "You are beautiful,", 
            "You are intelligent,", 
            "You are inspirational", 
            "You are everything I could have ever dreamed of and more.",
            "And the most amazing thing is that,", 
            "somehow...",
            "somehow...",
            "you are mine.",
            "And you continue to be mine, every single day.",
            "Together, every new memory we make becomes my new favourite memory,",
            "And I can't wait for every single one that we'll continue to create together in the future.", 
            "Every smile.", 
            "Every laugh.", 
            "Every child.", 
            "And every moment in an unending string of endless joy.",
            "I love you, and all you are.", 
            "For now and for always.",
            "Happy First Anniversary."
        };
    }
}
















