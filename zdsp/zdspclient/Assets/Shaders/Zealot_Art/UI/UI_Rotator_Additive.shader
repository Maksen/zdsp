// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Mobile/Particles/Additive Culled,iptp:1,cusa:False,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:33745,y:32766,varname:node_3138,prsc:2|emission-7899-OUT;n:type:ShaderForge.SFN_Tex2d,id:1332,x:32056,y:33227,ptovrint:False,ptlb:EmissionTex,ptin:_EmissionTex,varname:_EmissionTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:8242,x:32056,y:33455,ptovrint:False,ptlb:EmissionColor,ptin:_EmissionColor,varname:_EmissionColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:1656,x:32704,y:33354,varname:node_1656,prsc:2|A-1332-RGB,B-6263-OUT;n:type:ShaderForge.SFN_TexCoord,id:9063,x:31050,y:33378,varname:node_9063,prsc:2,uv:0;n:type:ShaderForge.SFN_Slider,id:4069,x:31191,y:33465,ptovrint:False,ptlb:UVSpeed,ptin:_UVSpeed,varname:_UVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:1,max:10;n:type:ShaderForge.SFN_Rotator,id:3234,x:31554,y:33222,varname:node_3234,prsc:2|UVIN-9063-UVOUT,SPD-4069-OUT;n:type:ShaderForge.SFN_Tex2d,id:6712,x:31965,y:34133,ptovrint:False,ptlb:UVAnimatedTex,ptin:_UVAnimatedTex,varname:_UVAnimatedTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-3234-UVOUT;n:type:ShaderForge.SFN_Multiply,id:7899,x:33381,y:33442,varname:node_7899,prsc:2|A-1656-OUT,B-6712-RGB;n:type:ShaderForge.SFN_ValueProperty,id:1256,x:32233,y:33983,ptovrint:False,ptlb:FlashIntensity,ptin:_FlashIntensity,varname:_FlashIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:6263,x:32505,y:33412,varname:node_6263,prsc:2|A-8242-RGB,B-3009-OUT;n:type:ShaderForge.SFN_Add,id:3009,x:32432,y:33675,varname:node_3009,prsc:2|A-838-OUT,B-5065-OUT;n:type:ShaderForge.SFN_Vector1,id:838,x:32056,y:33900,varname:node_838,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:5065,x:32419,y:33839,varname:node_5065,prsc:2|A-7522-OUT,B-1256-OUT;n:type:ShaderForge.SFN_Vector1,id:527,x:32056,y:33824,varname:node_527,prsc:2,v1:10;n:type:ShaderForge.SFN_Subtract,id:7522,x:32233,y:33812,varname:node_7522,prsc:2|A-527-OUT,B-838-OUT;proporder:1332-8242-6712-4069-1256;pass:END;sub:END;*/

Shader "AAA_UI/UI_Rotator_Additive" {
    Properties {
        _EmissionTex ("EmissionTex", 2D) = "white" {}
        _EmissionColor ("EmissionColor", Color) = (0.5,0.5,0.5,1)
        _UVAnimatedTex ("UVAnimatedTex", 2D) = "white" {}
        _UVSpeed ("UVSpeed", Range(-10, 10)) = 1
        _FlashIntensity ("FlashIntensity", Float ) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
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
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _EmissionTex; uniform float4 _EmissionTex_ST;
            uniform float4 _EmissionColor;
            uniform float _UVSpeed;
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
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 _EmissionTex_var = tex2D(_EmissionTex,TRANSFORM_TEX(i.uv0, _EmissionTex));
                float node_838 = 1.0;
                float4 node_1020 = _Time + _TimeEditor;
                float node_3234_ang = node_1020.g;
                float node_3234_spd = _UVSpeed;
                float node_3234_cos = cos(node_3234_spd*node_3234_ang);
                float node_3234_sin = sin(node_3234_spd*node_3234_ang);
                float2 node_3234_piv = float2(0.5,0.5);
                float2 node_3234 = (mul(i.uv0-node_3234_piv,float2x2( node_3234_cos, -node_3234_sin, node_3234_sin, node_3234_cos))+node_3234_piv);
                float4 _UVAnimatedTex_var = tex2D(_UVAnimatedTex,TRANSFORM_TEX(node_3234, _UVAnimatedTex));
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
