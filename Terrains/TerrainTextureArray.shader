Shader "Custom/TerrainTextureArray"
{
    Properties
    {
        _TextureArray("Texture Array", 2DArray) = "" { }
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            UNITY_DECLARE_TEX2DARRAY(_TextureArray);

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2_f
            {
                float4 pos : POSITION;
                float3 normal : TEXCOORD0;
                float3 world_pos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2_f vert(appdata v)
            {
                v2_f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world_pos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2_f i) : SV_Target
            {
                half4 tex_colour = half4(0, 0, 0, 1);
                
                float sand_Level = 1.0;
                float grass_Level = 3.0;
                float rock_Level = 4.0;
                float snow_Level = 5.0;
                
                float slope = dot(i.normal, float3(0.0, 1.0, 0.0));
                
                int dominant_Layer = 0;
                if (i.world_pos.y >= snow_Level) dominant_Layer = 3;
                else if (i.world_pos.y >= rock_Level) dominant_Layer = 2;
                else if (i.world_pos.y >= grass_Level) dominant_Layer = 1;
                else dominant_Layer = 0;
                
                tex_colour.rgb = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, float3(i.uv, dominant_Layer)).rgb;
                
                if (slope < 0.2)
                {
                    tex_colour.rgb += UNITY_SAMPLE_TEX2DARRAY(_TextureArray, float3(i.uv, 1)).rgb * 0.2;
                }
                else if (slope >= 0.2 && slope < 0.5)
                {
                    tex_colour.rgb += UNITY_SAMPLE_TEX2DARRAY(_TextureArray, float3(i.uv, 2)).rgb * 0.3;
                }
                else
                {
                    tex_colour.rgb += UNITY_SAMPLE_TEX2DARRAY(_TextureArray, float3(i.uv, 3)).rgb * 0.4;
                }
                
                return tex_colour;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}