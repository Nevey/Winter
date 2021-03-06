// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Paint/PaintedSurface" {

    Properties {
        [HideInInspector] _MainTex ("Ground Texture", 2D) = "white" {}
        [HideInInspector] _MainNormal ("Ground Normal Map", 2D) = "bump" {}
        
        [HideInInspector] _DeformTex ("Deformed Texture", 2D) = "white" {}
        [HideInInspector] _DeformDispTex ("Deformed Displacement Texture", 2D) = "white" {}
        [HideInInspector] _DeformAlpha ("Deformed Alpha Map", 2D) = "black" {}
        
        [HideInInspector] _PaintTex0 ("Painted Texture 1", 2D) = "white" {}
        [HideInInspector] _PaintNormal0 ("Painted Normal Map 1", 2D) = "bump" {}
        [HideInInspector] _PaintAlpha0 ("Painted Alpha Map 1", 2D) = "black" {}
        
        [HideInInspector] _PaintTex1 ("Painted Texture 2", 2D) = "white" {}
        [HideInInspector] _PaintNormal1 ("Painted Normal Map 2", 2D) = "bump" {}
        [HideInInspector] _PaintAlpha1 ("Painted Alpha Map 2", 2D) = "black" {}
        
        [HideInInspector] _PaintTex2 ("Painted Texture 3", 2D) = "white" {}
        [HideInInspector] _PaintNormal2 ("Painted Normal Map 3", 2D) = "bump" {}
        [HideInInspector] _PaintAlpha2 ("Painted Alpha Map 3", 2D) = "black" {}
        
        [HideInInspector] _PaintTex3 ("Painted Texture 4", 2D) = "white" {}
        [HideInInspector] _PaintNormal3 ("Painted Normal Map 4", 2D) = "bump" {}
        [HideInInspector] _PaintAlpha3 ("Painted Alpha Map 4", 2D) = "black" {}
        
        [HideInInspector] _PaintTex4 ("Painted Texture 5", 2D) = "white" {}
        [HideInInspector] _PaintNormal4 ("Painted Normal Map 5", 2D) = "bump" {}
        [HideInInspector] _PaintAlpha4 ("Painted Alpha Map 5", 2D) = "black" {}
        
        _Tess ("Tessellation", Range(1,32)) = 16
        _TessMinDist ("Tessellation Min Distance", Range(0, 10)) = 0
        _TessMaxDist ("Tessellation Max Distance", Range(0, 20)) = 20
        
        _Displacement ("Displacement", Range(0, 10.0)) = 1
        
        _Color ("Color", color) = (1,1,1,0)
        
        _AlphaOffset ("Alpha Offset", Range(0, 1)) = 0.1
    }
    SubShader {
    
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        
        // 1st pass - surface texture
        CGPROGRAM
        
        #pragma surface surf BlinnPhong 
        //addshadow fullforwardshadows
        #pragma target 4.6

        struct appdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
        };

        struct Input {
            float2 uv_MainTex;
            float2 uv_MainNormal;
        };

        sampler2D _MainTex;
        sampler2D _MainNormal;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutput o)
        {
            half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
             
            o.Albedo = c.rgb;
            o.Alpha = 1;
            o.Normal = UnpackNormal (tex2D (_MainNormal, IN.uv_MainNormal)); 
        }
        
        ENDCG
        
        
        // 2nd pass - surface paint
        CGPROGRAM
        
        #pragma surface surf BlinnPhong keepalpha
        //addshadow fullforwardshadows
        #pragma target 4.6

        struct Input {
            float2 uv_PaintTex0;
            float2 uv_PaintNormal0;
            float2 uv_PaintAlpha0;
            
            float2 uv_PaintTex1;
            float2 uv_PaintNormal1;
            float2 uv_PaintAlpha1;
            
            float2 uv_PaintTex2;
            float2 uv_PaintNormal2;
            float2 uv_PaintAlpha2;
            
            float2 uv_PaintTex3;
            float2 uv_PaintNormal3;
            float2 uv_PaintAlpha3;
            
            float2 uv_PaintTex4;
            float2 uv_PaintNormal4;
            float2 uv_PaintAlpha4;
        };
        
        sampler2D _PaintTex0;
        sampler2D _PaintNormal0;
        sampler2D _PaintAlpha0;
        
        sampler2D _PaintTex1;
        sampler2D _PaintNormal1;
        sampler2D _PaintAlpha1;
        
        sampler2D _PaintTex2;
        sampler2D _PaintNormal2;
        sampler2D _PaintAlpha2;
        
        sampler2D _PaintTex3;
        sampler2D _PaintNormal3;
        sampler2D _PaintAlpha3;
        
        sampler2D _PaintTex4;
        sampler2D _PaintNormal4;
        sampler2D _PaintAlpha4;

        void surf (Input IN, inout SurfaceOutput o)
        {            
            half4 c0 = tex2D(_PaintTex0, IN.uv_PaintTex0);
            half4 c1 = tex2D(_PaintTex1, IN.uv_PaintTex1);
            half4 c2 = tex2D(_PaintTex2, IN.uv_PaintTex2);
            half4 c3 = tex2D(_PaintTex3, IN.uv_PaintTex3);
            half4 c4 = tex2D(_PaintTex4, IN.uv_PaintTex4);
            
            half alpha0 = tex2D(_PaintAlpha0, IN.uv_PaintAlpha0).r;
            half alpha1 = tex2D(_PaintAlpha1, IN.uv_PaintAlpha1).r;
            half alpha2 = tex2D(_PaintAlpha2, IN.uv_PaintAlpha2).r;
            half alpha3 = tex2D(_PaintAlpha3, IN.uv_PaintAlpha3).r;
            half alpha4 = tex2D(_PaintAlpha4, IN.uv_PaintAlpha4).r;
            
            o.Albedo = c0.rgb * alpha0 
                        + c1.rgb * alpha1
                        + c2.rgb * alpha2
                        + c3.rgb * alpha3
                        + c4.rgb * alpha4;
                        
            o.Alpha = alpha0 + alpha1 + alpha2 + alpha3 + alpha4;
            
            // TODO: Normals...
//            o.Normal = UnpackNormal (tex2D (_PaintNormal0, IN.uv_PaintNormal0)); 
        }
        
        ENDCG

        
        // 3nd pass - deform paint
        CGPROGRAM
        
        #pragma surface surf BlinnPhong addshadow fullforwardshadows keepalpha vertex:disp tessellate:tessDistance
        #pragma target 4.6
        
        #include "Tessellation.cginc"

        struct appdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
        };

        float _Tess;
        float _TessMinDist;
        float _TessMaxDist;

        float4 tessDistance (appdata v0, appdata v1, appdata v2) {
            float minDist = _TessMinDist;
            float maxDist = _TessMaxDist;
            return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
        }

        sampler2D _DispTex;
        sampler2D _DefaultAlphaMap;
        
        float _Displacement;

        struct Input {
            float2 uv_DeformTex;
            float2 uv_DeformDispTex;
            float2 uv_DeformAlpha;
        };
        
        sampler2D _DeformTex;
        sampler2D _DeformDispTex;
        sampler2D _DeformAlpha;
        
        fixed4 _Color;
        float _AlphaOffset;
        float4x4 _World2Camera;
        float4 _DeformDispTex_ST;
        
        void disp (inout appdata v)
        {
            half alpha = tex2Dlod(_DeformAlpha, float4(v.texcoord.xy,0,0)).r;
            float disp = tex2Dlod(_DeformDispTex, float4(v.texcoord.xy * _DeformDispTex_ST,0,0)).r * _Displacement * alpha;
            
            v.vertex.xyz += v.normal * disp;
            float4 vertex_view = mul(_World2Camera, mul(unity_ObjectToWorld, v.vertex));
            
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            half4 c = tex2D(_DeformTex, IN.uv_DeformTex);
            half alpha = tex2D(_DeformAlpha, IN.uv_DeformAlpha).r;
            
            o.Albedo = c.rgb;
            o.Alpha = saturate(alpha / _AlphaOffset);
        }
      
        ENDCG
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    
    FallBack "Diffuse"
}