Shader "Bytework/HorizSlider"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[KeywordEnum(HSV, RGB)] _SliderType("SliderType", int) = 0//切换色彩模式
		_SelPos("SelPos",Vector) = (1,1,1,1)

		//[HideInInspector]_SliderWidth("SliderWidth",float) = 0.01
		[HideInInspector]_BarWidth("BarWidth",float) = 0.08
		[HideInInspector]_FirstBarY("FirstBarY",float)=0.125
		[HideInInspector]_BarDist("BarDist",float) = 0.25

		[HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
		[HideInInspector]_Stencil("Stencil ID", Float) = 0
		[HideInInspector]_StencilOp("Stencil Operation", Float) = 0
		[HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
		[HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
		[HideInInspector]_ColorMask("Color Mask", Float) = 15
	}
		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Fog { Mode Off }
			Blend One One
			ColorMask[_ColorMask]


			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				#ifdef GL_ES
				precision mediump float;
				#endif

				half _DispRegion;
				float4 _SelPos;

				#define _SliderWidth 0.012
				float _BarWidth;
				float _FirstBarY;
				float _BarDist;

				half _SliderType;

				float3 Hue(float hue)
				{
					float3 rgb = frac(hue + float3(0.0, 2.0 / 3.0, 1.0 / 3.0));

					rgb = abs(rgb*2.0 - 1.0);

					return clamp(rgb*3.0 - 1.0, 0.0, 1.0);
				}

				struct appdata
				{
					float4 vertex : POSITION;
					fixed2 uv : TEXCOORD0;
				};

				float3 HSVtoRGB(float3 hsv)
				{
					return ((Hue(hsv.x) - 1.0)*hsv.y + 1.0) * hsv.z;
				}

				struct v2f
				{
					fixed2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				float4 drawhsva(fixed2 uv)
				{
					fixed4 finalColor = fixed4(0.02, 0.02, 0.02, 1.0);
					float3 sliderCol = float3(0.6, 0.6, 0.6);

					fixed3 sat = HSVtoRGB(fixed3(_SelPos.x, uv.x, _SelPos.z));
					fixed3 vis = HSVtoRGB(fixed3(_SelPos.x, _SelPos.y, uv.x));

					fixed msk = step(abs(uv.y - _FirstBarY), _BarWidth);
					fixed chek = 1 - (step(sin(uv.x * 100.0) * cos(uv.y * 100.0), 0))*0.2;
					//alpha checker +current color  *mask
					fixed3 clwitha = lerp(fixed3(chek, chek, chek), HSVtoRGB(fixed3(_SelPos.x, _SelPos.y, _SelPos.z)), uv.x);	  //A
					//alpha bar
					finalColor.rgb = lerp(finalColor.rgb, clwitha, msk);
					//alpha slider
					fixed rect = step(abs(uv.x - _SelPos.w), _SliderWidth)*msk;
					finalColor.rgb = lerp(finalColor.rgb, sliderCol, rect);

					//V bar
					fixed msk2 = step(abs(uv.y - (_FirstBarY + _BarDist)), _BarWidth);
					finalColor.rgb = lerp(finalColor.rgb, vis, msk2);
					//V slider
					fixed rect2 = step(abs(uv.x - _SelPos.z), _SliderWidth)*msk2;
					finalColor.rgb = lerp(finalColor.rgb, sliderCol, rect2);
					
					//S bar
					fixed msk3 = step(abs(uv.y - (_FirstBarY + 2.0*_BarDist)), _BarWidth);
					finalColor.rgb = lerp(finalColor.rgb, sat, msk3);	//S
					//S slider
					fixed rect3 = step(abs(uv.x - _SelPos.y), _SliderWidth)*msk3;
					finalColor.rgb = lerp(finalColor.rgb, sliderCol, rect3);
					
					//H bar
					fixed msk4 = step(abs(uv.y - (_FirstBarY + 3.0*_BarDist)), _BarWidth);
					finalColor.rgb = lerp(finalColor.rgb, Hue(uv.x), msk4);	//H
					//H slider
					fixed rect4 = step(abs(uv.x - _SelPos.x), _SliderWidth)*msk4;
					finalColor.rgb = lerp(finalColor.rgb, sliderCol, rect4);

					return finalColor;
				}

				float4 drawrgba(fixed2 uv)
				{
					fixed4 finalColor = fixed4(0.02, 0.02, 0.02, 1.0);
					float3 sliderCol = float3(0.6, 0.6, 0.6);

					fixed msk = step(abs(uv.y - _FirstBarY), _BarWidth);
					fixed chek = 1 - (step(sin(uv.x * 100.0) * cos(uv.y * 100.0), 0))*0.2;
					//alpha checker +current color  *mask
					fixed3 clwitha = lerp(fixed3(chek, chek, chek), fixed3(_SelPos.x, _SelPos.y, _SelPos.z), uv.x);
					//alpha bar
					finalColor.rgb = lerp(finalColor.rgb, clwitha, msk);	//A
					//alpha slider
					fixed rect = step(abs(uv.x - _SelPos.w), _SliderWidth)*msk;
					finalColor.rgb = lerp(finalColor.rgb, sliderCol, rect);

					//B
					fixed msk2 = step(abs(uv.y - (_FirstBarY + _BarDist)), _BarWidth);
					finalColor.rgb = lerp(finalColor.rgb, float3(_SelPos.x, _SelPos.y, uv.x), msk2);	//B
					//B slider
					fixed rect2 = step(abs(uv.x - _SelPos.z), _SliderWidth)*msk2;
					finalColor.rgb = lerp(finalColor.rgb, sliderCol, rect2);
					
					//G
					fixed msk3 = step(abs(uv.y - (_FirstBarY + 2.0*_BarDist)), _BarWidth);
					finalColor.rgb = lerp(finalColor.rgb, float3(_SelPos.x, uv.x, _SelPos.z), msk3);	//G
					//G slider
					fixed rect3 = step(abs(uv.x - _SelPos.y), _SliderWidth)*msk3;
					finalColor.rgb = lerp(finalColor.rgb, sliderCol, rect3);
					
					//R
					fixed msk4 = step(abs(uv.y - (_FirstBarY + 3.0*_BarDist)), _BarWidth);
					finalColor.rgb = lerp(finalColor.rgb, float3(uv.x, _SelPos.y, _SelPos.z), msk4);	//R
					//R slider
					fixed rect4 = step(abs(uv.x - _SelPos.x), _SliderWidth)*msk4;
					finalColor.rgb = lerp(finalColor.rgb, sliderCol, rect4);

					return finalColor;
				}

				fixed4 frag(v2f input) : SV_Target
				{
					if (_SliderType==0)
						return drawhsva(input.uv);
					else
						return drawrgba(input.uv);
				}
				ENDCG
			}
		}
}
