// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:32697,y:33049,varname:node_4795,prsc:2|emission-5968-OUT,alpha-4704-OUT;n:type:ShaderForge.SFN_Tex2d,id:5574,x:31219,y:33840,ptovrint:False,ptlb:Noise,ptin:_Noise,varname:_Noise,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:1464,x:30634,y:33618,ptovrint:False,ptlb:Dissolve_amount,ptin:_Dissolve_amount,varname:_Dissolve_amount,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Add,id:4024,x:31456,y:33639,varname:node_4024,prsc:2|A-7102-OUT,B-5574-R;n:type:ShaderForge.SFN_RemapRange,id:7102,x:31219,y:33639,varname:node_7102,prsc:2,frmn:0,frmx:1,tomn:-0.7,tomx:1.1|IN-6254-OUT;n:type:ShaderForge.SFN_RemapRange,id:704,x:31021,y:32784,varname:node_704,prsc:2,frmn:0,frmx:1,tomn:-4,tomx:4|IN-4024-OUT;n:type:ShaderForge.SFN_Clamp01,id:6270,x:31218,y:32784,varname:node_6270,prsc:2|IN-704-OUT;n:type:ShaderForge.SFN_OneMinus,id:9394,x:31410,y:32784,varname:node_9394,prsc:2|IN-6270-OUT;n:type:ShaderForge.SFN_Tex2d,id:708,x:31958,y:32772,ptovrint:False,ptlb:Diffuse,ptin:_Diffuse,varname:_Diffuse,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:5927,x:31968,y:33045,varname:node_5927,prsc:2|A-2251-OUT,B-532-RGB;n:type:ShaderForge.SFN_Multiply,id:2251,x:31764,y:32789,varname:node_2251,prsc:2|A-9394-OUT,B-8609-OUT;n:type:ShaderForge.SFN_Vector1,id:8609,x:31570,y:32809,varname:node_8609,prsc:2,v1:4;n:type:ShaderForge.SFN_Add,id:5968,x:32172,y:32971,varname:node_5968,prsc:2|A-6640-OUT,B-5927-OUT;n:type:ShaderForge.SFN_Color,id:532,x:31749,y:33063,ptovrint:False,ptlb:Dissolve_edge_color,ptin:_Dissolve_edge_color,varname:_Dissolve_edge_color,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_VertexColor,id:4141,x:30712,y:33752,varname:node_4141,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6254,x:30946,y:33690,varname:node_6254,prsc:2|A-1464-OUT,B-4141-A;n:type:ShaderForge.SFN_Step,id:8107,x:31752,y:33646,varname:node_8107,prsc:2|A-7473-OUT,B-4024-OUT;n:type:ShaderForge.SFN_Slider,id:7473,x:31501,y:33925,ptovrint:False,ptlb:Dissolve_edge_distance,ptin:_Dissolve_edge_distance,varname:_Dissolve_edge_distance,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.6,max:1;n:type:ShaderForge.SFN_Color,id:8413,x:31958,y:32553,ptovrint:False,ptlb:Diffuse_color,ptin:_Diffuse_color,varname:_Diffuse_color,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:6640,x:32172,y:32729,varname:node_6640,prsc:2|A-8413-RGB,B-708-RGB;n:type:ShaderForge.SFN_Multiply,id:4704,x:32129,y:33695,varname:node_4704,prsc:2|A-8107-OUT,B-4548-OUT;n:type:ShaderForge.SFN_Slider,id:4548,x:31836,y:33947,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:_Opacity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;proporder:8413-4548-708-5574-1464-532-7473;pass:END;sub:END;*/

Shader "ZD_Shader/Effect/Effect_Obj/Petrochemical" {
    Properties {
        [HDR]_Diffuse_color ("Diffuse_color", Color) = (0.5,0.5,0.5,1)
        _Opacity ("Opacity", Range(0, 1)) = 1
        _Diffuse ("Diffuse", 2D) = "white" {}
        _Noise ("Noise", 2D) = "white" {}
		[PerRendererData]_Dissolve_amount ("Dissolve_amount", Range(0, 1)) = 1
        [HDR]_Dissolve_edge_color ("Dissolve_edge_color", Color) = (0.5,0.5,0.5,1)
        _Dissolve_edge_distance ("Dissolve_edge_distance", Range(0, 1)) = 0.6
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
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            uniform float _Dissolve_amount;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float4 _Dissolve_edge_color;
            uniform float _Dissolve_edge_distance;
            uniform float4 _Diffuse_color;
            uniform float _Opacity;
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
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse));
                float4 _Noise_var = tex2D(_Noise,TRANSFORM_TEX(i.uv0, _Noise));
                float node_4024 = (((_Dissolve_amount*i.vertexColor.a)*1.8+-0.7)+_Noise_var.r);
                float3 emissive = ((_Diffuse_color.rgb*_Diffuse_var.rgb)+(((1.0 - saturate((node_4024*8.0+-4.0)))*4.0)*_Dissolve_edge_color.rgb));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(step(_Dissolve_edge_distance,node_4024)*_Opacity));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
