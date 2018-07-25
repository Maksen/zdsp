// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ZD_Shader/Scenes/Aldedo_UvAnimation_Unlit"
{
	Properties
	{
		_Aldedo("Aldedo", 2D) = "white" {}
		_speed("speed", Vector) = (0,1,0,0)
		_Color0("Color 0", Color) = (0,0,0,0)
		_Mask("Mask", 2D) = "white" {}
		_intensity("intensity", Range( 0 , 10)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color0;
		uniform sampler2D _Aldedo;
		uniform float2 _speed;
		uniform float _intensity;
		uniform sampler2D _Mask;

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_TexCoord51 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			float2 panner50 = ( uv_TexCoord51 + _Time.x * _speed);
			o.Emission = ( _Color0 + tex2D( _Aldedo, panner50 ) + _intensity ).rgb;
			o.Alpha = tex2D( _Mask, panner50 ).r;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14201
-1603;203;1426;713;1776.109;491.2725;1.873262;True;False
Node;AmplifyShaderEditor.Vector2Node;54;-825.222,137.6873;Float;False;Property;_speed;speed;2;0;Create;0,1;0,20;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;51;-686.6794,-28.72831;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;55;-846.422,279.6873;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;50;-443.3869,149.9588;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;48;-188.1512,41.77824;Float;True;Property;_Aldedo;Aldedo;1;0;Create;None;01541e05cd100704bac43a2efde9467e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;60;-139.2365,-152.1102;Float;False;Property;_Color0;Color 0;3;0;Create;0,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;94;-169.4642,-242.9192;Float;False;Property;_intensity;intensity;5;0;Create;0;0.47;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;91;-162.9654,269.2809;Float;True;Property;_Mask;Mask;4;0;Create;None;abf88770c06051a4cad6b59d9b1fb44e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;92;198.4358,49.58066;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;81;373.824,39.15722;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;ZD_Shader/Scenes/Aldedo_UvAnimation_Unlit;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;2;0;False;0;0;Custom;0.5;True;False;0;True;Transparent;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;False;2;SrcAlpha;OneMinusSrcAlpha;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;0;0;False;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;50;0;51;0
WireConnection;50;2;54;0
WireConnection;50;1;55;1
WireConnection;48;1;50;0
WireConnection;91;1;50;0
WireConnection;92;0;60;0
WireConnection;92;1;48;0
WireConnection;92;2;94;0
WireConnection;81;2;92;0
WireConnection;81;9;91;0
ASEEND*/
//CHKSM=E8CBC0F47297E7E9745C0B6BD672E1F36BD7BEB6