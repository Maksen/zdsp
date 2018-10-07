// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatmap/Splatmap4Unlit"
{
	Properties
	{
		_SplatMap01 ("Splat Map 01", 2D) = "black" {}
		_Albedo01 ("Albedo 01", 2D) = "white" {}
		_Albedo02 ("Albedo 02", 2D) = "white" {}
		_Albedo03 ("Albedo 03", 2D) = "white" {}
		_Albedo04 ("Albedo 04", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
		
			#include "UnityCG.cginc"

			sampler2D _SplatMap01;
			float4 _SplatMap01_ST;
			sampler2D _Albedo01, _Albedo02, _Albedo03, _Albedo04;
			float4 _Albedo01_ST, _Albedo02_ST, _Albedo03_ST, _Albedo04_ST;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.uv0 = v.texcoord.xy;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed3 frag(v2f i) : COLOR
			{
				fixed4 splatMap01 = tex2D(_SplatMap01, TRANSFORM_TEX(i.uv0, _SplatMap01)).rgba;
				fixed3 albedo01 = tex2D(_Albedo01, TRANSFORM_TEX(i.uv0, _Albedo01)).rgb;
				fixed3 albedo02 = tex2D(_Albedo02, TRANSFORM_TEX(i.uv0, _Albedo02)).rgb;
				fixed3 albedo03 = tex2D(_Albedo03, TRANSFORM_TEX(i.uv0, _Albedo03)).rgb;
				fixed3 albedo04 = tex2D(_Albedo04, TRANSFORM_TEX(i.uv0, _Albedo04)).rgb;

				return splatMap01.r * albedo01.rgb
					 + splatMap01.g * albedo02.rgb
					 + splatMap01.b * albedo03.rgb
					 + splatMap01.a * albedo04.rgb;
			}

			ENDCG
		}
	}
	
	FallBack "Diffuse"
}
