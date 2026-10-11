Shader "Untitled/HD2D/Cave Pixel Prop"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.45
        _FrameRect("Frame UV rectangle", Vector) = (0,0,1,1)
        _WhiteKey("Remove white background", Float) = 0
        _WhiteThreshold("White background cutoff", Range(0,1)) = 0.94
        _WhiteNeutralTolerance("Neutral background tolerance", Range(0,0.2)) = 0
        _Emission("Flame glow", Range(0,4)) = 0
        _FlameMin("Flame UV min", Float) = 0.55
        _FlameMax("Flame UV max", Float) = 1
        _Smoothness("Smoothness", Range(0,1)) = 0
        _Metallic("Metallic", Range(0,1)) = 0
        _MetallicGlossMap("Metallic", 2D) = "white" {}
        _SpecColor("Specular", Color) = (0.2,0.2,0.2,1)
        _SpecGlossMap("Specular", 2D) = "white" {}
        _BumpMap("Normal", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1
        _OcclusionMap("Occlusion", 2D) = "white" {}
        _OcclusionStrength("Occlusion Strength", Range(0,1)) = 1
        _EmissionMap("Emission", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,1)
        [HideInInspector] _Surface("Surface", Float) = 0
        [HideInInspector] _Cull("Cull", Float) = 0
        [HideInInspector] _AlphaClip("Alpha Clip", Float) = 0
        [HideInInspector] _ReceiveShadows("Receive Shadows", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "RenderPipeline"="UniversalPipeline" "UniversalMaterialType"="Lit" }
        Cull [_Cull]
        ZWrite On

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

        // HybridProp sets the frame per renderer; shared materials remain immutable at runtime.
        float4 _FrameRect;
        float _WhiteKey, _WhiteThreshold, _WhiteNeutralTolerance;
        float _Emission, _FlameMin, _FlameMax;
        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            half4 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            float2 uv : TEXCOORD2;
            half wall : TEXCOORD3;
            half fog : TEXCOORD4;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };
        Varyings Vert(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
            output.positionCS = TransformWorldToHClip(output.positionWS);
            output.normalWS = TransformObjectToWorldNormal(input.normalOS);
            output.uv = input.uv;
            output.wall = input.color.r;
            output.fog = ComputeFogFactor(output.positionCS.z);
            return output;
        }
        void ClipPixel(Varyings input)
        {
            half4 color = SampleAlbedoAlpha(_FrameRect.xy + input.uv * _FrameRect.zw, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
            clip(color.a - _Cutoff);
            if (_WhiteKey > 0.5)
            {
                half minimum = min(color.r, min(color.g, color.b));
                half chroma = max(color.r, max(color.g, color.b)) - minimum;
                if (_WhiteNeutralTolerance <= 0 || chroma <= _WhiteNeutralTolerance) clip(_WhiteThreshold - minimum);
            }
        }
        half4 ForwardFrag(Varyings input, FRONT_FACE_TYPE frontFace : FRONT_FACE_SEMANTIC) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            ClipPixel(input);
            SurfaceData surface;
            InitializeStandardLitSurfaceData(_FrameRect.xy + input.uv * _FrameRect.zw, surface);
            half flame = step(_FlameMin, input.uv.y) * step(input.uv.y, _FlameMax)
                * step(0.45, surface.albedo.r) * step(0.09, surface.albedo.g)
                * step(surface.albedo.b, surface.albedo.r * 0.45) * step(surface.albedo.g, surface.albedo.r * 0.87);
            surface.emission += surface.albedo * flame * _Emission * (0.94 + 0.06 * sin(_Time.y * 8.1));
            InputData lighting = (InputData)0;
            lighting.positionWS = input.positionWS;
            lighting.normalWS = NormalizeNormalPerPixel(input.normalWS) * IS_FRONT_VFACE(frontFace, 1, -1);
            lighting.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
            lighting.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
            lighting.bakedGI = SampleSH(lighting.normalWS);
            lighting.vertexLighting = VertexLighting(input.positionWS, lighting.normalWS);
            lighting.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
            lighting.shadowMask = half4(1,1,1,1);
            half4 color = UniversalFragmentPBR(lighting, surface);
            color.rgb = MixFog(color.rgb, input.fog);
            return color;
        }
        half4 DepthFrag(Varyings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            ClipPixel(input);
            return input.positionCS.zzzz;
        }
        half4 NormalsFrag(Varyings input, FRONT_FACE_TYPE frontFace : FRONT_FACE_SEMANTIC) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            ClipPixel(input);
            half3 normalWS = NormalizeNormalPerPixel(input.normalWS) * IS_FRONT_VFACE(frontFace, 1, -1);
            #if defined(_GBUFFER_NORMALS_OCT)
                float2 oct = PackNormalOctQuadEncode(normalWS) * 0.5 + 0.5;
                return half4(PackFloat2To888(oct), 0);
            #else
                return half4(normalWS, 0);
            #endif
        }
        float3 _LightDirection;
        float3 _LightPosition;
        Varyings ShadowVert(Attributes input)
        {
            Varyings output = Vert(input);
            float3 direction = _LightDirection;
            #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                direction = normalize(_LightPosition - output.positionWS);
            #endif
            output.positionCS = TransformWorldToHClip(ApplyShadowBias(output.positionWS, output.normalWS, direction));
            #if UNITY_REVERSED_Z
                output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
            return output;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForwardOnly" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment ForwardFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fog
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _RECEIVE_SHADOWS_OFF
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ColorMask R
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormalsOnly" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment NormalsFrag
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ColorMask 0
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowVert
            #pragma fragment DepthFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
