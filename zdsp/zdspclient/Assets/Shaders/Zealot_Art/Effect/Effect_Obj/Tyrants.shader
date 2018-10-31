// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:32719,y:32712,varname:node_3138,prsc:2|emission-1120-OUT,voffset-4317-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:31826,y:32437,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_Fresnel,id:1762,x:31826,y:32652,varname:node_1762,prsc:2|EXP-618-OUT;n:type:ShaderForge.SFN_Multiply,id:1120,x:32225,y:32585,varname:node_1120,prsc:2|A-7241-RGB,B-1762-OUT;n:type:ShaderForge.SFN_ValueProperty,id:618,x:31626,y:32686,ptovrint:False,ptlb:Rim,ptin:_Rim,varname:_Rim,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:4;n:type:ShaderForge.SFN_NormalVector,id:4876,x:31787,y:33767,prsc:2,pt:False;n:type:ShaderForge.SFN_Multiply,id:4732,x:32135,y:33602,varname:node_4732,prsc:2|A-8513-OUT,B-4876-OUT;n:type:ShaderForge.SFN_Time,id:7584,x:31432,y:33604,varname:node_7584,prsc:2;n:type:ShaderForge.SFN_Multiply,id:82,x:31616,y:33459,varname:node_82,prsc:2|A-2750-OUT,B-7584-T;n:type:ShaderForge.SFN_Sin,id:9578,x:31828,y:33459,varname:node_9578,prsc:2|IN-82-OUT;n:type:ShaderForge.SFN_Multiply,id:4317,x:32311,y:33478,varname:node_4317,prsc:2|A-9210-OUT,B-4732-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9210,x:32052,y:33396,ptovrint:False,ptlb:Size,ptin:_Size,varname:_Size,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.015;n:type:ShaderForge.SFN_Distance,id:8513,x:31967,y:33572,varname:node_8513,prsc:2|A-9578-OUT,B-553-OUT;n:type:ShaderForge.SFN_ValueProperty,id:553,x:31788,y:33680,ptovrint:False,ptlb:Distance,ptin:_Distance,varname:_Distance,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.5;n:type:ShaderForge.SFN_ValueProperty,id:2750,x:31432,y:33506,ptovrint:False,ptlb:time,ptin:_time,varname:_time,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.5;proporder:7241-618-9210-553-2750;pass:END;sub:END;*/

Shader "ZD_Shader/Effect/Effect_Obj/Tyrants " {
    Properties {
        [HDR]_Color ("Color", Color) = (0.07843138,0.3921569,0.7843137,1)
		[PerRendererData]_Rim ("Rim", Float ) = 1000
        _Size ("Size", Float ) = 0.015
        _Distance ("Distance", Float ) = 0.5
        _time ("time", Float ) = 0.5
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
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float4 _Color;
            uniform float _Rim;
            uniform float _Size;
            uniform float _Distance;
            uniform float _time;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_7584 = _Time + _TimeEditor;
                v.vertex.xyz += (_Size*(distance(sin((_time*node_7584.g)),_Distance)*v.normal));
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float3 emissive = (_Color.rgb*pow(1.0-max(0,dot(normalDirection, viewDirection)),_Rim));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
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
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float _Size;
            uniform float _Distance;
            uniform float _time;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float3 normalDir : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_7584 = _Time + _TimeEditor;
                v.vertex.xyz += (_Size*(distance(sin((_time*node_7584.g)),_Distance)*v.normal));
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
