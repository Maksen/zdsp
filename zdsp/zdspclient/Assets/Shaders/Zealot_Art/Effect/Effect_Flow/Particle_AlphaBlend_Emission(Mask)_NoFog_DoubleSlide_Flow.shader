// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:Mobile/Particles/Alpha Blended,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:32790,y:32755,varname:node_4795,prsc:2|emission-4879-OUT,alpha-4907-OUT;n:type:ShaderForge.SFN_Tex2d,id:5390,x:32299,y:32665,ptovrint:True,ptlb:Emission(Mask),ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1780-UVOUT;n:type:ShaderForge.SFN_Multiply,id:4879,x:32559,y:32849,varname:node_4879,prsc:2|A-5390-RGB,B-2883-RGB,C-7040-RGB;n:type:ShaderForge.SFN_VertexColor,id:2883,x:32299,y:32865,varname:node_2883,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4907,x:32573,y:33006,varname:node_4907,prsc:2|A-5390-A,B-2883-A,C-7040-A;n:type:ShaderForge.SFN_Color,id:7040,x:32288,y:33110,ptovrint:True,ptlb:Color,ptin:_TintColor,varname:_TintColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Panner,id:1780,x:32057,y:32665,varname:node_1780,prsc:2,spu:0,spv:1|UVIN-9363-UVOUT,DIST-4306-OUT;n:type:ShaderForge.SFN_TexCoord,id:1586,x:31292,y:32410,varname:node_1586,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:7784,x:31730,y:32690,varname:node_7784,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4306,x:31900,y:32740,varname:node_4306,prsc:2|A-7784-T,B-4260-OUT;n:type:ShaderForge.SFN_Rotator,id:9363,x:31730,y:32525,varname:node_9363,prsc:2|UVIN-1586-UVOUT,ANG-2772-OUT;n:type:ShaderForge.SFN_Slider,id:5851,x:30982,y:32703,ptovrint:False,ptlb:FlowAngle,ptin:_FlowAngle,varname:_FlowAngle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:279,x:31172,y:32574,varname:node_279,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2772,x:31292,y:32601,varname:node_2772,prsc:2|A-279-OUT,B-5851-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4260,x:31740,y:32886,ptovrint:False,ptlb:FlowSpeed,ptin:_FlowSpeed,varname:_FlowSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:7040-5390-5851-4260;pass:END;sub:END;*/

Shader "ZD_Shader/Effect/Effect_Flow/Particle_AlphaBlend_Emission(Mask)_NoFog_DoubleSlided_Flow" {
    Properties {
        [HDR]_TintColor ("Color", Color) = (1,1,1,1)
        _MainTex ("Emission(Mask)", 2D) = "white" {}
        _FlowAngle ("FlowAngle", Range(0, 2)) = 0
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
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform float _FlowAngle;
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
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_7784 = _Time + _TimeEditor;
                float node_9363_ang = (3.141592654*_FlowAngle);
                float node_9363_spd = 1.0;
                float node_9363_cos = cos(node_9363_spd*node_9363_ang);
                float node_9363_sin = sin(node_9363_spd*node_9363_ang);
                float2 node_9363_piv = float2(0.5,0.5);
                float2 node_9363 = (mul(i.uv0-node_9363_piv,float2x2( node_9363_cos, -node_9363_sin, node_9363_sin, node_9363_cos))+node_9363_piv);
                float2 node_1780 = (node_9363+(node_7784.g*_FlowSpeed)*float2(0,1));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_1780, _MainTex));
                float3 emissive = (_MainTex_var.rgb*i.vertexColor.rgb*_TintColor.rgb);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(_MainTex_var.a*i.vertexColor.a*_TintColor.a));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
