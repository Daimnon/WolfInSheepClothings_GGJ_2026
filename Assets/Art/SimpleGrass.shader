Shader "Custom/URP_SimpleGrass"
{
    Properties
    {
        _MainTex ("Grass Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        _WindSpeed ("Wind Speed", Range(0, 10)) = 2
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "RenderPipeline" = "UniversalPipeline" "Queue"="AlphaTest" }
        LOD 100
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _BaseColor;
            float _Cutoff;
            float _WindSpeed;
            float _WindStrength;

            Varyings vert (Attributes v)
            {
                Varyings o;
                
                // Wind Math
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                float wind = sin(_Time.y * _WindSpeed + worldPos.x + worldPos.z);
                
                // Mask by UV.y so roots don't move
                v.positionOS.x += wind * _WindStrength * v.uv.y;

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv) * _BaseColor;
                
                // Alpha Clipping
                clip(texColor.a - _Cutoff);
                
                return texColor;
            }
            ENDHLSL
        }
    }
}