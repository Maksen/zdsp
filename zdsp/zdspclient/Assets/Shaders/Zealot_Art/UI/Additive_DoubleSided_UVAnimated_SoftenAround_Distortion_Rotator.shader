// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:Mobile/Particles/Additive Culled,iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:32823,y:32774,varname:node_4013,prsc:2|emission-1655-OUT;n:type:ShaderForge.SFN_TexCoord,id:6687,x:31118,y:32619,varname:node_6687,prsc:2,uv:0;n:type:ShaderForge.SFN_Distance,id:4908,x:31270,y:33219,varname:node_4908,prsc:2|A-6687-UVOUT,B-7129-OUT;n:type:ShaderForge.SFN_Vector2,id:7129,x:31071,y:33237,varname:node_7129,prsc:2,v1:0.5,v2:0.5;n:type:ShaderForge.SFN_Color,id:8182,x:32033,y:32790,ptovrint:False,ptlb:EmissionColor,ptin:_EmissionColor,varname:_EmissionColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.4852941,c2:0.4049696,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:1655,x:32612,y:32812,varname:node_1655,prsc:2|A-3760-RGB,B-7544-OUT,C-4792-OUT;n:type:ShaderForge.SFN_Tex2d,id:3760,x:32207,y:32610,ptovrint:False,ptlb:UVAnimatedTex,ptin:_UVAnimatedTex,varname:_UVAnimatedTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-7163-OUT;n:type:ShaderForge.SFN_ConstantLerp,id:4792,x:31473,y:33219,varname:node_4792,prsc:2,a:1,b:-1|IN-4908-OUT;n:type:ShaderForge.SFN_Multiply,id:7544,x:32207,y:32822,varname:node_7544,prsc:2|A-8182-RGB,B-3146-OUT;n:type:ShaderForge.SFN_Vector1,id:3146,x:32033,y:32938,varname:node_3146,prsc:2,v1:5;n:type:ShaderForge.SFN_Rotator,id:4893,x:31619,y:32339,varname:node_4893,prsc:2|UVIN-6687-UVOUT,SPD-3175-OUT;n:type:ShaderForge.SFN_Multiply,id:7163,x:31800,y:32339,varname:node_7163,prsc:2|A-4893-UVOUT,B-2814-RGB;n:type:ShaderForge.SFN_Tex2d,id:2814,x:31619,y:32508,ptovrint:False,ptlb:DistortionTex,ptin:_DistortionTex,varname:_DistortionTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:3175,x:31264,y:32527,ptovrint:False,ptlb:UVSpeed,ptin:_UVSpeed,varname:_UVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-20,cur:0,max:20;proporder:3760-2814-8182-3175;pass:END;sub:END;*/

Shader "AAA_UI/Additive_DoubleSided_UVAnimated_SoftenAround_Distortion_Rotator" {
    Properties {
        _UVAnimatedTex ("UVAnimatedTex", 2D) = "white" {}
        _DistortionTex ("DistortionTex", 2D) = "white" {}
        _EmissionColor ("EmissionColor", Color) = (0.4852941,0.4049696,0,1)
        _UVSpeed ("UVSpeed", Range(-20, 20)) = 0
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
            Cull Off
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
            uniform float4 _EmissionColor;
            uniform sampler2D _UVAnimatedTex; uniform float4 _UVAnimatedTex_ST;
            uniform sampler2D _DistortionTex; uniform float4 _DistortionTex_ST;
            uniform float _UVSpeed;
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
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_927 = _Time + _TimeEditor;
                float node_4893_ang = node_927.g;
                float node_4893_spd = _UVSpeed;
                float node_4893_cos = cos(node_4893_spd*node_4893_ang);
                float node_4893_sin = sin(node_4893_spd*node_4893_ang);
                float2 node_4893_piv = float2(0.5,0.5);
                float2 node_4893 = (mul(i.uv0-node_4893_piv,float2x2( node_4893_cos, -node_4893_sin, node_4893_sin, node_4893_cos))+node_4893_piv);
                float4 _DistortionTex_var = tex2D(_DistortionTex,TRANSFORM_TEX(i.uv0, _DistortionTex));
                float3 node_7163 = (float3(node_4893,0.0)*_DistortionTex_var.rgb);
                float4 _UVAnimatedTex_var = tex2D(_UVAnimatedTex,TRANSFORM_TEX(node_7163, _UVAnimatedTex));
                float3 emissive = (_UVAnimatedTex_var.rgb*(_EmissionColor.rgb*5.0)*lerp(1,-1,distance(i.uv0,float2(0.5,0.5))));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Mobile/Particles/Additive Culled"
    CustomEditor "ShaderForgeMaterialInspector"
}
