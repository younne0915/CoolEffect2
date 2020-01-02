Shader "younne/MeltEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_AlbedoColor("Color", Color) = (1,1,1,1)
		_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Bump Scale", float) = 1.0
		_Specular("Specular", Color) = (1,1,1,1)
		_Gloss("Gloss", Range(8.0, 256)) = 20
		_DissolveColor("DissolveColor", Color) = (1,1,1,1)
		_ColorFactor("ColorFactor", Range(0, 0.08)) = 0.08
		_DissolveThreshold("DissolveThreshold", Float) = 0
	}
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

        Pass
        {
			Tags { "LightMode" = "ForwardBase"  }
			Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
			#include "Lighting.cginc"

			#pragma multi_compile_fwdbase
			// shadow helper functions and macros
			 #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				half3 normal    : NORMAL;
				half4 tangent : TANGENT;
				float2 uv1      : TEXCOORD1;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 pos : SV_POSITION;
				float3 tangentLightDir : TEXCOORD2;
				float3 tangentViewDir : TEXCOORD3;
				float3 worldPos : TEXCOORD4;
				SHADOW_COORDS(5)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _AlbedoColor;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			float _BumpScale;
			fixed4 _Specular;
			float _Gloss;
			fixed4 _DissolveColor;

			uniform fixed _ColorFactor;
			uniform fixed _DissolveThreshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.uv, _BumpMap);
				half sign = half(v.tangent.w) * half(unity_WorldTransformParams.w);
				float3 bioNormal = cross(v.normal, v.tangent) * sign;
				float3x3 matrixRot = float3x3(v.tangent.xyz, bioNormal, v.normal);
				o.tangentLightDir = mul(matrixRot, ObjSpaceLightDir(v.vertex)).xyz;
				o.tangentViewDir = mul(matrixRot, ObjSpaceViewDir(v.vertex)).xyz;
                UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float factor = i.worldPos.y - _DissolveThreshold;
				clip(factor);

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

				fixed shadow = 1;
				shadow = SHADOW_ATTENUATION(i);

				col = fixed4(ambient + (diffuse + specular) * shadow, 1);
				UNITY_APPLY_FOG(i.fogCoord, col);

				fixed lerpFactor = saturate(sign(_ColorFactor - factor));
				return lerpFactor * _DissolveColor + (1 - lerpFactor) * col;
            }
            ENDCG
        }

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f
			{
				float3 vertex : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}
    }
}
