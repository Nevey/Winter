Shader "Deformable Surface/Deformable Surface Standard Coutout" {
	Properties{
		[HideInInspector]_MainTex("Diffuse Base", 2D) = "white" {}
		[HideInInspector]_albedo_flat("Duffuse Offset", 2D) = "white" {}
		[HideInInspector]_BumpMap("Normal Base", 2D) = "bump" {}
		[HideInInspector]_normal_flat("Normal Offset", 2D) = "bump" {}

		[NoScaleOffset]_baseMap("Base Map", 2D) = "white" {}
		[HideInInspector]_height_max("Max Height", Float) = 10
		[HideInInspector]_offset_max("Max Offset", Float) = 0.5

		[HideInInspector]_metallic("Metallic", Range(0, 1)) = 0
		[HideInInspector]_smoothness("Smoothness", Range(0, 1)) = 0
		[HideInInspector]_blend_height("Texture Blend Height", Float) = 1
		[HideInInspector]_tess_amount("Tess Quality", Float) = 3
		[HideInInspector]_tess_distance("Tess Distance", Float) = 50
		_cutTresnhold ("Cutoff Treshold", Range(0, 1)) = 0.01
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "AutoLight.cginc"
	#include "Lighting.cginc"
	#include "Tessellation.cginc"
	#include "UnityPBSLighting.cginc"
	#include "UnityStandardBRDF.cginc"	

	#pragma exclude_renderers gles3 metal d3d11_9x xbox360 ps3 psp2
	#pragma target 5.0
	#pragma hull hull
	#pragma domain domain
	#pragma vertex tessvert

	uniform float _tess_distance;
	uniform float _tess_amount;
	uniform sampler2D _baseMap;

	uniform float _offset_max;
	uniform float _height_max;
	uniform float _cutTresnhold;

	struct VertexInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
		float2 texcoord0 : TEXCOORD0;
		float2 texcoord1 : TEXCOORD1;
		float2 texcoord2 : TEXCOORD2;
	};

#if defined (UNITY_CAN_COMPILE_TESSELLATION)
	struct TessVertex
	{
		float4 vertex : INTERNALTESSPOS;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
		float2 texcoord0 : TEXCOORD0;
		float2 texcoord1 : TEXCOORD1;
		float2 texcoord2 : TEXCOORD2;
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

	float GetDisp(float2 uv)
	{
		float2 h = tex2Dlod(_baseMap, float4(uv, 0, 0)).rg;
		return (h.r * _height_max) + (h.g * _offset_max);
	}

	int TessDist(TessVertex v)
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

	OutputPatchConstant hullconst(InputPatch<TessVertex, 4> v)
	{
		OutputPatchConstant o = (OutputPatchConstant)0;
		float4 ts = Tessellation(v[0], v[1], v[2], v[3]);
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
	TessVertex hull(InputPatch<TessVertex, 4> v, uint id : SV_OutputControlPointID)
	{
		return v[id];
	}
	#endif	

	fixed3 UnpackNormalFixed(fixed4 packednormal)
	{
	#if defined(UNITY_NO_DXT5nm)
		return packednormal.xyz * 2 - 1;
	#else
		return UnpackNormalDXT5nm(packednormal);
	#endif
	}

	fixed3 UnpackNormalBase(fixed4 packednormal)
	{
		fixed3 normal;
		normal.xy = packednormal.zw * 2 - 1;
		normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
		return normal;
	}
	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque"  "DisableBatching" = "True" }
		Pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma fragment frag
			#define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
			#define _GLOSSYENV 1
			#pragma multi_compile_fwdbase_fullshadows
			#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
			#pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
			#pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fog

			uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
			uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
			uniform sampler2D _albedo_flat; uniform float4 _albedo_flat_ST;
			uniform sampler2D _normal_flat; uniform float4 _normal_flat_ST;

			uniform float _blend_height;
			uniform float _metallic;
			uniform float _smoothness;

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float4 posWorld : TEXCOORD3;
				float3 normalDir : TEXCOORD4;
				float3 tangentDir : TEXCOORD5;
				float3 bitangentDir : TEXCOORD6;
				LIGHTING_COORDS(7,8)
				UNITY_FOG_COORDS(9)
				#if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
					float4 ambientOrLightmapUV : TEXCOORD10;
				#endif
			};

			VertexOutput vert(VertexInput v)
			{
				
				VertexOutput o = (VertexOutput)0;
				o.uv0 = v.texcoord0;
				o.uv1 = v.texcoord1;
				o.uv2 = v.texcoord2;

				#ifdef LIGHTMAP_ON
					o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					o.ambientOrLightmapUV.zw = 0;
				#elif UNITY_SHOULD_SAMPLE_SH
				#endif

				#ifdef DYNAMICLIGHTMAP_ON
				o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				o.normalDir = UnityObjectToWorldNormal(v.normal);
				o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
				o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);

				UNITY_TRANSFER_FOG(o, o.pos);
				TRANSFER_VERTEX_TO_FRAGMENT(o)

				return o;
			}

			#if defined (UNITY_CAN_COMPILE_TESSELLATION)
				TessVertex tessvert(VertexInput v)
				{
					TessVertex o;
					o.vertex = v.vertex;
					o.normal = v.normal;
					o.tangent = v.tangent;
					o.texcoord0 = v.texcoord0;
					o.texcoord1 = v.texcoord1;
					o.texcoord2 = v.texcoord2;
					return o;
				}

				[domain("quad")]
				VertexOutput domain(OutputPatchConstant tessFactors, const OutputPatch<TessVertex,4> vi, float2 bary : SV_DomainLocation)
				{
					VertexInput v = (VertexInput)0;

					v.vertex = lerp(lerp(vi[0].vertex, vi[1].vertex, bary.x), lerp(vi[3].vertex, vi[2].vertex, bary.x), bary.y);
					v.normal = float3(lerp(lerp(vi[0].normal, vi[1].normal, bary.x), lerp(vi[3].normal, vi[2].normal, bary.x), bary.y));
					v.tangent = lerp(lerp(vi[0].tangent, vi[1].tangent, bary.x), lerp(vi[3].tangent, vi[2].tangent, bary.x), bary.y);
					v.texcoord0 = lerp(lerp(vi[0].texcoord0, vi[1].texcoord0, bary.x), lerp(vi[3].texcoord0, vi[2].texcoord0, bary.x), bary.y);
					v.texcoord1 = lerp(lerp(vi[0].texcoord1, vi[1].texcoord1, bary.x), lerp(vi[3].texcoord1, vi[2].texcoord1, bary.x), bary.y);
					v.texcoord2 = lerp(lerp(vi[0].texcoord2, vi[1].texcoord2, bary.x), lerp(vi[3].texcoord2, vi[2].texcoord2, bary.x), bary.y);
					v.vertex.y = GetDisp(v.texcoord1);

					return vert(v);
				}
			#endif

			float4 frag(VertexOutput i) : COLOR
			{
				float4 bm = tex2D(_baseMap, i.uv1);

				if (bm.g - _cutTresnhold < 0)
					discard;

				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float ColorLerp = saturate(((bm.g / _offset_max) * _blend_height));
				float3 nb = UnpackNormalBase(bm).rgb + float3(0, 0, 1);

				float3 nbd = lerp(
					UnpackNormalFixed(tex2D(_normal_flat, TRANSFORM_TEX(i.uv0, _normal_flat))).rgb,
					UnpackNormalFixed(tex2D(_BumpMap, TRANSFORM_TEX(i.uv0, _BumpMap))).rgb, ColorLerp) * float3(-1, -1, 1);

				float3 normalDirection = normalize(mul(nb * dot(nb, nbd) / nb.z - nbd, float3x3(i.tangentDir, i.bitangentDir, normalize(i.normalDir))));
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 halfDirection = normalize(viewDirection + lightDirection);

				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * _LightColor0.xyz;

				float4 AlbedoS = lerp(
					tex2D(_albedo_flat, TRANSFORM_TEX(i.uv0, _albedo_flat)).rgba,
					tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex)).rgba,
					ColorLerp);

				float gloss = (AlbedoS.a * _smoothness);

				UnityLight light;
				#ifdef LIGHTMAP_OFF
					light.color = _LightColor0.rgb;
					light.dir = lightDirection;
					light.ndotl = LambertTerm(normalDirection, light.dir);
				#else
					light.color = half3(0.f, 0.f, 0.f);
					light.ndotl = 0.0f;
					light.dir = half3(0.f, 0.f, 0.f);
				#endif

				UnityGIInput d;
				d.light = light;
				d.worldPos = i.posWorld.xyz;
				d.worldViewDir = viewDirection;
				d.atten = attenuation;

				d.ambient = 0;
				d.lightmapUV = i.ambientOrLightmapUV;

				d.boxMax[0] = unity_SpecCube0_BoxMax;
				d.boxMin[0] = unity_SpecCube0_BoxMin;
				d.probePosition[0] = unity_SpecCube0_ProbePosition;
				d.probeHDR[0] = unity_SpecCube0_HDR;
				d.boxMax[1] = unity_SpecCube1_BoxMax;
				d.boxMin[1] = unity_SpecCube1_BoxMin;
				d.probePosition[1] = unity_SpecCube1_ProbePosition;
				d.probeHDR[1] = unity_SpecCube1_HDR;

				Unity_GlossyEnvironmentData ugls_en_data;
				ugls_en_data.roughness = 1.0 - gloss;
				ugls_en_data.reflUVW = reflect(-viewDirection, normalDirection);
				UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data);
				lightDirection = gi.light.dir;

				float NdotL = max(0, dot(normalDirection, lightDirection));
				float LdotH = max(0.0,dot(lightDirection, halfDirection));

				float sm;
				float3 spec = _metallic;
				float3 diffuseColor = DiffuseAndSpecularFromMetallic(AlbedoS.rgb, spec, spec, sm);

				float NdotV = max(0.0,dot(normalDirection, viewDirection));
				float NdotH = max(0.0,dot(normalDirection, halfDirection));
				float VdotH = max(0.0,dot(viewDirection, halfDirection));

				float visTerm = SmithJointGGXVisibilityTerm(NdotL, NdotV, 1.0 - gloss);
				float normTerm = max(0.0, GGXTerm(NdotH, 1.0 - gloss));
				float specularPBL = (NdotL * visTerm * normTerm) * (UNITY_PI / 4);

				if (IsGammaSpace())
					specularPBL = sqrt(max(1e-4h, specularPBL));
				specularPBL = max(0, specularPBL * NdotL);
				float3 directSpecular = attenColor * specularPBL*FresnelTerm(spec, LdotH);
				half grazingTerm = saturate(gloss + (1.0 - sm));
				float3 indirectSpecular = (gi.indirect.specular);
				indirectSpecular *= FresnelLerp(spec, grazingTerm, NdotV);

				NdotL = max(0.0, dot(normalDirection, lightDirection));
				half fd90 = 0.5 + 2 * LdotH * LdotH * (1 - gloss);

				float3 directDiffuse = ((1 + (fd90 - 1) * Pow5(1 - NdotL)) * (1 + (fd90 - 1) * Pow5(1 - NdotV)) * NdotL) * attenColor;
				float3 diffuse = (directDiffuse + gi.indirect.diffuse) * diffuseColor;

				float3 finalColor = diffuse + (directSpecular + indirectSpecular);
				fixed4 finalRGBA = fixed4(finalColor,1);
				UNITY_APPLY_FOG(i.fogCoord, finalRGBA);

				return finalRGBA;
			}
			ENDCG
		}


		Pass
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }
			Blend One One

			CGPROGRAM
			#pragma fragment frag
			#define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
			#define _GLOSSYENV 1
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile _ LIGHTMAP_OFF LIGHTMAP_ON
			#pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
			#pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fog
			uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
			uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
			uniform sampler2D _albedo_flat; uniform float4 _albedo_flat_ST;
			uniform sampler2D _normal_flat; uniform float4 _normal_flat_ST;

			uniform float _blend_height;
			uniform float _metallic;
			uniform float _smoothness;

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float4 posWorld : TEXCOORD3;
				float3 normalDir : TEXCOORD4;
				float3 tangentDir : TEXCOORD5;
				float3 bitangentDir : TEXCOORD6;
				LIGHTING_COORDS(7,8)
				UNITY_FOG_COORDS(9)
			};

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;
				o.uv0 = v.texcoord0;
				o.uv1 = v.texcoord1;
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
				o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_VERTEX_TO_FRAGMENT(o)
				return o;
			}

			#ifdef UNITY_CAN_COMPILE_TESSELLATION
				TessVertex tessvert(VertexInput v)
				{
					TessVertex o;
					o.vertex = v.vertex;
					o.normal = v.normal;
					o.tangent = v.tangent;
					o.texcoord0 = v.texcoord0;
					o.texcoord1 = v.texcoord1;
					o.texcoord2 = v.texcoord2;
					return o;
				}

				[domain("quad")]
				VertexOutput domain(OutputPatchConstant tessFactors, const OutputPatch<TessVertex,4> vi, float2 bary : SV_DomainLocation)
				{
					VertexInput v = (VertexInput)0;

					v.vertex = lerp(lerp(vi[0].vertex, vi[1].vertex, bary.x), lerp(vi[3].vertex, vi[2].vertex, bary.x), bary.y);
					v.normal = float3(lerp(lerp(vi[0].normal, vi[1].normal, bary.x), lerp(vi[3].normal, vi[2].normal, bary.x), bary.y));
					v.tangent = lerp(lerp(vi[0].tangent, vi[1].tangent, bary.x), lerp(vi[3].tangent, vi[2].tangent, bary.x), bary.y);
					v.texcoord0 = lerp(lerp(vi[0].texcoord0, vi[1].texcoord0, bary.x), lerp(vi[3].texcoord0, vi[2].texcoord0, bary.x), bary.y);
					v.texcoord1 = lerp(lerp(vi[0].texcoord1, vi[1].texcoord1, bary.x), lerp(vi[3].texcoord1, vi[2].texcoord1, bary.x), bary.y);
					v.texcoord2 = lerp(lerp(vi[0].texcoord2, vi[1].texcoord2, bary.x), lerp(vi[3].texcoord2, vi[2].texcoord2, bary.x), bary.y);
					v.vertex.y = GetDisp(v.texcoord1);

					return vert(v);
				}
			#endif

			float4 frag(VertexOutput i) : COLOR
			{
				float4 bm = tex2D(_baseMap, i.uv1);

				if (bm.g - _cutTresnhold < 0)
					discard;

				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float ColorLerp = saturate((((bm.r + bm.g - bm.r) / ((bm.r + _offset_max) - bm.r)) * _blend_height));
				float3 nb = UnpackNormalBase(bm).rgb + float3(0, 0, 1);

				float3 nbd = lerp(
					UnpackNormalFixed(tex2D(_normal_flat, TRANSFORM_TEX(i.uv0, _normal_flat))).rgb,
					UnpackNormalFixed(tex2D(_BumpMap, TRANSFORM_TEX(i.uv0, _BumpMap))).rgb, ColorLerp) * float3(-1, -1, 1);

				float3 normalDirection = normalize(mul(nb * dot(nb, nbd) / nb.z - nbd, float3x3(i.tangentDir, i.bitangentDir, normalize(i.normalDir))));
				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
				float3 halfDirection = normalize(viewDirection + lightDirection);

				float3 attenColor = LIGHT_ATTENUATION(i) * _LightColor0.xyz;

				float4 AlbedoS = lerp(
					tex2D(_albedo_flat, TRANSFORM_TEX(i.uv0, _albedo_flat)).rgba,
					tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex)).rgba,
					ColorLerp);

				float gloss = (AlbedoS.a * _smoothness);

				float NdotL = max(0, dot(normalDirection, lightDirection));
				float LdotH = max(0.0, dot(lightDirection, halfDirection));

				float sm;
				float3 spec = _metallic;
				float3 diffuseColor = DiffuseAndSpecularFromMetallic(AlbedoS.rgb, spec, spec, sm);
				sm = 1.0 - sm;
				float NdotV = max(0.0,dot(normalDirection, viewDirection));
				float NdotH = max(0.0,dot(normalDirection, halfDirection));
				float VdotH = max(0.0,dot(viewDirection, halfDirection));

				float visTerm = SmithJointGGXVisibilityTerm(NdotL, NdotV, 1.0 - gloss);
				float normTerm = max(0.0, GGXTerm(NdotH, 1.0 - gloss));
				float specularPBL = (NdotL * visTerm * normTerm) * (UNITY_PI / 4);

				if (IsGammaSpace())
					specularPBL = sqrt(max(1e-4h, specularPBL));
				specularPBL = max(0, specularPBL * NdotL);
				float3 specular = attenColor * specularPBL*FresnelTerm(spec, LdotH);

				NdotL = max(0.0,dot(normalDirection, lightDirection));
				half fd90 = 0.5 + 2 * LdotH * LdotH * (1 - gloss);
				float nlPow5 = Pow5(1 - NdotL);
				float nvPow5 = Pow5(1 - NdotV);
				float3 directDiffuse = ((1 + (fd90 - 1) * nlPow5) * (1 + (fd90 - 1) * nvPow5) * NdotL) * attenColor;
				float3 diffuse = directDiffuse * diffuseColor;

				float3 finalColor = diffuse + specular;
				fixed4 finalRGBA = fixed4(finalColor * 1,0);
				UNITY_APPLY_FOG(i.fogCoord, finalRGBA);

				return finalRGBA;
			}
			ENDCG
		}

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			Offset 1, 1

			CGPROGRAM
			#pragma fragment frag
			#define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
			#define _GLOSSYENV 1
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcaster
			#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
			#pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
			#pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fog

			struct VertexOutput
			{
				V2F_SHADOW_CASTER;
				float2 uv1 : TEXCOORD1;
				float4 posWorld : TEXCOORD3;
			};

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;
				o.uv1 = v.texcoord1;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			#ifdef UNITY_CAN_COMPILE_TESSELLATION
			TessVertex tessvert(VertexInput v)
			{
				TessVertex o;
				o.vertex = v.vertex;
				o.normal = v.normal;
				o.tangent = v.tangent;
				o.texcoord0 = v.texcoord0;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				return o;
			}

			[domain("quad")]
			VertexOutput domain(OutputPatchConstant tessFactors, const OutputPatch<TessVertex,4> vi, float2 bary : SV_DomainLocation)
			{
				VertexInput v = (VertexInput)0;

				v.vertex = lerp(lerp(vi[0].vertex, vi[1].vertex, bary.x), lerp(vi[3].vertex, vi[2].vertex, bary.x), bary.y);
				v.normal = float3(lerp(lerp(vi[0].normal, vi[1].normal, bary.x), lerp(vi[3].normal, vi[2].normal, bary.x), bary.y));
				v.tangent = lerp(lerp(vi[0].tangent, vi[1].tangent, bary.x), lerp(vi[3].tangent, vi[2].tangent, bary.x), bary.y);
				v.texcoord0 = lerp(lerp(vi[0].texcoord0, vi[1].texcoord0, bary.x), lerp(vi[3].texcoord0, vi[2].texcoord0, bary.x), bary.y);
				v.texcoord1 = lerp(lerp(vi[0].texcoord1, vi[1].texcoord1, bary.x), lerp(vi[3].texcoord1, vi[2].texcoord1, bary.x), bary.y);
				v.texcoord2 = lerp(lerp(vi[0].texcoord2, vi[1].texcoord2, bary.x), lerp(vi[3].texcoord2, vi[2].texcoord2, bary.x), bary.y);
				v.vertex.y = GetDisp(v.texcoord1);

				return vert(v);
			}
			#endif

			float4 frag(VertexOutput i) : COLOR
			{				
				if (tex2D(_baseMap, i.uv1).g - _cutTresnhold < 0)
					discard;

				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
		FallBack "Legacy Shaders/Diffuse"
}
