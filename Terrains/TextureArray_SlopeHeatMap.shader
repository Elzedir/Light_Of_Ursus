Shader "Custom/TerrainTextureArray"
{
    Properties
    {
        _TextureArray("Texture Array", 2DArray) = "" { }
        
        _MinSlope("Min Slope (Flat)", Range(0, 1)) = 0
        _MaxSlope("Max Slope (Steep)", Range(0, 1)) = 0.03
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
            };

            struct v2_f
            {
                float4 pos2 : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            float min_slope;
            float max_slope;

            v2_f vert(appdata v)
            {
                v2_f o;
                o.pos2 = UnityObjectToClipPos(v.vertex);
                o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
                return o;
            }

            half4 frag(v2_f i) : SV_Target
            {
                float slope = 1.0 - saturate(dot(i.normal, float3(0, 1, 0)));
                
                float slope_factor = saturate((slope - min_slope) / (max_slope - min_slope));
                
                half3 color = lerp(float3(0, 1, 0), float3(1, 0, 0), slope_factor);

                return half4(color, 1);
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}