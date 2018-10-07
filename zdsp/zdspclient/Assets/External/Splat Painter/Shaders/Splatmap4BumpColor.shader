Shader "Splatmap/Splatmap4BumpColor"
{
	Properties
	{
		_SplatMap01 ("Splat Map 01", 2D) = "black" {}
		_TintColor01 ("Tint Color 01", Color) = (1,1,1,1)
		_Albedo01 ("Albedo 01", 2D) = "white" {}
		_NormalMap01 ("Normal Map 01", 2D) = "bump" {}
		_TintColor02 ("Tint Color 02", Color) = (1,1,1,1)
		_Albedo02 ("Albedo 02", 2D) = "white" {}
		_NormalMap02 ("Normal Map 02", 2D) = "bump" {}
		_TintColor03 ("Tint Color 03", Color) = (1,1,1,1)
		_Albedo03 ("Albedo 03", 2D) = "white" {}
		_NormalMap03 ("Normal Map 03", 2D) = "bump" {}
		_TintColor04 ("Tint Color 04", Color) = (1,1,1,1)
		_Albedo04 ("Albedo 04", 2D) = "white" {}
		_NormalMap04 ("Normal Map 04", 2D) = "bump" {}
	}

	SubShader
	{
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
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
		fixed3 _TintColor01, _TintColor02, _TintColor03, _TintColor04;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.myUV = v.texcoord;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 splatMap01 = tex2D(_SplatMap01, TRANSFORM_TEX(IN.myUV, _SplatMap01)).rgba;
			fixed3 albedo01 = tex2D(_Albedo01, TRANSFORM_TEX(IN.myUV, _Albedo01)).rgb;
			fixed3 albedo02 = tex2D(_Albedo02, TRANSFORM_TEX(IN.myUV, _Albedo02)).rgb;
			fixed3 albedo03 = tex2D(_Albedo03, TRANSFORM_TEX(IN.myUV, _Albedo03)).rgb;
			fixed3 albedo04 = tex2D(_Albedo04, TRANSFORM_TEX(IN.myUV, _Albedo04)).rgb;

			o.Albedo = splatMap01.r * albedo01.rgb * _TintColor01.rgb
				+ splatMap01.g * albedo02.rgb * _TintColor02.rgb
				+ splatMap01.b * albedo03.rgb * _TintColor03.rgb
				+ splatMap01.a * albedo04.rgb * _TintColor04.rgb;

			o.Normal = splatMap01.r * UnpackNormal(tex2D(_NormalMap01, TRANSFORM_TEX(IN.myUV, _NormalMap01)))
				+ splatMap01.g * UnpackNormal(tex2D(_NormalMap02, TRANSFORM_TEX(IN.myUV, _NormalMap02)))
				+ splatMap01.b * UnpackNormal(tex2D(_NormalMap03, TRANSFORM_TEX(IN.myUV, _NormalMap03)))
				+ splatMap01.a * UnpackNormal(tex2D(_NormalMap04, TRANSFORM_TEX(IN.myUV, _NormalMap04)));
		}
		
		ENDCG  
	}
	
	FallBack "Diffuse"
}
