Shader "Splatmap/Splatmap4BumpDisp"
{
	Properties
	{
		_SplatMap01 ("Splat Map 01", 2D) = "black" {}
		_Parallax("Height", Range(0.005, 0.08)) = 0.02
		_Albedo01 ("Albedo 01", 2D) = "white" {}
		_NormalMap01 ("Normal Map 01", 2D) = "bump" {}
		_ParallaxMap01("Height Map 01 (A)", 2D) = "black" {}
		_Albedo02 ("Albedo 02", 2D) = "white" {}
		_NormalMap02 ("Normal Map 02", 2D) = "bump" {}
		_ParallaxMap02("Height Map 02 (A)", 2D) = "black" {}
		_Albedo03 ("Albedo 03", 2D) = "white" {}
		_NormalMap03 ("Normal Map 03", 2D) = "bump" {}
		_ParallaxMap03("Height Map 03 (A)", 2D) = "black" {}
		_Albedo04 ("Albedo 04", 2D) = "white" {}
		_NormalMap04 ("Normal Map 04", 2D) = "bump" {}
		_ParallaxMap04("Height Map 04 (A)", 2D) = "black" {}
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
			float3 viewDir;
		};

		float _Parallax;
		sampler2D _SplatMap01;
		float4 _SplatMap01_ST;
		sampler2D _Albedo01, _Albedo02, _Albedo03, _Albedo04;
		float4 _Albedo01_ST, _Albedo02_ST, _Albedo03_ST, _Albedo04_ST;
		sampler2D _NormalMap01, _NormalMap02, _NormalMap03, _NormalMap04;
		float4 _NormalMap01_ST, _NormalMap02_ST, _NormalMap03_ST, _NormalMap04_ST;
		sampler2D _ParallaxMap01, _ParallaxMap02, _ParallaxMap03, _ParallaxMap04;
		float4 _ParallaxMap01_ST, _ParallaxMap02_ST, _ParallaxMap03_ST, _ParallaxMap04_ST;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.myUV = v.texcoord;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 splatMap01 = tex2D(_SplatMap01, TRANSFORM_TEX(IN.myUV, _SplatMap01)).rgba;

			half h;
			float2 offset;

			h = tex2D(_ParallaxMap01, TRANSFORM_TEX(IN.myUV, _ParallaxMap01)).a;
			offset = ParallaxOffset(h, _Parallax, IN.viewDir);
			float2 uv01 = IN.myUV + offset;

			h = tex2D(_ParallaxMap02, TRANSFORM_TEX(IN.myUV, _ParallaxMap02)).a;
			offset = ParallaxOffset(h, _Parallax, IN.viewDir);
			float2 uv02 = IN.myUV + offset;

			h = tex2D(_ParallaxMap03, TRANSFORM_TEX(IN.myUV, _ParallaxMap03)).a;
			offset = ParallaxOffset(h, _Parallax, IN.viewDir);
			float2 uv03 = IN.myUV + offset;

			h = tex2D(_ParallaxMap04, TRANSFORM_TEX(IN.myUV, _ParallaxMap04)).a;
			offset = ParallaxOffset(h, _Parallax, IN.viewDir);
			float2 uv04 = IN.myUV + offset;

			
			fixed3 albedo01 = tex2D(_Albedo01, TRANSFORM_TEX(uv01, _Albedo01)).rgb;
			fixed3 albedo02 = tex2D(_Albedo02, TRANSFORM_TEX(uv02, _Albedo02)).rgb;
			fixed3 albedo03 = tex2D(_Albedo03, TRANSFORM_TEX(uv03, _Albedo03)).rgb;
			fixed3 albedo04 = tex2D(_Albedo04, TRANSFORM_TEX(uv04, _Albedo04)).rgb;

			o.Albedo = splatMap01.r * albedo01.rgb
				+ splatMap01.g * albedo02.rgb
				+ splatMap01.b * albedo03.rgb
				+ splatMap01.a * albedo04.rgb;

			o.Normal = splatMap01.r * UnpackNormal(tex2D(_NormalMap01, TRANSFORM_TEX(uv01, _NormalMap01)))
				+ splatMap01.g * UnpackNormal(tex2D(_NormalMap02, TRANSFORM_TEX(uv02, _NormalMap02)))
				+ splatMap01.b * UnpackNormal(tex2D(_NormalMap03, TRANSFORM_TEX(uv03, _NormalMap03)))
				+ splatMap01.a * UnpackNormal(tex2D(_NormalMap04, TRANSFORM_TEX(uv04, _NormalMap04)));
		}
		
		ENDCG  
	}
	
	FallBack "Legacy Shaders/Diffuse"
}
