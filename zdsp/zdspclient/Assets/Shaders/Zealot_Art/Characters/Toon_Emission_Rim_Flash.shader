// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Mobile/Diffuse,iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:9361,x:33726,y:32658,varname:node_9361,prsc:2|emission-5276-OUT,custl-3604-OUT;n:type:ShaderForge.SFN_Tex2d,id:5001,x:33117,y:32461,ptovrint:True,ptlb:DiffuseTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_LightVector,id:710,x:30777,y:34358,varname:node_710,prsc:2;n:type:ShaderForge.SFN_NormalVector,id:6307,x:30777,y:34193,prsc:2,pt:False;n:type:ShaderForge.SFN_Tex2d,id:4756,x:31852,y:34797,ptovrint:True,ptlb:ToonTex,ptin:_ToonTex,varname:_ToonTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:640eb59cb367f484e89f835e517e2eea,ntxv:0,isnm:False|UVIN-2798-OUT;n:type:ShaderForge.SFN_Fresnel,id:1261,x:32437,y:33001,cmnt:邊緣光顯示2段,varname:node_1261,prsc:2;n:type:ShaderForge.SFN_ToggleProperty,id:3451,x:32686,y:33467,ptovrint:False,ptlb:RimOn,ptin:_RimOn,varname:_RimOn,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True;n:type:ShaderForge.SFN_Slider,id:2526,x:32358,y:32861,ptovrint:False,ptlb:RimWidth,ptin:_RimWidth,varname:_RimWidth,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.7,max:1;n:type:ShaderForge.SFN_Add,id:3604,x:33188,y:33059,varname:node_3604,prsc:2|A-7022-OUT,B-4947-OUT;n:type:ShaderForge.SFN_Dot,id:1767,x:32683,y:33124,varname:node_1767,prsc:2,dt:1|A-710-OUT,B-6307-OUT;n:type:ShaderForge.SFN_Multiply,id:4947,x:32892,y:33084,cmnt:rim,varname:node_4947,prsc:2|A-7794-OUT,B-1767-OUT,C-2792-RGB,D-3451-OUT;n:type:ShaderForge.SFN_Step,id:7794,x:32670,y:32963,varname:node_7794,prsc:2|A-2526-OUT,B-1261-OUT;n:type:ShaderForge.SFN_Multiply,id:1323,x:32126,y:34356,varname:node_1323,prsc:2|A-4349-OUT,B-4756-RGB;n:type:ShaderForge.SFN_Multiply,id:4349,x:31869,y:33995,varname:node_4349,prsc:2|A-5001-RGB,B-985-OUT;n:type:ShaderForge.SFN_ToggleProperty,id:7548,x:32126,y:34554,ptovrint:False,ptlb:ToonOn,ptin:_ToonOn,varname:_ToonOn,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True;n:type:ShaderForge.SFN_Multiply,id:7022,x:32396,y:34468,cmnt:toon,varname:node_7022,prsc:2|A-1323-OUT,B-7548-OUT;n:type:ShaderForge.SFN_Color,id:1819,x:31361,y:33892,ptovrint:False,ptlb:ToonColor,ptin:_ToonColor,varname:_ToonColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.8382353,c2:0.6903114,c3:0.7668238,c4:1;n:type:ShaderForge.SFN_Multiply,id:985,x:31681,y:34018,varname:node_985,prsc:2|A-5933-OUT,B-6204-OUT;n:type:ShaderForge.SFN_Vector1,id:6204,x:31361,y:34096,varname:node_6204,prsc:2,v1:-1;n:type:ShaderForge.SFN_OneMinus,id:5933,x:31529,y:33892,varname:node_5933,prsc:2|IN-1819-RGB;n:type:ShaderForge.SFN_Fresnel,id:9589,x:31044,y:34587,varname:node_9589,prsc:2;n:type:ShaderForge.SFN_Append,id:2798,x:31676,y:34754,varname:node_2798,prsc:2|A-7693-OUT,B-1472-OUT;n:type:ShaderForge.SFN_Vector1,id:1472,x:31511,y:34865,varname:node_1472,prsc:2,v1:0;n:type:ShaderForge.SFN_Multiply,id:7693,x:31511,y:34702,varname:node_7693,prsc:2|A-12-OUT,B-8047-OUT;n:type:ShaderForge.SFN_Dot,id:12,x:31347,y:34576,varname:node_12,prsc:2,dt:4|A-8582-OUT,B-9589-OUT;n:type:ShaderForge.SFN_Multiply,id:7564,x:30956,y:34341,varname:node_7564,prsc:2|A-6307-OUT,B-710-OUT;n:type:ShaderForge.SFN_Slider,id:8047,x:31147,y:34896,ptovrint:False,ptlb:ToonWidth,ptin:_ToonWidth,varname:_ToonWidth,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1.3,max:2;n:type:ShaderForge.SFN_LightColor,id:2792,x:32683,y:33299,varname:node_2792,prsc:2;n:type:ShaderForge.SFN_Vector2,id:8124,x:30840,y:34605,varname:node_8124,prsc:2,v1:1,v2:0;n:type:ShaderForge.SFN_Multiply,id:8582,x:31179,y:34398,varname:node_8582,prsc:2|A-7564-OUT,B-8124-OUT;n:type:ShaderForge.SFN_Add,id:5276,x:33335,y:32614,varname:node_5276,prsc:2|A-5001-RGB,B-6303-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6303,x:33117,y:32725,ptovrint:False,ptlb:Flash,ptin:_Flash,varname:_Flash,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:5001-7548-4756-1819-8047-3451-2526-6303;pass:END;sub:END;*/

Shader "ZD_Shader/Characters/Toon_Emission_Rim_Flash" {
    Properties {
        _MainTex ("DiffuseTex", 2D) = "white" {}
        [MaterialToggle] _ToonOn ("ToonOn", Float ) = 1
        _ToonTex ("ToonTex", 2D) = "white" {}
        _ToonColor ("ToonColor", Color) = (0.8382353,0.6903114,0.7668238,1)
        _ToonWidth ("ToonWidth", Range(0, 2)) = 1.3
        [MaterialToggle] _RimOn ("RimOn", Float ) = 1
        _RimWidth ("RimWidth", Range(0, 1)) = 0.7
        _Flash ("Flash", Float ) = 0
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
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _ToonTex; uniform float4 _ToonTex_ST;
            uniform fixed _RimOn;
            uniform float _RimWidth;
            uniform fixed _ToonOn;
            uniform float4 _ToonColor;
            uniform float _ToonWidth;
            uniform float _Flash;
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
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 emissive = (_MainTex_var.rgb+_Flash);
                float2 node_2798 = float2((0.5*dot(((i.normalDir*lightDirection)*float3(float2(1,0),0.0)),(1.0-max(0,dot(normalDirection, viewDirection))))+0.5*_ToonWidth),0.0);
                float4 _ToonTex_var = tex2D(_ToonTex,TRANSFORM_TEX(node_2798, _ToonTex));
                float3 finalColor = emissive + ((((_MainTex_var.rgb*((1.0 - _ToonColor.rgb)*(-1.0)))*_ToonTex_var.rgb)*_ToonOn)+(step(_RimWidth,(1.0-max(0,dot(normalDirection, viewDirection))))*max(0,dot(lightDirection,i.normalDir))*_LightColor0.rgb*_RimOn));
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Mobile/Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
