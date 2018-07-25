//Extends from Mobile-Diffuse.shader to allow silhouette

Shader "Effects/Mobile/DiffuseSilhouette" 
{

Properties 
{
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_RimColor ("Rim Color", Color) = (1,1,0)
	_RimWidth ("Rim Width", Range (0, 10)) = 4
}

SubShader 
{
	Tags { "Queue" = "AlphaTest" "RenderType" = "Opaque" }
	LOD 150
	ZWrite Off
	ZTest Greater
	Blend One One
	CGPROGRAM
	#pragma surface surf Lambert
	struct Input {
		float2 uv_MainTex;
		float3 viewDir;
	};

	float4 _RimColor;
	float _RimWidth;
	void surf (Input IN, inout SurfaceOutput o) {		
		half rim = 1 - saturate(dot (normalize(IN.viewDir), o.Normal));
		o.Emission = _RimColor.rgb * pow(rim, _RimWidth);
	}
	ENDCG   

	ZWrite On
	ZTest LEqual
	Blend Off
	CGPROGRAM
	#pragma surface surf Lambert noforwardadd
	sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
	};

	void surf (Input IN, inout SurfaceOutput o) {
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	    o.Albedo = c.rgb;
	    o.Alpha = c.a;
    }
    ENDCG 

}

Fallback "Mobile/Diffuse"
}