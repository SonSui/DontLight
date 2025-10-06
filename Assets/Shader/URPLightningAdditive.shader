Shader "URP/LightningAdditive"
{
        Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,10)) = 1
        _BrightnessThreshold ("Brightness Threshold", Range(0,1)) = 0.1
        _Rows ("Rows", Float) = 2
        _Columns ("Columns", Float) = 2
        _Speed ("FPS", Float) = 8
        [Enum(LEqual,4, Always,8)] _ZTestMode ("ZTest", Float) = 4
        [Toggle(_FORCE_LOD0)] _ForceLOD0 ("Force LOD0 (No Mip)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "UniversalMaterialType"="Unlit" }
        LOD 100

        Pass
        {
            Name "UnlitAdditive"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            Blend One One
            ZTest [_ZTestMode]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _FORCE_LOD0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Tint;
            float  _Intensity;
            float  _BrightnessThreshold;
            float  _Rows;
            float  _Columns;
            float  _Speed;

            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings   { float4 positionHCS:SV_POSITION; float2 uv:TEXCOORD0; };

            Varyings vert(Attributes v){
                Varyings o;
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            float luminance_linear(float3 c){ return dot(c, float3(0.2126, 0.7152, 0.0722)); }

            half4 frag(Varyings i) : SV_Target
            {
                float total = max(1.0, _Rows * _Columns);
                float frame = fmod(floor(_Time.y * _Speed), total);
                float row   = floor(frame / _Columns);
                float col   = frame - row * _Columns;

                float2 uvScale = float2(1.0/_Columns, 1.0/_Rows);
                float2 baseUV  = i.uv * uvScale + float2(col, (_Rows - 1.0 - row)) * uvScale;
                float2 flipUV  = float2(i.uv.x, 1.0 - i.uv.y) * uvScale + float2(col, (_Rows - 1.0 - row)) * uvScale;

                float4 c1;
                float4 c2;
                #ifdef _FORCE_LOD0
                    c1 = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, baseUV, 0);
                    c2 = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, flipUV, 0);
                #else
                    c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, baseUV);
                    c2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, flipUV);
                #endif

                float b1 = luminance_linear(c1.rgb);
                float b2 = luminance_linear(c2.rgb);
                if (b1 < _BrightnessThreshold) c1 = 0;
                if (b2 < _BrightnessThreshold) c2 = 0;

                float4 colOut = (c1 + c2) * _Tint * _Intensity;
                colOut.a = saturate(max(c1.a, c2.a) * _Tint.a);
                return colOut;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
