// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:34393,y:32850,varname:node_3138,prsc:2|emission-7408-OUT,alpha-1539-OUT,clip-1539-OUT;n:type:ShaderForge.SFN_Tex2d,id:1679,x:33131,y:33082,varname:node_1679,prsc:2,tex:f845f32889ab9ad4a93666b7188bfaa2,ntxv:0,isnm:False|UVIN-3710-UVOUT,TEX-4019-TEX;n:type:ShaderForge.SFN_TexCoord,id:107,x:31409,y:32969,varname:node_107,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Panner,id:6221,x:31710,y:33041,varname:node_6221,prsc:2,spu:1,spv:0|UVIN-107-UVOUT,DIST-8512-OUT;n:type:ShaderForge.SFN_Panner,id:3607,x:31710,y:33185,varname:node_3607,prsc:2,spu:0,spv:1|UVIN-107-UVOUT,DIST-3056-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:1336,x:31710,y:32825,ptovrint:False,ptlb:FlowMap,ptin:_FlowMap,varname:node_1336,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:87f888c124cc0a54bb56b56f6ab8d358,ntxv:2,isnm:False;n:type:ShaderForge.SFN_ValueProperty,id:544,x:31216,y:33274,ptovrint:False,ptlb:U_Flowspeed,ptin:_U_Flowspeed,varname:node_544,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.25;n:type:ShaderForge.SFN_Time,id:7927,x:31216,y:33118,varname:node_7927,prsc:2;n:type:ShaderForge.SFN_Multiply,id:8512,x:31409,y:33131,varname:node_8512,prsc:2|A-7927-TSL,B-544-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4657,x:31216,y:33353,ptovrint:False,ptlb:V_Flowspeed,ptin:_V_Flowspeed,varname:_U_Flowspeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.5;n:type:ShaderForge.SFN_Multiply,id:3056,x:31409,y:33303,varname:node_3056,prsc:2|A-7927-T,B-4657-OUT;n:type:ShaderForge.SFN_Append,id:109,x:32280,y:33148,varname:node_109,prsc:2|A-9372-R,B-3405-G;n:type:ShaderForge.SFN_Multiply,id:750,x:32494,y:33148,varname:node_750,prsc:2|A-109-OUT,B-4663-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4663,x:32280,y:33310,ptovrint:False,ptlb:Flow Strength,ptin:_FlowStrength,varname:node_4663,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.07;n:type:ShaderForge.SFN_Tex2dAsset,id:4019,x:32816,y:32845,ptovrint:False,ptlb:BrushTex,ptin:_BrushTex,varname:node_4019,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:f845f32889ab9ad4a93666b7188bfaa2,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Add,id:7971,x:32673,y:33089,varname:node_7971,prsc:2|A-107-UVOUT,B-750-OUT;n:type:ShaderForge.SFN_Tex2d,id:9372,x:32003,y:33113,varname:node_9372,prsc:2,tex:87f888c124cc0a54bb56b56f6ab8d358,ntxv:0,isnm:False|UVIN-6221-UVOUT,TEX-1336-TEX;n:type:ShaderForge.SFN_Tex2d,id:3405,x:32003,y:33261,varname:node_3405,prsc:2,tex:87f888c124cc0a54bb56b56f6ab8d358,ntxv:0,isnm:False|UVIN-3607-UVOUT,TEX-1336-TEX;n:type:ShaderForge.SFN_Panner,id:3710,x:32881,y:33089,varname:node_3710,prsc:2,spu:-0.1,spv:0|UVIN-7971-OUT,DIST-6914-OUT;n:type:ShaderForge.SFN_Slider,id:6914,x:32494,y:33386,ptovrint:False,ptlb:Offset,ptin:_Offset,varname:node_6914,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Panner,id:7693,x:32881,y:33280,varname:node_7693,prsc:2,spu:-1,spv:0|UVIN-107-UVOUT,DIST-6914-OUT;n:type:ShaderForge.SFN_Tex2d,id:4452,x:33131,y:33280,varname:node_4452,prsc:2,tex:f845f32889ab9ad4a93666b7188bfaa2,ntxv:0,isnm:False|UVIN-7693-UVOUT,TEX-4019-TEX;n:type:ShaderForge.SFN_Multiply,id:225,x:33388,y:33107,varname:node_225,prsc:2|A-1679-R,B-4452-G;n:type:ShaderForge.SFN_Color,id:5527,x:33364,y:32939,ptovrint:False,ptlb:Emission,ptin:_Emission,varname:node_5527,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:7408,x:33691,y:32959,varname:node_7408,prsc:2|A-5527-RGB,B-225-OUT,C-6775-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6775,x:33388,y:33255,ptovrint:False,ptlb:EmissionStrength,ptin:_EmissionStrength,varname:node_6775,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Fresnel,id:3571,x:33347,y:33418,varname:node_3571,prsc:2|EXP-3959-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3959,x:33176,y:33494,ptovrint:False,ptlb:FresnelStrength,ptin:_FresnelStrength,varname:node_3959,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_OneMinus,id:6142,x:33540,y:33418,varname:node_6142,prsc:2|IN-3571-OUT;n:type:ShaderForge.SFN_Multiply,id:9374,x:33721,y:33418,varname:node_9374,prsc:2|A-6142-OUT,B-6142-OUT,C-7070-OUT;n:type:ShaderForge.SFN_Clamp01,id:3272,x:33721,y:33289,varname:node_3272,prsc:2|IN-9374-OUT;n:type:ShaderForge.SFN_Multiply,id:1539,x:33920,y:33221,varname:node_1539,prsc:2|A-757-OUT,B-3272-OUT,C-4104-OUT;n:type:ShaderForge.SFN_Vector1,id:7070,x:33540,y:33548,varname:node_7070,prsc:2,v1:1;n:type:ShaderForge.SFN_Tex2d,id:4164,x:33132,y:33606,varname:node_4164,prsc:2,tex:f845f32889ab9ad4a93666b7188bfaa2,ntxv:0,isnm:False|UVIN-1469-OUT,TEX-4019-TEX;n:type:ShaderForge.SFN_Set,id:739,x:31897,y:33554,varname:UV,prsc:2|IN-107-UVOUT;n:type:ShaderForge.SFN_Get,id:1469,x:32912,y:33606,varname:node_1469,prsc:2|IN-739-OUT;n:type:ShaderForge.SFN_Add,id:5997,x:33407,y:33684,varname:node_5997,prsc:2|A-4164-B,B-1294-OUT;n:type:ShaderForge.SFN_Slider,id:1294,x:33023,y:33784,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_1294,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:0.3165382,max:1;n:type:ShaderForge.SFN_Clamp01,id:4104,x:33721,y:33640,varname:node_4104,prsc:2|IN-5997-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1922,x:33591,y:33196,ptovrint:False,ptlb:Alpha Strength,ptin:_AlphaStrength,varname:node_1922,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:757,x:33750,y:33090,varname:node_757,prsc:2|A-225-OUT,B-1922-OUT;proporder:544-4657-1336-4663-4019-6914-5527-6775-3959-1294-1922;pass:END;sub:END;*/

