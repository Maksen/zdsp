Shader "Custom/AlphaGradient" {
	Properties{
		_MainTex("Main Texture", 2D) = "white" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
		_Intensity("Intensity", Range(0.01, 100)) = 1
	}

	SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		#pragma surface surf Lambert alpha
		sampler2D _MainTex;
		sampler2D _MaskTex;
		float _Intensity;

		struct Input {
			float2 uv_MainTex;
			float2 uv_MaskTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			half4 main = tex2D(_MainTex, IN.uv_MainTex);
			half4 mask = tex2D(_MaskTex, IN.uv_MaskTex);
			half3 glow = (main.rgb * _Intensity) * clamp(mask.a * main.a, 0.0f, 1.0f);
			half3 color = main.rgb * clamp(mask.a * main.a, 0.0f, 1.0f);
			half3 finalcolor = color * glow;
			o.Emission = finalcolor;
		}
		ENDCG
	}
}
