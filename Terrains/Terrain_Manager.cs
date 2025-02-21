using System;
using System.Linq;
using UnityEngine;

namespace Terrains
{
    public abstract class TerrainManager
    {
        static Terrain s_terrain;
        Texture2D[] _terrainTextures;
        Texture2DArray _textureArray;
        
        static TerrainData s_terrainData;
        static int s_alphamapWidth, s_alphamapHeight;
        static float[,,] s_alphamapWeights;

        public static Terrain S_Terrain => s_terrain ??= _initializeTerrain();
        public Texture2D[] TerrainTextures => _terrainTextures ??= _getTerrainTextures();
        Texture2D[] _getTerrainTextures() => Resources.LoadAll<Texture2D>("Textures/Terrain").ToArray();

        static Terrain _initializeTerrain()
        {
            var terrain = Terrain.activeTerrain;
            if (terrain is null) throw new Exception("No terrain found.");

            s_terrainData = terrain.terrainData;
            s_alphamapWidth = s_terrainData.alphamapWidth;
            s_alphamapHeight = s_terrainData.alphamapHeight;
            s_alphamapWeights = s_terrainData.GetAlphamaps(0, 0, s_alphamapWidth, s_alphamapHeight);
            
            return terrain;
        }

        public static float GetTerrainHeight(Vector3 worldPosition) => S_Terrain.SampleHeight(worldPosition);

        //* For now, the assumption is that terrainData will not change, but later, put in a check for this to update
        //* Height, Width and Weights if terrainData changes.
        public static int GetTextureIndexAtPosition(Vector3 worldPosition)
        {
            var terrainPosition = worldPosition - S_Terrain.transform.position; ;
            
            var normalisedWidth = terrainPosition.x / s_terrainData.size.x;
            var normalisedHeight = terrainPosition.z / s_terrainData.size.z;

            if (normalisedWidth < 0 || normalisedWidth > 1 || normalisedHeight < 0 || normalisedHeight > 1) return -1;
            
            var textureWidth = 
                Mathf.Clamp(Mathf.RoundToInt(normalisedWidth * s_alphamapWidth), 0, s_alphamapWidth - 1);
            var textureHeight = 
                Mathf.Clamp(Mathf.RoundToInt(normalisedHeight * s_alphamapHeight), 0, s_alphamapHeight - 1);

            var highestWeight = float.NegativeInfinity;
            var bestIndex = -1;

            for (var index = 0; index < s_alphamapWeights.GetLength(2); index++)
            {
                var alphamapWeight = s_alphamapWeights[textureHeight, textureWidth, index];
                
                if (alphamapWeight < highestWeight) continue;
                
                highestWeight = alphamapWeight;
                bestIndex = index;
            }

            return bestIndex;
        }
    }
    
    public enum TerrainName
    {
        Grass,
        Water,
        Air,
        Mud,
        Sand,
        Rock,
        Snow
    }
}