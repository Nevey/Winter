// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Deformable Surface/Surface Editor Brush" 
{
    Properties 
	{
        _brush ("brush", 2D) = "white" {}
		_brush_hardness("brush_hardness", Float) = 1

		[NoScaleOffset] _baseMap ("Base Map", 2D) = "black" {}
		_tess_distance("tess_distance", Float) = 60
		_tess_amount("tess_amount", Float) = 3

		_height_max("Max Height", Float) = 25
		_offset_max("Max Offset", Float) = 0.5
    }
    SubShader 
		{
        Tags 
		{
            "IgnoreProjector" = "True"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        Pass 
		{
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma hull hull
            #pragma domain domain
            #pragma vertex tessvert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "Tessellation.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 ps3 psp2 
            #pragma target 5.0

			uniform sampler2D _brush; uniform float4 _brush_ST;
			uniform float _brush_hardness;

			uniform sampler2D _baseMap;
            uniform float _tess_distance;
            uniform float _tess_amount;

			uniform float _height_max;
			uniform float _offset_max;

            struct VertexInput 
			{
                float4 vertex : POSITION;
                float2 texcoord1 : TEXCOORD1;
            };

            struct VertexOutput 
			{
                float4 pos : SV_POSITION;
                float2 uv1 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
            };

            VertexOutput vert (VertexInput v)
			{
                VertexOutput o = (VertexOutput)0;
                o.uv1 = v.texcoord1;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;				
            }

			#ifdef UNITY_CAN_COMPILE_TESSELLATION
			struct TessVertex 
			{
				float4 vertex : INTERNALTESSPOS;
				float2 texcoord1 : TEXCOORD1;
			};

			struct OutputPatchConstant 
			{
				float edge[4]         : SV_TessFactor;
				float inside[2]          : SV_InsideTessFactor;
				float3 vTangent[4]    : TANGENT;
				float2 vUV[4]         : TEXCOORD;
				float3 vTanUCorner[4] : TANUCORNER;
				float3 vTanVCorner[4] : TANVCORNER;
				float4 vCWts          : TANWEIGHTS;
			};   

			float TessDist(TessVertex v)
			{					
				return clamp(((1.0 / distance(mul(unity_ObjectToWorld, v.vertex).rgb, _WorldSpaceCameraPos)) * _tess_distance), 1.0, _tess_amount);
			}	

			float4 Tessellation(TessVertex v, TessVertex v1, TessVertex v2, TessVertex v3)
			{	
				float4 t = float4(TessDist(v), TessDist(v1), TessDist(v2), TessDist(v3));
				float4 tes;
				tes.x = (t.w + t.x) / 2;
				tes.y = (t.x + t.y) / 2;
				tes.z = (t.y + t.z) / 2;
				tes.w = (t.z + t.w) / 2;

				return tes;
			}

			OutputPatchConstant hullconst (InputPatch<TessVertex,4> v)
			{
				OutputPatchConstant o = (OutputPatchConstant)0;
				float4 ts = Tessellation( v[0], v[1], v[2], v[3]);
				o.edge[0] = ts.x;
				o.edge[1] = ts.y;
				o.edge[2] = ts.z;
				o.edge[3] = ts.w;
				o.inside[0] = o.inside[1] = (ts.x + ts.y + ts.z + ts.w) / 4;
				return o;
			}

			[domain("quad")]
			[partitioning("integer")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("hullconst")]
			[outputcontrolpoints(4)]
			TessVertex hull (InputPatch<TessVertex,4> v, uint id : SV_OutputControlPointID) 
			{
				return v[id];
			}

			TessVertex tessvert (VertexInput v) 
			{
                TessVertex o;
                o.vertex = v.vertex;
                o.texcoord1 = v.texcoord1;
                return o;
            }

            void displacement (inout VertexInput v)
			{
				float2 h = tex2Dlod(_baseMap, float4(v.texcoord1, 0, 0)).rg;
				v.vertex.y = (h.r * _height_max) + (h.g * _offset_max);
            }

            [domain("quad")]
            VertexOutput domain (OutputPatchConstant tessFactors, const OutputPatch<TessVertex,4> vi, float2 bary : SV_DomainLocation) 
			{                    
				VertexInput v = (VertexInput)0;

				v.vertex = lerp(lerp(vi[0].vertex, vi[1].vertex, bary.x), lerp(vi[3].vertex, vi[2].vertex, bary.x), bary.y);
				v.texcoord1 = lerp(lerp(vi[0].texcoord1, vi[1].texcoord1, bary.x), lerp(vi[3].texcoord1, vi[2].texcoord1, bary.x), bary.y);
                displacement(v);

                return vert(v);
            }
			#endif
            
            float4 frag(VertexOutput i) : COLOR 
			{                
                float bCol = clamp(tex2D(_brush, TRANSFORM_TEX(i.uv1, _brush)).r, 0.001, 0.99);               
				float3 rgb = float3(0, ceil(bCol), 0);
				float alpha = bCol * (_brush_hardness * 0.2 + 0.2);

                return fixed4(rgb, alpha);
            }
            ENDCG
        }        
    }
    FallBack "Diffuse"
}
