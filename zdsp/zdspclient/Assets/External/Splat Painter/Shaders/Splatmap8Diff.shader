Shader "Splatmap/Splatmap8Diff"
{
	Properties
	{
		_SplatMap01 ("Splat Map 01", 2D) = "black" {}
		_SplatMap02 ("Splat Map 02", 2D) = "black" {}
		_Albedo01 ("Albedo 01", 2D) = "white" {}
		_Albedo02 ("Albedo 02", 2D) = "white" {}
		_Albedo03 ("Albedo 03", 2D) = "white" {}
		_Albedo04 ("Albedo 04", 2D) = "white" {}
		_Albedo05 ("Albedo 05", 2D) = "white" {}
		_Albedo06 ("Albedo 06", 2D) = "white" {}
		_Albedo07 ("Albedo 07", 2D) = "white" {}
		_Albedo08 ("Albedo 08", 2D) = "white" {}
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

		sampler2D _SplatMap01, _SplatMap02;
		float4 _SplatMap01_ST, _SplatMap02_ST;
		sampler2D _Albedo01, _Albedo02, _Albedo03, _Albedo04, _Albedo05, _Albedo06, _Albedo07, _Albedo08;
		float4 _Albedo01_ST, _Albedo02_ST, _Albedo03_ST, _Albedo04_ST, _Albedo05_ST, _Albedo06_ST, _Albedo07_ST, _Albedo08_ST;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.myUV = v.texcoord;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 splatMap01 = tex2D(_SplatMap01, TRANSFORM_TEX(IN.myUV, _SplatMap01)).rgba;
			fixed4 splatMap02 = tex2D(_SplatMap02, TRANSFORM_TEX(IN.myUV, _SplatMap02)).rgba;
			fixed3 albedo01 = tex2D(_Albedo01, TRANSFORM_TEX(IN.myUV, _Albedo01)).rgb;
			fixed3 albedo02 = tex2D(_Albedo02, TRANSFORM_TEX(IN.myUV, _Albedo02)).rgb;
			fixed3 albedo03 = tex2D(_Albedo03, TRANSFORM_TEX(IN.myUV, _Albedo03)).rgb;
			fixed3 albedo04 = tex2D(_Albedo04, TRANSFORM_TEX(IN.myUV, _Albedo04)).rgb;
			fixed3 albedo05 = tex2D(_Albedo05, TRANSFORM_TEX(IN.myUV, _Albedo05)).rgb;
			fixed3 albedo06 = tex2D(_Albedo06, TRANSFORM_TEX(IN.myUV, _Albedo06)).rgb;
			fixed3 albedo07 = tex2D(_Albedo07, TRANSFORM_TEX(IN.myUV, _Albedo07)).rgb;
			fixed3 albedo08 = tex2D(_Albedo08, TRANSFORM_TEX(IN.myUV, _Albedo08)).rgb;

			o.Albedo = splatMap01.r * albedo01.rgb
				+ splatMap01.g * albedo02.rgb
				+ splatMap01.b * albedo03.rgb
				+ splatMap01.a * albedo04.rgb
				+ splatMap02.r * albedo05.rgb
				+ splatMap02.g * albedo06.rgb
				+ splatMap02.b * albedo07.rgb
				+ splatMap02.a * albedo08.rgb;
		}

		ENDCG
	}
	
	FallBack "Diffuse"
}
