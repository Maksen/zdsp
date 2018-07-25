// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Mobile/Diffuse,iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:9361,x:33936,y:32851,varname:node_9361,prsc:2|emission-5001-RGB,custl-3604-OUT,olwid-6061-OUT,olcol-6310-OUT;n:type:ShaderForge.SFN_Tex2d,id:5001,x:31814,y:31651,ptovrint:True,ptlb:DiffuseTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_LightVector,id:710,x:30777,y:32523,varname:node_710,prsc:2;n:type:ShaderForge.SFN_NormalVector,id:6307,x:30777,y:32651,prsc:2,pt:False;n:type:ShaderForge.SFN_Dot,id:512,x:31074,y:32577,varname:node_512,prsc:2,dt:4|A-9704-OUT,B-6307-OUT;n:type:ShaderForge.SFN_Vector1,id:7173,x:31074,y:32739,varname:node_7173,prsc:2,v1:0;n:type:ShaderForge.SFN_Append,id:1219,x:31248,y:32658,varname:node_1219,prsc:2|A-512-OUT,B-7173-OUT;n:type:ShaderForge.SFN_Tex2d,id:4756,x:31428,y:32658,ptovrint:True,ptlb:ToonTex,ptin:_ToonTex,varname:_ToonTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1219-OUT;n:type:ShaderForge.SFN_Fresnel,id:1261,x:32389,y:33356,cmnt:邊緣光顯示2段,varname:node_1261,prsc:2;n:type:ShaderForge.SFN_ToggleProperty,id:3451,x:32610,y:33895,ptovrint:False,ptlb:RimOn,ptin:_RimOn,varname:_RimOn,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True;n:type:ShaderForge.SFN_Slider,id:2526,x:32310,y:33225,ptovrint:False,ptlb:RimWidth,ptin:_RimWidth,varname:_RimWidth,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.7,max:1;n:type:ShaderForge.SFN_Add,id:3604,x:33188,y:33059,varname:node_3604,prsc:2|A-7022-OUT,B-4947-OUT;n:type:ShaderForge.SFN_Dot,id:1767,x:32139,y:33479,varname:node_1767,prsc:2,dt:1|A-710-OUT,B-1710-OUT;n:type:ShaderForge.SFN_Multiply,id:1710,x:31949,y:33499,varname:node_1710,prsc:2|A-6307-OUT,B-3743-RGB;n:type:ShaderForge.SFN_Multiply,id:4947,x:32854,y:33459,varname:node_4947,prsc:2|A-7794-OUT,B-1767-OUT,C-597-OUT,D-3451-OUT;n:type:ShaderForge.SFN_Color,id:3743,x:31681,y:33521,ptovrint:False,ptlb:RimAngle,ptin:_RimAngle,varname:_RimAngle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Step,id:7794,x:32618,y:33309,varname:node_7794,prsc:2|A-2526-OUT,B-1261-OUT;n:type:ShaderForge.SFN_Multiply,id:1323,x:32475,y:32608,varname:node_1323,prsc:2|A-4349-OUT,B-4756-RGB;n:type:ShaderForge.SFN_Multiply,id:4349,x:32083,y:32042,varname:node_4349,prsc:2|A-5001-RGB,B-985-OUT;n:type:ShaderForge.SFN_ToggleProperty,id:7548,x:32475,y:32806,ptovrint:False,ptlb:ToonOn,ptin:_ToonOn,varname:_ToonOn,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True;n:type:ShaderForge.SFN_Multiply,id:7022,x:32745,y:32720,varname:node_7022,prsc:2|A-1323-OUT,B-7548-OUT;n:type:ShaderForge.SFN_Color,id:9228,x:32449,y:33616,ptovrint:False,ptlb:RimColor,ptin:_RimColor,varname:_RimColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:597,x:32610,y:33682,varname:node_597,prsc:2|A-9228-RGB,B-506-OUT;n:type:ShaderForge.SFN_Vector1,id:506,x:32449,y:33826,varname:node_506,prsc:2,v1:2;n:type:ShaderForge.SFN_Slider,id:3456,x:33310,y:33471,ptovrint:False,ptlb:OutlineWidth,ptin:_OutlineWidth,varname:_OutlineWidth,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.005,max:1;n:type:ShaderForge.SFN_Color,id:1485,x:33455,y:33617,ptovrint:False,ptlb:OutlineColor,ptin:_OutlineColor,varname:_OutlineColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:6310,x:33689,y:33617,varname:node_6310,prsc:2|A-1485-RGB,B-3596-OUT;n:type:ShaderForge.SFN_ToggleProperty,id:3596,x:33455,y:33842,ptovrint:False,ptlb:OutlineOn,ptin:_OutlineOn,varname:_OutlineOn,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False;n:type:ShaderForge.SFN_Multiply,id:6061,x:33689,y:33469,varname:node_6061,prsc:2|A-3456-OUT,B-3596-OUT;n:type:ShaderForge.SFN_Color,id:1819,x:31482,y:31940,ptovrint:False,ptlb:ToonColor,ptin:_ToonColor,varname:_ToonColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.8382353,c2:0.6903114,c3:0.7668238,c4:1;n:type:ShaderForge.SFN_Multiply,id:985,x:31802,y:32066,varname:node_985,prsc:2|A-5933-OUT,B-6204-OUT;n:type:ShaderForge.SFN_Vector1,id:6204,x:31482,y:32144,varname:node_6204,prsc:2,v1:-1;n:type:ShaderForge.SFN_OneMinus,id:5933,x:31650,y:31940,varname:node_5933,prsc:2|IN-1819-RGB;n:type:ShaderForge.SFN_Multiply,id:9704,x:30921,y:32240,varname:node_9704,prsc:2|A-1225-RGB,B-710-OUT;n:type:ShaderForge.SFN_Color,id:1225,x:30698,y:32140,ptovrint:False,ptlb:ToonAngle,ptin:_ToonAngle,varname:_ToonAngle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5125351,c2:0,c3:1,c4:1;proporder:5001-7548-4756-1819-1225-3451-9228-2526-3743-3596-3456-1485;pass:END;sub:END;*/

