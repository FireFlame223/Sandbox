Shader "Hidden/EdgeDetection"
{
        SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "ColorBlitPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            float _Intensity;

            // half4 frag (Varyings input) : SV_Target
            // {
            //     float4 color = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, input.texcoord);
            //     return float4(color.r, 0, 0, 1);
            // }

            half4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                float scale = 1;
                // Lees centrale en buurpixels
                float c = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv).r;
                float l = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(-_BlitTexture_TexelSize.x * scale, 0)).r;
                float r = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(_BlitTexture_TexelSize.x * scale, 0)).r;
                float u = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(0, _BlitTexture_TexelSize.y * scale)).r;
                float d = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(0, -_BlitTexture_TexelSize.y * scale)).r;

                // Detecteer verschillen (alleen 0 of 1)
                float edge = step(0.2, abs(c - l) + abs(c - r) + abs(c - u) + abs(c - d));
                edge = (abs(c - l) + abs(c - r) + abs(c - u) + abs(c - d));
                edge = edge / 1.0;
                edge = edge * edge * 2; 

                return float4(edge, edge, edge, (edge > 0)?1:0);
            }
            ENDHLSL
        }
    }
}