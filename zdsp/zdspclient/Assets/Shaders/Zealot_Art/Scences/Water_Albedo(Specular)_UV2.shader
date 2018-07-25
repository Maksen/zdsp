// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:0,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2865,x:32626,y:32964,varname:node_2865,prsc:2|spec-4153-OUT,emission-5009-OUT;n:type:ShaderForge.SFN_Tex2d,id:9707,x:31907,y:33144,varname:node_9707,prsc:2,ntxv:0,isnm:False|UVIN-8505-OUT,TEX-5270-TEX;n:type:ShaderForge.SFN_Tex2d,id:3430,x:31907,y:33326,varname:node_3430,prsc:2,ntxv:0,isnm:False|UVIN-7953-OUT,TEX-5270-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:5270,x:31596,y:33339,ptovrint:False,ptlb:Diffuse,ptin:_Diffuse,varname:_Diffuse,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:1,isnm:False;n:type:ShaderForge.SFN_Lerp,id:5009,x:32374,y:33270,varname:node_5009,prsc:2|A-536-OUT,B-9101-OUT,T-3960-OUT;n:type:ShaderForge.SFN_Slider,id:3960,x:31857,y:33785,ptovrint:False,ptlb:Blend streagth,ptin:_Blendstreagth,varname:_Blendstreagth,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_FragmentPosition,id:490,x:30734,y:32365,varname:node_490,prsc:2;n:type:ShaderForge.SFN_Append,id:111,x:30957,y:32389,varname:node_111,prsc:2|A-490-X,B-490-Z;n:type:ShaderForge.SFN_Divide,id:2815,x:31167,y:32389,varname:node_2815,prsc:2|A-111-OUT,B-5254-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5254,x:30957,y:32589,ptovrint:False,ptlb:UVScale,ptin:_UVScale,varname:_UVScale,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Set,id:5618,x:31341,y:32389,varname:_worldUV,prsc:2|IN-2815-OUT;n:type:ShaderForge.SFN_Get,id:7953,x:31596,y:33204,varname:node_7953,prsc:2|IN-3359-OUT;n:type:ShaderForge.SFN_Set,id:4244,x:30900,y:32830,varname:_UV1,prsc:2|IN-4663-OUT;n:type:ShaderForge.SFN_Set,id:3359,x:30900,y:33091,varname:_UV2,prsc:2|IN-6518-OUT;n:type:ShaderForge.SFN_Get,id:834,x:29725,y:32814,varname:node_834,prsc:2|IN-5618-OUT;n:type:ShaderForge.SFN_Vector4Property,id:9953,x:29999,y:32627,ptovrint:False,ptlb:UV1 Animator,ptin:_UV1Animator,varname:_UV1Animator,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_Vector4Property,id:8825,x:29999,y:33180,ptovrint:False,ptlb:UV2 Animator,ptin:_UV2Animator,varname:_UV2Animator,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0,v2:0,v3:0,v4:0;n:type:ShaderForge.SFN_ComponentMask,id:9952,x:30245,y:32906,varname:node_9952,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-88-OUT;n:type:ShaderForge.SFN_ComponentMask,id:7515,x:30245,y:33053,varname:node_7515,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-1827-OUT;n:type:ShaderForge.SFN_Time,id:5165,x:29999,y:33337,varname:node_5165,prsc:2;n:type:ShaderForge.SFN_Multiply,id:5944,x:30245,y:32779,varname:node_5944,prsc:2|A-9953-Y,B-5165-TSL;n:type:ShaderForge.SFN_Multiply,id:1430,x:30245,y:32648,varname:node_1430,prsc:2|A-9953-X,B-5165-TSL;n:type:ShaderForge.SFN_Add,id:7331,x:30503,y:32779,varname:node_7331,prsc:2|A-1430-OUT,B-9952-R;n:type:ShaderForge.SFN_Add,id:8990,x:30503,y:32906,varname:node_8990,prsc:2|A-5944-OUT,B-9952-G;n:type:ShaderForge.SFN_Append,id:4663,x:30710,y:32830,varname:node_4663,prsc:2|A-7331-OUT,B-8990-OUT;n:type:ShaderForge.SFN_Add,id:7885,x:30503,y:33077,varname:node_7885,prsc:2|A-7515-R,B-6009-OUT;n:type:ShaderForge.SFN_Add,id:4944,x:30503,y:33193,varname:node_4944,prsc:2|A-7515-G,B-3760-OUT;n:type:ShaderForge.SFN_Multiply,id:6009,x:30245,y:33207,varname:node_6009,prsc:2|A-8825-X,B-5165-TSL;n:type:ShaderForge.SFN_Multiply,id:3760,x:30245,y:33337,varname:node_3760,prsc:2|A-8825-Y,B-5165-TSL;n:type:ShaderForge.SFN_Append,id:6518,x:30712,y:33091,varname:node_6518,prsc:2|A-7885-OUT,B-4944-OUT;n:type:ShaderForge.SFN_Multiply,id:88,x:29999,y:32892,varname:node_88,prsc:2|A-834-OUT,B-8022-OUT;n:type:ShaderForge.SFN_Multiply,id:1827,x:29999,y:33033,varname:node_1827,prsc:2|A-834-OUT,B-8316-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8022,x:29746,y:32949,ptovrint:False,ptlb:UV 1 tiling,ptin:_UV1tiling,varname:_UV1tiling,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:8316,x:29746,y:33067,ptovrint:False,ptlb:UV 2 tiling,ptin:_UV2tiling,varname:_UV2tiling,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Get,id:8505,x:31596,y:33102,varname:node_8505,prsc:2|IN-4244-OUT;n:type:ShaderForge.SFN_Color,id:8836,x:31907,y:32954,ptovrint:False,ptlb:UV 1 color,ptin:_UV1color,varname:_UV1color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:536,x:32104,y:33000,varname:node_536,prsc:2|A-8836-RGB,B-9707-RGB;n:type:ShaderForge.SFN_Color,id:6602,x:31907,y:33507,ptovrint:False,ptlb:UV 2 color,ptin:_UV2color,varname:_UV2color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:9101,x:32106,y:33326,varname:node_9101,prsc:2|A-3430-RGB,B-6602-RGB;n:type:ShaderForge.SFN_Multiply,id:9372,x:32258,y:32859,varname:node_9372,prsc:2|A-9707-A,B-3430-A;n:type:ShaderForge.SFN_Multiply,id:4153,x:32413,y:32797,varname:node_4153,prsc:2|A-3953-OUT,B-9372-OUT;n:type:ShaderForge.SFN_Slider,id:3953,x:32144,y:32705,ptovrint:False,ptlb:Spec Value,ptin:_SpecValue,varname:_SpecValue,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;proporder:5270-3960-5254-8836-8022-9953-6602-8316-8825-3953;pass:END;sub:END;*/

