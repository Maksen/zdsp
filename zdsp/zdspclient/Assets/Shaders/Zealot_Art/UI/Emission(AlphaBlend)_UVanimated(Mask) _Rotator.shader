// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:False;n:type:ShaderForge.SFN_Final,id:4013,x:32720,y:32697,varname:node_4013,prsc:2|emission-4130-OUT,alpha-1104-A;n:type:ShaderForge.SFN_Tex2d,id:1104,x:32054,y:32624,ptovrint:False,ptlb:EmissionTex(Alphablend),ptin:_EmissionTexAlphablend,varname:_EmissionTexAlphablend,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:2837,x:31739,y:33007,varname:node_5820,prsc:2,ntxv:0,isnm:False|UVIN-2752-UVOUT,TEX-567-TEX;n:type:ShaderForge.SFN_Multiply,id:1128,x:32325,y:32865,varname:node_1128,prsc:2|A-2837-RGB,B-8631-A;n:type:ShaderForge.SFN_Tex2dAsset,id:567,x:31458,y:33259,ptovrint:False,ptlb:UVAnimatedTex(Mask),ptin:_UVAnimatedTexMask,varname:_UVAnimatedTexMask,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:8631,x:31724,y:33412,varname:node_9273,prsc:2,ntxv:0,isnm:False|TEX-567-TEX;n:type:ShaderForge.SFN_TexCoord,id:3348,x:31002,y:32785,varname:node_3348,prsc:2,uv:0;n:type:ShaderForge.SFN_OneMinus,id:7058,x:32054,y:32814,varname:node_7058,prsc:2|IN-8631-A;n:type:ShaderForge.SFN_Multiply,id:1255,x:32306,y:32674,varname:node_1255,prsc:2|A-1104-RGB,B-7058-OUT;n:type:ShaderForge.SFN_Add,id:4130,x:32525,y:32798,varname:node_4130,prsc:2|A-1255-OUT,B-1128-OUT;n:type:ShaderForge.SFN_Rotator,id:2752,x:31376,y:32848,varname:node_2752,prsc:2|UVIN-3348-UVOUT,SPD-4398-OUT;n:type:ShaderForge.SFN_Slider,id:4398,x:30939,y:33002,ptovrint:False,ptlb:UVSpeed,ptin:_UVSpeed,varname:_UVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-5,cur:5,max:5;proporder:1104-567-4398;pass:END;sub:END;*/

Shader "AAA_UI/Emission(AlphaBlend)_UVAnimated(Mask)_Rotator" {
    Properties {
        _EmissionTexAlphablend ("EmissionTex(Alphablend)", 2D) = "white" {}
        _UVAnimatedTexMask ("UVAnimatedTex(Mask)", 2D) = "white" {}
        _UVSpeed ("UVSpeed", Range(-5, 5)) = 5
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
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
            Blend SrcAlpha OneMinusSrcAlpha
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
            uniform sampler2D _EmissionTexAlphablend; uniform float4 _EmissionTexAlphablend_ST;
            uniform sampler2D _UVAnimatedTexMask; uniform float4 _UVAnimatedTexMask_ST;
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
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 _EmissionTexAlphablend_var = tex2D(_EmissionTexAlphablend,TRANSFORM_TEX(i.uv0, _EmissionTexAlphablend));
                float4 node_9273 = tex2D(_UVAnimatedTexMask,TRANSFORM_TEX(i.uv0, _UVAnimatedTexMask));
                float4 node_9259 = _Time + _TimeEditor;
                float node_2752_ang = node_9259.g;
                float node_2752_spd = _UVSpeed;
                float node_2752_cos = cos(node_2752_spd*node_2752_ang);
                float node_2752_sin = sin(node_2752_spd*node_2752_ang);
                float2 node_2752_piv = float2(0.5,0.5);
                float2 node_2752 = (mul(i.uv0-node_2752_piv,float2x2( node_2752_cos, -node_2752_sin, node_2752_sin, node_2752_cos))+node_2752_piv);
                float4 node_5820 = tex2D(_UVAnimatedTexMask,TRANSFORM_TEX(node_2752, _UVAnimatedTexMask));
                float3 emissive = ((_EmissionTexAlphablend_var.rgb*(1.0 - node_9273.a))+(node_5820.rgb*node_9273.a));
                float3 finalColor = emissive;
                return fixed4(finalColor,_EmissionTexAlphablend_var.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
