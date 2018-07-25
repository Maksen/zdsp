// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Mobile/Unlit (Supports Lightmap),iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:33199,y:33870,varname:node_4013,prsc:2|emission-6676-OUT,alpha-4968-OUT;n:type:ShaderForge.SFN_Tex2d,id:2356,x:32176,y:34536,ptovrint:False,ptlb:FlowNoiseTex,ptin:_FlowNoiseTex,varname:_FlowNoiseTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-4010-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:2461,x:32375,y:34084,ptovrint:False,ptlb:MaskTex,ptin:_MaskTex,varname:_MaskTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Panner,id:4010,x:31858,y:34145,varname:node_4010,prsc:2,spu:0,spv:-1|UVIN-1223-UVOUT,DIST-1103-OUT;n:type:ShaderForge.SFN_TexCoord,id:8447,x:31343,y:33998,varname:node_8447,prsc:2,uv:0;n:type:ShaderForge.SFN_Step,id:1676,x:32806,y:34333,varname:node_1676,prsc:2|A-6489-OUT,B-2356-R;n:type:ShaderForge.SFN_Tex2d,id:6574,x:32648,y:33760,ptovrint:True,ptlb:EmissionTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-6158-OUT;n:type:ShaderForge.SFN_Multiply,id:6158,x:32336,y:33811,varname:node_6158,prsc:2|A-2356-RGB,B-8447-UVOUT;n:type:ShaderForge.SFN_Color,id:8448,x:32592,y:33389,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:6676,x:32907,y:33674,varname:node_6676,prsc:2|A-6913-OUT,B-6574-RGB,C-7707-RGB;n:type:ShaderForge.SFN_Multiply,id:6913,x:32752,y:33489,varname:node_6913,prsc:2|A-8448-RGB,B-1301-OUT;n:type:ShaderForge.SFN_Vector1,id:1301,x:32592,y:33588,varname:node_1301,prsc:2,v1:2;n:type:ShaderForge.SFN_Multiply,id:1103,x:31774,y:34583,varname:node_1103,prsc:2|A-8326-T,B-5449-OUT;n:type:ShaderForge.SFN_Time,id:8326,x:31604,y:34477,varname:node_8326,prsc:2;n:type:ShaderForge.SFN_Rotator,id:1223,x:31608,y:34166,varname:node_1223,prsc:2|UVIN-8447-UVOUT,ANG-1538-OUT;n:type:ShaderForge.SFN_Multiply,id:1538,x:31343,y:34203,varname:node_1538,prsc:2|A-6859-OUT,B-646-OUT;n:type:ShaderForge.SFN_Slider,id:646,x:31033,y:34265,ptovrint:False,ptlb:UVAngle,ptin:_UVAngle,varname:_UVAngle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:6859,x:31223,y:34144,varname:node_6859,prsc:2;n:type:ShaderForge.SFN_VertexColor,id:7707,x:32755,y:33983,varname:node_7707,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4968,x:32993,y:34131,varname:node_4968,prsc:2|A-7707-A,B-1676-OUT;n:type:ShaderForge.SFN_OneMinus,id:6489,x:32615,y:34193,varname:node_6489,prsc:2|IN-2461-R;n:type:ShaderForge.SFN_ValueProperty,id:5449,x:31530,y:34839,ptovrint:False,ptlb:UVSpeed,ptin:_UVSpeed,varname:_UVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:8448-6574-2461-2356-5449-646;pass:END;sub:END;*/

Shader "ZD_Shader/Characters/Fire_Particle_EmissionTex_MaskTex_FlowNoiseTex" {
    Properties {
        _Color ("Color", Color) = (0.5,0.5,0.5,1)
        _MainTex ("EmissionTex", 2D) = "white" {}
        _MaskTex ("MaskTex", 2D) = "black" {}
        _FlowNoiseTex ("FlowNoiseTex", 2D) = "white" {}
        _UVSpeed ("UVSpeed", Float ) = 1
        _UVAngle ("UVAngle", Range(0, 2)) = 0
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
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _FlowNoiseTex; uniform float4 _FlowNoiseTex_ST;
            uniform sampler2D _MaskTex; uniform float4 _MaskTex_ST;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _Color;
            uniform float _UVAngle;
            uniform float _UVSpeed;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_8326 = _Time + _TimeEditor;
                float node_1223_ang = (3.141592654*_UVAngle);
                float node_1223_spd = 1.0;
                float node_1223_cos = cos(node_1223_spd*node_1223_ang);
                float node_1223_sin = sin(node_1223_spd*node_1223_ang);
                float2 node_1223_piv = float2(0.5,0.5);
                float2 node_1223 = (mul(i.uv0-node_1223_piv,float2x2( node_1223_cos, -node_1223_sin, node_1223_sin, node_1223_cos))+node_1223_piv);
                float2 node_4010 = (node_1223+(node_8326.g*_UVSpeed)*float2(0,-1));
                float4 _FlowNoiseTex_var = tex2D(_FlowNoiseTex,TRANSFORM_TEX(node_4010, _FlowNoiseTex));
                float3 node_6158 = (_FlowNoiseTex_var.rgb*float3(i.uv0,0.0));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_6158, _MainTex));
                float3 emissive = ((_Color.rgb*2.0)*_MainTex_var.rgb*i.vertexColor.rgb);
                float3 finalColor = emissive;
                float4 _MaskTex_var = tex2D(_MaskTex,TRANSFORM_TEX(i.uv0, _MaskTex));
                fixed4 finalRGBA = fixed4(finalColor,(i.vertexColor.a*step((1.0 - _MaskTex_var.r),_FlowNoiseTex_var.r)));
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Mobile/Unlit (Supports Lightmap)"
    CustomEditor "ShaderForgeMaterialInspector"
}
