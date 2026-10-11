Shader "Untitled/HD2D/Cave Water"
{
    Properties
    {
        _DeepColor("Deep water", Color) = (0.025, 0.13, 0.17, 1)
        _ShallowColor("Shallow water", Color) = (0.09, 0.31, 0.34, 1)
        _FoamColor("Shore ripples", Color) = (0.32, 0.57, 0.55, 1)
        _PixelsPerUnit("Water pixel density", Float) = 32
        _FlowSpeed("Flow speed", Range(0, 2)) = 0.65
        _Smoothness("Smoothness", Range(0, 1)) = 0.86
        _CellSize("Cell size", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "WaterForward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #define _SPECULAR_SETUP 1
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _FoamColor;
                float _PixelsPerUnit;
                float _FlowSpeed;
                half _Smoothness;
                float _CellSize;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 banks : COLOR;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                half4 banks : TEXCOORD2;
                half fog : TEXCOORD3;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = input.uv;
                output.banks = input.banks;
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // World coordinates keep waves continuous across cells and chunk boundaries.
                float2 p = floor(input.positionWS.xz * _PixelsPerUnit) / _PixelsPerUnit;
                float t = _Time.y * _FlowSpeed;
                float a = p.x * 5.5 + p.y * 2.0 + t;
                float b = p.y * 8.0 - p.x * 1.5 - t * 1.3;
                float wave = sin(a) * sin(b);
                float ripple = step(0.88, wave);
                float2 slope = float2(5.5 * cos(a) * sin(b) - 1.5 * sin(a) * cos(b),
                    2.0 * cos(a) * sin(b) + 8.0 * sin(a) * cos(b));
                half3 normalWS = normalize(float3(-slope.x * 0.025, 1, -slope.y * 0.025));

                float4 distances = lerp(float4(10, 10, 10, 10),
                    float4(input.uv.x, 1 - input.uv.x, input.uv.y, 1 - input.uv.y), input.banks);
                float shoreDistance = min(min(distances.x, distances.y), min(distances.z, distances.w)) * _CellSize;
                float shallow = 1 - smoothstep(0.0, 0.35, shoreDistance);
                float foamWidth = 0.035 + sin(t * 1.4 + p.x * 2 + p.y * 3) * 0.01;
                float foam = (1 - step(foamWidth, shoreDistance)) * (0.5 + 0.5 * sin(a));

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = lerp(_DeepColor.rgb, _ShallowColor.rgb, saturate(shallow * 0.65 + wave * 0.08 + 0.08));
                surface.albedo = lerp(surface.albedo, _FoamColor.rgb, saturate(foam + ripple * 0.45));
                surface.specular = half3(0.045, 0.045, 0.045);
                surface.smoothness = _Smoothness;
                surface.normalTS = half3(0, 0, 1);
                surface.occlusion = 1;
                surface.alpha = 1;
                surface.emission = _ShallowColor.rgb * (0.045 + ripple * 0.08);

                InputData lighting = (InputData)0;
                lighting.positionWS = input.positionWS;
                lighting.normalWS = normalWS;
                lighting.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lighting.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                lighting.bakedGI = SampleSH(normalWS);
                lighting.vertexLighting = VertexLighting(input.positionWS, normalWS);
                lighting.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                lighting.shadowMask = half4(1, 1, 1, 1);
                half4 color = UniversalFragmentPBR(lighting, surface);
                color.rgb = MixFog(color.rgb, input.fog);
                return color;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
