// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:Toon/Basic,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2865,x:34868,y:31951,varname:node_2865,prsc:2|normal-7223-RGB,custl-6221-OUT;n:type:ShaderForge.SFN_Tex2d,id:4952,x:31414,y:32308,ptovrint:True,ptlb:BaseTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1339-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:7223,x:34250,y:32039,ptovrint:True,ptlb:NormalMap,ptin:_BumpMap,varname:_BumpMap,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_NormalVector,id:9757,x:31459,y:33994,prsc:2,pt:True;n:type:ShaderForge.SFN_LightVector,id:5704,x:31459,y:34134,varname:node_5704,prsc:2;n:type:ShaderForge.SFN_Dot,id:2771,x:31770,y:34187,varname:node_2771,prsc:2,dt:4|A-9757-OUT,B-5704-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:9958,x:32146,y:34484,varname:node_9958,prsc:2|IN-6767-OUT,IMIN-4989-OUT,IMAX-1246-OUT,OMIN-9315-OUT,OMAX-5670-OUT;n:type:ShaderForge.SFN_OneMinus,id:6767,x:31992,y:34230,varname:node_6767,prsc:2|IN-2771-OUT;n:type:ShaderForge.SFN_Lerp,id:3454,x:31894,y:32354,varname:node_3454,prsc:2|A-8793-OUT,B-7859-OUT,T-9954-OUT;n:type:ShaderForge.SFN_Clamp01,id:9954,x:32327,y:34484,varname:node_9954,prsc:2|IN-9958-OUT;n:type:ShaderForge.SFN_LightColor,id:7872,x:31084,y:32063,varname:node_7872,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7859,x:31659,y:32594,varname:node_7859,prsc:2|A-4952-RGB,B-2440-OUT;n:type:ShaderForge.SFN_Color,id:9038,x:31414,y:32095,ptovrint:True,ptlb:BaseColor,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5019608,c2:0.5019608,c3:0.5019608,c4:1;n:type:ShaderForge.SFN_Add,id:4029,x:33793,y:31769,varname:node_4029,prsc:2|A-8472-OUT,B-3454-OUT,C-5805-OUT,D-6541-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2740,x:33453,y:32333,ptovrint:False,ptlb:Flash,ptin:_Flash,varname:_Flash,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_TexCoord,id:1339,x:31151,y:32286,varname:node_1339,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_OneMinus,id:9315,x:31991,y:34814,varname:node_9315,prsc:2|IN-5670-OUT;n:type:ShaderForge.SFN_Add,id:4989,x:31900,y:34512,varname:node_4989,prsc:2|A-1246-OUT,B-5670-OUT;n:type:ShaderForge.SFN_Slider,id:5670,x:31580,y:34815,ptovrint:False,ptlb:TerminatorSharpness,ptin:_TerminatorSharpness,varname:_TerminatorSharpness,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.0001,cur:0.0001,max:0.5;n:type:ShaderForge.SFN_Tex2d,id:962,x:31407,y:34362,ptovrint:False,ptlb:ESSGMask,ptin:_ESSGMask,cmnt:R發光遮罩  G陰影遮罩 控制陰影拉扯速率_50度灰中間值AO合併進這  B外邊框遮罩 ,varname:_ESSGMask,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1339-UVOUT;n:type:ShaderForge.SFN_Color,id:2003,x:31854,y:33287,ptovrint:False,ptlb:EmissionColor,ptin:_EmissionColor,varname:_EmissionColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_SwitchProperty,id:6079,x:31854,y:33503,ptovrint:False,ptlb:Emission x Base,ptin:_EmissionxBase,varname:_EmissionxBase,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-962-R,B-5761-OUT;n:type:ShaderForge.SFN_Multiply,id:5761,x:31653,y:33523,varname:node_5761,prsc:2|A-4952-RGB,B-962-R;n:type:ShaderForge.SFN_Multiply,id:8793,x:31686,y:32354,varname:node_8793,prsc:2|A-9895-OUT,B-7872-RGB;n:type:ShaderForge.SFN_Multiply,id:5805,x:32021,y:33441,varname:node_5805,prsc:2|A-2003-RGB,B-6079-OUT,C-1405-OUT;n:type:ShaderForge.SFN_ToggleProperty,id:1405,x:31854,y:33701,ptovrint:False,ptlb:EmissionOn,ptin:_EmissionOn,varname:_EmissionOn,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False;n:type:ShaderForge.SFN_Add,id:1246,x:31634,y:34508,varname:node_1246,prsc:2|A-962-G,B-1846-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1846,x:31419,y:34588,ptovrint:True,ptlb:ShadowRange,ptin:_ShadowRange,varname:_ShadowRange,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.85;n:type:ShaderForge.SFN_Multiply,id:9895,x:31686,y:32162,varname:node_9895,prsc:2|A-9038-RGB,B-4952-RGB;n:type:ShaderForge.SFN_Slider,id:3816,x:29716,y:31248,ptovrint:True,ptlb:Gloss (texture=1),ptin:_Gloss,varname:_Gloss,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Color,id:7338,x:30867,y:31220,ptovrint:False,ptlb:SpecularColor,ptin:_SpecularColor,varname:_SpecularColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Add,id:6221,x:34245,y:31744,varname:node_6221,prsc:2|A-6833-OUT,B-4029-OUT;n:type:ShaderForge.SFN_Dot,id:8939,x:33747,y:31277,varname:node_8939,prsc:2,dt:1|A-9757-OUT,B-5704-OUT;n:type:ShaderForge.SFN_Multiply,id:6833,x:33948,y:31240,varname:node_6833,prsc:2|A-8125-OUT,B-3754-OUT,C-8939-OUT;n:type:ShaderForge.SFN_Step,id:3754,x:33529,y:30747,varname:node_3754,prsc:2|A-4919-OUT,B-2602-OUT;n:type:ShaderForge.SFN_Fresnel,id:2602,x:33328,y:30799,varname:node_2602,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:3284,x:33164,y:30654,ptovrint:False,ptlb:RimWidth,ptin:_RimWidth,varname:_RimWidth,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.3;n:type:ShaderForge.SFN_OneMinus,id:4919,x:33328,y:30637,varname:node_4919,prsc:2|IN-3284-OUT;n:type:ShaderForge.SFN_Multiply,id:8125,x:33519,y:30441,varname:node_8125,prsc:2|A-7872-RGB,B-3223-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3223,x:33333,y:30460,ptovrint:False,ptlb:RimIntensity,ptin:_RimIntensity,varname:_RimIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.1;n:type:ShaderForge.SFN_Fresnel,id:4616,x:33453,y:32427,varname:node_4616,prsc:2;n:type:ShaderForge.SFN_Vector3,id:8775,x:33453,y:32165,varname:node_8775,prsc:2,v1:1,v2:0.3171664,v3:0.2549019;n:type:ShaderForge.SFN_Multiply,id:6541,x:33668,y:32333,varname:node_6541,prsc:2|A-8775-OUT,B-4616-OUT,C-2740-OUT;n:type:ShaderForge.SFN_Posterize,id:6421,x:30850,y:31063,varname:node_6421,prsc:2|IN-6957-OUT,STPS-1292-OUT;n:type:ShaderForge.SFN_Vector1,id:1292,x:30657,y:31214,varname:node_1292,prsc:2,v1:2;n:type:ShaderForge.SFN_Lerp,id:8472,x:31263,y:31040,varname:node_8472,prsc:2|A-9929-OUT,B-6421-OUT,T-7038-OUT;n:type:ShaderForge.SFN_Vector1,id:9929,x:31059,y:30887,varname:node_9929,prsc:2,v1:0;n:type:ShaderForge.SFN_Multiply,id:7038,x:31049,y:31318,varname:node_7038,prsc:2|A-7338-RGB,B-962-B;n:type:ShaderForge.SFN_Multiply,id:5190,x:30131,y:31370,varname:node_5190,prsc:2|A-3816-OUT,B-962-A;n:type:ShaderForge.SFN_Power,id:6957,x:30647,y:31063,varname:node_6957,prsc:2|VAL-6180-OUT,EXP-8703-OUT;n:type:ShaderForge.SFN_Exp,id:8703,x:30493,y:31152,varname:node_8703,prsc:2,et:1|IN-3089-OUT;n:type:ShaderForge.SFN_ConstantLerp,id:3089,x:30298,y:31152,varname:node_3089,prsc:2,a:1,b:11|IN-5190-OUT;n:type:ShaderForge.SFN_HalfVector,id:484,x:30175,y:31036,varname:node_484,prsc:2;n:type:ShaderForge.SFN_Dot,id:6180,x:30346,y:30994,cmnt:Blinn-Phong,varname:node_6180,prsc:2,dt:1|A-9757-OUT,B-484-OUT;n:type:ShaderForge.SFN_AmbientLight,id:6903,x:31118,y:32641,varname:node_6903,prsc:2;n:type:ShaderForge.SFN_HsvToRgb,id:801,x:30989,y:32614,varname:node_801,prsc:2|H-5429-HOUT,S-7952-OUT,V-5429-VOUT;n:type:ShaderForge.SFN_RgbToHsv,id:5429,x:30678,y:32614,varname:node_5429,prsc:2|IN-4952-RGB;n:type:ShaderForge.SFN_Multiply,id:7952,x:30846,y:32773,varname:node_7952,prsc:2|A-5429-SOUT,B-8210-OUT;n:type:ShaderForge.SFN_Vector1,id:8210,x:30626,y:32822,varname:node_8210,prsc:2,v1:3.5;n:type:ShaderForge.SFN_Lerp,id:2440,x:31356,y:32619,varname:node_2440,prsc:2|A-801-OUT,B-6903-RGB,T-5254-OUT;n:type:ShaderForge.SFN_Vector1,id:5254,x:31108,y:32813,varname:node_5254,prsc:2,v1:0.7;proporder:9038-4952-962-1405-2003-6079-1846-5670-7338-3816-7223-3223-3284-2740;pass:END;sub:END;*/

