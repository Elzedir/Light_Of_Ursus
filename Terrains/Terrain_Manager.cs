using System;
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
        
        static TerrainData _terrainData;
        static int _alphamapWidth, _alphamapHeight;
        static float[,,] _alphamaps;
        
        public static Terrain S_Terrain => s_terrain ??= 
            Manager_Game.FindTransformRecursively(GameObject.Find("GameObjects").transform, "Terrain")
                .GetComponent<Terrain>();
        public Texture2D[] TerrainTextures => _terrainTextures ??= _getTerrainTextures();
        Texture2D[] _getTerrainTextures() => Resources.LoadAll<Texture2D>("Textures/Terrain").ToArray();

        static void _initializeTextureArray()
        {
            s_terrain = Terrain.activeTerrain;
            if (s_terrain == null) throw new Exception("No terrain found.");

            _terrainData = s_terrain.terrainData;
            _alphamapWidth = _terrainData.alphamapWidth;
            _alphamapHeight = _terrainData.alphamapHeight;
            _alphamaps = _terrainData.GetAlphamaps(0, 0, _alphamapWidth, _alphamapHeight);
            
            // var textureSize = TerrainTextures[0].width;
            // _textureArray = new Texture2DArray(textureSize, textureSize, TerrainTextures.Length, TextureFormat.RGBA32, false);
            //
            // for (var i = 0; i < TerrainTextures.Length; i++)
            //     Graphics.CopyTexture(TerrainTextures[i], 0, 0, _textureArray, i, 0);
        }

        public static float GetTerrainHeight(Vector3 worldPosition)
        {
            return S_Terrain.SampleHeight(worldPosition);
        }

        public static int GetTextureIndexAtPosition(Vector3 worldPosition)
        {
            if (s_terrain == null) _initializeTextureArray();
            
            var terrainPos = worldPosition - s_terrain.transform.position;

            var normX = terrainPos.x / _terrainData.size.x;
            var normZ = terrainPos.z / _terrainData.size.z;

            var x = Mathf.Clamp(Mathf.RoundToInt(normX * _alphamapWidth), 0, _alphamapWidth - 1);
            var z = Mathf.Clamp(Mathf.RoundToInt(normZ * _alphamapHeight), 0, _alphamapHeight - 1);

            var maxWeight = 0f;
            var bestIndex = 0;

            for (var i = 0; i < _alphamaps.GetLength(2); i++)
            {
                if (!(_alphamaps[z, x, i] > maxWeight)) continue;
                
                maxWeight = _alphamaps[z, x, i];
                bestIndex = i;
            }

            return bestIndex;
            
            // var terrainSize = S_Terrain.terrainData.size;
            // var x = Mathf.RoundToInt((worldPosition.x / terrainSize.x) * S_Terrain.terrainData.alphamapWidth);
            // var y = Mathf.RoundToInt((worldPosition.z / terrainSize.z) * S_Terrain.terrainData.alphamapHeight);
            //
            // var splatMap = S_Terrain.terrainData.GetAlphamaps(x, y, 1, 1);
            // var bestIndex = 0;
            // float maxWeight = 0;
            //
            // for (var i = 0; i < S_Terrain.terrainData.alphamapLayers; i++)
            // {
            //     if (!(splatMap[0, 0, i] > maxWeight)) continue;
            //     
            //     maxWeight = splatMap[0, 0, i];
            //     bestIndex = i;
            // }
            //
            // return bestIndex;
        }
    }
}