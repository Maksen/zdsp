// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:Mobile/Particles/Additive Culled,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:34431,y:34399,varname:node_3138,prsc:2|emission-7990-OUT;n:type:ShaderForge.SFN_Color,id:8242,x:32289,y:32743,ptovrint:False,ptlb:Emission_Red,ptin:_Emission_Red,varname:_EmissionColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:1656,x:32937,y:32642,varname:node_1656,prsc:2|A-6874-OUT,B-6263-OUT;n:type:ShaderForge.SFN_Panner,id:3408,x:32053,y:32674,varname:node_3408,prsc:2,spu:0,spv:-0.2|UVIN-3234-UVOUT,DIST-8773-OUT;n:type:ShaderForge.SFN_TexCoord,id:9063,x:31283,y:32666,varname:node_9063,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:5723,x:30749,y:33233,varname:node_5723,prsc:2;n:type:ShaderForge.SFN_Slider,id:4069,x:31548,y:32856,ptovrint:False,ptlb:UVSpeed_Red,ptin:_UVSpeed_Red,varname:_UVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:1,max:10;n:type:ShaderForge.SFN_Multiply,id:8773,x:31896,y:32749,varname:node_8773,prsc:2|A-2031-OUT,B-4069-OUT;n:type:ShaderForge.SFN_Rotator,id:3234,x:31726,y:32563,varname:node_3234,prsc:2|UVIN-9063-UVOUT,ANG-9048-OUT;n:type:ShaderForge.SFN_Slider,id:6930,x:31175,y:32477,ptovrint:False,ptlb:Angle_Red,ptin:_Angle_Red,varname:_Angle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:8311,x:31365,y:32356,varname:node_8311,prsc:2;n:type:ShaderForge.SFN_Multiply,id:9048,x:31485,y:32415,varname:node_9048,prsc:2|A-8311-OUT,B-6930-OUT;n:type:ShaderForge.SFN_Tex2d,id:6712,x:32198,y:33421,ptovrint:False,ptlb:UVAnimTex_Red,ptin:_UVAnimTex_Red,varname:_UVAnimatedTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-3408-UVOUT;n:type:ShaderForge.SFN_Multiply,id:7899,x:33075,y:33380,varname:node_7899,prsc:2|A-1656-OUT,B-6712-RGB;n:type:ShaderForge.SFN_ValueProperty,id:1256,x:32466,y:33271,ptovrint:False,ptlb:FlashIntensity_Red,ptin:_FlashIntensity_Red,varname:_FlashIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:6263,x:32738,y:32700,varname:node_6263,prsc:2|A-8242-RGB,B-3009-OUT;n:type:ShaderForge.SFN_Add,id:3009,x:32665,y:32963,varname:node_3009,prsc:2|A-838-OUT,B-5065-OUT;n:type:ShaderForge.SFN_Vector1,id:838,x:32289,y:33249,varname:node_838,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:5065,x:32652,y:33127,varname:node_5065,prsc:2|A-7522-OUT,B-1256-OUT;n:type:ShaderForge.SFN_Vector1,id:527,x:32289,y:33159,varname:node_527,prsc:2,v1:10;n:type:ShaderForge.SFN_Subtract,id:7522,x:32466,y:33100,varname:node_7522,prsc:2|A-527-OUT,B-838-OUT;n:type:ShaderForge.SFN_Color,id:5880,x:32320,y:33952,ptovrint:False,ptlb:Emission_Green,ptin:_Emission_Green,varname:_EmissionColor_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:3197,x:33009,y:33884,varname:node_3197,prsc:2|A-3856-OUT,B-7633-OUT;n:type:ShaderForge.SFN_Panner,id:5417,x:32125,y:33916,varname:node_5417,prsc:2,spu:0,spv:-0.2|UVIN-5843-UVOUT,DIST-1341-OUT;n:type:ShaderForge.SFN_TexCoord,id:4781,x:31355,y:33908,varname:node_4781,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Slider,id:2404,x:31620,y:34098,ptovrint:False,ptlb:UVSpeed_Green,ptin:_UVSpeed_Green,varname:_UVSpeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:1,max:10;n:type:ShaderForge.SFN_Multiply,id:1341,x:31968,y:33991,varname:node_1341,prsc:2|A-8114-OUT,B-2404-OUT;n:type:ShaderForge.SFN_Rotator,id:5843,x:31798,y:33805,varname:node_5843,prsc:2|UVIN-4781-UVOUT,ANG-4059-OUT;n:type:ShaderForge.SFN_Slider,id:9824,x:31247,y:33719,ptovrint:False,ptlb:Angle_Green,ptin:_Angle_Green,varname:_Angle_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:4845,x:31437,y:33598,varname:node_4845,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4059,x:31557,y:33657,varname:node_4059,prsc:2|A-4845-OUT,B-9824-OUT;n:type:ShaderForge.SFN_Tex2d,id:5500,x:32270,y:34663,ptovrint:False,ptlb:UVAnimTex_Green,ptin:_UVAnimTex_Green,varname:_UVAnimatedTex_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-5417-UVOUT;n:type:ShaderForge.SFN_Multiply,id:3251,x:33147,y:34622,varname:node_3251,prsc:2|A-3197-OUT,B-5500-RGB;n:type:ShaderForge.SFN_ValueProperty,id:1570,x:32538,y:34513,ptovrint:False,ptlb:FlashIntensity_Green,ptin:_FlashIntensity_Green,varname:_FlashIntensity_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:7633,x:32810,y:33942,varname:node_7633,prsc:2|A-5880-RGB,B-8884-OUT;n:type:ShaderForge.SFN_Add,id:8884,x:32737,y:34205,varname:node_8884,prsc:2|A-860-OUT,B-6745-OUT;n:type:ShaderForge.SFN_Vector1,id:860,x:32361,y:34491,varname:node_860,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:6745,x:32724,y:34369,varname:node_6745,prsc:2|A-566-OUT,B-1570-OUT;n:type:ShaderForge.SFN_Vector1,id:4193,x:32361,y:34401,varname:node_4193,prsc:2,v1:10;n:type:ShaderForge.SFN_Subtract,id:566,x:32538,y:34342,varname:node_566,prsc:2|A-4193-OUT,B-860-OUT;n:type:ShaderForge.SFN_Color,id:6885,x:32357,y:35235,ptovrint:False,ptlb:Emission_Blue,ptin:_Emission_Blue,varname:_EmissionColor_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:3891,x:33005,y:35134,varname:node_3891,prsc:2|A-379-OUT,B-9810-OUT;n:type:ShaderForge.SFN_Panner,id:6966,x:32121,y:35166,varname:node_6966,prsc:2,spu:0,spv:-0.2|UVIN-1975-UVOUT,DIST-8190-OUT;n:type:ShaderForge.SFN_TexCoord,id:4950,x:31351,y:35158,varname:node_4950,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Slider,id:8474,x:31616,y:35348,ptovrint:False,ptlb:UVSpeed_Blue,ptin:_UVSpeed_Blue,varname:_UVSpeed_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:1,max:10;n:type:ShaderForge.SFN_Multiply,id:8190,x:31964,y:35241,varname:node_8190,prsc:2|A-6072-OUT,B-8474-OUT;n:type:ShaderForge.SFN_Rotator,id:1975,x:31794,y:35055,varname:node_1975,prsc:2|UVIN-4950-UVOUT,ANG-51-OUT;n:type:ShaderForge.SFN_Slider,id:5539,x:31243,y:34969,ptovrint:False,ptlb:Angle_Blue,ptin:_Angle_Blue,varname:_Angle_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:7805,x:31433,y:34848,varname:node_7805,prsc:2;n:type:ShaderForge.SFN_Multiply,id:51,x:31553,y:34907,varname:node_51,prsc:2|A-7805-OUT,B-5539-OUT;n:type:ShaderForge.SFN_Tex2d,id:2139,x:32266,y:35913,ptovrint:False,ptlb:UVAnimTex_Blue,ptin:_UVAnimTex_Blue,varname:_UVAnimatedTex_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-6966-UVOUT;n:type:ShaderForge.SFN_Multiply,id:1647,x:33143,y:35872,varname:node_1647,prsc:2|A-3891-OUT,B-2139-RGB;n:type:ShaderForge.SFN_ValueProperty,id:7994,x:32534,y:35763,ptovrint:False,ptlb:FlashIntensity_Blue,ptin:_FlashIntensity_Blue,varname:_FlashIntensity_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:9810,x:32806,y:35192,varname:node_9810,prsc:2|A-6885-RGB,B-8443-OUT;n:type:ShaderForge.SFN_Add,id:8443,x:32733,y:35455,varname:node_8443,prsc:2|A-6521-OUT,B-5343-OUT;n:type:ShaderForge.SFN_Vector1,id:6521,x:32357,y:35741,varname:node_6521,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:5343,x:32720,y:35619,varname:node_5343,prsc:2|A-3481-OUT,B-7994-OUT;n:type:ShaderForge.SFN_Vector1,id:9494,x:32357,y:35651,varname:node_9494,prsc:2,v1:10;n:type:ShaderForge.SFN_Subtract,id:3481,x:32534,y:35592,varname:node_3481,prsc:2|A-9494-OUT,B-6521-OUT;n:type:ShaderForge.SFN_Add,id:7990,x:33769,y:34682,varname:node_7990,prsc:2|A-7899-OUT,B-3251-OUT,C-1647-OUT;n:type:ShaderForge.SFN_Set,id:1353,x:30954,y:33249,varname:Timer,prsc:2|IN-5723-T;n:type:ShaderForge.SFN_Get,id:2031,x:31705,y:32749,varname:node_2031,prsc:2|IN-1353-OUT;n:type:ShaderForge.SFN_Get,id:8114,x:31745,y:33991,varname:node_8114,prsc:2|IN-1353-OUT;n:type:ShaderForge.SFN_Get,id:6072,x:31752,y:35241,varname:node_6072,prsc:2|IN-1353-OUT;n:type:ShaderForge.SFN_Tex2d,id:8027,x:30749,y:33011,ptovrint:False,ptlb:Texture_RGB,ptin:_Texture_RGB,varname:node_8027,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Set,id:8864,x:30966,y:33011,varname:TexRed,prsc:2|IN-8027-R;n:type:ShaderForge.SFN_Set,id:7343,x:30966,y:33064,varname:TexGreen,prsc:2|IN-8027-G;n:type:ShaderForge.SFN_Set,id:519,x:30966,y:33122,varname:TexBlue,prsc:2|IN-8027-B;n:type:ShaderForge.SFN_Get,id:6874,x:32289,y:32583,varname:node_6874,prsc:2|IN-8864-OUT;n:type:ShaderForge.SFN_Get,id:3856,x:32361,y:33801,varname:node_3856,prsc:2|IN-7343-OUT;n:type:ShaderForge.SFN_Get,id:379,x:32357,y:35105,varname:node_379,prsc:2|IN-519-OUT;proporder:8027-8242-6712-4069-6930-1256-5880-5500-2404-9824-1570-6885-2139-8474-5539-7994;pass:END;sub:END;*/

