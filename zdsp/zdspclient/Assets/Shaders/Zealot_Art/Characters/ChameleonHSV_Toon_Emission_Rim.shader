// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Mobile/Diffuse,iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:9361,x:31660,y:31705,varname:node_9361,prsc:2|emission-7724-OUT,custl-3604-OUT;n:type:ShaderForge.SFN_Tex2d,id:5001,x:30317,y:31451,ptovrint:True,ptlb:EmissionTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-8337-UVOUT;n:type:ShaderForge.SFN_LightVector,id:710,x:30496,y:34929,varname:node_710,prsc:2;n:type:ShaderForge.SFN_NormalVector,id:6307,x:30496,y:34764,prsc:2,pt:False;n:type:ShaderForge.SFN_Tex2d,id:4756,x:31571,y:35368,ptovrint:True,ptlb:ToonTex,ptin:_ToonTex,varname:_ToonTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:640eb59cb367f484e89f835e517e2eea,ntxv:0,isnm:False|UVIN-2798-OUT;n:type:ShaderForge.SFN_Fresnel,id:1261,x:32437,y:33001,cmnt:邊緣光顯示2段,varname:node_1261,prsc:2;n:type:ShaderForge.SFN_ToggleProperty,id:3451,x:32686,y:33467,ptovrint:False,ptlb:RimOn,ptin:_RimOn,varname:_RimOn,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True;n:type:ShaderForge.SFN_Slider,id:2526,x:32358,y:32861,ptovrint:False,ptlb:RimWidth,ptin:_RimWidth,varname:_RimWidth,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.7,max:1;n:type:ShaderForge.SFN_Add,id:3604,x:33188,y:33059,varname:node_3604,prsc:2|A-7022-OUT,B-4947-OUT;n:type:ShaderForge.SFN_Dot,id:1767,x:32683,y:33124,varname:node_1767,prsc:2,dt:1|A-710-OUT,B-6307-OUT;n:type:ShaderForge.SFN_Multiply,id:4947,x:32892,y:33084,cmnt:rim,varname:node_4947,prsc:2|A-7794-OUT,B-1767-OUT,C-2792-RGB,D-3451-OUT;n:type:ShaderForge.SFN_Step,id:7794,x:32670,y:32963,varname:node_7794,prsc:2|A-2526-OUT,B-1261-OUT;n:type:ShaderForge.SFN_Multiply,id:1323,x:31845,y:34927,varname:node_1323,prsc:2|A-4349-OUT,B-4756-RGB;n:type:ShaderForge.SFN_Multiply,id:4349,x:31588,y:34566,varname:node_4349,prsc:2|A-5001-RGB,B-985-OUT;n:type:ShaderForge.SFN_ToggleProperty,id:7548,x:31845,y:35125,ptovrint:False,ptlb:ToonOn,ptin:_ToonOn,varname:_ToonOn,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True;n:type:ShaderForge.SFN_Multiply,id:7022,x:32115,y:35039,cmnt:toon,varname:node_7022,prsc:2|A-1323-OUT,B-7548-OUT;n:type:ShaderForge.SFN_Color,id:1819,x:31080,y:34463,ptovrint:False,ptlb:ToonColor,ptin:_ToonColor,varname:_ToonColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.8382353,c2:0.6903114,c3:0.7668238,c4:1;n:type:ShaderForge.SFN_Multiply,id:985,x:31400,y:34589,varname:node_985,prsc:2|A-5933-OUT,B-6204-OUT;n:type:ShaderForge.SFN_Vector1,id:6204,x:31080,y:34667,varname:node_6204,prsc:2,v1:-1;n:type:ShaderForge.SFN_OneMinus,id:5933,x:31248,y:34463,varname:node_5933,prsc:2|IN-1819-RGB;n:type:ShaderForge.SFN_Fresnel,id:9589,x:30763,y:35158,varname:node_9589,prsc:2;n:type:ShaderForge.SFN_Append,id:2798,x:31395,y:35325,varname:node_2798,prsc:2|A-7693-OUT,B-1472-OUT;n:type:ShaderForge.SFN_Vector1,id:1472,x:31230,y:35436,varname:node_1472,prsc:2,v1:0;n:type:ShaderForge.SFN_Multiply,id:7693,x:31230,y:35273,varname:node_7693,prsc:2|A-12-OUT,B-8047-OUT;n:type:ShaderForge.SFN_Dot,id:12,x:31066,y:35147,varname:node_12,prsc:2,dt:4|A-8582-OUT,B-9589-OUT;n:type:ShaderForge.SFN_Multiply,id:7564,x:30675,y:34912,varname:node_7564,prsc:2|A-6307-OUT,B-710-OUT;n:type:ShaderForge.SFN_Slider,id:8047,x:30866,y:35467,ptovrint:False,ptlb:ToonWidth,ptin:_ToonWidth,varname:_ToonWidth,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1.3,max:2;n:type:ShaderForge.SFN_LightColor,id:2792,x:32683,y:33299,varname:node_2792,prsc:2;n:type:ShaderForge.SFN_Vector2,id:8124,x:30559,y:35176,varname:node_8124,prsc:2,v1:1,v2:0;n:type:ShaderForge.SFN_Multiply,id:8582,x:30898,y:34969,varname:node_8582,prsc:2|A-7564-OUT,B-8124-OUT;n:type:ShaderForge.SFN_HsvToRgb,id:8670,x:31153,y:31671,varname:node_8670,prsc:2|H-4968-OUT,S-9038-OUT,V-3342-OUT;n:type:ShaderForge.SFN_RgbToHsv,id:4406,x:30502,y:31485,varname:node_4406,prsc:2|IN-5001-RGB;n:type:ShaderForge.SFN_Time,id:4487,x:29454,y:31572,varname:node_4487,prsc:2;n:type:ShaderForge.SFN_RgbToHsv,id:5811,x:30502,y:31721,cmnt:用Chameleon貼圖去更改Emsiion顏色,varname:node_5811,prsc:2|IN-7096-RGB;n:type:ShaderForge.SFN_Multiply,id:9038,x:30897,y:31687,varname:node_9038,prsc:2|A-4406-SOUT,B-5811-SOUT;n:type:ShaderForge.SFN_ObjectPosition,id:2302,x:29450,y:31960,cmnt:以X_Z軸散列,varname:node_2302,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:7096,x:30304,y:31714,ptovrint:False,ptlb:ChameleonHSVTex,ptin:_ChameleonHSVTex,varname:_ChameleonHSVTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-3863-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:8337,x:29676,y:31509,varname:node_8337,prsc:2,uv:0;n:type:ShaderForge.SFN_ValueProperty,id:5885,x:29450,y:31788,ptovrint:False,ptlb:ChaneleonFlowSpeed,ptin:_ChaneleonFlowSpeed,varname:_ChaneleonFlowSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:5;n:type:ShaderForge.SFN_Multiply,id:2730,x:29676,y:31681,varname:node_2730,prsc:2|A-4487-T,B-5885-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1411,x:29450,y:32126,ptovrint:False,ptlb:Chameleon X&Z HashDistance,ptin:_ChameleonXZHashDistance,varname:_ChameleonXZHashDistance,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Multiply,id:8506,x:29659,y:31981,varname:node_8506,prsc:2|A-2302-X,B-2302-Z,C-1411-OUT;n:type:ShaderForge.SFN_Add,id:5115,x:29887,y:31661,varname:node_5115,prsc:2|A-8337-UVOUT,B-8506-OUT;n:type:ShaderForge.SFN_Panner,id:3863,x:30095,y:31714,varname:node_3863,prsc:2,spu:0,spv:10|UVIN-5115-OUT,DIST-2730-OUT;n:type:ShaderForge.SFN_Tex2d,id:749,x:31103,y:32140,ptovrint:False,ptlb:ChameleonMask,ptin:_ChameleonMask,varname:_ChameleonMask,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False|UVIN-8337-UVOUT;n:type:ShaderForge.SFN_Lerp,id:7724,x:31373,y:31804,varname:node_7724,prsc:2|A-8670-OUT,B-5001-RGB,T-749-R;n:type:ShaderForge.SFN_Multiply,id:3342,x:30897,y:31817,varname:node_3342,prsc:2|A-4406-VOUT,B-5811-VOUT;n:type:ShaderForge.SFN_Add,id:4968,x:30897,y:31553,varname:node_4968,prsc:2|A-4406-HOUT,B-5811-HOUT;proporder:5001-7548-4756-1819-8047-3451-2526-7096-749-5885-1411;pass:END;sub:END;*/

