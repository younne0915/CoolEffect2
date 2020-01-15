// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

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
        _OutlineThickness("OutlineThickness", Range(0, 10)) = 0
        _OutlinePatternSize("OutlinePatternSize", Range(0, 256)) = 0
        _OutlineDepthBias("OutlineDepthBias", Range(0, 0.05)) = 0
        _OutlineScaleMax("OutlineScaleMax", Range(0, 256)) = 0
        [NoScaleOffset] _OutlinePattern("Outline Pattern", 2D) = "" {}
        _OutlineEffectColor("_OutlineEffectColor", Color) = (1,1,1,1)
        _OutlineColor("OutlineColor", Color) = (1,1,1,1)

    }
    SubShader
    {

        Pass
        {
            Tags
            {
                "RenderType" = "Opaque"
                "LightMode" = "ForwardBase"
            }

            ZWrite On
            Cull Front
            //Cull Back

            CGPROGRAM
            #pragma vertex vertOutLine
            #pragma fragment fragOutLine
            //#pragma target 3.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "UnityShaderVariables.cginc"

            struct OutlineVertexOutput
            {
                float4 pos : SV_POSITION;
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
            float _OutlineScaleMax = 50.0f;

            sampler2D _OutlinePattern;
            float4 _OutlineEffectColor;
            float4 _OutlineColor;

            sampler2D _MainTex;
            float4 _MainTex_ST;

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

            float3_t BlendLinearBurn(float3_t a, float3_t b)
            {
                return a + b - float3_t(1.0h, 1.0h, 1.0h);
            }

            OutlineVertexOutput vertOutLine(OutlineVertexInput v) {

                OutlineVertexOutput o;

                v.vertex.xyz += v.normal * _OutlineDepthBias;
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;

                //float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos).xyz;
                //float3 offsetVector = _OutlineDepthBias * viewDir;

                //float3 worldnormal = mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz;
                //float3 offsetVector = worldnormal * _OutlineDepthBias;
                //worldPos += offsetVector;
                //float4 projPos = UnityObjectToClipPos(v.vertex);
                float4 projPos = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                //float4 projviewdir = mul(UNITY_MATRIX_VP, float4(viewDir, 0));
                //float4 offsetprojviewvector =  projviewdir * _OutlineDepthBias;
                //projPos += offsetprojviewvector;

                float3 normal = v.normal;
                float4 projNormal = normalize(UnityObjectToClipPos(float4(normal, 0)));

                // Scale the normal to be the size of one pixel, but not to exceed kOutlineNormalScaleMax
                // so the edge doesn't get too thick when the character is too far away
                
                float wScale = sign(projPos.w) * min(abs(projPos.w), _OutlineScaleMax);
                
                const float kOutlineThicknessScale = 1.6f;
                float2 halfScreenSize = 0.5f * _ScreenParams.xy;
                float4 normalScale = (kOutlineThicknessScale * _OutlineThickness * wScale) / halfScreenSize.xyxx;

                projNormal *= normalScale;
                projPos += projNormal;

                o.pos = projPos;

                float2 texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                o.UV = float4(texcoord, projPos.xy / (projPos.w * _OutlinePatternSize));
                o.normal = mul(unity_ObjectToWorld, float4(normal, 0)).xyz;
                o.worldPos = worldPos;
                return o;
            }

            float4_t fragOutLine(OutlineVertexOutput IN, fixed facing : VFACE) : SV_Target
            {
                float4_t finalColor;

                float4_t baseColor = tex2D(_MainTex, IN.UV.xy);
                float3_t outlinePatternColor = tex2D(_OutlinePattern, IN.UV.zw).rgb;
                float_t opacity = baseColor.a;

                float3_t originalColor = baseColor.rgb;
                /*float3_t outputColor = saturate(originalColor + _OutlineColor - float3_t(1.0h, 1.0h, 1.0h));
                outputColor = lerp(originalColor, outputColor, outlinePatternColor.r);*/

                //outputColor = lerp(outputColor, _OutlineEffectColor.rgb, _OutlineEffectColor.a);

                float3_t outputColor = lerp(originalColor, _OutlineEffectColor.rgb, 0.5);

                finalColor = float4_t(outputColor, opacity);

                return finalColor;
            }

            ENDCG
        }


        
        Pass
        {        
            Tags
            {
                "RenderType" = "Opaque" 
                "LightMode" = "ForwardBase"
            }

            //Cull Back
            Blend One Zero
            ZWrite On

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