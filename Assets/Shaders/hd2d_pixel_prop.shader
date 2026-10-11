Shader "Untitled/HD2D Pixel Prop"
{
 Properties
 {
  [MainTexture] _BaseMap("Original image",2D)="white"{}
  _FrameRect("Frame UV rectangle",Vector)=(0,0,1,1)
  _Cutoff("Alpha cutoff",Range(0,1))=.45
  _WhiteKey("Remove white reference background",Float)=0
  _WhiteThreshold("White background cutoff",Range(0,1))=.94
  _WhiteNeutralTolerance("Neutral background tolerance",Range(0,.2))=0
  _Lighting("Scene lighting influence",Range(0,1))=.65
  _Emission("Flame glow",Range(0,4))=0
  _FlameMin("Flame UV min",Float)=.55
  _FlameMax("Flame UV max",Float)=1
 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
  Cull Off ZWrite On
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
  TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
  CBUFFER_START(UnityPerMaterial)
   float4 _FrameRect;
   float _Cutoff,_WhiteKey,_WhiteThreshold,_WhiteNeutralTolerance,_Lighting,_Emission,_FlameMin,_FlameMax;
  CBUFFER_END
  struct Attr {float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0;};
  struct Var {float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 positionWS:TEXCOORD1; float3 normalWS:TEXCOORD2;};
  half4 Pixel(float2 uv) {
   half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,_FrameRect.xy+uv*_FrameRect.zw);
   clip(c.a-_Cutoff);
   // The large container and vegetable references have a white JPEG background.
   if(_WhiteKey>.5) {
    half minimum=min(c.r,min(c.g,c.b));
    half chroma=max(c.r,max(c.g,c.b))-minimum;
    // JPEG compression adds grey edge pixels. Restrict the stronger key to neutral colours,
    // preserving the blue water and cool stone highlights inside the container.
    if(_WhiteNeutralTolerance<=0 || chroma<=_WhiteNeutralTolerance) clip(_WhiteThreshold-minimum);
   }
   return c;
  }
  Var Vert(Attr i) {
   Var o; o.positionWS=TransformObjectToWorld(i.positionOS.xyz);
   o.positionCS=TransformWorldToHClip(o.positionWS);
   o.normalWS=TransformObjectToWorldNormal(i.normalOS); o.uv=i.uv; return o;
  }
  half4 Depth(Var i):SV_Target { Pixel(i.uv); return 0; }
  ENDHLSL
  Pass
  {
   Name "ForwardLit"
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile _ _ADDITIONAL_LIGHTS
   #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fog
   half4 Frag(Var i,FRONT_FACE_TYPE frontFace:FRONT_FACE_SEMANTIC):SV_Target {
    half4 c=Pixel(i.uv);
    half3 n=normalize(i.normalWS)*IS_FRONT_VFACE(frontFace,1,-1);
    Light key=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
    half3 lit=SampleSH(n)+key.color*(.3+.7*saturate(dot(n,key.direction)))*key.shadowAttenuation;
    #if defined(_ADDITIONAL_LIGHTS)
    uint count=GetAdditionalLightsCount();
    for(uint j=0;j<count;j++) {
     Light light=GetAdditionalLight(j,i.positionWS);
     lit+=light.color*light.distanceAttenuation*light.shadowAttenuation*(.3+.7*saturate(dot(n,light.direction)));
    }
    #endif
    half3 color=c.rgb*lerp(half3(1,1,1),clamp(lit,.18,2.2),_Lighting);
    // Only orange pixels in the actual flame region glow. Direction frames are never fire animation frames.
    half flame=step(_FlameMin,i.uv.y)*step(i.uv.y,_FlameMax)
      *step(.45,c.r)*step(.09,c.g)*step(c.b,c.r*.45)*step(c.g,c.r*.87);
    color+=c.rgb*flame*_Emission*(.92+.08*sin(_Time.y*8.1)+.04*sin(_Time.y*13.7));
    color=MixFog(color,ComputeFogFactor(i.positionCS.z));
    return half4(color,1);
   }
   ENDHLSL
  }
  Pass
  {
   Name "DepthOnly"
   Tags {"LightMode"="DepthOnly"}
   ColorMask 0
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Depth
   ENDHLSL
  }
  Pass
  {
   Name "ShadowCaster"
   Tags {"LightMode"="ShadowCaster"}
   ColorMask 0
   HLSLPROGRAM
   #pragma vertex ShadowVert
   #pragma fragment Depth
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   float3 _LightDirection;
   float3 _LightPosition;
   Var ShadowVert(Attr i) {
    Var o=Vert(i);
    float3 dir=_LightDirection;
    #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
    dir=normalize(_LightPosition-o.positionWS);
    #endif
    o.positionCS=TransformWorldToHClip(ApplyShadowBias(o.positionWS,o.normalWS,dir));
    #if UNITY_REVERSED_Z
    o.positionCS.z=min(o.positionCS.z,UNITY_NEAR_CLIP_VALUE);
    #else
    o.positionCS.z=max(o.positionCS.z,UNITY_NEAR_CLIP_VALUE);
    #endif
    return o;
   }
   ENDHLSL
  }
 }
}
