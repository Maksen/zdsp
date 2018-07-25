// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:True,stva:1,stmr:255,stmw:255,stcp:2,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:32724,y:32693,varname:node_4795,prsc:2|emission-5558-OUT;n:type:ShaderForge.SFN_VertexColor,id:2053,x:32235,y:32772,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:1755,x:32235,y:32572,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_1755,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:5558,x:32452,y:32720,varname:node_5558,prsc:2|A-1755-RGB,B-2053-RGB;n:type:ShaderForge.SFN_ValueProperty,id:7116,x:32198,y:33039,ptovrint:False,ptlb:ColorMask,ptin:_ColorMask,cmnt:Add this to reduce unity warnings only,varname:_StencilReadMask_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:7454,x:32198,y:33144,ptovrint:False,ptlb:Stencil,ptin:_Stencil,cmnt:Add this to reduce unity warnings only,varname:_ColorMask_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:7844,x:32198,y:33228,ptovrint:False,ptlb:StencilOp,ptin:_StencilOp,cmnt:Add this to reduce unity warnings only,varname:_Stencil_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:474,x:32339,y:33087,ptovrint:False,ptlb:StencilComp,ptin:_StencilComp,cmnt:Add this to reduce unity warnings only,varname:_StencilOp_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:8078,x:32339,y:33192,ptovrint:False,ptlb:StencilReadMask,ptin:_StencilReadMask,cmnt:Add this to reduce unity warnings only,varname:_StencilComp_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:5385,x:33442,y:33449,ptovrint:False,ptlb:StencilComp_copy,ptin:_StencilComp_copy,cmnt:Ned All these if not will have unity warning,varname:_StencilComp_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:8;n:type:ShaderForge.SFN_ValueProperty,id:5237,x:33567,y:33449,ptovrint:False,ptlb:Stencil_copy,ptin:_Stencil_copy,varname:_Stencil_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:8392,x:33425,y:33531,ptovrint:False,ptlb:StencilOp_copy,ptin:_StencilOp_copy,varname:_StencilOp_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:4682,x:33555,y:33531,ptovrint:False,ptlb:StencilWriteMask,ptin:_StencilWriteMask,varname:_StencilComp_copy_copy_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:255;n:type:ShaderForge.SFN_ValueProperty,id:6003,x:33425,y:33613,ptovrint:False,ptlb:StencilReadMask_copy,ptin:_StencilReadMask_copy,varname:_StencilReadMask_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:255;n:type:ShaderForge.SFN_ValueProperty,id:6979,x:33567,y:33613,ptovrint:False,ptlb:ColorMask_copy,ptin:_ColorMask_copy,varname:_ColorMask_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:15;n:type:ShaderForge.SFN_ValueProperty,id:3712,x:32339,y:33301,ptovrint:False,ptlb:StencilWriteMask,ptin:_StencilWriteMask,cmnt:Add this to reduce unity warnings only,varname:_StencilReadMask_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:1755-7116-7454-7844-474-8078-3712;pass:END;sub:END;*/

Shader "AAA_UI/AdditiveCulled_ForUiMask" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
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
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
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
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 emissive = (_MainTex_var.rgb*i.vertexColor.rgb);
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
