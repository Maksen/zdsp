// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:32376,y:32679,varname:node_4013,prsc:2|emission-8564-OUT,clip-3905-OUT;n:type:ShaderForge.SFN_Tex2d,id:541,x:31227,y:33150,ptovrint:False,ptlb:Noise,ptin:_Noise,varname:_Noise,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:5289,x:30670,y:32949,ptovrint:False,ptlb:Dissolve amount,ptin:_Dissolveamount,varname:_Dissolveamount,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Add,id:3905,x:31438,y:32949,varname:node_3905,prsc:2|A-5277-OUT,B-541-R;n:type:ShaderForge.SFN_RemapRange,id:5277,x:31227,y:32949,varname:node_5277,prsc:2,frmn:0,frmx:1,tomn:-0.7,tomx:0.7|IN-8179-OUT;n:type:ShaderForge.SFN_OneMinus,id:8179,x:31025,y:32949,varname:node_8179,prsc:2|IN-5289-OUT;n:type:ShaderForge.SFN_RemapRange,id:9202,x:31055,y:32395,varname:node_9202,prsc:2,frmn:0,frmx:1,tomn:-4,tomx:4|IN-3905-OUT;n:type:ShaderForge.SFN_Clamp01,id:1692,x:31252,y:32395,varname:node_1692,prsc:2|IN-9202-OUT;n:type:ShaderForge.SFN_OneMinus,id:6418,x:31444,y:32395,varname:node_6418,prsc:2|IN-1692-OUT;n:type:ShaderForge.SFN_Tex2d,id:7810,x:31969,y:32501,ptovrint:False,ptlb:Diffuse,ptin:_Diffuse,varname:_Diffuse,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Vector3,id:7840,x:31762,y:32792,varname:node_7840,prsc:2,v1:1,v2:0.3,v3:0.1;n:type:ShaderForge.SFN_Multiply,id:658,x:31979,y:32774,varname:node_658,prsc:2|A-6799-OUT,B-7840-OUT;n:type:ShaderForge.SFN_Multiply,id:6799,x:31775,y:32518,varname:node_6799,prsc:2|A-6418-OUT,B-9406-OUT;n:type:ShaderForge.SFN_Vector1,id:9406,x:31581,y:32538,varname:node_9406,prsc:2,v1:4;n:type:ShaderForge.SFN_Add,id:8564,x:32183,y:32700,varname:node_8564,prsc:2|A-7810-RGB,B-658-OUT;proporder:7810-541-5289;pass:END;sub:END;*/

Shader "FXModel/Dissolve" {
    Properties {
        _Diffuse ("Diffuse", 2D) = "white" {}
        _Noise ("Noise", 2D) = "white" {}
        _Dissolveamount ("Dissolve amount", Range(0, 1)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            uniform float _Dissolveamount;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
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
            float4 frag(VertexOutput i) : COLOR {
                float4 _Noise_var = tex2D(_Noise,TRANSFORM_TEX(i.uv0, _Noise));
                float node_3905 = (((1.0 - _Dissolveamount)*1.4+-0.7)+_Noise_var.r);
                clip(node_3905 - 0.5);
////// Lighting:
////// Emissive:
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                float3 emissive = (_Diffuse_var.rgb+(((1.0 - saturate((node_3905*8.0+-4.0)))*4.0)*float3(1,0.3,0.1)));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            uniform float _Dissolveamount;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 _Noise_var = tex2D(_Noise,TRANSFORM_TEX(i.uv0, _Noise));
                float node_3905 = (((1.0 - _Dissolveamount)*1.4+-0.7)+_Noise_var.r);
                clip(node_3905 - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
