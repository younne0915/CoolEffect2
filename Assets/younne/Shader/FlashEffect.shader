// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "younne/FlashEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_AlbedoColor("Color", Color) = (1,1,1,1)
		_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Bump Scale", float) = 1.0
		_Specular("Specular", Color) = (1,1,1,1)
		_Gloss("Gloss", Range(8.0, 256)) = 20
		[NoScaleOffset] _OutlinePattern("Outline Pattern", 2D) = "" {}

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
			 #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				half3 normal    : NORMAL;
				half4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float3 tangentLightDir : TEXCOORD2;
				float3 tangentViewDir : TEXCOORD3;
				float3 worldPos : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _AlbedoColor;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			float _BumpScale;
			fixed4 _Specular;
			float _Gloss;
			

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.uv, _BumpMap);
				half sign = half(v.tangent.w) * half(unity_WorldTransformParams.w);
				float3 bioNormal = cross(v.normal, v.tangent) * sign;
				float3x3 matrixRot = float3x3(v.tangent.xyz, bioNormal, v.normal);
				o.tangentLightDir = mul(matrixRot, ObjSpaceLightDir(v.vertex)).xyz;
				o.tangentViewDir = mul(matrixRot, ObjSpaceViewDir(v.vertex)).xyz;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 tangentLightDir = normalize(i.tangentLightDir);
				float3 tangentViewDir = normalize(i.tangentViewDir);

                fixed4 col = tex2D(_MainTex, i.uv.xy) * _AlbedoColor;
				fixed4 packedNormal = tex2D(_BumpMap, i.uv.zw);
				fixed3 tangentNormal = UnpackNormal(packedNormal);
				tangentNormal.xy *= _BumpScale;
				tangentNormal.z = sqrt(1 - saturate(dot(tangentNormal.xy, tangentNormal.xy)));
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * col;
				fixed3 diffuse = _LightColor0.rgb * col * max(0, dot(tangentNormal, tangentLightDir));
				fixed3 halfDir = normalize(tangentLightDir + tangentViewDir);
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(tangentNormal, halfDir)), _Gloss);

				col = fixed4(ambient + diffuse + specular, 1);
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }

		Pass
		{
			CGPROGRAM
			#pragma vertex vertOutLine
			#pragma fragment fragOutLine

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct OutlineVertexOutput
			{
				float4 pos : POSITION;
				float4 UV : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};

			struct OutlineVertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};

			float _OutlineThickness;
			float _OutlinePatternSize;
			float _OutlineDepthBias;
			float _GlobalOutlineDepthBias;
			float _OutlineScaleMax = 50.0f;
			float4 _DirectionalLightColor1;

			float4 _Color;
			sampler2D _OpacityMap;
			sampler2D _OutlinePattern;
			float _Cutoff;
			float4 _OutlineEffectColor;
			float4 _OutlineColor;

			sampler2D _MainTex;
			float4 _MainTex_ST;

//#if ENABLE_CAPTURE_FLAGS
//			int _ForceNoOutline;
//			int _RenderAsSolidColor;
//			int _RenderOutlineAsSolidColor;
//#endif

