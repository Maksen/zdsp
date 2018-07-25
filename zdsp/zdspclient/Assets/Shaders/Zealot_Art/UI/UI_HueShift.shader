Shader "AAA_UI/UI_HueShift"
{
	Properties {
	_Hue ("Hue", Range(0,359)) = 0
	_Saturation ("Saturation", Range(0,3.0)) = 1.0
    _Contrast ("Contrast", Range(0,3.0)) = 1.0
	
	//Pull in texture
	[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
	
	// A subset of blend mode values, just "One" (value 1) and "SrcAlpha" (value 5)
	[HideInInspector][Enum(One,1)] _SourceBlend ("Source Blend Mode", Float) = 1
	[Enum(Normal,10,Additive,1)] _DestBlend ("Blend Mode", Float) = 10
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend [_SourceBlend] [_DestBlend]
			
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

	        sampler2D _MainTex;
	        half _Hue;
	        half _Saturation;
	        half _Contrast;

	        struct Input {
	            float2 uv_MainTex;
	        };
			
	        //RGB to HSV
			float3 RGBConvertToHSV(float3 rgb)
	        {
		        float R = rgb.x,G = rgb.y,B = rgb.z;
			    float3 hsv;
				float max1=max(R,max(G,B));
				float min1=min(R,min(G,B));
				if (R == max1) 
				{
	                hsv.x = (G-B)/(max1-min1);
				}
				if (G == max1) 
				{
					hsv.x = 2 + (B-R)/(max1-min1);
					}
				if (B == max1) 
				{
					hsv.x = 4 + (R-G)/(max1-min1);
					}
				hsv.x = hsv.x * 60.0;   
				if (hsv.x < 0) 
                hsv.x = hsv.x + 360;
				hsv.z=max1;
				hsv.y=(max1-min1)/max1;
				return hsv;
			}

			//HSV to RGB
			float3 HSVConvertToRGB(float3 hsv)
			{
				float R,G,B;
				//float3 rgb;
				if( hsv.y == 0 )
				{
					R=G=B=hsv.z;
				}
				else
				{
					hsv.x = hsv.x/60.0; 
					int i = (int)hsv.x;
					float f = hsv.x - (float)i;
					float a = hsv.z * ( 1 - hsv.y );
					float b = hsv.z * ( 1 - hsv.y * f );
					float c = hsv.z * ( 1 - hsv.y * (1 - f ) );
				//	switch(i)
				//	{
				//		case 0: R = hsv.z; G = c; B = a;
				//			break;
				//		case 1: R = b; G = hsv.z; B = a; 
				//			break;
				//		case 2: R = a; G = hsv.z; B = c; 
				//			break;
				//		case 3: R = a; G = b; B = hsv.z; 
				//			break;
				//		case 4: R = c; G = a; B = hsv.z; 
				//			break;
				//		default: R = hsv.z; G = a; B = b; 
				//			break;
				//	}

					if(i == 0)
					{	R = hsv.z; G = c; B = a; }
					else if(i == 1)
					{	R = b; G = hsv.z; B = a; }
					else if(i == 2)	
					{	R = a; G = hsv.z; B = c; }
					else if(i == 3)		
					{	R = a; G = b; B = hsv.z; }
					else if(i == 4)	
					{	R = c; G = a; B = hsv.z; }
					else
					{	R = hsv.z; G = a; B = b; }
				
				}
				return float3(R,G,B);
			}       

			fixed4 frag (v2f_img i) : SV_Target
			{
				fixed4 original = tex2D(_MainTex, i.uv);		//retrieve original color

				float3 colorHSV;    
				colorHSV.xyz = RGBConvertToHSV(original.xyz);   //Change to HSV
				colorHSV.x += _Hue;								//Adjust Hue
				colorHSV.x = colorHSV.x%360;					//exceed360 become 0

				colorHSV.y *= _Saturation;						//Adjust Saturation
				colorHSV.z *= _Contrast;                           

				original.xyz = HSVConvertToRGB(colorHSV.xyz);   //Change HSV back to RGB after edit

				return original;
			}
			ENDCG
		}			
	}
	
}
