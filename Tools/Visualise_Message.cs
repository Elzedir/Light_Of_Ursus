using System.Collections;
using TMPro;
using UnityEngine;

namespace Tools
{
    public abstract class Visualise_Message
    {
        static Transform s_parent;
        public static Transform Parent => s_parent ??= _getParent();
        static Transform _getParent() => GameObject.Find("VisualiseMessages").transform;
        
        public static GameObject Show_Message(Vector3 position, string text, float delay = 0)
        {
            var messageGO = _create_Object(position);
            var rectTransform = messageGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, rectTransform.sizeDelta.y);
            messageGO.name = $"Message: {position} - Message";
            var textBox = messageGO.GetComponent<TextMeshPro>();
            textBox.text = "";
            textBox.fontSize = 20f;
            textBox.alignment = TextAlignmentOptions.Center;

            Manager_Dialogue.Instance.StartCoroutine(_typeMessage(textBox, text, delay));
            
            return messageGO;
        }
        
        static GameObject _create_Object(Vector3 position)
        {
            var go = new GameObject($"{position}");
            go.AddComponent<TextMeshPro>().text = "";
            go.transform.SetParent(Parent);
            go.transform.localPosition = position;
            return go;
        }

        static IEnumerator _typeMessage(TextMeshPro textBox, string text, float delay)
        {
            foreach (var character in text)
            {
                textBox.text += character;
                
                delay = Input.GetKey(KeyCode.Space) ? delay / 10 : delay;
                
                if (delay != 0) yield return new WaitForSeconds(delay);
            }
        }
    }
}