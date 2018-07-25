// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Mobile/Unlit (Supports Lightmap),iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:33898,y:33747,varname:node_4013,prsc:2|emission-4339-RGB,alpha-9254-OUT;n:type:ShaderForge.SFN_Panner,id:4010,x:33299,y:34537,varname:node_4010,prsc:2,spu:0,spv:-1|UVIN-1223-UVOUT,DIST-4504-OUT;n:type:ShaderForge.SFN_TexCoord,id:8447,x:32851,y:34369,varname:node_8447,prsc:2,uv:0;n:type:ShaderForge.SFN_Rotator,id:1223,x:33116,y:34537,varname:node_1223,prsc:2|UVIN-8447-UVOUT,ANG-1538-OUT;n:type:ShaderForge.SFN_Multiply,id:1538,x:32855,y:34647,varname:node_1538,prsc:2|A-6859-OUT,B-646-OUT;n:type:ShaderForge.SFN_Slider,id:646,x:32534,y:34745,ptovrint:False,ptlb:UVAngle_01,ptin:_UVAngle_01,varname:_UVAngle_01,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:6859,x:32735,y:34588,varname:node_6859,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:8813,x:33469,y:34537,ptovrint:False,ptlb:FlowMask_01,ptin:_FlowMask_01,varname:_FlowMask_01,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-4010-UVOUT;n:type:ShaderForge.SFN_Color,id:4339,x:33670,y:33846,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Tex2d,id:1312,x:33463,y:34252,ptovrint:False,ptlb:Mask_00,ptin:_Mask_00,varname:_Mask_00,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:9254,x:33676,y:34365,varname:node_9254,prsc:2|A-1312-R,B-8813-R,C-4859-R;n:type:ShaderForge.SFN_Time,id:9435,x:32855,y:34842,varname:node_9435,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4504,x:33041,y:34869,varname:node_4504,prsc:2|A-9435-T,B-7461-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7461,x:32855,y:35041,ptovrint:False,ptlb:FlowSpeed_01,ptin:_FlowSpeed_01,varname:_FlowSpeed_01,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Tex2d,id:4859,x:33488,y:35357,ptovrint:False,ptlb:FlowMask_02,ptin:_FlowMask_02,varname:_FlowMask_02,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1227-UVOUT;n:type:ShaderForge.SFN_Panner,id:1227,x:33283,y:35421,varname:node_1227,prsc:2,spu:0,spv:-1|UVIN-5291-UVOUT,DIST-4716-OUT;n:type:ShaderForge.SFN_TexCoord,id:478,x:32835,y:35253,varname:node_478,prsc:2,uv:0;n:type:ShaderForge.SFN_Rotator,id:5291,x:33100,y:35421,varname:node_5291,prsc:2|UVIN-478-UVOUT,ANG-7012-OUT;n:type:ShaderForge.SFN_Multiply,id:7012,x:32839,y:35531,varname:node_7012,prsc:2|A-5148-OUT,B-6818-OUT;n:type:ShaderForge.SFN_Slider,id:6818,x:32518,y:35629,ptovrint:False,ptlb:UVAngle_02,ptin:_UVAngle_02,varname:_UVAngle_02,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:5148,x:32719,y:35472,varname:node_5148,prsc:2;n:type:ShaderForge.SFN_Time,id:9385,x:32839,y:35726,varname:node_9385,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4716,x:33025,y:35753,varname:node_4716,prsc:2|A-9385-T,B-9011-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9011,x:32839,y:35925,ptovrint:False,ptlb:FlowSpeed_02,ptin:_FlowSpeed_02,varname:_FlowSpeed_02,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:4339-1312-8813-646-7461-4859-6818-9011;pass:END;sub:END;*/

Shader "ZD_Shader/TEST/TEST_02" {
    Properties {
        _Color ("Color", Color) = (0.5,0.5,0.5,1)
        _Mask_00 ("Mask_00", 2D) = "white" {}
        _FlowMask_01 ("FlowMask_01", 2D) = "white" {}
        _UVAngle_01 ("UVAngle_01", Range(0, 2)) = 0
        _FlowSpeed_01 ("FlowSpeed_01", Float ) = 1
        _FlowMask_02 ("FlowMask_02", 2D) = "white" {}
        _UVAngle_02 ("UVAngle_02", Range(0, 2)) = 0
        _FlowSpeed_02 ("FlowSpeed_02", Float ) = 1
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
            Cull Off
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
            uniform float _UVAngle_01;
            uniform sampler2D _FlowMask_01; uniform float4 _FlowMask_01_ST;
            uniform float4 _Color;
            uniform sampler2D _Mask_00; uniform float4 _Mask_00_ST;
            uniform float _FlowSpeed_01;
            uniform sampler2D _FlowMask_02; uniform float4 _FlowMask_02_ST;
            uniform float _UVAngle_02;
            uniform float _FlowSpeed_02;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float3 emissive = _Color.rgb;
                float3 finalColor = emissive;
                float4 _Mask_00_var = tex2D(_Mask_00,TRANSFORM_TEX(i.uv0, _Mask_00));
                float4 node_9435 = _Time + _TimeEditor;
                float node_1223_ang = (3.141592654*_UVAngle_01);
                float node_1223_spd = 1.0;
                float node_1223_cos = cos(node_1223_spd*node_1223_ang);
                float node_1223_sin = sin(node_1223_spd*node_1223_ang);
                float2 node_1223_piv = float2(0.5,0.5);
                float2 node_1223 = (mul(i.uv0-node_1223_piv,float2x2( node_1223_cos, -node_1223_sin, node_1223_sin, node_1223_cos))+node_1223_piv);
                float2 node_4010 = (node_1223+(node_9435.g*_FlowSpeed_01)*float2(0,-1));
                float4 _FlowMask_01_var = tex2D(_FlowMask_01,TRANSFORM_TEX(node_4010, _FlowMask_01));
                float4 node_9385 = _Time + _TimeEditor;
                float node_5291_ang = (3.141592654*_UVAngle_02);
                float node_5291_spd = 1.0;
                float node_5291_cos = cos(node_5291_spd*node_5291_ang);
                float node_5291_sin = sin(node_5291_spd*node_5291_ang);
                float2 node_5291_piv = float2(0.5,0.5);
                float2 node_5291 = (mul(i.uv0-node_5291_piv,float2x2( node_5291_cos, -node_5291_sin, node_5291_sin, node_5291_cos))+node_5291_piv);
                float2 node_1227 = (node_5291+(node_9385.g*_FlowSpeed_02)*float2(0,-1));
                float4 _FlowMask_02_var = tex2D(_FlowMask_02,TRANSFORM_TEX(node_1227, _FlowMask_02));
                fixed4 finalRGBA = fixed4(finalColor,(_Mask_00_var.r*_FlowMask_01_var.r*_FlowMask_02_var.r));
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Mobile/Unlit (Supports Lightmap)"
    CustomEditor "ShaderForgeMaterialInspector"
}
