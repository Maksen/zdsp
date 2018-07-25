// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ZD_Shader/Scenes/Albedo_Flag_Unit"
{
	Properties
	{
		_Amplitude("Amplitude", Range( 0 , 10)) = 0
		_Frequency("Frequency", Float) = 0
		_Normalpositiondeviation("Normal position deviation", Range( 0.01 , 1)) = 0.1
		_Flagalbedo("Flag albedo", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		BlendOp Add
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Flagalbedo;
		uniform float4 _Flagalbedo_ST;
		uniform float _Frequency;
		uniform float _Amplitude;
		uniform float _Normalpositiondeviation;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float frequency291 = _Frequency;
			float mulTime4_g20 = _Time.y * 2.0;
			float amplitude287 = _Amplitude;
			float3 appendResult11_g20 = (float3(ase_vertex3Pos.x , ( ase_vertex3Pos.y + ( sin( ( ( ase_vertex3Pos.x * frequency291 ) + mulTime4_g20 ) ) * ( amplitude287 * v.texcoord.xy.x ) ) ) , ase_vertex3Pos.z));
			float3 newVertexPos56 = appendResult11_g20;
			v.vertex.xyz = newVertexPos56;
			float deviation199 = _Normalpositiondeviation;
			float3 appendResult206 = (float3(0.0 , deviation199 , 0.0));
			float3 ase_vertexNormal = v.normal.xyz;
			float3 ase_vertexTangent = v.tangent.xyz;
			float3x3 ObjectToTangent121 = float3x3(cross( ase_vertexNormal , ase_vertexTangent ), ase_vertexTangent, ase_vertexNormal);
			float mulTime4_g21 = _Time.y * 2.0;
			float3 appendResult11_g21 = (float3(mul( ( appendResult206 + mul( ObjectToTangent121, ase_vertex3Pos ) ), ObjectToTangent121 ).x , ( mul( ( appendResult206 + mul( ObjectToTangent121, ase_vertex3Pos ) ), ObjectToTangent121 ).y + ( sin( ( ( mul( ( appendResult206 + mul( ObjectToTangent121, ase_vertex3Pos ) ), ObjectToTangent121 ).x * frequency291 ) + mulTime4_g21 ) ) * ( amplitude287 * v.texcoord.xy.x ) ) ) , mul( ( appendResult206 + mul( ObjectToTangent121, ase_vertex3Pos ) ), ObjectToTangent121 ).z));
			float3 yDeviation114 = appendResult11_g21;
			float3 appendResult198 = (float3(deviation199 , 0.0 , 0.0));
			float mulTime4_g19 = _Time.y * 2.0;
			float3 appendResult11_g19 = (float3(mul( ( appendResult198 + mul( ObjectToTangent121, ase_vertex3Pos ) ), ObjectToTangent121 ).x , ( mul( ( appendResult198 + mul( ObjectToTangent121, ase_vertex3Pos ) ), ObjectToTangent121 ).y + ( sin( ( ( mul( ( appendResult198 + mul( ObjectToTangent121, ase_vertex3Pos ) ), ObjectToTangent121 ).x * frequency291 ) + mulTime4_g19 ) ) * ( amplitude287 * v.texcoord.xy.x ) ) ) , mul( ( appendResult198 + mul( ObjectToTangent121, ase_vertex3Pos ) ), ObjectToTangent121 ).z));
			float3 xDeviation113 = appendResult11_g19;
			float3 normalizeResult97 = normalize( cross( ( yDeviation114 - newVertexPos56 ) , ( xDeviation113 - newVertexPos56 ) ) );
			v.normal = normalizeResult97;
		}

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_Flagalbedo = i.uv_texcoord * _Flagalbedo_ST.xy + _Flagalbedo_ST.zw;
			float4 tex2DNode305 = tex2D( _Flagalbedo, uv_Flagalbedo );
			o.Emission = tex2DNode305.rgb;
			o.Alpha = tex2DNode305.a;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14201
279;301;1265;746;-896.1532;1000.529;1.021195;True;False
Node;AmplifyShaderEditor.CommentaryNode;296;-3621.198,212.0746;Float;False;1078.618;465.5402;object to tangent matrix without tangent sign;5;116;121;125;118;117;Object to tangent matrix;1,1,1,1;0;0
Node;AmplifyShaderEditor.TangentVertexDataNode;118;-3574.62,369.6892;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;117;-3565.198,513.2958;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;295;-2853.043,-482.4444;Float;False;645.3955;379.0187;Comment;6;130;199;127;291;112;287;Inputs;1,1,1,1;0;0
Node;AmplifyShaderEditor.CrossProductOpNode;125;-3222.62,305.6892;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;202;-1552,192;Float;False;1552.676;586.3004;move the position in tangent Y direction by the deviation amount;14;311;310;114;313;292;210;288;209;208;206;207;205;203;204;delta Y position;1,1,1,1;0;0
Node;AmplifyShaderEditor.MatrixFromVectors;116;-3046.62,337.6892;Float;False;FLOAT3x3;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-2800,-224;Float;False;Property;_Normalpositiondeviation;Normal position deviation;3;0;Create;0.1;0;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;177;-1568,-528;Float;False;1562.402;582.1888;move the position in tangent X direction by the deviation amount;14;308;289;314;309;113;197;293;196;195;198;194;192;200;201;delta X position;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;200;-1472,-432;Float;False;199;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;201;-1472,-208;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;192;-1511.005,-316.4977;Float;False;121;0;1;FLOAT3x3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;121;-2790.62,337.6892;Float;False;ObjectToTangent;-1;True;1;0;FLOAT3x3;0.0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.PosVertexDataNode;205;-1472,496;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;204;-1504,400;Float;False;121;0;1;FLOAT3x3;0
Node;AmplifyShaderEditor.GetLocalVarNode;203;-1456,288;Float;False;199;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;199;-2480,-224;Float;False;deviation;-1;True;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;207;-1216,448;Float;False;2;2;0;FLOAT3x3;0.0;False;1;FLOAT3;0.0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;198;-1216,-416;Float;False;FLOAT3;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;206;-1216,320;Float;False;FLOAT3;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;112;-2800,-416;Float;False;Property;_Amplitude;Amplitude;1;0;Create;0;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;194;-1216,-288;Float;False;2;2;0;FLOAT3x3;0.0;False;1;FLOAT3;0.0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;77;-1568,-1184;Float;False;959.9028;475.1613;simply apply vertex transformation;7;56;312;15;306;294;290;307;new vertex position;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;290;-1456,-960;Float;False;287;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;307;-1472,-880;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;288;-800,560;Float;False;287;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;309;-784,-96;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;289;-768,-176;Float;False;287;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;311;-800,640;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;127;-2688,-320;Float;False;Property;_Frequency;Frequency;2;0;Create;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;287;-2480,-416;Float;False;amplitude;-1;True;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;195;-1024,-368;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;196;-1072,-224;Float;False;121;0;1;FLOAT3x3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;209;-1024,368;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;208;-1072,528;Float;False;121;0;1;FLOAT3x3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;306;-1232,-960;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;308;-560,-192;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;294;-1332.203,-1039.546;Float;False;291;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;310;-592,528;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;210;-784,368;Float;False;2;2;0;FLOAT3;0.0,0,0;False;1;FLOAT3x3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;293;-768,-256;Float;False;291;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;292;-800,480;Float;False;291;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;15;-1520,-1104;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;197;-736,-368;Float;False;2;2;0;FLOAT3;0.0,0,0;False;1;FLOAT3x3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;291;-2480,-320;Float;False;frequency;-1;True;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;314;-432,-368;Float;False;Waving Vertex;-1;;19;872b3757863bb794c96291ceeebfb188;3;1;FLOAT3;0,0,0;False;12;FLOAT;0.0;False;13;FLOAT;0.0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;312;-1056,-1104;Float;False;Waving Vertex;-1;;20;872b3757863bb794c96291ceeebfb188;3;1;FLOAT3;0,0,0;False;12;FLOAT;0.0;False;13;FLOAT;0.0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;297;657.8127,-173.1141;Float;False;927.4102;507.1851;calculated new normal by derivation;8;223;107;224;108;88;93;96;97;new normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;313;-480,368;Float;False;Waving Vertex;-1;;21;872b3757863bb794c96291ceeebfb188;3;1;FLOAT3;0,0,0;False;12;FLOAT;0.0;False;13;FLOAT;0.0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;-240,-368;Float;False;xDeviation;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;56;-832,-1104;Float;False;newVertexPos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;114;-240,368;Float;False;yDeviation;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;108;707.4468,193.0864;Float;False;56;0;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;223;707.4468,-78.91376;Float;False;113;0;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;707.4468,113.0864;Float;False;114;0;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;107;707.8127,-6.447388;Float;False;56;0;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;93;979.4468,113.0864;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0.0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;88;979.4468,-62.91375;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0.0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CrossProductOpNode;96;1171.447,17.08633;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;1363.447,-366.9138;Float;False;56;0;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;97;1395.447,17.08633;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;305;1191.023,-963.8842;Float;True;Property;_Flagalbedo;Flag albedo;4;0;Create;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1742.518,-706.5498;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;ZD_Shader/Scenes/Albedo_Flag_Unit;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;3;False;0;0;Custom;0.5;True;True;0;False;TransparentCutout;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;4;10;25;False;0.5;True;2;SrcAlpha;OneMinusSrcAlpha;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Absolute;0;;0;-1;-1;-1;0;0;0;False;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;125;0;117;0
WireConnection;125;1;118;0
WireConnection;116;0;125;0
WireConnection;116;1;118;0
WireConnection;116;2;117;0
WireConnection;121;0;116;0
WireConnection;199;0;130;0
WireConnection;207;0;204;0
WireConnection;207;1;205;0
WireConnection;198;0;200;0
WireConnection;206;1;203;0
WireConnection;194;0;192;0
WireConnection;194;1;201;0
WireConnection;287;0;112;0
WireConnection;195;0;198;0
WireConnection;195;1;194;0
WireConnection;209;0;206;0
WireConnection;209;1;207;0
WireConnection;306;0;290;0
WireConnection;306;1;307;1
WireConnection;308;0;289;0
WireConnection;308;1;309;1
WireConnection;310;0;288;0
WireConnection;310;1;311;1
WireConnection;210;0;209;0
WireConnection;210;1;208;0
WireConnection;197;0;195;0
WireConnection;197;1;196;0
WireConnection;291;0;127;0
WireConnection;314;1;197;0
WireConnection;314;12;293;0
WireConnection;314;13;308;0
WireConnection;312;1;15;0
WireConnection;312;12;294;0
WireConnection;312;13;306;0
WireConnection;313;1;210;0
WireConnection;313;12;292;0
WireConnection;313;13;310;0
WireConnection;113;0;314;0
WireConnection;56;0;312;0
WireConnection;114;0;313;0
WireConnection;93;0;224;0
WireConnection;93;1;108;0
WireConnection;88;0;223;0
WireConnection;88;1;107;0
WireConnection;96;0;93;0
WireConnection;96;1;88;0
WireConnection;97;0;96;0
WireConnection;0;2;305;0
WireConnection;0;9;305;4
WireConnection;0;11;109;0
WireConnection;0;12;97;0
ASEEND*/
//CHKSM=EA6533F741B48F38E12E57D93588B6E4E40A8511