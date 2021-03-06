Shader "Paint/Brush"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Coordinate ("Coordinate", Vector) = (0,0,0,0)
        _Color ("DrawColor", Color) = (1,0,0,0)
        _Size ("Size", Range(1, 500)) = 1
        _Strength ("Strength", Range(0, 1)) = 1
        _Appends ("Appends", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Coordinate, _Color;
            half _Size, _Strength;
            fixed _Appends;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 500 / _Size is to get a value between 0 and 1
                float draw = pow(saturate(1 - distance(i.uv, _Coordinate.xy)), 500 / _Size);
                fixed4 drawCol = _Color * (draw * _Strength);
                
                if (_Appends >= 1)
                {
                    return saturate(col + drawCol);
                }
                else
                {
                    return saturate(col - drawCol);
                }
            }
            
            ENDCG
        }
    }
}
