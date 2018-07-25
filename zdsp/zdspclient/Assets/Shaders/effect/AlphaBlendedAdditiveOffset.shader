// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Particles/Alpha blended Additive Offset"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Zoffset("Zoffset Neg value", Int) = -5000
	}
	SubShader
	{
		Tags 
		{
			"Queue" = "Transparent" 
			"RenderType"="Transparent"
		}
		Blend SrcAlpha One

		Cull Off 
		Lighting Off
	//	ZTest Always
		
		Offset [_Zoffset], [_Zoffset]
		ZWrite off
			
		Pass
		{
			
			Offset [_Zoffset], [_Zoffset]
			ZWrite off
			
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv,_MainTex);
				o.color = v.color;
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				return col;
			}
			ENDCG
		}			
	}
	
}
