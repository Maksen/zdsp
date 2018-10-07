Shader "Splatmap/Splatmap4BumpSpec"
{
	Properties
	{
		_SpecColor("Specular Color", Color) = (0.5,0.5,0.5,1)
		_SplatMap01 ("Splat Map 01", 2D) = "black" {}
		_Shininess01("Shininess 01", Range(0.03, 1)) = 0.078125
		_Albedo01 ("Base 01 (RGB) Gloss (A)", 2D) = "white" {}
		_NormalMap01 ("Normal Map 01", 2D) = "bump" {}
		_Shininess02("Shininess 02", Range(0.03, 1)) = 0.078125
		_Albedo02 ("Base 02 (RGB) Gloss (A)", 2D) = "white" {}
		_NormalMap02 ("Normal Map 02", 2D) = "bump" {}
		_Shininess03("Shininess 03", Range(0.03, 1)) = 0.078125
		_Albedo03 ("Base 03 (RGB) Gloss (A)", 2D) = "white" {}
		_NormalMap03 ("Normal Map 03", 2D) = "bump" {}
		_Shininess04("Shininess 04", Range(0.03, 1)) = 0.078125
		_Albedo04 ("Base 04 (RGB) Gloss (A)", 2D) = "white" {}
		_NormalMap04 ("Normal Map 04", 2D) = "bump" {}
	}

	SubShader
	{
		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert
		#pragma target 3.0
		#include "UnityCG.cginc"

		struct Input
		{
			float2 myUV;
		};

		sampler2D _SplatMap01;
		float4 _SplatMap01_ST;
		sampler2D _Albedo01, _Albedo02, _Albedo03, _Albedo04;
		float4 _Albedo01_ST, _Albedo02_ST, _Albedo03_ST, _Albedo04_ST;
		sampler2D _NormalMap01, _NormalMap02, _NormalMap03, _NormalMap04;
		float4 _NormalMap01_ST, _NormalMap02_ST, _NormalMap03_ST, _NormalMap04_ST;
		half _Shininess01, _Shininess02, _Shininess03, _Shininess04;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.myUV = v.texcoord;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 splatMap01 = tex2D(_SplatMap01, TRANSFORM_TEX(IN.myUV, _SplatMap01)).rgba;
			fixed4 albedo01 = tex2D(_Albedo01, TRANSFORM_TEX(IN.myUV, _Albedo01)).rgba;
			fixed4 albedo02 = tex2D(_Albedo02, TRANSFORM_TEX(IN.myUV, _Albedo02)).rgba;
			fixed4 albedo03 = tex2D(_Albedo03, TRANSFORM_TEX(IN.myUV, _Albedo03)).rgba;
			fixed4 albedo04 = tex2D(_Albedo04, TRANSFORM_TEX(IN.myUV, _Albedo04)).rgba;

			o.Albedo = splatMap01.r * albedo01.rgb
				+ splatMap01.g * albedo02.rgb
				+ splatMap01.b * albedo03.rgb
				+ splatMap01.a * albedo04.rgb;

			o.Normal = splatMap01.r * UnpackNormal(tex2D(_NormalMap01, TRANSFORM_TEX(IN.myUV, _NormalMap01)))
				+ splatMap01.g * UnpackNormal(tex2D(_NormalMap02, TRANSFORM_TEX(IN.myUV, _NormalMap02)))
				+ splatMap01.b * UnpackNormal(tex2D(_NormalMap03, TRANSFORM_TEX(IN.myUV, _NormalMap03)))
				+ splatMap01.a * UnpackNormal(tex2D(_NormalMap04, TRANSFORM_TEX(IN.myUV, _NormalMap04)));

			o.Specular = splatMap01.r * _Shininess01
				+ splatMap01.g * _Shininess02
				+ splatMap01.b * _Shininess03
				+ splatMap01.a * _Shininess04;

			o.Gloss = splatMap01.r * albedo01.a
				+ splatMap01.g * albedo02.a
				+ splatMap01.b * albedo03.a
				+ splatMap01.a * albedo04.a;
		}
		
		ENDCG  
	}
	
	FallBack "Diffuse"
}