Shader "Shader Forge/Ink" {
    Properties {
        _U_Flowspeed ("U_Flowspeed", Float ) = 0.25
        _V_Flowspeed ("V_Flowspeed", Float ) = 0.5
        _FlowMap ("FlowMap", 2D) = "black" {}
        _FlowStrength ("Flow Strength", Float ) = 0.07
        _BrushTex ("BrushTex", 2D) = "black" {}
        _Offset ("Offset", Range(0, 1)) = 1
        _Emission ("Emission", Color) = (0,0,0,1)
        _EmissionStrength ("EmissionStrength", Float ) = 1
        _FresnelStrength ("FresnelStrength", Float ) = 2
        _Opacity ("Opacity", Range(-1, 1)) = 0.3165382
        _AlphaStrength ("Alpha Strength", Float ) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
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
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _FlowMap; uniform float4 _FlowMap_ST;
            uniform float _U_Flowspeed;
            uniform float _V_Flowspeed;
            uniform float _FlowStrength;
            uniform sampler2D _BrushTex; uniform float4 _BrushTex_ST;
            uniform float _Offset;
            uniform float4 _Emission;
            uniform float _EmissionStrength;
            uniform float _FresnelStrength;
            uniform float _Opacity;
            uniform float _AlphaStrength;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 node_7927 = _Time + _TimeEditor;
                float2 node_6221 = (i.uv0+(node_7927.r*_U_Flowspeed)*float2(1,0));
                float4 node_9372 = tex2D(_FlowMap,TRANSFORM_TEX(node_6221, _FlowMap));
                float2 node_3607 = (i.uv0+(node_7927.g*_V_Flowspeed)*float2(0,1));
                float4 node_3405 = tex2D(_FlowMap,TRANSFORM_TEX(node_3607, _FlowMap));
                float2 node_3710 = ((i.uv0+(float2(node_9372.r,node_3405.g)*_FlowStrength))+_Offset*float2(-0.1,0));
                float4 node_1679 = tex2D(_BrushTex,TRANSFORM_TEX(node_3710, _BrushTex));
                float2 node_7693 = (i.uv0+_Offset*float2(-1,0));
                float4 node_4452 = tex2D(_BrushTex,TRANSFORM_TEX(node_7693, _BrushTex));
                float node_225 = (node_1679.r*node_4452.g);
                float node_6142 = (1.0 - pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelStrength));
                float2 UV = i.uv0;
                float2 node_1469 = UV;
                float4 node_4164 = tex2D(_BrushTex,TRANSFORM_TEX(node_1469, _BrushTex));
                float node_1539 = ((node_225*_AlphaStrength)*saturate((node_6142*node_6142*1.0))*saturate((node_4164.b+_Opacity)));
                clip(node_1539 - 0.5);
////// Lighting:
////// Emissive:
                float3 emissive = (_Emission.rgb*node_225*_EmissionStrength);
                float3 finalColor = emissive;
                return fixed4(finalColor,node_1539);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _FlowMap; uniform float4 _FlowMap_ST;
            uniform float _U_Flowspeed;
            uniform float _V_Flowspeed;
            uniform float _FlowStrength;
            uniform sampler2D _BrushTex; uniform float4 _BrushTex_ST;
            uniform float _Offset;
            uniform float _FresnelStrength;
            uniform float _Opacity;
            uniform float _AlphaStrength;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 node_7927 = _Time + _TimeEditor;
                float2 node_6221 = (i.uv0+(node_7927.r*_U_Flowspeed)*float2(1,0));
                float4 node_9372 = tex2D(_FlowMap,TRANSFORM_TEX(node_6221, _FlowMap));
                float2 node_3607 = (i.uv0+(node_7927.g*_V_Flowspeed)*float2(0,1));
                float4 node_3405 = tex2D(_FlowMap,TRANSFORM_TEX(node_3607, _FlowMap));
                float2 node_3710 = ((i.uv0+(float2(node_9372.r,node_3405.g)*_FlowStrength))+_Offset*float2(-0.1,0));
                float4 node_1679 = tex2D(_BrushTex,TRANSFORM_TEX(node_3710, _BrushTex));
                float2 node_7693 = (i.uv0+_Offset*float2(-1,0));
                float4 node_4452 = tex2D(_BrushTex,TRANSFORM_TEX(node_7693, _BrushTex));
                float node_225 = (node_1679.r*node_4452.g);
                float node_6142 = (1.0 - pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelStrength));
                float2 UV = i.uv0;
                float2 node_1469 = UV;
                float4 node_4164 = tex2D(_BrushTex,TRANSFORM_TEX(node_1469, _BrushTex));
                float node_1539 = ((node_225*_AlphaStrength)*saturate((node_6142*node_6142*1.0))*saturate((node_4164.b+_Opacity)));
                clip(node_1539 - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
