Shader "younne/BlurEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

			//// Camera motion vectors texture
			//TEXTURE2D_SAMPLER2D(_CameraMotionVectorsTexture, sampler_CameraMotionVectorsTexture);
			//float4 _CameraMotionVectorsTexture_TexelSize;

			Texture2D _CameraMotionVectorsTexture;
			SamplerState sampler_CameraMotionVectorsTexture;

			Texture2D _CameraDepthTexture;
			SamplerState sampler_CameraDepthTexture;

			float _BlurSize = 2;

            float4 _MainTex_TexelSize;

            half4 DownsampleBox4Tap(Texture2D tex, SamplerState samplerTex, float2 uv, float2 texelSize)
            {
                float4 d = texelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0);               
                half4 s;

                s = tex.Sample(samplerTex, saturate(uv + d.xy * _BlurSize));
                s += tex.Sample(samplerTex, saturate(uv + d.zy * _BlurSize));
                s += tex.Sample(samplerTex, saturate(uv + d.xw * _BlurSize));
                s += tex.Sample(samplerTex, saturate(uv + d.zw * _BlurSize));
                
                return s * (1.0 / 4.0);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half4 color = DownsampleBox4Tap(_MainTex, sampler_MainTex, i.uv, _MainTex_TexelSize.xy);

				//color = _CameraDepthTexture.Sample(sampler_CameraDepthTexture, i.uv);
				//color = _CameraMotionVectorsTexture.Sample(sampler_CameraMotionVectorsTexture, i.uv);

                //return Prefilter(SafeHDR(color), i.uv);


                return color;
            }

            
            ENDCG
        }
    }
}
