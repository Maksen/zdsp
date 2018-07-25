// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:Mobile/Particles/Alpha Blended,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:14,ufog:False,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:32797,y:32667,varname:node_4795,prsc:2|emission-2393-OUT,alpha-798-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:32226,y:32722,ptovrint:True,ptlb:FlowEmission(Mask),ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-9259-UVOUT;n:type:ShaderForge.SFN_Multiply,id:2393,x:32495,y:32785,varname:node_2393,prsc:2|A-6074-RGB,B-2053-RGB;n:type:ShaderForge.SFN_VertexColor,id:2053,x:32226,y:32944,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_Multiply,id:798,x:32495,y:32940,varname:node_798,prsc:2|A-6074-A,B-2053-A,C-2359-R;n:type:ShaderForge.SFN_Tex2d,id:2359,x:32226,y:33104,ptovrint:False,ptlb:Mask_02,ptin:_Mask_02,varname:_Mask_02,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Panner,id:9259,x:31982,y:32771,varname:node_9259,prsc:2,spu:0,spv:1|UVIN-8504-UVOUT,DIST-4993-OUT;n:type:ShaderForge.SFN_TexCoord,id:5252,x:31494,y:32623,varname:node_5252,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Rotator,id:8504,x:31655,y:32773,varname:node_8504,prsc:2|UVIN-5252-UVOUT,ANG-2532-OUT;n:type:ShaderForge.SFN_Slider,id:3179,x:31149,y:32948,ptovrint:False,ptlb:Angle,ptin:_Angle,varname:_Angle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:797,x:31339,y:32808,varname:node_797,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2532,x:31495,y:32869,varname:node_2532,prsc:2|A-797-OUT,B-3179-OUT;n:type:ShaderForge.SFN_Time,id:5571,x:31554,y:33031,varname:node_5571,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4993,x:31794,y:33072,varname:node_4993,prsc:2|A-5571-T,B-3000-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3000,x:31554,y:33245,ptovrint:False,ptlb:FlowSpeed,ptin:_FlowSpeed,varname:_FlowSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:6074-2359-3179-3000;pass:END;sub:END;*/

Shader "ZD_Shader/Effect/Effect_Flow/Particle_AlphaBlend_FlowEmission(Mask)_Mask02_NoFog" {
    Properties {
        _MainTex ("FlowEmission(Mask)", 2D) = "white" {}
        _Mask_02 ("Mask_02", 2D) = "white" {}
        _Angle ("Angle", Range(0, 2)) = 0
        _FlowSpeed ("FlowSpeed", Float ) = 1
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
            ColorMask RGB
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _Mask_02; uniform float4 _Mask_02_ST;
            uniform float _Angle;
            uniform float _FlowSpeed;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_5571 = _Time + _TimeEditor;
                float node_8504_ang = (3.141592654*_Angle);
                float node_8504_spd = 1.0;
                float node_8504_cos = cos(node_8504_spd*node_8504_ang);
                float node_8504_sin = sin(node_8504_spd*node_8504_ang);
                float2 node_8504_piv = float2(0.5,0.5);
                float2 node_8504 = (mul(i.uv0-node_8504_piv,float2x2( node_8504_cos, -node_8504_sin, node_8504_sin, node_8504_cos))+node_8504_piv);
                float2 node_9259 = (node_8504+(node_5571.g*_FlowSpeed)*float2(0,1));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_9259, _MainTex));
                float3 emissive = (_MainTex_var.rgb*i.vertexColor.rgb);
                float3 finalColor = emissive;
                float4 _Mask_02_var = tex2D(_Mask_02,TRANSFORM_TEX(i.uv0, _Mask_02));
                return fixed4(finalColor,(_MainTex_var.a*i.vertexColor.a*_Mask_02_var.r));
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