Shader "ZD_Shader/Characters/ZDSPToon_Mobile_ForU2017.3" {
    Properties {
        [HDR]_Color ("BaseColor", Color) = (0.5019608,0.5019608,0.5019608,1)
        _MainTex ("BaseTex", 2D) = "white" {}
        _ESSGMask ("ESSGMask", 2D) = "white" {}
        [MaterialToggle] _EmissionOn ("EmissionOn", Float ) = 0
        [HDR]_EmissionColor ("EmissionColor", Color) = (1,1,1,1)
        [MaterialToggle] _EmissionxBase ("Emission x Base", Float ) = 0
        _ShadowRange ("ShadowRange", Float ) = 0.85
        _TerminatorSharpness ("TerminatorSharpness", Range(0.0001, 0.5)) = 0.0001
        [HDR]_SpecularColor ("SpecularColor", Color) = (0,0,0,1)
        _Gloss ("Gloss (texture=1)", Range(0, 1)) = 1
        _BumpMap ("NormalMap", 2D) = "bump" {}
        _RimIntensity ("RimIntensity", Float ) = 0.1
        _RimWidth ("RimWidth", Float ) = 0.3
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
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float4 _Color;
            uniform float _Flash;
            uniform float _TerminatorSharpness;
            uniform sampler2D _ESSGMask; uniform float4 _ESSGMask_ST;
            uniform float4 _EmissionColor;
            uniform fixed _EmissionxBase;
            uniform fixed _EmissionOn;
            uniform float _ShadowRange;
            uniform float _Gloss;
            uniform float4 _SpecularColor;
            uniform float _RimWidth;
            uniform float _RimIntensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap)));
                float3 normalLocal = _BumpMap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float node_9929 = 0.0;
                float4 _ESSGMask_var = tex2D(_ESSGMask,TRANSFORM_TEX(i.uv0, _ESSGMask)); // R發光遮罩  G陰影遮罩 控制陰影拉扯速率_50度灰中間值AO合併進這  B外邊框遮罩 
                float node_1292 = 2.0;
                float node_6421 = floor(pow(max(0,dot(normalDirection,halfDirection)),exp2(lerp(1,11,(_Gloss*_ESSGMask_var.a)))) * node_1292) / (node_1292 - 1);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 node_5429_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_5429_p = lerp(float4(float4(_MainTex_var.rgb,0.0).zy, node_5429_k.wz), float4(float4(_MainTex_var.rgb,0.0).yz, node_5429_k.xy), step(float4(_MainTex_var.rgb,0.0).z, float4(_MainTex_var.rgb,0.0).y));
                float4 node_5429_q = lerp(float4(node_5429_p.xyw, float4(_MainTex_var.rgb,0.0).x), float4(float4(_MainTex_var.rgb,0.0).x, node_5429_p.yzx), step(node_5429_p.x, float4(_MainTex_var.rgb,0.0).x));
                float node_5429_d = node_5429_q.x - min(node_5429_q.w, node_5429_q.y);
                float node_5429_e = 1.0e-10;
                float3 node_5429 = float3(abs(node_5429_q.z + (node_5429_q.w - node_5429_q.y) / (6.0 * node_5429_d + node_5429_e)), node_5429_d / (node_5429_q.x + node_5429_e), node_5429_q.x);;
                float node_1246 = (_ESSGMask_var.g+_ShadowRange);
                float node_4989 = (node_1246+_TerminatorSharpness);
                float node_9315 = (1.0 - _TerminatorSharpness);
                float3 finalColor = (((_LightColor0.rgb*_RimIntensity)*step((1.0 - _RimWidth),(1.0-max(0,dot(normalDirection, viewDirection))))*max(0,dot(normalDirection,lightDirection)))+(lerp(float3(node_9929,node_9929,node_9929),float3(node_6421,node_6421,node_6421),(_SpecularColor.rgb*_ESSGMask_var.b))+lerp(((_Color.rgb*_MainTex_var.rgb)*_LightColor0.rgb),(_MainTex_var.rgb*lerp((lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(node_5429.r+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(node_5429.g*3.5))*node_5429.b),UNITY_LIGHTMODEL_AMBIENT.rgb,0.7)),saturate((node_9315 + ( ((1.0 - 0.5*dot(normalDirection,lightDirection)+0.5) - node_4989) * (_TerminatorSharpness - node_9315) ) / (node_1246 - node_4989))))+(_EmissionColor.rgb*lerp( _ESSGMask_var.r, (_MainTex_var.rgb*_ESSGMask_var.r), _EmissionxBase )*_EmissionOn)+(float3(1,0.3171664,0.2549019)*(1.0-max(0,dot(normalDirection, viewDirection)))*_Flash)));
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
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float4 _Color;
            uniform float _Flash;
            uniform float _TerminatorSharpness;
            uniform sampler2D _ESSGMask; uniform float4 _ESSGMask_ST;
            uniform float4 _EmissionColor;
            uniform fixed _EmissionxBase;
            uniform fixed _EmissionOn;
            uniform float _ShadowRange;
            uniform float _Gloss;
            uniform float4 _SpecularColor;
            uniform float _RimWidth;
            uniform float _RimIntensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap)));
                float3 normalLocal = _BumpMap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float node_9929 = 0.0;
                float4 _ESSGMask_var = tex2D(_ESSGMask,TRANSFORM_TEX(i.uv0, _ESSGMask)); // R發光遮罩  G陰影遮罩 控制陰影拉扯速率_50度灰中間值AO合併進這  B外邊框遮罩 
                float node_1292 = 2.0;
                float node_6421 = floor(pow(max(0,dot(normalDirection,halfDirection)),exp2(lerp(1,11,(_Gloss*_ESSGMask_var.a)))) * node_1292) / (node_1292 - 1);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 node_5429_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_5429_p = lerp(float4(float4(_MainTex_var.rgb,0.0).zy, node_5429_k.wz), float4(float4(_MainTex_var.rgb,0.0).yz, node_5429_k.xy), step(float4(_MainTex_var.rgb,0.0).z, float4(_MainTex_var.rgb,0.0).y));
                float4 node_5429_q = lerp(float4(node_5429_p.xyw, float4(_MainTex_var.rgb,0.0).x), float4(float4(_MainTex_var.rgb,0.0).x, node_5429_p.yzx), step(node_5429_p.x, float4(_MainTex_var.rgb,0.0).x));
                float node_5429_d = node_5429_q.x - min(node_5429_q.w, node_5429_q.y);
                float node_5429_e = 1.0e-10;
                float3 node_5429 = float3(abs(node_5429_q.z + (node_5429_q.w - node_5429_q.y) / (6.0 * node_5429_d + node_5429_e)), node_5429_d / (node_5429_q.x + node_5429_e), node_5429_q.x);;
                float node_1246 = (_ESSGMask_var.g+_ShadowRange);
                float node_4989 = (node_1246+_TerminatorSharpness);
                float node_9315 = (1.0 - _TerminatorSharpness);
                float3 finalColor = (((_LightColor0.rgb*_RimIntensity)*step((1.0 - _RimWidth),(1.0-max(0,dot(normalDirection, viewDirection))))*max(0,dot(normalDirection,lightDirection)))+(lerp(float3(node_9929,node_9929,node_9929),float3(node_6421,node_6421,node_6421),(_SpecularColor.rgb*_ESSGMask_var.b))+lerp(((_Color.rgb*_MainTex_var.rgb)*_LightColor0.rgb),(_MainTex_var.rgb*lerp((lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac(node_5429.r+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(node_5429.g*3.5))*node_5429.b),UNITY_LIGHTMODEL_AMBIENT.rgb,0.7)),saturate((node_9315 + ( ((1.0 - 0.5*dot(normalDirection,lightDirection)+0.5) - node_4989) * (_TerminatorSharpness - node_9315) ) / (node_1246 - node_4989))))+(_EmissionColor.rgb*lerp( _ESSGMask_var.r, (_MainTex_var.rgb*_ESSGMask_var.r), _EmissionxBase )*_EmissionOn)+(float3(1,0.3171664,0.2549019)*(1.0-max(0,dot(normalDirection, viewDirection)))*_Flash)));
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Toon/Basic"
    CustomEditor "ShaderForgeMaterialInspector"
}
