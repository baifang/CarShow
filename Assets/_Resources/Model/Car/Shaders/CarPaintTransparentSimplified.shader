Shader "Beffio/Car Paint Transparent (Simplified)"
{
	Properties
	{
		_DiffuseColor("Paint Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_DiffuseMapColorAndAlpha("Texture Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_DiffuseMap("Diffuse Map", 2D) = "white" {}
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

		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off

		CGPROGRAM
		#pragma surface SurfaceMain Lambert alpha

		float4 _DiffuseColor;
		float4 _DiffuseMapColorAndAlpha;
		sampler2D _DiffuseMap;
		float4 _ReflectionColor;
		samplerCUBE _ReflectionMap;
		float _ReflectionStrength;

		struct Input
		{
			float3 worldRefl;
			float2 uv_DiffuseMap;
		};

		void SurfaceMain(Input input, inout SurfaceOutput output)
		{
			float3 reflectionColor = texCUBE(_ReflectionMap, input.worldRefl).rgb * _ReflectionColor.rgb;
			float3 diffuseMapColor = tex2D(_DiffuseMap, input.uv_DiffuseMap).rgb * _DiffuseMapColorAndAlpha.rgb;
			float3 diffuseColor = _DiffuseColor.rgb * lerp(float3(1.0, 1.0, 1.0), diffuseMapColor, _DiffuseMapColorAndAlpha.a);

			output.Albedo = lerp(diffuseColor, reflectionColor, _ReflectionStrength);
			output.Alpha = _DiffuseColor.a;
		}
		ENDCG
	}

	FallBack Off
} 
