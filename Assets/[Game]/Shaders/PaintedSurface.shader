Shader "Paint/PaintedSurface" {
        Properties {
            _Tess ("Tessellation", Range(1,32)) = 4
            _MainTex ("Base (RGB)", 2D) = "white" {}
            _DispTex ("Disp Texture", 2D) = "gray" {}
            _AlphaMap ("Alpha Map", 2D) = "white" {}
            _Displacement ("Displacement", Range(0, 10.0)) = 0.3
            _Color ("Color", color) = (1,1,1,0)
        }
        SubShader {
            Tags {"RenderType"="Transparent"}
            
            CGPROGRAM
            #pragma surface surf Standard addshadow fullforwardshadows vertex:disp tessellate:tessFixed nolightmap alpha:fade
            #pragma target 4.6

            struct appdata {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            float _Tess;

            float4 tessFixed()
            {
                return _Tess;
            }

            sampler2D _DispTex;
            sampler2D _AlphaMap;
            
            float _Displacement;

            void disp (inout appdata v)
            {
                float alpha = tex2Dlod(_AlphaMap, float4(v.texcoord.xy,0,0)).r;                
                float d = tex2Dlod(_DispTex, float4(v.texcoord.xy,0,0)).r * _Displacement * alpha;
                v.vertex.xyz += v.normal * d;
            }

            struct Input {
                float2 uv_MainTex;
                float2 uv_AlphaMap;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            
            float quadraticIn(float t) {
                return t * t;
            }

            void surf (Input IN, inout SurfaceOutputStandard o) {
            
                half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                
                half alpha = tex2D(_AlphaMap, IN.uv_AlphaMap).r;
                 
                o.Albedo = c.rgb;
                o.Alpha = alpha;
            }
            ENDCG
        }
        FallBack "Diffuse"
    }