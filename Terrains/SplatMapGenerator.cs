using System.IO;
using UnityEditor;
using UnityEngine;

namespace Terrains
{
    public class SplatMapGenerator : MonoBehaviour
    {
        [SerializeField] int _splatWidth = 512;
        [SerializeField] int _splatHeight = 512;
        const string _savePath = "Assets/Resources/Textures/SplatMap.png";

        [ContextMenu("Generate SplatMap")]
        public void GenerateSplatMap()
        {
            var splatMap = new Texture2D(_splatWidth, _splatHeight, TextureFormat.RGBA32, false);

            for (var y = 0; y < _splatHeight; y++)
            {
                for (var x = 0; x < _splatWidth; x++)
                {
                    var r = Random.Range(0f, 1f);
                    var g = Random.Range(0f, 1f);
                    var b = Random.Range(0f, 1f);
                    var a = Random.Range(0f, 1f);
                    
                    var total = r + g + b + a;
                    var color = new Color(r / total, g / total, b / total, a / total);

                    splatMap.SetPixel(x, y, color);
                }
            }

            splatMap.Apply();
            
            var bytes = splatMap.EncodeToPNG();

            var iteration = 0;
            
            while (File.Exists($"{_savePath}{iteration}"))
            {
                iteration++;
            }
            
            File.WriteAllBytes($"{_savePath}{iteration}", bytes);
            AssetDatabase.Refresh();

            Debug.Log($"SplatMap saved at: {_savePath}");
        }
    }
    
    [CustomEditor(typeof(SplatMapGenerator))]
    public class SplatMapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draw normal inspector fields

            var creator = (SplatMapGenerator)target;

            if (GUILayout.Button("Generate SplatMap"))
            {
                creator.GenerateSplatMap();
            }
        }
    }
}