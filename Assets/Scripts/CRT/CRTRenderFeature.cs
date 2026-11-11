using System.Security.Cryptography;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace PSX
{
    public class CRTRenderFeature : ScriptableRendererFeature
    {
        CRTPass crtPass;
        Crt crt;
        public Material material;

        public override void Create()
        {
            crtPass = new CRTPass(RenderPassEvent.BeforeRenderingPostProcessing);
        }

        //ScripstableRendererFeature is an abstract class, you need this method
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            /*
            if(material == null)
            {
                Debug.LogWarning("CRTRenderFeature material is null and will be skipped.");
                return;
            }
            */

            //crtPass.Setup(material);
            var stack = VolumeManager.instance.stack;
            crt = stack.GetComponent<Crt>();

            if (crt == null || !crt.IsActive())
            {
                Debug.Log("No CRT. Deactivated.");
                return;
            }

            renderer.EnqueuePass(crtPass);
        }

    }


    public class CRTPass : ScriptableRenderPass
    {
        private static readonly string shaderPath = "PostEffect/CRTShader";
        static readonly string k_RenderTag = "Render CRT Effects";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int TempTargetId = Shader.PropertyToID("_TempTargetCRT");

        static readonly int ScanLinesWeight = Shader.PropertyToID("_ScanlinesWeight");
        static readonly int NoiseWeight = Shader.PropertyToID("_NoiseWeight");
        
        static readonly int ScreenBendX = Shader.PropertyToID("_ScreenBendX");
        static readonly int ScreenBendY = Shader.PropertyToID("_ScreenBendY");
        static readonly int VignetteAmount = Shader.PropertyToID("_VignetteAmount");
        static readonly int VignetteSize = Shader.PropertyToID("_VignetteSize");
        static readonly int VignetteRounding = Shader.PropertyToID("_VignetteRounding");
        static readonly int VignetteSmoothing = Shader.PropertyToID("_VignetteSmoothing");

        static readonly int ScanLinesDensity = Shader.PropertyToID("_ScanLinesDensity");
        static readonly int ScanLinesSpeed = Shader.PropertyToID("_ScanLinesSpeed");
        static readonly int NoiseAmount = Shader.PropertyToID("_NoiseAmount");

        static readonly int ChromaticRed = Shader.PropertyToID("_ChromaticRed");
        static readonly int ChromaticGreen = Shader.PropertyToID("_ChromaticGreen");
        static readonly int ChromaticBlue = Shader.PropertyToID("_ChromaticBlue");
        
        static readonly int GrilleOpacity = Shader.PropertyToID("_GrilleOpacity");
        static readonly int GrilleCounterOpacity = Shader.PropertyToID("_GrilleCounterOpacity");
        static readonly int GrilleResolution = Shader.PropertyToID("_GrilleResolution");
        static readonly int GrilleCounterResolution = Shader.PropertyToID("_GrilleCounterResolution");
        static readonly int GrilleBrightness = Shader.PropertyToID("_GrilleBrightness");
        static readonly int GrilleUvRotation = Shader.PropertyToID("_GrilleUvRotation");
        static readonly int GrilleUvMidPoint = Shader.PropertyToID("_GrilleUvMidPoint");
        static readonly int GrilleShift = Shader.PropertyToID("_GrilleShift");

        Crt m_Crt;
        Material crtMaterial;
        RenderTargetIdentifier currentTarget;


        private readonly CRTRenderFeature _renderFeature;

        public CRTPass(CRTRenderFeature renderFeature)
        {
            _renderFeature = renderFeature;
        }

        public CRTPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                Debug.LogError("Shader not found (crt).");
                return;
            }

            this.crtMaterial = CoreUtils.CreateEngineMaterial(shader);
            requiresIntermediateTexture = true;
        }

        public void Setup(Material mat)
        {
            crtMaterial = mat;
            requiresIntermediateTexture = true;
        }


        public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
        {
            var res = frameData.Get<UniversalResourceData>();
            // Early-out if the current active target is the back buffer or the color texture is invalid
            if (res.isActiveTargetBackBuffer || !res.activeColorTexture.IsValid())
                return;

            // Fetch volume component and validate material/shader
            var stack = VolumeManager.instance.stack;
            var crt = stack.GetComponent<Crt>();
            if (crt == null || !crt.IsActive() || crtMaterial == null || !crtMaterial.shader)
                return;

            // Push Volume parameter values into the material
            var mat = crtMaterial;
            mat.SetFloat(ScanLinesWeight, crt.scanlinesWeight.value);
            mat.SetFloat(NoiseWeight, crt.noiseWeight.value);
            mat.SetFloat(ScreenBendX, crt.screenBendX.value);
            mat.SetFloat(ScreenBendY, crt.screenBendY.value);
            mat.SetFloat(VignetteAmount, crt.vignetteAmount.value);
            mat.SetFloat(VignetteSize, crt.vignetteSize.value);
            mat.SetFloat(VignetteRounding, crt.vignetteRounding.value);
            mat.SetFloat(VignetteSmoothing, crt.vignetteSmoothing.value);
            mat.SetFloat(ScanLinesDensity, crt.scanlinesDensity.value);
            mat.SetFloat(ScanLinesSpeed, crt.scanlinesSpeed.value);
            mat.SetFloat(NoiseAmount, crt.noiseAmount.value);
            mat.SetVector(ChromaticRed, crt.chromaticRed.value);
            mat.SetVector(ChromaticGreen, crt.chromaticGreen.value);
            mat.SetVector(ChromaticBlue, crt.chromaticBlue.value);
            mat.SetFloat(GrilleOpacity, crt.grilleOpacity.value);
            mat.SetFloat(GrilleCounterOpacity, crt.grilleCounterOpacity.value);
            mat.SetFloat(GrilleResolution, crt.grilleResolution.value);
            mat.SetFloat(GrilleCounterResolution, crt.grilleCounterResolution.value);
            mat.SetFloat(GrilleBrightness, crt.grilleBrightness.value);
            mat.SetFloat(GrilleUvRotation, crt.grilleUvRotation.value);
            mat.SetFloat(GrilleUvMidPoint, crt.grilleUvMidPoint.value);
            mat.SetVector(GrilleShift, crt.grilleShift.value);

            // Create destination texture (same desc as source) and enqueue a blit pass
            var src = res.activeColorTexture;
            var desc = rg.GetTextureDesc(src);
            desc.name = "CRT-Output";
            desc.clearBuffer = false;
            var dst = rg.CreateTexture(desc);

            // Use pass=1 for pure copy debugging, pass=0 for the CRT effect
            int passIndex = 0; // set to 1 to test the "pure copy" pass
            var para = new RenderGraphUtils.BlitMaterialParameters(src, dst, mat, passIndex);
            rg.AddBlitPass(para, passName: passIndex == 0 ? "CRT Effect" : "CRT Debug Copy");

            // Hand the result back to the pipeline for subsequent passes
            res.cameraColor = dst;
        }

    }
}