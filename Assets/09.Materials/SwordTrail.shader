// 칼끝이 지나간 자리에 남는 빛 잔상(TrailRenderer)용 셰이더.
// 트레일의 폭 방향(uv.y) 가운데가 밝고 가장자리는 부드럽게 사라지며, 꼬리 쪽은 정점 색의 알파로 사라진다.
Shader "Custom/SwordTrail"
{
    Properties
    {
        [HDR] _Color ("잔상 색", Color) = (1.0, 0.6, 0.2, 1)
        _Intensity ("세기", Range(0, 4)) = 1.5
        _EdgeSoftness ("가장자리 부드러움", Range(0.5, 6)) = 2.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+1"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "SwordTrail"
            Tags { "LightMode" = "UniversalForward" }

            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Intensity;
                float _EdgeSoftness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 폭 방향 가운데(0.5)에서 1, 가장자리에서 0
                float across = 1.0 - abs(input.uv.y * 2.0 - 1.0);
                float soft = pow(saturate(across), _EdgeSoftness);

                // 꼬리(uv.x가 클수록 오래된 쪽)로 갈수록 정점 색 알파가 줄어든다.
                float fade = input.color.a;

                half3 rgb = _Color.rgb * input.color.rgb * soft * fade * _Intensity;
                return half4(rgb, 1);
            }
            ENDHLSL
        }
    }
}
