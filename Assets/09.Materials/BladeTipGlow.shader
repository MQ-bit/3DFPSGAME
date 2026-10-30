// 칼날 끝부분만 빛나게 하는 이펙트 셰이더.
// 검 메시와 같은 메시에 한 겹 덧씌워 쓴다. (칼날의 모양을 그대로 따라가고, 끝으로 갈수록 밝다)
Shader "Custom/BladeTipGlow"
{
    Properties
    {
        [HDR] _Color ("글로우 색", Color) = (1.0, 0.55, 0.15, 1)
        _Intensity ("세기 (스크립트가 조절)", Range(0, 3)) = 1
        _BladeBottomY ("글로우가 시작되는 높이 (메시 로컬 y)", Float) = 0.45
        _BladeTopY ("칼끝 높이 (메시 로컬 y)", Float) = 0.98
        _TipPower ("끝으로 몰리는 정도", Range(0.5, 6)) = 2.5
        _RimPower ("테두리 빛 (클수록 얇게)", Range(0.5, 8)) = 2.5
        _StreakScale ("빛줄기 촘촘함", Float) = 22
        _StreakSpeed ("빛줄기 속도", Float) = 7
        _Push ("메시를 살짝 부풀리는 양 (m)", Range(0, 0.02)) = 0.004
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "BladeTipGlow"
            Tags { "LightMode" = "UniversalForward" }

            Blend One One      // 더하기: 어두운 곳에서 더 또렷하게 빛난다.
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Intensity;
                float _BladeBottomY;
                float _BladeTopY;
                float _TipPower;
                float _RimPower;
                float _StreakScale;
                float _StreakSpeed;
                float _Push;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                // 원래 칼 위에 겹쳐 그려도 깜박이지 않게 법선 방향으로 아주 조금 부풀린다.
                float3 pushed = input.positionOS.xyz + normalize(input.normalOS) * _Push;
                float3 positionWS = TransformObjectToWorld(pushed);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionOS = input.positionOS.xyz;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. 칼끝 쪽일수록 1에 가까운 값 (칼날 아랫부분은 0)
                float along = saturate((input.positionOS.y - _BladeBottomY) / max(0.0001, _BladeTopY - _BladeBottomY));
                float tip = pow(along, _TipPower);

                // 2. 테두리(옆에서 볼수록) 밝게
                float3 n = normalize(input.normalWS);
                float3 v = normalize(input.viewDirWS);
                float rim = pow(1.0 - saturate(abs(dot(n, v))), _RimPower);

                // 3. 칼끝 쪽으로 흘러가는 빛줄기
                float streak = sin(input.positionOS.y * _StreakScale - _Time.y * _StreakSpeed) * 0.5 + 0.5;

                float glow = tip * (0.35 + 0.65 * rim + 0.35 * streak);
                return half4(_Color.rgb * glow * _Intensity, 1);
            }
            ENDHLSL
        }
    }
}
