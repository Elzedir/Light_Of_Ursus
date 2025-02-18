Shader "Custom/TerrainTextureArray"
{
    Properties
    {
        _TextureArray("Texture Array", 2DArray) = "" { }
        _SplatMap("Splat Map", 2D) = "" { }
        _TerrainCompatible("Terrain Compatible", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        a
        //* ChatGPt hallucinated up this texture. Somehow, turn it into a real texture.
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Texture array and splatmap
            UNITY_DECLARE_TEX2DARRAY(_TextureArray);
            uniform sampler2D _SplatMap;
            uniform float _TerrainCompatible;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Sample the splat map at the given UV coordinates
                float3 splat_weights = tex2D(_SplatMap, i.uv).rgb;
                
                half4 tex_colour = half4(0, 0, 0, 1);
                
                //* Starting with 4 layers, but can be adjusted later.
                for (int layer = 0; layer < 4; layer++)
                {
                    if (splat_weights[layer] == 0) break;
                    
                    tex_colour.rgb += UNITY_SAMPLE_TEX2DARRAY(_TextureArray, float3(i.uv, layer)).rgb * splat_weights[layer];
                }

                // Add variation to the texture by adjusting color based on terrain compatibility
                if (_TerrainCompatible > 0.5)
                {
                    tex_colour.rgb += float3(0.1, 0.2, 0.3); // Adding color variation for terrain compatibility
                }
                else
                {
                    tex_colour.rgb *= 0.8; // Darken the texture for non-compatible terrain
                }

                if (splat_weights[0] > 0.5) tex_colour.rgb += float3(0.2, 0.6, 0.3); // Grass
                if (splat_weights[1] > 0.5) tex_colour.rgb += float3(0.9, 0.8, 0.4); // Sand
                if (splat_weights[2] > 0.5) tex_colour.rgb += float3(0.5, 0.5, 0.5); // Rock
                if (splat_weights[3] > 0.5) tex_colour.rgb += float3(1.0, 1.0, 1.0); // Snow

                return tex_colour;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
