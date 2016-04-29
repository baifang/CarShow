Shader "Beffio/Car Paint Transparent"
{
	Properties
	{
		_DetailColor("Detail Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_DetailMap("Detail Map", 2D) = "white" {}
		_DetailMapParams("Detail Map Tiling (XY), Depth Bias (Z), Unused (W)", Vector) = (1.0, 1.0, 1.0, 1.0)
		_DiffuseColor("Paint Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_DiffuseMapColorAndAlpha("Texture Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_DiffuseMap("Diffuse Map", 2D) = "white" {}
		_MatCapLookup("MatCap Lookup", 2D) = "white" {}
		_ReflectionColor("Reflection Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_ReflectionMap("Reflection Map", Cube) = "" {}
		_ReflectionStrength("Reflection Strength", Range(0.0, 1.0)) = 0.5
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			ZWrite Off

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma fragment FragmentMain
			#pragma vertex VertexMain

			float4 _DetailColor;
			sampler2D _DetailMap;
			float4 _DetailMapParams;
			float4 _DiffuseColor;
			float4 _DiffuseMapColorAndAlpha;
			sampler2D _DiffuseMap;
			float4 _DiffuseMap_ST;
			sampler2D _MatCapLookup;
			float4 _ReflectionColor;
			samplerCUBE _ReflectionMap;
			float _ReflectionStrength;

			struct VertexInput
			{
				float3 normal : NORMAL;
				float4 position : POSITION;
				float2 UVCoordsChannel1: TEXCOORD0;
			};

			struct VertexToFragment
			{
				float3 detailUVCoordsAndDepth : TEXCOORD0;
				float2 matCapLookupCoords : TEXCOORD1;
				float4 position : SV_POSITION;
				float3 worldSpaceReflectionVector : TEXCOORD2;
				float2 diffuseLookupCoords : TEXCOORD3;
			};

			float4 FragmentMain(VertexToFragment input) : COLOR
			{
				float3 reflectionColor = texCUBE(_ReflectionMap, input.worldSpaceReflectionVector) * _ReflectionColor.rgb;
				float3 diffuseMapColor = tex2D(_DiffuseMap, input.diffuseLookupCoords).rgb * _DiffuseMapColorAndAlpha.rgb;
				float3 diffuseColor = _DiffuseColor.rgb * lerp(float3(1.0, 1.0, 1.0), diffuseMapColor, _DiffuseMapColorAndAlpha.a);
				float3 finalColor = lerp(diffuseColor, reflectionColor, _ReflectionStrength);

				float3 detailMask = tex2D(_DetailMap, input.detailUVCoordsAndDepth.xy);
				float3 detailColor = lerp(_DetailColor.rgb, finalColor, detailMask);
				finalColor = lerp(detailColor, finalColor, saturate(input.detailUVCoordsAndDepth.z * _DetailMapParams.z));

				float3 matCapColor = tex2D(_MatCapLookup, input.matCapLookupCoords);
				return float4(finalColor * matCapColor * 2.0, _DiffuseColor.a);
			}

			VertexToFragment VertexMain(VertexInput input)
			{
				VertexToFragment output;

				output.matCapLookupCoords.x = dot(normalize(UNITY_MATRIX_IT_MV[0].xyz), input.normal);
				output.matCapLookupCoords.y = dot(normalize(UNITY_MATRIX_IT_MV[1].xyz), input.normal);
				output.matCapLookupCoords = output.matCapLookupCoords * 0.5 + 0.5;

				output.position = mul(UNITY_MATRIX_MVP, input.position);

				output.detailUVCoordsAndDepth.xy = input.UVCoordsChannel1 * _DetailMapParams.xy;
				output.detailUVCoordsAndDepth.z = output.position.z;

				float3 worldSpacePosition = mul(_Object2World, input.position);
				float3 worldSpaceNormal = normalize(mul((float3x3)_Object2World, input.normal));
				output.worldSpaceReflectionVector = reflect(worldSpacePosition - _WorldSpaceCameraPos.xyz, worldSpaceNormal);
				
				output.diffuseLookupCoords = input.UVCoordsChannel1 * _DiffuseMap_ST.xy;

				return output;
			}
			ENDCG
		}
	}

	Fallback Off
}
