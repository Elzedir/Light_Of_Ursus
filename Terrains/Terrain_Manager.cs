using System.Linq;
using Managers;
using UnityEngine;

namespace Terrains
{
    public abstract class TerrainManager
    {
        static Terrain s_terrain;
        Texture2D[] _terrainTextures;
        Texture2DArray _textureArray;
        
        public static Terrain S_Terrain => s_terrain ??= 
            Manager_Game.FindTransformRecursively(GameObject.Find("GameObjects").transform, "Terrain")
                .GetComponent<Terrain>();
        public Texture2D[] TerrainTextures => _terrainTextures ??= _getTerrainTextures();
        Texture2D[] _getTerrainTextures() => Resources.LoadAll<Texture2D>("Textures/Terrain").ToArray();

        void Start()
        {
            InitializeTextureArray();
        }

        void InitializeTextureArray()
        {
            var textureSize = TerrainTextures[0].width;
            _textureArray = new Texture2DArray(textureSize, textureSize, TerrainTextures.Length, TextureFormat.RGBA32, false);
        
            for (var i = 0; i < TerrainTextures.Length; i++)
                Graphics.CopyTexture(TerrainTextures[i], 0, 0, _textureArray, i, 0);
        }

        public static float GetTerrainHeight(Vector3 worldPosition)
        {
            return S_Terrain.SampleHeight(worldPosition);
        }

        public static int GetTextureIndexAtPosition(Vector3 worldPosition)
        {
            var terrainSize = S_Terrain.terrainData.size;
            var x = Mathf.RoundToInt((worldPosition.x / terrainSize.x) * S_Terrain.terrainData.alphamapWidth);
            var y = Mathf.RoundToInt((worldPosition.z / terrainSize.z) * S_Terrain.terrainData.alphamapHeight);
        
            var splatMap = S_Terrain.terrainData.GetAlphamaps(x, y, 1, 1);
            var bestIndex = 0;
            float maxWeight = 0;

            for (var i = 0; i < S_Terrain.terrainData.alphamapLayers; i++)
            {
                if (!(splatMap[0, 0, i] > maxWeight)) continue;
                
                maxWeight = splatMap[0, 0, i];
                bestIndex = i;
            }

            return bestIndex;
        }
    }
}