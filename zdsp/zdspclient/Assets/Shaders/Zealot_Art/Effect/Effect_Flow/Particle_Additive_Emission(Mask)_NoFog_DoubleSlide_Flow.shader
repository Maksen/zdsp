// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:Mobile/Particles/Additive,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:32553,y:32770,varname:node_4795,prsc:2|emission-5582-OUT;n:type:ShaderForge.SFN_VertexColor,id:2355,x:31889,y:32940,varname:node_2355,prsc:2;n:type:ShaderForge.SFN_Panner,id:3725,x:31615,y:32523,varname:node_3725,prsc:2,spu:0,spv:1|UVIN-8906-UVOUT,DIST-6254-OUT;n:type:ShaderForge.SFN_TexCoord,id:7517,x:30850,y:32268,varname:node_7517,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:1965,x:31288,y:32548,varname:node_1965,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6254,x:31458,y:32598,varname:node_6254,prsc:2|A-1965-T,B-3039-OUT;n:type:ShaderForge.SFN_Color,id:1269,x:32099,y:33122,ptovrint:True,ptlb:Color,ptin:_TintColor,varname:_TintColor,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:4080,x:32099,y:32934,varname:node_4080,prsc:2|A-9662-A,B-2355-A;n:type:ShaderForge.SFN_Rotator,id:8906,x:31288,y:32383,varname:node_8906,prsc:2|UVIN-7517-UVOUT,ANG-5062-OUT;n:type:ShaderForge.SFN_Slider,id:3715,x:30540,y:32561,ptovrint:False,ptlb:FlowAngle,ptin:_FlowAngle,varname:_FlowAngle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Pi,id:4947,x:30730,y:32432,varname:node_4947,prsc:2;n:type:ShaderForge.SFN_Multiply,id:5062,x:30850,y:32459,varname:node_5062,prsc:2|A-4947-OUT,B-3715-OUT;n:type:ShaderForge.SFN_Tex2d,id:9662,x:31893,y:32657,ptovrint:True,ptlb:EmissionTex(Mask),ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-3725-UVOUT;n:type:ShaderForge.SFN_ValueProperty,id:3039,x:31288,y:32743,ptovrint:False,ptlb:FlowSpeed,ptin:_FlowSpeed,varname:_FlowSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:5582,x:32315,y:32890,varname:node_5582,prsc:2|A-9662-RGB,B-2355-RGB,C-4080-OUT,D-1269-RGB,E-1269-A;proporder:1269-9662-3715-3039;pass:END;sub:END;*/

Shader "ZD_Shader/Effect/Effect_Flow/Particle_Additive_Emission(Mask)_NoFog_DoubleSlide_Flow" {
    Properties {
        [HDR]_TintColor ("Color", Color) = (1,1,1,1)
        _MainTex ("EmissionTex(Mask)", 2D) = "white" {}
        _FlowAngle ("FlowAngle", Range(0, 2)) = 0
        _FlowSpeed ("FlowSpeed", Float ) = 1
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
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles metal 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float4 _TintColor;
            uniform float _FlowAngle;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
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
                float4 node_1965 = _Time + _TimeEditor;
                float node_8906_ang = (3.141592654*_FlowAngle);
                float node_8906_spd = 1.0;
                float node_8906_cos = cos(node_8906_spd*node_8906_ang);
                float node_8906_sin = sin(node_8906_spd*node_8906_ang);
                float2 node_8906_piv = float2(0.5,0.5);
                float2 node_8906 = (mul(i.uv0-node_8906_piv,float2x2( node_8906_cos, -node_8906_sin, node_8906_sin, node_8906_cos))+node_8906_piv);
                float2 node_3725 = (node_8906+(node_1965.g*_FlowSpeed)*float2(0,1));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_3725, _MainTex));
                float3 emissive = (_MainTex_var.rgb*i.vertexColor.rgb*(_MainTex_var.a*i.vertexColor.a)*_TintColor.rgb*_TintColor.a);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
