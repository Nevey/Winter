Shader "Snow/SnowTracks"
{
    Properties
    {
        _Tess ("Tessellation", Range(1,32)) = 4
        _TessMinDist ("Tessellation Min Distance", Range(10, 50)) = 25
        _TessMaxDist ("Tessellation Max Distance", Range(50, 100)) = 75
        _SnowColor ("Snow Color", Color) = (1,1,1,1)
        _SnowTex ("Snow (RGB)", 2D) = "white" {}
        _GroundColor ("Ground Color", Color) = (1,1,1,1)
        _GroundTex ("Ground (RGB)", 2D) = "white" {}
        _Splat ("SplatMap", 2D) = "black" {}
        _Displacement ("Displacement", Range(0, 5)) = 0.3
        _BumpMap ("Bumpmap", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:disp tessellate:tessDistance

        #pragma target 4.6
        
        #include "Tessellation.cginc"

        struct appdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
            float2 texcoord1 : TEXCOORD1;
            float2 texcoord2 : TEXCOORD2;
        };

        float _Tess;
        float _TessMinDist;
        float _TessMaxDist;

        float4 tessDistance (appdata v0, appdata v1, appdata v2) {
            float minDist = _TessMinDist;
            float maxDist = _TessMaxDist;
            return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
        }

        sampler2D _Splat;
        float _Displacement;
        float _DisplacementMultiplier;
        float _SnowDepth;

        void disp (inout appdata v)
        {
            float d = tex2Dlod(_Splat, float4(v.texcoord.xy,0,0)).r * _Displacement;
            v.vertex.xyz += v.normal * (_Displacement - d);
        }

        struct Input
        {
            float2 uv_SnowTex;
            float2 uv_GroundTex;
            float2 uv_Splat;
            float2 uv_BumpMap;
        };
        
        sampler2D _SnowTex;
        fixed4 _SnowColor;
        sampler2D _GroundTex;
        fixed4 _GroundColor;
        sampler2D _BumpMap;

        half _Glossiness;
        half _Metallic;
        half _AlbedoOffset;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        float quadraticOut(float t) {
            return -t * (t - 2.0);
        }
        
        float quadraticIn(float t) {
            return t * t;
        }
        
        float exponentialOut(float t) {
            return t == 1.0 ? t : 1.0 - pow(2.0, -10.0 * t);
        }
        
        float exponentialIn(float t) {
            return t == 0.0 ? t : pow(2.0, 10.0 * (t - 1.0));
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half UVvalue = tex2Dlod(_Splat, float4(IN.uv_Splat,0,0)).r;
            
            // Revert the UV value as we're displacing downwards
            half amount = quadraticIn(UVvalue);
            fixed4 c = lerp(tex2D (_SnowTex, IN.uv_SnowTex) * _SnowColor, tex2D (_GroundTex, IN.uv_GroundTex) * _GroundColor, amount);
            
            // Albedo comes from a texture tinted by color
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            
            o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
