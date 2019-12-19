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
		_FlashTex("FlashTex", 2D) = "white" {}
		_FlashColor("Color", Color) = (1,1,1,1)
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _AlbedoColor;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			float _BumpScale;
			fixed4 _Specular;
			float _Gloss;
			fixed4 _FlashColor;

			sampler2D _FlashTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);


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

				float2 flashUv = i.uv;
                // sample the texture
				flashUv.xy += _Time.y;
				fixed4 flashCol = tex2D(_FlashTex, flashUv.xy) * _FlashColor;

                fixed4 col = tex2D(_MainTex, i.uv.xy) * _AlbedoColor;
				fixed4 packedNormal = tex2D(_BumpMap, i.uv.zw);
				fixed3 tangentNormal = UnpackNormal(packedNormal);
				tangentNormal.xy *= _BumpScale;
				tangentNormal.z = sqrt(1 - saturate(dot(tangentNormal.xy, tangentNormal.xy)));
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * col;
				fixed3 diffuse = _LightColor0.rgb * col * max(0, dot(tangentNormal, tangentLightDir));
				fixed3 halfDir = normalize(tangentLightDir + tangentViewDir);
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(tangentNormal, halfDir)), _Gloss);
                // apply fog
				col = fixed4(ambient + diffuse + specular + flashCol, 1);
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
