// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Mobile/Particles/Alpha Blended,iptp:0,cusa:True,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True;n:type:ShaderForge.SFN_Final,id:4795,x:32716,y:32678,varname:node_4795,prsc:2|emission-2393-OUT,alpha-798-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:32163,y:32601,ptovrint:True,ptlb:Emission(Mask),ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:2393,x:32495,y:32785,varname:node_2393,prsc:2|A-6074-RGB,B-2053-RGB,C-9580-OUT;n:type:ShaderForge.SFN_VertexColor,id:2053,x:32163,y:32869,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_Multiply,id:798,x:32495,y:32938,varname:node_798,prsc:2|A-6074-A,B-2053-A,C-2056-A,D-3159-A;n:type:ShaderForge.SFN_Panner,id:3591,x:31929,y:33126,varname:node_3591,prsc:2,spu:0,spv:-0.2|UVIN-3911-UVOUT,DIST-2288-OUT;n:type:ShaderForge.SFN_TexCoord,id:6608,x:31186,y:33106,varname:node_6608,prsc:2,uv:0;n:type:ShaderForge.SFN_Time,id:1589,x:31544,y:33278,varname:node_1589,prsc:2;n:type:ShaderForge.SFN_Slider,id:5441,x:31387,y:33481,ptovrint:False,ptlb:UVSpeed,ptin:_UVSpeed,varname:_UVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:1,max:10;n:type:ShaderForge.SFN_Multiply,id:2288,x:31771,y:33179,varname:node_2288,prsc:2|A-1589-T,B-5441-OUT;n:type:ShaderForge.SFN_Rotator,id:3911,x:31629,y:33003,varname:node_3911,prsc:2|UVIN-6608-UVOUT,ANG-2835-OUT;n:type:ShaderForge.SFN_Slider,id:3769,x:31078,y:32917,ptovrint:False,ptlb:Angle,ptin:_Angle,varname:_Angle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:4839,x:31268,y:32796,varname:node_4839,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2835,x:31388,y:32855,varname:node_2835,prsc:2|A-4839-OUT,B-3769-OUT;n:type:ShaderForge.SFN_Tex2d,id:2056,x:32152,y:33169,ptovrint:False,ptlb:FlowTexture,ptin:_FlowTexture,varname:_FlowTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-3591-UVOUT;n:type:ShaderForge.SFN_Color,id:3159,x:32152,y:33404,ptovrint:False,ptlb:FlowTextureColor,ptin:_FlowTextureColor,varname:_FlowTextureColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:9580,x:32367,y:33251,varname:node_9580,prsc:2|A-2056-RGB,B-3159-RGB;proporder:6074-3159-2056-5441-3769;pass:END;sub:END;*/

Shader "AAA_UI/UI_Flow_AlphaBland" {
    Properties {
        _MainTex ("Emission(Mask)", 2D) = "white" {}
        _FlowTextureColor ("FlowTextureColor", Color) = (0.5,0.5,0.5,1)
        _FlowTexture ("FlowTexture", 2D) = "white" {}
        _UVSpeed ("UVSpeed", Range(-10, 10)) = 1
        _Angle ("Angle", Range(0, 2)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
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
            #pragma exclude_renderers gles3 d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _UVSpeed;
            uniform float _Angle;
            uniform sampler2D _FlowTexture; uniform float4 _FlowTexture_ST;
            uniform float4 _FlowTextureColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 node_1589 = _Time + _TimeEditor;
                float node_3911_ang = (3.141592654*_Angle);
                float node_3911_spd = 1.0;
                float node_3911_cos = cos(node_3911_spd*node_3911_ang);
                float node_3911_sin = sin(node_3911_spd*node_3911_ang);
                float2 node_3911_piv = float2(0.5,0.5);
                float2 node_3911 = (mul(i.uv0-node_3911_piv,float2x2( node_3911_cos, -node_3911_sin, node_3911_sin, node_3911_cos))+node_3911_piv);
                float2 node_3591 = (node_3911+(node_1589.g*_UVSpeed)*float2(0,-0.2));
                float4 _FlowTexture_var = tex2D(_FlowTexture,TRANSFORM_TEX(node_3591, _FlowTexture));
                float3 emissive = (_MainTex_var.rgb*i.vertexColor.rgb*(_FlowTexture_var.rgb*_FlowTextureColor.rgb));
                float3 finalColor = emissive;
                return fixed4(finalColor,(_MainTex_var.a*i.vertexColor.a*_FlowTexture_var.a*_FlowTextureColor.a));
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
