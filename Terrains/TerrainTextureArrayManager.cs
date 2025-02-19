using UnityEditor;
using UnityEngine;

namespace Terrains
{
    public class TerrainTextureArrayManager : MonoBehaviour
    {
        [SerializeField] Texture2D[] _textures;
        const string _savePath = "Assets/Resources/Textures/TerrainTextureArray.asset";

        public void CreateTextureArray()
        {
            if (_textures == null || _textures.Length == 0)
            {
                Debug.LogError("No textures assigned!");
                return;
            }

            var size = _textures[0].width; // Assumes all textures are the same size
            var format = _textures[0].format;
            var textureArray = new Texture2DArray(size, size, _textures.Length, format, false);

            for (var i = 0; i < _textures.Length; i++)
            {
                Graphics.CopyTexture(_textures[i], 0, 0, textureArray, i, 0);
            }

            var iteration = 0;
            
            while (AssetDatabase.LoadAssetAtPath<Texture2DArray>($"{_savePath}{iteration}.asset") is not null)
            {
                iteration++;
            }
            
            AssetDatabase.CreateAsset(textureArray, $"{_savePath}{iteration}");
            AssetDatabase.SaveAssets();
            Debug.Log($"Texture2DArray saved at {_savePath}");
        }
    }
    
    [CustomEditor(typeof(TerrainTextureArrayManager))]
    public class TextureArrayCreatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var creator = (TerrainTextureArrayManager)target;

            if (GUILayout.Button("Create Texture2DArray"))
            {
                creator.CreateTextureArray();
            }
        }
    }
}