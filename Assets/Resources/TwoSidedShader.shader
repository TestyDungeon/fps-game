Shader "Custom/TwoSidedDifferent"
{
    Properties
    {
        _FrontColor ("Front Color", Color) = (1,1,1,1)
        _BackColor ("Back Color", Color) = (1,0,0,1)
        _FrontTex ("Front Texture", 2D) = "white" {}
        _BackTex ("Back Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
    
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };
        
            TEXTURE2D(_FrontTex); SAMPLER(sampler_FrontTex);
            TEXTURE2D(_BackTex);  SAMPLER(sampler_BackTex);
            float4 _FrontColor;
            float4 _BackColor; // alpha channel matters now
        
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }
        
            half4 frag(Varyings IN, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                half4 tex = isFrontFace
                    ? SAMPLE_TEXTURE2D(_FrontTex, sampler_FrontTex, IN.uv) * _FrontColor
                    : SAMPLE_TEXTURE2D(_BackTex, sampler_BackTex, IN.uv) * _BackColor;
                return tex; // alpha carries the transparency
            }
            ENDHLSL
        }
    }
}