Shader "ZD_Shader/TEST/TEST_Toon_Emission_04" {
    Properties {
        _MainTex ("DiffuseTex", 2D) = "white" {}
        [MaterialToggle] _ToonOn ("ToonOn", Float ) = 1
        _ToonTex ("ToonTex", 2D) = "white" {}
        _ToonColor ("ToonColor", Color) = (0.8382353,0.6903114,0.7668238,1)
        _ToonAngle ("ToonAngle", Color) = (0.5125351,0,1,1)
        [MaterialToggle] _RimOn ("RimOn", Float ) = 1
        _RimColor ("RimColor", Color) = (1,1,1,1)
        _RimWidth ("RimWidth", Range(0, 1)) = 0.7
        _RimAngle ("RimAngle", Color) = (1,0,0,1)
        [MaterialToggle] _OutlineOn ("OutlineOn", Float ) = 0
        _OutlineWidth ("OutlineWidth", Range(0, 1)) = 0.005
        _OutlineColor ("OutlineColor", Color) = (0,0,0,1)
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "Outline"
            Tags {
            }
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float _OutlineWidth;
            uniform float4 _OutlineColor;
            uniform fixed _OutlineOn;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_FOG_COORDS(0)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + v.normal*(_OutlineWidth*_OutlineOn),1) );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                return fixed4((_OutlineColor.rgb*_OutlineOn),0);
            }
            ENDCG
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
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _ToonTex; uniform float4 _ToonTex_ST;
            uniform fixed _RimOn;
            uniform float _RimWidth;
            uniform float4 _RimAngle;
            uniform fixed _ToonOn;
            uniform float4 _RimColor;
            uniform float4 _ToonColor;
            uniform float4 _ToonAngle;
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
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 emissive = _MainTex_var.rgb;
                float2 node_1219 = float2(0.5*dot((_ToonAngle.rgb*lightDirection),i.normalDir)+0.5,0.0);
                float4 _ToonTex_var = tex2D(_ToonTex,TRANSFORM_TEX(node_1219, _ToonTex));
                float3 finalColor = emissive + ((((_MainTex_var.rgb*((1.0 - _ToonColor.rgb)*(-1.0)))*_ToonTex_var.rgb)*_ToonOn)+(step(_RimWidth,(1.0-max(0,dot(normalDirection, viewDirection))))*max(0,dot(lightDirection,(i.normalDir*_RimAngle.rgb)))*(_RimColor.rgb*2.0)*_RimOn));
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
