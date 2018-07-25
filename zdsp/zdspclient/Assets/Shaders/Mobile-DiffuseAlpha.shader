// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//From Mobile Diffuse
//Added alpha:fade to support transparency

Shader "Mobile/DiffuseAlpha" {
Properties {
	_Color("Main Color", Color) = (1,1,1,1)
	_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
}



SubShader{
	Tags{ "RenderType" = "Opaque" }
	LOD 150

	Pass
	{
		Name "WriteZOnly"
		ZTest LEqual
		ZWrite On
		ColorMask 0
		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag

		// vertex shader inputs
		struct appdata
		{
			float4 vertex : POSITION; // vertex position			
		};

		//fragment shader inputs
		struct v2f {
			float4 pos : SV_POSITION;
		};

		// vertex shader
		v2f vert(appdata v)
		{
			v2f o;
			// transform position to clip space
			// (multiply with model*view*projection matrix)
			o.pos = UnityObjectToClipPos(v.vertex);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			return fixed4(1,0,0,1);
		}
		ENDCG
	}



	Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
	LOD 150
	ZTest Equal

	CGPROGRAM
	#pragma surface surf Lambert noforwardadd alpha:fade

	sampler2D _MainTex;
	fixed4 _Color;

	struct Input {
		float2 uv_MainTex;
	};

	void surf(Input IN, inout SurfaceOutput o) {
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
		o.Albedo = c.rgb;
		o.Alpha = c.a;
	}
	ENDCG
}

Fallback "Mobile/VertexLit"
}