Shader "ZD_Shader/Characters/ChameleonHSV_Toon_Emission_Rim" {
    Properties {
        _MainTex ("EmissionTex", 2D) = "white" {}
        [MaterialToggle] _ToonOn ("ToonOn", Float ) = 1
        _ToonTex ("ToonTex", 2D) = "white" {}
        _ToonColor ("ToonColor", Color) = (0.8382353,0.6903114,0.7668238,1)
        _ToonWidth ("ToonWidth", Range(0, 2)) = 1.3
        [MaterialToggle] _RimOn ("RimOn", Float ) = 1
        _RimWidth ("RimWidth", Range(0, 1)) = 0.7
        _ChameleonHSVTex ("ChameleonHSVTex", 2D) = "white" {}
        _ChameleonMask ("ChameleonMask", 2D) = "black" {}
        _ChaneleonFlowSpeed ("ChaneleonFlowSpeed", Float ) = 5
        _ChameleonXZHashDistance ("Chameleon X&Z HashDistance", Float ) = 2
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
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _ToonTex; uniform float4 _ToonTex_ST;
            uniform fixed _RimOn;
            uniform float _RimWidth;
            uniform fixed _ToonOn;
            uniform float4 _ToonColor;
            uniform float _ToonWidth;
            uniform sampler2D _ChameleonHSVTex; uniform float4 _ChameleonHSVTex_ST;
            uniform float _ChaneleonFlowSpeed;
            uniform float _ChameleonXZHashDistance;
            uniform sampler2D _ChameleonMask; uniform float4 _ChameleonMask_ST;
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
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 node_4406_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_4406_p = lerp(float4(float4(_MainTex_var.rgb,0.0).zy, node_4406_k.wz), float4(float4(_MainTex_var.rgb,0.0).yz, node_4406_k.xy), step(float4(_MainTex_var.rgb,0.0).z, float4(_MainTex_var.rgb,0.0).y));
                float4 node_4406_q = lerp(float4(node_4406_p.xyw, float4(_MainTex_var.rgb,0.0).x), float4(float4(_MainTex_var.rgb,0.0).x, node_4406_p.yzx), step(node_4406_p.x, float4(_MainTex_var.rgb,0.0).x));
                float node_4406_d = node_4406_q.x - min(node_4406_q.w, node_4406_q.y);
                float node_4406_e = 1.0e-10;
                float3 node_4406 = float3(abs(node_4406_q.z + (node_4406_q.w - node_4406_q.y) / (6.0 * node_4406_d + node_4406_e)), node_4406_d / (node_4406_q.x + node_4406_e), node_4406_q.x);;
                float4 node_4487 = _Time + _TimeEditor;
                float2 node_3863 = ((i.uv0+(objPos.r*objPos.b*_ChameleonXZHashDistance))+(node_4487.g*_ChaneleonFlowSpeed)*float2(0,10));
                float4 _ChameleonHSVTex_var = tex2D(_ChameleonHSVTex,TRANSFORM_TEX(node_3863, _ChameleonHSVTex));
                float4 node_5811_k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 node_5811_p = lerp(float4(float4(_ChameleonHSVTex_var.rgb,0.0).zy, node_5811_k.wz), float4(float4(_ChameleonHSVTex_var.rgb,0.0).yz, node_5811_k.xy), step(float4(_ChameleonHSVTex_var.rgb,0.0).z, float4(_ChameleonHSVTex_var.rgb,0.0).y));
                float4 node_5811_q = lerp(float4(node_5811_p.xyw, float4(_ChameleonHSVTex_var.rgb,0.0).x), float4(float4(_ChameleonHSVTex_var.rgb,0.0).x, node_5811_p.yzx), step(node_5811_p.x, float4(_ChameleonHSVTex_var.rgb,0.0).x));
                float node_5811_d = node_5811_q.x - min(node_5811_q.w, node_5811_q.y);
                float node_5811_e = 1.0e-10;
                float3 node_5811 = float3(abs(node_5811_q.z + (node_5811_q.w - node_5811_q.y) / (6.0 * node_5811_d + node_5811_e)), node_5811_d / (node_5811_q.x + node_5811_e), node_5811_q.x);; // 用Chameleon貼圖去更改Emsiion顏色
                float4 _ChameleonMask_var = tex2D(_ChameleonMask,TRANSFORM_TEX(i.uv0, _ChameleonMask));
                float3 emissive = lerp((lerp(float3(1,1,1),saturate(3.0*abs(1.0-2.0*frac((node_4406.r+node_5811.r)+float3(0.0,-1.0/3.0,1.0/3.0)))-1),(node_4406.g*node_5811.g))*(node_4406.b*node_5811.b)),_MainTex_var.rgb,_ChameleonMask_var.r);
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
