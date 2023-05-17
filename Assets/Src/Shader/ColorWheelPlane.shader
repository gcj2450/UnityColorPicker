Shader "Bytework/ColorWheelPlane"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Hue("Hue",range(0,1))=0.5
		_SelRampPos("SelRampPos",Vector) = (1,1,1,1)
		//_SelHuePos("SelHuePos",Vector) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
			[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
		_ColorMask("Color Mask", Float) = 15
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
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend One One
		ColorMask[_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define vec2 float2
			#define vec3 float3
			#define vec4 float4
			#define mix lerp
			#include "UnityCG.cginc"

			fixed4 _Color;
		float4 _MainTex_ST;
			//中间渐变选择的位置
			float2 _SelRampPos;
			//色相环选择的位置
			//float2 _SelHuePos;
			float _Hue;
			sampler2D _MainTex;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#define TWO_PI 6.28318530718

			//vec2 rotateCW(vec2 p, float a)
			//{
			//	float2x2 m = float2x2(cos(a), -sin(a), sin(a), cos(a));
			//	//return p * m;
			//	return mul(m,p);
			//}

			vec3 rgb2hsb(in vec3 c) {
				vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
				vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			vec3 hsb2rgb(in vec3 c) {
				vec3 rgb = clamp(abs(fmod(c.x * 6.0 + vec3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
				rgb = rgb * rgb * (3.0 - 2.0 * rgb);
				return c.z * mix(vec3(1.0, 1.0, 1.0), rgb, c.y);
			}

			float3 hsv2rgb(float3 c)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}

			float circle(in vec2 _st, in float _radius) {
				vec2 dist = _st - vec2(0.5, 0.5);
				return 1. - smoothstep(_radius - (_radius*0.015),
					_radius + (_radius*0.015),
					dot(dist, dist)*4.0);
			}

			//画多边形形状
			//float polyShape(in vec2 st)
			//{
			//	float d = 0.0;
			//	// Remap the space to -1. to 1.
			//	st = st * 2. - 1.;
			//	// 形状边数
			//	int N = 4;
			//	// Angle and radius from the current pixel
			//	float a = atan2(st.x, st.y) + UNITY_PI;
			//	float r = UNITY_TWO_PI / float(N);
			//	// Shaping function that modulate the distance
			//	d = cos(floor(.5 + a / r)*r - a)*length(st);
			//	return 1.0 - smoothstep(0.4, 0.41, d);
			//}

			//根据对角点画一个矩形
			fixed rect(fixed2 uv, fixed2 p1, fixed2 p2)
			{
				fixed2 edge = abs((p1 + p2)*0.5 - uv);
				fixed2 srect = smoothstep(edge - fixed2(0.001, 0.001), edge + fixed2(0.001, 0.001), abs(p1 - p2)*0.5);
				return srect.x*srect.y;
			}

			//float3 polyShape(in vec2 st,float h)
			//{
			//	float d = 0.0;
			//	// 映射uv到0-1
			//	st = st * 2. - 1.;
			//	// 中间方块边数
			//	int N = 4;
			//	// 角度和大小
			//	float a = atan2(st.y,st.x) + UNITY_PI;
			//	float r = UNITY_TWO_PI / float(N);
			//	//映射到0-1的颜色渐变
			//	float3 col=hsb2rgb(fixed3(h, (0.5-st.x), (0.5-st.y)));
			//	//小圆圈
			//	fixed ring = 0.001 / abs(length(st - _SelRampPos.xy) - 0.03);
			//	d = cos(floor(.5 + a / r)*r - a)*length(st);
			//	float sm = smoothstep(0.4999, 0.5001, d);
			//	//rampColor+Ring
			//	return (1.0 - sm)*col+step(sm,0)*fixed3(ring, ring, ring);
			//}

			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.uv = v.texcoord;

				OUT.color = v.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				vec2 uv = i.uv.xy;

				vec3 color = vec3(0.0, 0.0, 0.0);

				vec2 toCenter = vec2(0.5, 0.5) - uv;
				float angle = atan2(toCenter.y, toCenter.x);
				float radius = length(toCenter) * 2.0;

				color = hsb2rgb(vec3((angle / TWO_PI) + 0.5, 1.0, 1.0));
				
				//外围色相环
				float ring = circle(uv, 0.95) - circle(uv, 0.55);

				color =lerp(color,vec3(0.05,0.05,0.05),1-ring);

				//根据色相计算角度
				float ang = -_Hue * TWO_PI-UNITY_PI*0.5;
				float2 pos = 0.429*(vec2(sin(ang), cos(ang)));

				//色相选择的圈圈
				fixed hueRing = 0.003 / abs(length(toCenter - pos.xy) - 0.05);
				//这里不被ring裁切一次也可以，因为它一直都是在ring范围内的
				color += hueRing * step(0,ring);
				
				vec2 st = uv * 2. - 1.0;

				float3 col = hsb2rgb(fixed3(_Hue, st+0.5));
				//中间矩形
				float rec= rect(uv, vec2(0.25, 0.25), vec2(0.75, 0.75));
				color = lerp(color, col, rec);

				//矩形内小圆圈
				float d = length(st - _SelRampPos.xy);
				fixed rmpRing = 0.005 / abs(d-0.03);
				//float3 rmpRingCl = step(1 - rec, 0)*rmpRing*fixed3(0.1, 0.1, 0.1);	//被中间矩形裁切一次
				float3 rmpRingCl=rmpRing*(uv.y+.5);

				color = lerp(color, rmpRingCl, rmpRing);

				//color += rmpRingCl;

				return vec4(GammaToLinearSpace(color), 1.0);
			}
			ENDCG
		}
	}
}