#ifdef FP_PRECISION_HALF
#define float_t  half
#define float2_t half2
#define float3_t half3
#define float4_t half4
#else
#define float_t  float
#define float2_t float2
#define float3_t float3
#define float4_t float4
#endif

			inline float3 GetViewDir(float3 worldPos)
			{
				return normalize(_WorldSpaceCameraPos - worldPos).xyz;
			}

			float4 ApplyDepthOffset(OutlineVertexInput v, float3 worldPos)
			{
				float depthOffset = _OutlineDepthBias + _GlobalOutlineDepthBias;
				float3 viewDir = GetViewDir(worldPos);
				float3 offsetVector = depthOffset * viewDir;

#if ENABLE_DEPTH_OFFSET
				float4 mapValue = tex2Dlod(_DepthOffsetMap, float4(v.texcoord.xy, 0.0, 0.0));
				offsetVector += (_DepthOffset * mapValue.r) * viewDir;
#endif

				worldPos += offsetVector;
				return mul(UNITY_MATRIX_VP, float4(worldPos, 1));
			}

			float4 ComputeOutlineProjPos(OutlineVertexInput v, float3 worldPos, float3 normal)
			{
				// Transform the position and normal into projection space
				float4 projPos = ApplyDepthOffset(v, worldPos);
				float4 projNormal = normalize(UnityObjectToClipPos(float4(normal, 0)));

				// Scale the normal to be the size of one pixel, but not to exceed kOutlineNormalScaleMax
				// so the edge doesn't get too thick when the character is too far away
				float wScale = sign(projPos.w) * min(abs(projPos.w), _OutlineScaleMax);

				const float kOutlineThicknessScale = 1.6f;
				float2 halfScreenSize = 0.5f * _ScreenParams.xy;
				float4 normalScale = (kOutlineThicknessScale * _OutlineThickness * wScale) / halfScreenSize.xyxx;

				projNormal *= normalScale;
				projPos += projNormal;
				return projPos;
			}

			float3 GetKeyLightColor() { return _DirectionalLightColor1.rgb; }

			float3_t ApplyKeyLightColor(float3_t inColor)
			{
				return inColor * GetKeyLightColor();
			}

			float3_t BlendLinearBurn(float3_t a, float3_t b)
			{
				return a + b - float3_t(1.0h, 1.0h, 1.0h);
			}

			float4_t ComputeMainColor(OutlineVertexOutput IN, fixed facing)
			{
				float4_t baseColor = tex2D(_MainTex, IN.UV.xy);
				float3_t outlinePatternColor = tex2D(_OutlinePattern, IN.UV.zw).rgb;
#if ENABLE_OPACITY_MAP
				float_t opacity = tex2D(_OpacityMap, IN.UV.xy).r;
#else
				float_t opacity = baseColor.a;
#endif

#if ENABLE_ALPHA_CLIP && !GLOBAL_DISABLE_ALPHA_CLIP
				clip(opacity - _Cutoff);
#endif

				float3_t outputColor = baseColor.rgb;

#if ENABLE_KEY_LIGHT_COLOR
				outputColor = ApplyKeyLightColor(outputColor);
#endif

				// New Ishima Style 2017.02.24
				float3_t originalColor = outputColor;
				outputColor = saturate(BlendLinearBurn(outputColor, _OutlineColor));
				outputColor = lerp(originalColor, outputColor, outlinePatternColor.r);

#if ENABLE_OUTLINE_RIM_LIGHTING
				outputColor = ApplyRimLighting(IN, baseColor.rgb, facing, outputColor);
#endif

				// Outline effect color
				outputColor = lerp(outputColor, _OutlineEffectColor.rgb, _OutlineEffectColor.a);

				//outputColor = ApplyAdditiveColor(IN.worldPos, outputColor);

				return float4_t(outputColor, opacity);
			}

			float4_t GetMainOutputColor(OutlineVertexOutput IN, fixed facing)
			{

				float4_t outputColor = ComputeMainColor(IN, facing);

//#if OUTPUT_COLOR_SPACE_GAMMA
//				outputColor.rgb = LINEAR_TO_GAMMA(outputColor.rgb);
//#endif

				return outputColor;
			}


			OutlineVertexOutput vertOutLine(OutlineVertexInput v) {

				OutlineVertexOutput o;

				float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
				float3 normal = v.normal;
				float4 projPos = ComputeOutlineProjPos(v, worldPos, normal);
				o.pos = projPos;

				float2 texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
				o.UV = float4(texcoord, projPos.xy / (projPos.w * _OutlinePatternSize));
				o.normal = mul(unity_ObjectToWorld, float4(normal, 0)).xyz;
				o.worldPos = worldPos;
				return o;
			}

			float4_t fragOutLine(OutlineVertexOutput IN, fixed facing : VFACE) : SV_Target
			{
				return GetMainOutputColor(IN, facing);
			}

			ENDCG

		}
    }
}
