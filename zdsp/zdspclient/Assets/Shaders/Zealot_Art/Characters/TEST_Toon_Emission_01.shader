// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Mobile/Diffuse,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:9361,x:32849,y:32907,varname:node_9361,prsc:2|emission-3646-RGB,custl-9069-OUT;n:type:ShaderForge.SFN_LightAttenuation,id:8068,x:32056,y:33015,varname:node_8068,prsc:2;n:type:ShaderForge.SFN_LightColor,id:3406,x:32056,y:32877,varname:node_3406,prsc:2;n:type:ShaderForge.SFN_LightVector,id:4140,x:31415,y:33333,varname:node_4140,prsc:2;n:type:ShaderForge.SFN_NormalVector,id:9693,x:31415,y:33171,prsc:2,pt:False;n:type:ShaderForge.SFN_Dot,id:3017,x:31629,y:33305,varname:node_3017,prsc:2,dt:4|A-9693-OUT,B-4140-OUT;n:type:ShaderForge.SFN_Vector1,id:2962,x:31629,y:33475,varname:node_2962,prsc:2,v1:0;n:type:ShaderForge.SFN_Append,id:993,x:31865,y:33421,varname:node_993,prsc:2|A-3017-OUT,B-2962-OUT;n:type:ShaderForge.SFN_Tex2d,id:7635,x:32074,y:33421,ptovrint:False,ptlb:Toon,ptin:_Toon,varname:_Toon,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-993-OUT;n:type:ShaderForge.SFN_Multiply,id:9782,x:32261,y:32988,varname:node_9782,prsc:2|A-3406-RGB,B-8068-OUT;n:type:ShaderForge.SFN_Multiply,id:4868,x:32603,y:33298,varname:node_4868,prsc:2|A-9782-OUT,B-2547-OUT;n:type:ShaderForge.SFN_Tex2d,id:3646,x:32384,y:32661,ptovrint:True,ptlb:Emission,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:False;n:type:ShaderForge.SFN_Add,id:2547,x:32374,y:33503,varname:node_2547,prsc:2|A-7635-RGB,B-4105-OUT;n:type:ShaderForge.SFN_Vector1,id:4105,x:32074,y:33631,varname:node_4105,prsc:2,v1:-0.5;n:type:ShaderForge.SFN_ToggleProperty,id:130,x:32603,y:33481,ptovrint:False,ptlb:ToonON,ptin:_ToonON,varname:_ToonON,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True;n:type:ShaderForge.SFN_Multiply,id:9069,x:32734,y:33411,varname:node_9069,prsc:2|A-4868-OUT,B-130-OUT;proporder:130-7635-3646;pass:END;sub:END;*/

Shader "ZD_Shader/TEST/TEST_Toon_Emission_01" {
    Properties {
        [MaterialToggle] _ToonON ("ToonON", Float ) = 1
        _Toon ("Toon", 2D) = "white" {}
        _MainTex ("Emission", 2D) = "bump" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
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
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Toon; uniform float4 _Toon_ST;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform fixed _ToonON;
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
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 emissive = _MainTex_var.rgb;
                float2 node_993 = float2(0.5*dot(i.normalDir,lightDirection)+0.5,0.0);
                float4 _Toon_var = tex2D(_Toon,TRANSFORM_TEX(node_993, _Toon));
                float3 finalColor = emissive + (((_LightColor0.rgb*attenuation)*(_Toon_var.rgb+(-0.5)))*_ToonON);
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _Toon; uniform float4 _Toon_ST;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform fixed _ToonON;
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
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float2 node_993 = float2(0.5*dot(i.normalDir,lightDirection)+0.5,0.0);
                float4 _Toon_var = tex2D(_Toon,TRANSFORM_TEX(node_993, _Toon));
                float3 finalColor = (((_LightColor0.rgb*attenuation)*(_Toon_var.rgb+(-0.5)))*_ToonON);
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Mobile/Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
