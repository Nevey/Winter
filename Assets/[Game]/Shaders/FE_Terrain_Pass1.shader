Shader "Custom/FE_Terrain_Pass1"
{
    Properties
    {
        _Control ("Control (RGBA)", 2D) = "red" {}
        _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        _Splat0 ("Layer 0 (R)", 2D) = "white" {}
        _SnowNormTex ("Snow Normal Map", 2D) = "black" {}
        _SnowColor ("Snow Colour", Color) = (1, 1, 1, 1)
        _SnowAmount ("Snow Amount", Range(0, 1)) = 0
    }
 
 
    SubShader
    {
        Tags
        {
            "SplatCount" = "4"
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque"
        }
 
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert
        #pragma target 3.0
        #include "UnityCG.cginc"
 
        struct Input
        {
            float3 worldPos;
            float2 uv_Control : TEXCOORD0;
            float2 uv_Splat0 : TEXCOORD1;
            float2 uv_Splat1 : TEXCOORD2;
            float2 uv_Splat2 : TEXCOORD3;
            float2 uv_Splat3 : TEXCOORD4;
            float2 uv_SnowNormTex;
            fixed4 color : COLOR;
 
        };
 
        // Supply the shader with tangents for the terrain
        void vert (inout appdata_full v)
        {
            // A general tangent estimation
            float3 T1 = float3(1, 0, 1);
            float3 Bi = cross(T1, v.normal);
            float3 newTangent = cross(v.normal, Bi);
            normalize(newTangent);
            v.tangent.xyz = newTangent.xyz;
            if (dot(cross(v.normal,newTangent),Bi) < 0)
                v.tangent.w = -1.0f;
            else
                v.tangent.w = 1.0f;
        }
       
        sampler2D _Control;
        sampler2D _BumpMap0, _BumpMap1, _BumpMap2, _BumpMap3;
        sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
        float _Tile0, _Tile1, _Tile2, _Tile3, _TerrainX, _TerrainZ;
        float4 _v4CameraPos;
        sampler2D _SnowNormTex;
        float _SnowAmount;
        float4 _SnowColor;
 
 
        void surf (Input IN, inout SurfaceOutput o) {
       
            half4 splat_control = tex2D (_Control, IN.uv_Control);
            half3 pixcolor;
           
            // first texture which is usually some kind of rock texture gets mixed with itself
            // see: http://forum.unity3d.com/threads/116509-Improved-Terrain-Texture-Tiling    
            pixcolor = splat_control.r * tex2D (_Splat0, IN.uv_Splat0).rgb * tex2D (_Splat0, IN.uv_Splat0 * -0.125).rgb * 4;
            o.Normal = splat_control.r * UnpackNormal(tex2D(_BumpMap0, float2(IN.uv_Control.x * (_TerrainX/_Tile0), IN.uv_Control.y * (_TerrainZ/_Tile0))));
       
            pixcolor += splat_control.g * tex2D (_Splat1, IN.uv_Splat1).rgb;
            o.Normal += splat_control.g * UnpackNormal(tex2D(_BumpMap1, float2(IN.uv_Control.x * (_TerrainX/_Tile1), IN.uv_Control.y * (_TerrainZ/_Tile1))));
           
            pixcolor += splat_control.b * tex2D (_Splat2, IN.uv_Splat2).rgb;
            o.Normal += splat_control.b * UnpackNormal(tex2D(_BumpMap2, float2(IN.uv_Control.x * (_TerrainX/_Tile2), IN.uv_Control.y * (_TerrainZ/_Tile2))));
           
            pixcolor += splat_control.a * tex2D (_Splat3, IN.uv_Splat3).rgb;
            o.Normal += splat_control.a * UnpackNormal(tex2D(_BumpMap3, float2(IN.uv_Control.x * (_TerrainX/_Tile3), IN.uv_Control.y * (_TerrainZ/_Tile3))));
           
            // blend the pixel colour with the snow (if required)
            if(WorldNormalVector(IN, o.Normal.y) > lerp(1, -1, _SnowAmount))
            {
                o.Albedo.rgb = _SnowColor.rgb;
                o.Normal = UnpackNormal (tex2D (_SnowNormTex, IN.uv_SnowNormTex));
            } else {
                o.Albedo.rgb = pixcolor.rgb;
            }
            o.Alpha = 0;
        }
       
        ENDCG  
    }
 
    //Dependency "AddPassShader" = "Custom/FE_Terrain_Pass2"
    Dependency "BaseMapShader" = "Diffuse"
 
    // Fallback to Diffuse
    Fallback "Diffuse"
}
 