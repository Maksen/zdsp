// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:Mobile/Particles/Additive Culled,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:True,stva:1,stmr:255,stmw:255,stcp:2,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:33322,y:32768,varname:node_3138,prsc:2|emission-7899-OUT;n:type:ShaderForge.SFN_Tex2d,id:1332,x:32289,y:32515,ptovrint:False,ptlb:EmissionTex,ptin:_EmissionTex,varname:_EmissionTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:8242,x:32289,y:32743,ptovrint:False,ptlb:EmissionColor,ptin:_EmissionColor,varname:_EmissionColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:1656,x:32937,y:32642,varname:node_1656,prsc:2|A-1332-RGB,B-6263-OUT;n:type:ShaderForge.SFN_Panner,id:3408,x:32053,y:32674,varname:node_3408,prsc:2,spu:0,spv:-0.2|UVIN-3234-UVOUT,DIST-8773-OUT;n:type:ShaderForge.SFN_TexCoord,id:9063,x:31283,y:32666,varname:node_9063,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:5723,x:31726,y:32699,varname:node_5723,prsc:2;n:type:ShaderForge.SFN_Slider,id:4069,x:31561,y:32854,ptovrint:False,ptlb:UVSpeed,ptin:_UVSpeed,varname:_UVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:1,max:10;n:type:ShaderForge.SFN_Multiply,id:8773,x:31896,y:32749,varname:node_8773,prsc:2|A-5723-T,B-4069-OUT;n:type:ShaderForge.SFN_Rotator,id:3234,x:31726,y:32563,varname:node_3234,prsc:2|UVIN-9063-UVOUT,ANG-9048-OUT;n:type:ShaderForge.SFN_Slider,id:6930,x:31175,y:32477,ptovrint:False,ptlb:Angle,ptin:_Angle,varname:_Angle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:8311,x:31365,y:32356,varname:node_8311,prsc:2;n:type:ShaderForge.SFN_Multiply,id:9048,x:31485,y:32415,varname:node_9048,prsc:2|A-8311-OUT,B-6930-OUT;n:type:ShaderForge.SFN_Tex2d,id:6712,x:32198,y:33421,ptovrint:False,ptlb:UVAnimatedTex,ptin:_UVAnimatedTex,varname:_UVAnimatedTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-3408-UVOUT;n:type:ShaderForge.SFN_Multiply,id:7899,x:33075,y:33380,varname:node_7899,prsc:2|A-1656-OUT,B-6712-RGB;n:type:ShaderForge.SFN_ValueProperty,id:1256,x:32466,y:33271,ptovrint:False,ptlb:FlashIntensity,ptin:_FlashIntensity,varname:_FlashIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:6263,x:32738,y:32700,varname:node_6263,prsc:2|A-8242-RGB,B-3009-OUT;n:type:ShaderForge.SFN_Add,id:3009,x:32665,y:32963,varname:node_3009,prsc:2|A-838-OUT,B-5065-OUT;n:type:ShaderForge.SFN_Vector1,id:838,x:32289,y:33249,varname:node_838,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:5065,x:32652,y:33127,varname:node_5065,prsc:2|A-7522-OUT,B-1256-OUT;n:type:ShaderForge.SFN_Vector1,id:527,x:32289,y:33159,varname:node_527,prsc:2,v1:10;n:type:ShaderForge.SFN_Subtract,id:7522,x:32466,y:33100,varname:node_7522,prsc:2|A-527-OUT,B-838-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5341,x:33317,y:33309,ptovrint:False,ptlb:ColorMask,ptin:_ColorMask,cmnt:add this to reduce unity warnings only,varname:_StencilReadMask_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:2203,x:33306,y:33414,ptovrint:False,ptlb:Stencil,ptin:_Stencil,cmnt:Add this to reduce unity warnings only,varname:_Stencil_copy_copy_copy_copy_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:5818,x:33501,y:33414,ptovrint:False,ptlb:StencilComp,ptin:_StencilComp,cmnt:Add this to reduce unity warnings only,varname:_StencilComp_copy_copy_copy_copy_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:8822,x:33501,y:33517,ptovrint:False,ptlb:StencilReadMask,ptin:_StencilReadMask,cmnt:Add this to reduce unity warnings only,varname:_StencilReadMask_copy_copy_copy_copy_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:4193,x:33292,y:33517,ptovrint:False,ptlb:StencilOp,ptin:_StencilOp,cmnt:Add this to reduce unity warnings only,varname:_StencilOp_copy_copy_copy_copy_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:4134,x:33501,y:33626,ptovrint:False,ptlb:StencilWriteMask,ptin:_StencilWriteMask,cmnt:Add this to reduce unity warnings only,varname:_StencilWriteMask_copy_copy_copy_copy_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:1332-8242-6712-4069-6930-1256-5341-2203-4193-5818-8822-4134;pass:END;sub:END;*/

Shader "AAA_UI/Additive_DoubleSided_Emission_UVanimated_Angle_ForUiMask" {
    Properties {
        _EmissionTex ("EmissionTex", 2D) = "white" {}
        _EmissionColor ("EmissionColor", Color) = (0.5,0.5,0.5,1)
        _UVAnimatedTex ("UVAnimatedTex", 2D) = "white" {}
        _UVSpeed ("UVSpeed", Range(-10, 10)) = 1
        _Angle ("Angle", Range(0, 2)) = 0
        _FlashIntensity ("FlashIntensity", Float ) = 0
        _ColorMask ("ColorMask", Float ) = 0
        _Stencil ("Stencil", Float ) = 0
        _StencilOp ("StencilOp", Float ) = 0
        _StencilComp ("StencilComp", Float ) = 0
        _StencilReadMask ("StencilReadMask", Float ) = 0
        _StencilWriteMask ("StencilWriteMask", Float ) = 0
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
            
            Stencil {
                Ref 1
                Comp LEqual
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal n3ds wiiu 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _EmissionTex; uniform float4 _EmissionTex_ST;
            uniform float4 _EmissionColor;
            uniform float _UVSpeed;
            uniform float _Angle;
            uniform sampler2D _UVAnimatedTex; uniform float4 _UVAnimatedTex_ST;
            uniform float _FlashIntensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 _EmissionTex_var = tex2D(_EmissionTex,TRANSFORM_TEX(i.uv0, _EmissionTex));
                float node_838 = 1.0;
                float4 node_5723 = _Time + _TimeEditor;
                float node_3234_ang = (3.141592654*_Angle);
                float node_3234_spd = 1.0;
                float node_3234_cos = cos(node_3234_spd*node_3234_ang);
                float node_3234_sin = sin(node_3234_spd*node_3234_ang);
                float2 node_3234_piv = float2(0.5,0.5);
                float2 node_3234 = (mul(i.uv0-node_3234_piv,float2x2( node_3234_cos, -node_3234_sin, node_3234_sin, node_3234_cos))+node_3234_piv);
                float2 node_3408 = (node_3234+(node_5723.g*_UVSpeed)*float2(0,-0.2));
                float4 _UVAnimatedTex_var = tex2D(_UVAnimatedTex,TRANSFORM_TEX(node_3408, _UVAnimatedTex));
                float3 emissive = ((_EmissionTex_var.rgb*(_EmissionColor.rgb*(node_838+((10.0-node_838)*_FlashIntensity))))*_UVAnimatedTex_var.rgb);
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Mobile/Particles/Additive Culled"
    CustomEditor "ShaderForgeMaterialInspector"
}
