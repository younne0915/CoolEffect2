Shader "younne/ShadowValid"
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
			Tags { "LightMode" = "ForwardBase" }

			//Cull[_Culling]
			//Blend[_SrcBlend][_DstBlend]
			//ZWrite On

			CGPROGRAM
			#pragma multi_compile_fwdbase	
			#pragma vertex vert
			#pragma fragment frag

			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			struct a2v {
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				SHADOW_COORDS(0)
			};

			v2f vert(a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_SHADOW(o);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				fixed shadow = SHADOW_ATTENUATION(i) * 10;
				return fixed4(shadow, shadow, shadow, 1);
			}

			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			//Cull[_Culling]
			//Blend[_SrcBlend][_DstBlend]
			//ZWrite On

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
