Shader "younne/MotionBlur"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			//sampler2D _MainTex;
			//float4 _MainTex_ST;
			Texture2D _MainTex;
			SamplerState sampler_MainTex;
			float4 _MainTex_TexelSize;

			//Texture2D _CameraDepthTexture;
			//SamplerState sampler__CameraDepthTexture;
			//float4 _CameraDepthTexture_TexelSize;

			//Camera motion vectors texture
			Texture2D _CameraMotionVectorsTexture;
			SamplerState sampler_CameraMotionVectorsTexture;
			float4 _CameraMotionVectorsTexture_TexelSize;

			float _BlurIntensity = 2;
			float _RcpMaxBlurRadius = 1.0 / 10;
			float _VelocityScale = 2;

			half2 VelocitySetup(float2 texcoord)
			{
				// Sample the motion vector.
				float2 v = _CameraMotionVectorsTexture.Sample(sampler_CameraMotionVectorsTexture, texcoord).rg;

				// Apply the exposure time and convert to the pixel space.
				v *= (_VelocityScale * 0.5) * _CameraMotionVectorsTexture_TexelSize.zw;

				// Clamp the vector with the maximum blur radius.
				v /= max(1.0, length(v) * _RcpMaxBlurRadius);

				// Sample the depth of the pixel.
				//half d = Linear01Depth(_CameraDepthTexture.Sample(sampler_CameraMotionVectorsTexture, texcoord).r);
				
				// Pack into 10/10/10/2 format.
				//return half4((v * _RcpMaxBlurRadius + 1.0) * 0.5, d, 0.0);

				return v;
			}

			half4 DownsampleBox4Tap(Texture2D tex, SamplerState samplerTex, float2 uv, float2 texelSize)
			{
				float2 v = VelocitySetup(uv);
				uv += _BlurIntensity * v;

				half4 s = tex.Sample(samplerTex, uv);
				float4 currentColor = 0;
				for (int i = 0; i < 3; i++, uv += _BlurIntensity * v)
				{
					currentColor = tex.Sample(samplerTex, uv);
					s += currentColor;
				}
				s /= 4;

				return fixed4(s.rgb, 1);
			}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half4 color = DownsampleBox4Tap(_MainTex, sampler_MainTex, i.uv, _MainTex_TexelSize.xy);
				return color;
			}


			ENDCG
		}
	}
}
