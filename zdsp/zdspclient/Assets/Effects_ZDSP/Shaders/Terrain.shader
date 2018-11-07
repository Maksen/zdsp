// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Transparent/Terrain"
{
	Properties
	{
		_AlbedoColor("AlbedoColor", Color) = (0,0,0,0)
		_MainTex("MainTex ", 2D) = "bump" {}
		_AlbedoBrightness("Albedo Brightness", Range( -1 , 1)) = 1
		_Cutoff( "Mask Clip Value", Float ) = 0
		_AlphaCutoff("Alpha Cutoff", Range( 0 , 1)) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.5
		#pragma exclude_renderers wiiu 
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _MainTex;
		uniform float _AlbedoBrightness;
		uniform float4 _AlbedoColor;
		uniform float _AlphaCutoff;
		uniform float _Cutoff = 0;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 tex2DNode19 = tex2D( _MainTex, i.uv_texcoord );
			o.Albedo = ( ( tex2DNode19 * _AlbedoBrightness ) * _AlbedoColor ).rgb;
			o.Alpha = 1;
			clip( ( tex2DNode19.a - _AlphaCutoff ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14201
1483;62;1330;617;2134.867;1084.601;2.294651;True;True
Node;AmplifyShaderEditor.TexCoordVertexDataNode;7;-1005.02,-391.5909;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;18;-1016.691,-621.7714;Float;True;Property;_MainTex;MainTex ;1;0;Create;None;22406ffaf8aee494ab5f97b8de6f3728;False;bump;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;19;-628.3338,-496.3489;Float;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;34;-340.624,-440.8079;Float;False;Property;_AlbedoBrightness;Albedo Brightness;2;0;Create;1;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-5.875972,-710.9266;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;48;-36.30776,-475.5556;Float;False;Property;_AlbedoColor;AlbedoColor;0;0;Create;0,0,0,0;1,0.4411765,0.4411765,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;45;-313.2122,-240.2934;Float;False;Property;_AlphaCutoff;Alpha Cutoff;6;0;Create;0.5;0.531;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;220.9197,-608.1965;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;49;52.90861,-259.7884;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;25;-668.8846,195.9168;Float;True;Property;_TextureSample2;Texture Sample 2;3;0;Create;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;26;-1171.173,51.18328;Float;True;Property;_Emission;Emission;5;0;Create;None;56584924016bf7b44a32d1c355ecd59d;False;black;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleAddOpNode;42;-112.3776,696.2578;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-219.5049,88.36668;Float;False;2;2;0;FLOAT;0,0,0,0;False;1;COLOR;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;33;-42.75056,263.8492;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;27;-1028.077,267.052;Float;True;Property;_Specular;Specular;9;0;Create;None;None;False;white;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-189.323,373.9693;Float;False;Property;_EmissionStrength;Emission Strength;8;0;Create;1;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;20;-624.7712,-198.5116;Float;True;Property;_TextureSample1;Texture Sample 1;6;0;Create;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;28;-648.3145,693.4806;Float;True;Property;_TextureSample3;Texture Sample 3;3;0;Create;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;43;-258.9503,806.3778;Float;False;Property;_SpecularPower;SpecularPower;10;0;Create;1;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;10;-1031.104,-189.0068;Float;True;Property;_Bump;Bump;3;0;Create;None;ef379089572832b4ea2301f3a1bf205b;True;bump;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ColorNode;40;-608.4612,446.6749;Float;False;Property;_SpecularColour;SpecularColour;11;0;Create;0,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;80.47379,704.6496;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;29;-538.8345,14.26619;Float;False;Property;_EmissionColor;EmissionColor;7;0;Create;0,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;150.1009,272.2411;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-289.1322,520.7751;Float;False;2;2;0;FLOAT;0,0,0,0;False;1;COLOR;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;16;609.6932,-212.4881;Float;False;True;3;Float;ASEMaterialInspector;0;0;Standard;Custom/Transparent/Terrain;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;Back;1;0;False;0;0;Masked;0;True;True;0;False;TransparentCutout;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;False;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;2;15;10;25;False;0.5;True;0;SrcAlpha;OneMinusSrcAlpha;0;SrcAlpha;OneMinusSrcAlpha;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;4;-1;-1;-1;0;0;0;False;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;19;0;18;0
WireConnection;19;1;7;0
WireConnection;35;0;19;0
WireConnection;35;1;34;0
WireConnection;47;0;35;0
WireConnection;47;1;48;0
WireConnection;49;0;19;4
WireConnection;49;1;45;0
WireConnection;25;0;26;0
WireConnection;25;1;7;0
WireConnection;42;0;28;0
WireConnection;42;1;41;0
WireConnection;32;0;25;3
WireConnection;32;1;29;0
WireConnection;33;0;25;0
WireConnection;33;1;32;0
WireConnection;20;0;10;0
WireConnection;20;1;7;0
WireConnection;28;0;27;0
WireConnection;28;1;7;0
WireConnection;44;0;42;0
WireConnection;44;1;43;0
WireConnection;38;0;33;0
WireConnection;38;1;39;0
WireConnection;41;0;28;4
WireConnection;41;1;40;0
WireConnection;16;0;47;0
WireConnection;16;10;49;0
ASEEND*/
//CHKSM=EBCDB28FF52F1CF219A3967D9ED102075CBAE4CE