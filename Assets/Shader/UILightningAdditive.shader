Shader "UI/LightningAdditive"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BrightnessThreshold ("Brightness Threshold", Range(0,1)) = 0.1
        _Rows ("Rows", Float) = 2
        _Columns ("Columns", Float) = 2
        _Speed ("FPS", Float) = 8
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanvasCompatible"="True" }
        Lighting Off
        ZWrite Off
        Blend One One // Additive blending
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BrightnessThreshold;
            float _Rows;
            float _Columns;
            float _Speed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float totalFrames = _Rows * _Columns;
                float frame = floor(_Time.y * _Speed) % totalFrames;

                float row = floor(frame / _Columns);
                float col = frame % _Columns;

                float2 uvScale = float2(1.0 / _Columns, 1.0 / _Rows);
                float2 baseUV = uv * uvScale + float2(col, 1.0 - row - 1) * uvScale;

                float2 flippedUV = uv;
                flippedUV.y = 1.0 - flippedUV.y;
                flippedUV = flippedUV * uvScale + float2(col, 1.0 - row - 1) * uvScale;

                
                fixed4 col1 = tex2D(_MainTex, baseUV);
                
                fixed4 col2 = tex2D(_MainTex, flippedUV);

                 
                float b1 = dot(col1.rgb, float3(0.299, 0.587, 0.114));
                float b2 = dot(col2.rgb, float3(0.299, 0.587, 0.114));
                col1 = (b1 < _BrightnessThreshold) ? fixed4(0, 0, 0, 0) : col1;
                col2 = (b2 < _BrightnessThreshold) ? fixed4(0, 0, 0, 0) : col2;

                return col1 + col2;
            }
            ENDCG
        }
    }
}
