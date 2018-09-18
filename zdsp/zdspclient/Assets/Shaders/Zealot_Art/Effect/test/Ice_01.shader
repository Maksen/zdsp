// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:34071,y:33485,varname:node_3138,prsc:2|emission-4540-OUT,alpha-3721-OUT,refract-7181-OUT,voffset-5912-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32858,y:32923,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:9610,x:32960,y:34622,ptovrint:False,ptlb:Height,ptin:_Height,varname:_Height,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2;n:type:ShaderForge.SFN_NormalVector,id:3809,x:33170,y:34592,prsc:2,pt:False;n:type:ShaderForge.SFN_Multiply,id:5912,x:33318,y:34545,varname:node_5912,prsc:2|A-2884-OUT,B-3809-OUT;n:type:ShaderForge.SFN_Tex2d,id:646,x:32960,y:34390,ptovrint:False,ptlb:HeightMap,ptin:_HeightMap,varname:_HeightMap,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:2884,x:33165,y:34467,varname:node_2884,prsc:2|A-646-RGB,B-9610-OUT;n:type:ShaderForge.SFN_Fresnel,id:1092,x:32481,y:33132,varname:node_1092,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:4671,x:32481,y:33344,ptovrint:False,ptlb:Fresnel,ptin:_Fresnel,varname:_Fresnel,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Slider,id:6312,x:33439,y:33711,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:_Opacity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8,max:1;n:type:ShaderForge.SFN_Tex2d,id:9450,x:33193,y:34030,ptovrint:False,ptlb:RefractionTex,ptin:_RefractionTex,varname:_RefractionTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Multiply,id:7181,x:33367,y:33905,varname:node_7181,prsc:2|A-2826-OUT,B-9450-RGB;n:type:ShaderForge.SFN_Vector1,id:9452,x:32975,y:33951,varname:node_9452,prsc:2,v1:0.1;n:type:ShaderForge.SFN_Multiply,id:2826,x:33193,y:33865,varname:node_2826,prsc:2|A-5596-OUT,B-9452-OUT;n:type:ShaderForge.SFN_Slider,id:5596,x:32854,y:33815,ptovrint:False,ptlb:Refraction,ptin:_Refraction,varname:_Refraction,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.1,max:1;n:type:ShaderForge.SFN_Posterize,id:936,x:33026,y:33136,varname:node_936,prsc:2|IN-6722-OUT,STPS-745-OUT;n:type:ShaderForge.SFN_Multiply,id:3721,x:33873,y:33665,varname:node_3721,prsc:2|A-1741-OUT,B-6312-OUT;n:type:ShaderForge.SFN_Multiply,id:4540,x:33477,y:32972,varname:node_4540,prsc:2|A-7241-RGB,B-936-OUT;n:type:ShaderForge.SFN_Fresnel,id:9969,x:33528,y:33473,varname:node_9969,prsc:2;n:type:ShaderForge.SFN_Add,id:6722,x:32705,y:33149,varname:node_6722,prsc:2|A-1092-OUT,B-4671-OUT;n:type:ShaderForge.SFN_Vector1,id:6109,x:33517,y:33621,varname:node_6109,prsc:2,v1:0.2;n:type:ShaderForge.SFN_Add,id:1741,x:33731,y:33555,varname:node_1741,prsc:2|A-9969-OUT,B-6109-OUT;n:type:ShaderForge.SFN_Vector1,id:745,x:32866,y:33349,varname:node_745,prsc:2,v1:4;proporder:7241-6312-4671-646-9610-9450-5596;pass:END;sub:END;*/

Shader "ZD_Shader/Effect/test/Ice_01" {
    Properties {
        [HDR]_Color ("Color", Color) = (0.07843138,0.3921569,0.7843137,1)
        _Opacity ("Opacity", Range(0, 1)) = 0.8
        _Fresnel ("Fresnel", Float ) = 1
        _HeightMap ("HeightMap", 2D) = "white" {}
        _Height ("Height", Float ) = 0.2
        _RefractionTex ("RefractionTex", 2D) = "bump" {}
        _Refraction ("Refraction", Range(0, 1)) = 0.1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        GrabPass{ }
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
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform float4 _Color;
            uniform float _Height;
            uniform sampler2D _HeightMap; uniform float4 _HeightMap_ST;
            uniform float _Fresnel;
            uniform float _Opacity;
            uniform sampler2D _RefractionTex; uniform float4 _RefractionTex_ST;
            uniform float _Refraction;
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
                float4 screenPos : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 _HeightMap_var = tex2Dlod(_HeightMap,float4(TRANSFORM_TEX(o.uv0, _HeightMap),0.0,0));
                v.vertex.xyz += ((_HeightMap_var.rgb*_Height)*v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                o.screenPos = o.pos;
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 _RefractionTex_var = UnpackNormal(tex2D(_RefractionTex,TRANSFORM_TEX(i.uv0, _RefractionTex)));
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + ((_Refraction*0.1)*_RefractionTex_var.rgb).rg;
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
////// Lighting:
////// Emissive:
                float node_745 = 4.0;
                float3 emissive = (_Color.rgb*floor(((1.0-max(0,dot(normalDirection, viewDirection)))+_Fresnel) * node_745) / (node_745 - 1));
                float3 finalColor = emissive;
                return fixed4(lerp(sceneColor.rgb, finalColor,(((1.0-max(0,dot(normalDirection, viewDirection)))+0.2)*_Opacity)),1);
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
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform float _Height;
            uniform sampler2D _HeightMap; uniform float4 _HeightMap_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 _HeightMap_var = tex2Dlod(_HeightMap,float4(TRANSFORM_TEX(o.uv0, _HeightMap),0.0,0));
                v.vertex.xyz += ((_HeightMap_var.rgb*_Height)*v.normal);
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
