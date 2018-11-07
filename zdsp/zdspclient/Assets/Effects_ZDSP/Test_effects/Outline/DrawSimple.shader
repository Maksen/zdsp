// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DrawSimple"
{
	Properties
	{
		_MainTex("Screen", 2D) = "black" {}
	}

		SubShader
	{
		Tags{  }

		ZTest Always
		Cull Off
		ZWrite Off


		Pass
		{
			CGPROGRAM

			#pragma vertex vert_img_custom
			#pragma fragment frag
			#include "UnityCG.cginc"


			struct appdata_img_custom
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct v2f_img_custom
			{
				float4 pos : SV_POSITION;
				half2 uv   : TEXCOORD0;
				half2 stereoUV : TEXCOORD2;
		#if UNITY_UV_STARTS_AT_TOP
				half4 uv2 : TEXCOORD1;
				half4 stereoUV2 : TEXCOORD3;
		#endif
			};

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _MainTex_ST;


			v2f_img_custom vert_img_custom(appdata_img_custom v )
			{
				v2f_img_custom o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = float4(v.texcoord.xy, 1, 1);

				#if UNITY_UV_STARTS_AT_TOP
					o.uv2 = float4(v.texcoord.xy, 1, 1);
					o.stereoUV2 = UnityStereoScreenSpaceUVAdjust(o.uv2, _MainTex_ST);

					if (_MainTex_TexelSize.y < 0.0)
						o.uv.y = 1.0 - o.uv.y;
				#endif
				o.stereoUV = UnityStereoScreenSpaceUVAdjust(o.uv, _MainTex_ST);
				return o;
			}

			half4 frag(v2f_img_custom i ) : SV_Target
			{
				#ifdef UNITY_UV_STARTS_AT_TOP
					half2 uv = i.uv2;
					half2 stereoUV = i.stereoUV2;
				#else
					half2 uv = i.uv;
					half2 stereoUV = i.stereoUV;
				#endif

				half4 finalColor;

				// ase common template code

				finalColor = float4(1,1,1,1);

				return finalColor;
			}
			ENDCG
		}
	}
		CustomEditor "ASEMaterialInspector"
}/*ASEBEGIN
Version=14201
1483;62;1330;616;1088.331;397.1715;1.730589;True;True
Node;AmplifyShaderEditor.ColorNode;1;-536.3055,-11.65683;Float;False;Constant;_Color0;Color 0;0;0;Create;1,1,1,1;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMasterNode;4;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;1;Custom/DrawSimple;c71b220b631b6344493ea3cf87110c93;ASETemplateShaders/PostProcess;Off;1;0;FLOAT4;0,0,0,0;False;0
WireConnection;4;0;1;0
ASEEND*/
//CHKSM=62C32EBAC3F2BDC897713CF6E53B5B70EC7E6BF0