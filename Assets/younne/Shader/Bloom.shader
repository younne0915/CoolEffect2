// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Shader "younne/Bloom" {
//	Properties{
//		_MainTex("Base (RGB)", 2D) = "white" {}
//		_Bloom("Bloom (RGB)", 2D) = "black" {}
//		_LuminanceThreshold("Luminance Threshold", Float) = 0.5
//		_BlurSize("Blur Size", Float) = 1.0
//	}
//		SubShader{
//			CGINCLUDE
//
//			#include "UnityCG.cginc"
//
//			sampler2D _MainTex;
//			half4 _MainTex_TexelSize;
//			sampler2D _Bloom;
//			float _LuminanceThreshold;
//			float _BlurSize;
//
//			struct v2f {
//				float4 pos : SV_POSITION;
//				half2 uv : TEXCOORD0;
//			};
//
//			v2f vertExtractBright(appdata_img v) {
//				v2f o;
//
//				o.pos = UnityObjectToClipPos(v.vertex);
//
//				o.uv = v.texcoord;
//
//				return o;
//			}
//
//			fixed luminance(fixed4 color) {
//				return  0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
//			}
//
//			fixed4 fragExtractBright(v2f i) : SV_Target {
//				fixed4 c = tex2D(_MainTex, i.uv);
//				fixed val = clamp(luminance(c) - _LuminanceThreshold, 0.0, 1.0);
//
//				return c * val;
//			}
//
//			struct v2fBloom {
//				float4 pos : SV_POSITION;
//				half4 uv : TEXCOORD0;
//			};
//
//			v2fBloom vertBloom(appdata_img v) {
//				v2fBloom o;
//
//				o.pos = UnityObjectToClipPos(v.vertex);
//				o.uv.xy = v.texcoord;
//				o.uv.zw = v.texcoord;
//
//				#if UNITY_UV_STARTS_AT_TOP			
//				if (_MainTex_TexelSize.y < 0.0)
//					o.uv.w = 1.0 - o.uv.w;
//				#endif
//
//				return o;
//			}
//
//			fixed4 fragBloom(v2fBloom i) : SV_Target {
//				return tex2D(_MainTex, i.uv.xy) + tex2D(_Bloom, i.uv.zw);
//			}
//
//			ENDCG
//
//			ZTest Always Cull Off ZWrite Off
//
//			Pass {
//				CGPROGRAM
//				#pragma vertex vertExtractBright  
//				#pragma fragment fragExtractBright  
//
//				ENDCG
//			}
//
//			UsePass "younne/GaussianBlur/GAUSSIAN_BLUR_VERTICAL"
//
//			UsePass "younne/GaussianBlur/GAUSSIAN_BLUR_HORIZONTAL"
//
//			Pass {
//				CGPROGRAM
//				#pragma vertex vertBloom  
//				#pragma fragment fragBloom  
//
//				ENDCG
//			}
//		}
//			FallBack Off
//}


Shader "younne/Bloom" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex, _SourceTex;
	float4 _MainTex_TexelSize;

	//half _Threshold, _SoftThreshold;
	half4 _Filter;
	half _Intensity;

	struct VertexData {
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct Interpolators {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	Interpolators VertexProgram(VertexData v) {
		Interpolators i;
		i.pos = UnityObjectToClipPos(v.vertex);
		i.uv = v.uv;
		return i;
	}

	half3 Sample(float2 uv) {
		return tex2D(_MainTex, uv).rgb;
	}

	half3 SampleBox(float2 uv, float delta) {
		float4 o = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
		half3 s =
			Sample(uv + o.xy) + Sample(uv + o.zy) +
			Sample(uv + o.xw) + Sample(uv + o.zw);
		return s * 0.25f;
	}

	half3 Prefilter(half3 c) {
		half brightness = max(c.r, max(c.g, c.b));
		//half knee = _Threshold * _SoftThreshold;
		half soft = brightness - _Filter.y;
		soft = clamp(soft, 0, _Filter.z);//clamp(x, min, max)
		soft = soft * soft * _Filter.w;
		half contribution = max(soft, brightness - _Filter.x);
		contribution /= max(brightness, 0.00001);
		return c * contribution;
	}

	ENDCG

	SubShader{
		Cull Off
		ZTest Always
		ZWrite Off

		Pass { // 0
			CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram

			half4 FragmentProgram(Interpolators i) : SV_Target {
				return half4(Prefilter(SampleBox(i.uv, 1)), 1);
			}
			ENDCG
		}

		Pass { // 1
					CGPROGRAM
						#pragma vertex VertexProgram
						#pragma fragment FragmentProgram

						half4 FragmentProgram(Interpolators i) : SV_Target {
				//return tex2D(_MainTex, i.uv) * half4(1, 0, 0, 0);
				return half4(SampleBox(i.uv, 1), 1);
			}
		ENDCG
	}

	Pass { // 2
		Blend One One
		CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram

			half4 FragmentProgram(Interpolators i) : SV_Target {
				return half4(SampleBox(i.uv, 0.5), 1);
			}
		ENDCG
	}

	Pass { // 3
		CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram

			half4 FragmentProgram(Interpolators i) : SV_Target {
				half4 c = tex2D(_SourceTex, i.uv);
				c.rgb += _Intensity * SampleBox(i.uv, 0.5);
				return c;
			}
		ENDCG
	}

	Pass { // 4
		CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram

			half4 FragmentProgram(Interpolators i) : SV_Target {
				return half4(_Intensity * SampleBox(i.uv, 0.5), 1);
			}
		ENDCG
	}
	}
}