Shader "AAA_UI/Additive_DoubleSided_Emission_UVanimated_Angle_RGB" {
    Properties {
        _Texture_RGB ("Texture_RGB", 2D) = "white" {}
        _Emission_Red ("Emission_Red", Color) = (0,0,0,1)
        _UVAnimTex_Red ("UVAnimTex_Red", 2D) = "white" {}
        _UVSpeed_Red ("UVSpeed_Red", Range(-10, 10)) = 1
        _Angle_Red ("Angle_Red", Range(0, 2)) = 0
        _FlashIntensity_Red ("FlashIntensity_Red", Float ) = 0
        _Emission_Green ("Emission_Green", Color) = (0,0,0,1)
        _UVAnimTex_Green ("UVAnimTex_Green", 2D) = "white" {}
        _UVSpeed_Green ("UVSpeed_Green", Range(-10, 10)) = 1
        _Angle_Green ("Angle_Green", Range(0, 2)) = 0
        _FlashIntensity_Green ("FlashIntensity_Green", Float ) = 0
        _Emission_Blue ("Emission_Blue", Color) = (0,0,0,1)
        _UVAnimTex_Blue ("UVAnimTex_Blue", 2D) = "white" {}
        _UVSpeed_Blue ("UVSpeed_Blue", Range(-10, 10)) = 1
        _Angle_Blue ("Angle_Blue", Range(0, 2)) = 0
        _FlashIntensity_Blue ("FlashIntensity_Blue", Float ) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float4 _Emission_Red;
            uniform float _UVSpeed_Red;
            uniform float _Angle_Red;
            uniform sampler2D _UVAnimTex_Red; uniform float4 _UVAnimTex_Red_ST;
            uniform float _FlashIntensity_Red;
            uniform float4 _Emission_Green;
            uniform float _UVSpeed_Green;
            uniform float _Angle_Green;
            uniform sampler2D _UVAnimTex_Green; uniform float4 _UVAnimTex_Green_ST;
            uniform float _FlashIntensity_Green;
            uniform float4 _Emission_Blue;
            uniform float _UVSpeed_Blue;
            uniform float _Angle_Blue;
            uniform sampler2D _UVAnimTex_Blue; uniform float4 _UVAnimTex_Blue_ST;
            uniform float _FlashIntensity_Blue;
            uniform sampler2D _Texture_RGB; uniform float4 _Texture_RGB_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 _Texture_RGB_var = tex2D(_Texture_RGB,TRANSFORM_TEX(i.uv0, _Texture_RGB));
                float TexRed = _Texture_RGB_var.r;
                float node_838 = 1.0;
                float4 node_5723 = _Time + _TimeEditor;
                float Timer = node_5723.g;
                float node_3234_ang = (3.141592654*_Angle_Red);
                float node_3234_spd = 1.0;
                float node_3234_cos = cos(node_3234_spd*node_3234_ang);
                float node_3234_sin = sin(node_3234_spd*node_3234_ang);
                float2 node_3234_piv = float2(0.5,0.5);
                float2 node_3234 = (mul(i.uv0-node_3234_piv,float2x2( node_3234_cos, -node_3234_sin, node_3234_sin, node_3234_cos))+node_3234_piv);
                float2 node_3408 = (node_3234+(Timer*_UVSpeed_Red)*float2(0,-0.2));
                float4 _UVAnimTex_Red_var = tex2D(_UVAnimTex_Red,TRANSFORM_TEX(node_3408, _UVAnimTex_Red));
                float TexGreen = _Texture_RGB_var.g;
                float node_860 = 1.0;
                float node_5843_ang = (3.141592654*_Angle_Green);
                float node_5843_spd = 1.0;
                float node_5843_cos = cos(node_5843_spd*node_5843_ang);
                float node_5843_sin = sin(node_5843_spd*node_5843_ang);
                float2 node_5843_piv = float2(0.5,0.5);
                float2 node_5843 = (mul(i.uv0-node_5843_piv,float2x2( node_5843_cos, -node_5843_sin, node_5843_sin, node_5843_cos))+node_5843_piv);
                float2 node_5417 = (node_5843+(Timer*_UVSpeed_Green)*float2(0,-0.2));
                float4 _UVAnimTex_Green_var = tex2D(_UVAnimTex_Green,TRANSFORM_TEX(node_5417, _UVAnimTex_Green));
                float TexBlue = _Texture_RGB_var.b;
                float node_6521 = 1.0;
                float node_1975_ang = (3.141592654*_Angle_Blue);
                float node_1975_spd = 1.0;
                float node_1975_cos = cos(node_1975_spd*node_1975_ang);
                float node_1975_sin = sin(node_1975_spd*node_1975_ang);
                float2 node_1975_piv = float2(0.5,0.5);
                float2 node_1975 = (mul(i.uv0-node_1975_piv,float2x2( node_1975_cos, -node_1975_sin, node_1975_sin, node_1975_cos))+node_1975_piv);
                float2 node_6966 = (node_1975+(Timer*_UVSpeed_Blue)*float2(0,-0.2));
                float4 _UVAnimTex_Blue_var = tex2D(_UVAnimTex_Blue,TRANSFORM_TEX(node_6966, _UVAnimTex_Blue));
                float3 emissive = (((TexRed*(_Emission_Red.rgb*(node_838+((10.0-node_838)*_FlashIntensity_Red))))*_UVAnimTex_Red_var.rgb)+((TexGreen*(_Emission_Green.rgb*(node_860+((10.0-node_860)*_FlashIntensity_Green))))*_UVAnimTex_Green_var.rgb)+((TexBlue*(_Emission_Blue.rgb*(node_6521+((10.0-node_6521)*_FlashIntensity_Blue))))*_UVAnimTex_Blue_var.rgb));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Mobile/Particles/Additive Culled"
    CustomEditor "ShaderForgeMaterialInspector"
}