Shader "ZD_Shader/Scenes/Water_Albedo(Specular)_UV2" {
    Properties {
        _Diffuse ("Diffuse", 2D) = "gray" {}
        _Blendstreagth ("Blend streagth", Range(0, 1)) = 0
        _UVScale ("UVScale", Float ) = 0
        _UV1color ("UV 1 color", Color) = (0.5,0.5,0.5,1)
        _UV1tiling ("UV 1 tiling", Float ) = 0
        _UV1Animator ("UV1 Animator", Vector) = (0,0,0,0)
        _UV2color ("UV 2 color", Color) = (0.5,0.5,0.5,1)
        _UV2tiling ("UV 2 tiling", Float ) = 0
        _UV2Animator ("UV2 Animator", Vector) = (0,0,0,0)
        _SpecValue ("Spec Value", Range(0, 1)) = 0
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
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float _Blendstreagth;
            uniform float _UVScale;
            uniform float4 _UV1Animator;
            uniform float4 _UV2Animator;
            uniform float _UV1tiling;
            uniform float _UV2tiling;
            uniform float4 _UV1color;
            uniform float4 _UV2color;
            uniform float _SpecValue;
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_5165 = _Time;
                float2 _worldUV = (float2(i.posWorld.r,i.posWorld.b)/_UVScale);
                float2 node_834 = _worldUV;
                float2 node_9952 = (node_834*_UV1tiling).rg;
                float2 _UV1 = float2(((_UV1Animator.r*node_5165.r)+node_9952.r),((_UV1Animator.g*node_5165.r)+node_9952.g));
                float2 node_8505 = _UV1;
                float4 node_9707 = tex2D(_Diffuse,TRANSFORM_TEX(node_8505, _Diffuse));
                float2 node_7515 = (node_834*_UV2tiling).rg;
                float2 _UV2 = float2((node_7515.r+(_UV2Animator.r*node_5165.r)),(node_7515.g+(_UV2Animator.g*node_5165.r)));
                float2 node_7953 = _UV2;
                float4 node_3430 = tex2D(_Diffuse,TRANSFORM_TEX(node_7953, _Diffuse));
                float3 emissive = lerp((_UV1color.rgb*node_9707.rgb),(node_3430.rgb*_UV2color.rgb),_Blendstreagth);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float _Blendstreagth;
            uniform float _UVScale;
            uniform float4 _UV1Animator;
            uniform float4 _UV2Animator;
            uniform float _UV1tiling;
            uniform float _UV2tiling;
            uniform float4 _UV1color;
            uniform float4 _UV2color;
            uniform float _SpecValue;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                float4 node_5165 = _Time;
                float2 _worldUV = (float2(i.posWorld.r,i.posWorld.b)/_UVScale);
                float2 node_834 = _worldUV;
                float2 node_9952 = (node_834*_UV1tiling).rg;
                float2 _UV1 = float2(((_UV1Animator.r*node_5165.r)+node_9952.r),((_UV1Animator.g*node_5165.r)+node_9952.g));
                float2 node_8505 = _UV1;
                float4 node_9707 = tex2D(_Diffuse,TRANSFORM_TEX(node_8505, _Diffuse));
                float2 node_7515 = (node_834*_UV2tiling).rg;
                float2 _UV2 = float2((node_7515.r+(_UV2Animator.r*node_5165.r)),(node_7515.g+(_UV2Animator.g*node_5165.r)));
                float2 node_7953 = _UV2;
                float4 node_3430 = tex2D(_Diffuse,TRANSFORM_TEX(node_7953, _Diffuse));
                o.Emission = lerp((_UV1color.rgb*node_9707.rgb),(node_3430.rgb*_UV2color.rgb),_Blendstreagth);
                
                float3 diffColor = float3(0,0,0);
                o.Albedo = diffColor;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
