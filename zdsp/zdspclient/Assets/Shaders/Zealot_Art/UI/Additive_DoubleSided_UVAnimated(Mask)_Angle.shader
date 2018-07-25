// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:Mobile/Particles/Additive Culled,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:False,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:4013,x:32823,y:32774,varname:node_4013,prsc:2|emission-1655-OUT;n:type:ShaderForge.SFN_Panner,id:3188,x:31989,y:32610,varname:node_3188,prsc:2,spu:0,spv:-0.2|UVIN-3336-UVOUT,DIST-1339-OUT;n:type:ShaderForge.SFN_TexCoord,id:6687,x:31219,y:32602,varname:node_6687,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:8454,x:31662,y:32635,varname:node_8454,prsc:2;n:type:ShaderForge.SFN_Slider,id:5129,x:31497,y:32790,ptovrint:False,ptlb:UVSpeed,ptin:_UVSpeed,varname:_UVSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:1,max:10;n:type:ShaderForge.SFN_Multiply,id:1339,x:31832,y:32685,varname:node_1339,prsc:2|A-8454-T,B-5129-OUT;n:type:ShaderForge.SFN_Multiply,id:1655,x:32580,y:32813,varname:node_1655,prsc:2|A-3760-RGB,B-1273-RGB;n:type:ShaderForge.SFN_Tex2d,id:3760,x:32207,y:32610,varname:node_3760,prsc:2,ntxv:0,isnm:False|UVIN-3188-UVOUT,TEX-1613-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:1613,x:32050,y:32777,ptovrint:False,ptlb:UVAnimatedTex(Mask),ptin:_UVAnimatedTexMask,varname:_UVAnimatedTexMask,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:3955,x:32207,y:32854,varname:node_3955,prsc:2,ntxv:0,isnm:False|TEX-1613-TEX;n:type:ShaderForge.SFN_Rotator,id:3336,x:31662,y:32499,varname:node_3336,prsc:2|UVIN-6687-UVOUT,ANG-7571-OUT;n:type:ShaderForge.SFN_Slider,id:7571,x:31349,y:32451,ptovrint:False,ptlb:Angle,ptin:_Angle,varname:_Angle,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:7;n:type:ShaderForge.SFN_Distance,id:5652,x:31850,y:33341,varname:node_5652,prsc:2|B-3349-OUT;n:type:ShaderForge.SFN_Vector2,id:3349,x:31652,y:33384,varname:node_3349,prsc:2,v1:0.5,v2:0.5;n:type:ShaderForge.SFN_ConstantLerp,id:8075,x:32048,y:33341,varname:node_8075,prsc:2,a:1,b:-1|IN-5652-OUT;n:type:ShaderForge.SFN_VertexColor,id:1273,x:32346,y:33005,varname:node_1273,prsc:2;proporder:1613-5129-7571;pass:END;sub:END;*/

Shader "AAA_UI/Additive_DoubleSided_UVAnimated(Mask)_Angle" {
    Properties {
        _UVAnimatedTexMask ("UVAnimatedTex(Mask)", 2D) = "white" {}
        _UVSpeed ("UVSpeed", Range(-10, 10)) = 1
        _Angle ("Angle", Range(0, 7)) = 0
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
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal n3ds wiiu 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float _UVSpeed;
            uniform sampler2D _UVAnimatedTexMask; uniform float4 _UVAnimatedTexMask_ST;
            uniform float _Angle;
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
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_8454 = _Time + _TimeEditor;
                float node_3336_ang = _Angle;
                float node_3336_spd = 1.0;
                float node_3336_cos = cos(node_3336_spd*node_3336_ang);
                float node_3336_sin = sin(node_3336_spd*node_3336_ang);
                float2 node_3336_piv = float2(0.5,0.5);
                float2 node_3336 = (mul(i.uv0-node_3336_piv,float2x2( node_3336_cos, -node_3336_sin, node_3336_sin, node_3336_cos))+node_3336_piv);
                float2 node_3188 = (node_3336+(node_8454.g*_UVSpeed)*float2(0,-0.2));
                float4 node_3760 = tex2D(_UVAnimatedTexMask,TRANSFORM_TEX(node_3188, _UVAnimatedTexMask));
                float3 emissive = (node_3760.rgb*i.vertexColor.rgb);
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Mobile/Particles/Additive Culled"
    CustomEditor "ShaderForgeMaterialInspector"
}
