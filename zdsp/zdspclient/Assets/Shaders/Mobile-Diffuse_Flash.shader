//Add on to Mobile/Diffuse to include flash property

Shader "Mobile/Diffuse_Flash" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_FlashColor ("FlashColor", Color) = (1,1,1,1)
    _FlashIntensity ("FlashIntensity", Float ) = 0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
uniform float4 _FlashColor;
uniform float _FlashIntensity;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);	
	o.Albedo = lerp(c.rgb, _FlashColor, _FlashIntensity);
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/Diffuse"
}
