// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:2,spmd:1,trmd:1,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3506591,fgcg:0.3363971,fgcb:0.75,fgca:1,fgde:0.01,fgrn:80,fgrf:200,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:491,x:33457,y:32516,varname:node_491,prsc:2|diff-4746-OUT,alpha-1373-OUT,refract-3703-OUT;n:type:ShaderForge.SFN_Color,id:4623,x:32736,y:32243,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.1400102,c2:0.8335096,c3:0.8655173,c4:1;n:type:ShaderForge.SFN_ComponentMask,id:9059,x:32890,y:32950,varname:node_9059,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-9365-RGB;n:type:ShaderForge.SFN_Slider,id:5477,x:32616,y:33205,ptovrint:False,ptlb:Refraction,ptin:_Refraction,varname:_Refraction,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Time,id:5765,x:31937,y:32872,varname:node_5765,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:2238,x:32223,y:32762,varname:node_2238,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Append,id:883,x:31937,y:32679,varname:node_883,prsc:2|A-3480-OUT,B-7238-OUT;n:type:ShaderForge.SFN_Add,id:711,x:32449,y:32857,varname:node_711,prsc:2|A-2238-UVOUT,B-9853-OUT;n:type:ShaderForge.SFN_Multiply,id:9853,x:32229,y:32938,varname:node_9853,prsc:2|A-883-OUT,B-5765-T;n:type:ShaderForge.SFN_ValueProperty,id:3480,x:31760,y:32723,ptovrint:False,ptlb:Speed U,ptin:_SpeedU,varname:_SpeedU,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:7238,x:31760,y:32854,ptovrint:False,ptlb:Speed V,ptin:_SpeedV,varname:_SpeedV,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Tex2d,id:6847,x:32736,y:32456,ptovrint:False,ptlb:Diffuse Texture,ptin:_DiffuseTexture,varname:_DiffuseTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-711-OUT;n:type:ShaderForge.SFN_Multiply,id:4746,x:32942,y:32253,varname:node_4746,prsc:2|A-4623-RGB,B-6847-RGB;n:type:ShaderForge.SFN_ValueProperty,id:330,x:32953,y:32744,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:_Opacity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Tex2d,id:8614,x:32953,y:32509,ptovrint:False,ptlb:Mask(Alpha),ptin:_MaskAlpha,varname:_MaskAlpha,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9365,x:32694,y:32950,ptovrint:False,ptlb:Refraction Texture,ptin:_RefractionTexture,varname:_RefractionTexture,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-711-OUT;n:type:ShaderForge.SFN_Multiply,id:3703,x:33100,y:33059,varname:node_3703,prsc:2|A-9059-OUT,B-5477-OUT;n:type:ShaderForge.SFN_Multiply,id:1373,x:33161,y:32651,varname:node_1373,prsc:2|A-8614-A,B-330-OUT;proporder:4623-5477-3480-7238-6847-330-8614-9365;pass:END;sub:END;*/

Shader "ZD_Shader/Scenes/Aldedo_Uvanimation_Refraction" {
    Properties {
        _Color ("Color", Color) = (0.1400102,0.8335096,0.8655173,1)
        _Refraction ("Refraction", Range(0, 1)) = 0
        _SpeedU ("Speed U", Float ) = 0
        _SpeedV ("Speed V", Float ) = 0
        _DiffuseTexture ("Diffuse Texture", 2D) = "white" {}
        _Opacity ("Opacity", Float ) = 0
        _MaskAlpha ("Mask(Alpha)", 2D) = "white" {}
        _RefractionTexture ("Refraction Texture", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        GrabPass{ }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _GrabTexture;
            uniform float4 _TimeEditor;
            uniform float4 _Color;
            uniform float _Refraction;
            uniform float _SpeedU;
            uniform float _SpeedV;
            uniform sampler2D _DiffuseTexture; uniform float4 _DiffuseTexture_ST;
            uniform float _Opacity;
            uniform sampler2D _MaskAlpha; uniform float4 _MaskAlpha_ST;
            uniform sampler2D _RefractionTexture; uniform float4 _RefractionTexture_ST;
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
                UNITY_FOG_COORDS(4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
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
                float3 normalDirection = i.normalDir;
                float4 node_5765 = _Time + _TimeEditor;
                float2 node_711 = (i.uv0+(float2(_SpeedU,_SpeedV)*node_5765.g));
                float4 _RefractionTexture_var = tex2D(_RefractionTexture,TRANSFORM_TEX(node_711, _RefractionTexture));
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + (_RefractionTexture_var.rgb.rg*_Refraction);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float4 _DiffuseTexture_var = tex2D(_DiffuseTexture,TRANSFORM_TEX(node_711, _DiffuseTexture));
                float3 diffuseColor = (_Color.rgb*_DiffuseTexture_var.rgb);
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float4 _MaskAlpha_var = tex2D(_MaskAlpha,TRANSFORM_TEX(i.uv0, _MaskAlpha));
                float3 finalColor = diffuse * (_MaskAlpha_var.a*_Opacity);
                fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor,(_MaskAlpha_var.a*_Opacity)),1);
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
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _GrabTexture;
            uniform float4 _TimeEditor;
            uniform float4 _Color;
            uniform float _Refraction;
            uniform float _SpeedU;
            uniform float _SpeedV;
            uniform sampler2D _DiffuseTexture; uniform float4 _DiffuseTexture_ST;
            uniform float _Opacity;
            uniform sampler2D _MaskAlpha; uniform float4 _MaskAlpha_ST;
            uniform sampler2D _RefractionTexture; uniform float4 _RefractionTexture_ST;
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
                LIGHTING_COORDS(4,5)
                UNITY_FOG_COORDS(6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.screenPos = o.pos;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
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
                float3 normalDirection = i.normalDir;
                float4 node_5765 = _Time + _TimeEditor;
                float2 node_711 = (i.uv0+(float2(_SpeedU,_SpeedV)*node_5765.g));
                float4 _RefractionTexture_var = tex2D(_RefractionTexture,TRANSFORM_TEX(node_711, _RefractionTexture));
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + (_RefractionTexture_var.rgb.rg*_Refraction);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float4 _DiffuseTexture_var = tex2D(_DiffuseTexture,TRANSFORM_TEX(node_711, _DiffuseTexture));
                float3 diffuseColor = (_Color.rgb*_DiffuseTexture_var.rgb);
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float4 _MaskAlpha_var = tex2D(_MaskAlpha,TRANSFORM_TEX(i.uv0, _MaskAlpha));
                float3 finalColor = diffuse * (_MaskAlpha_var.a*_Opacity);
                fixed4 finalRGBA = fixed4(finalColor,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
