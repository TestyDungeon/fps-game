using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class PosterizeFeature : ScriptableRendererFeature
{
    public enum CameraFilterMode
    {
        AllGameCameras,
        BaseOnly,
        CameraName
    }

    [System.Serializable]
    public class Settings
    {
        public Material posterizeMaterial;
        [Range(2, 128)] public int stepCount = 6;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public CameraFilterMode cameraFilterMode = CameraFilterMode.BaseOnly;
        public string cameraName = "Camera";
        public bool debugLogging;
    }

    public Settings settings = new Settings();
    PosterizePass m_Pass;

    public override void Create()
    {
        m_Pass = new PosterizePass
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;
        var camera = cameraData.camera;
        if (camera == null)
            return;

        // Skip editor and preview cameras to avoid null RTHandles/assertions in inspectors.
        if (cameraData.isPreviewCamera || cameraData.isSceneViewCamera || camera.cameraType != CameraType.Game)
            return;

        if (settings.cameraFilterMode == CameraFilterMode.BaseOnly && cameraData.renderType != CameraRenderType.Base)
            return;

        if (settings.cameraFilterMode == CameraFilterMode.CameraName)
        {
            if (string.IsNullOrWhiteSpace(settings.cameraName))
                return;

            if (!string.Equals(camera.name, settings.cameraName, System.StringComparison.Ordinal))
                return;
        }

        if (settings.posterizeMaterial == null)
            return;

        m_Pass.Setup(settings.posterizeMaterial, settings.stepCount, settings.debugLogging);
        var passEvent = settings.renderPassEvent;
        if (!cameraData.postProcessEnabled && passEvent >= RenderPassEvent.AfterRenderingPostProcessing)
            passEvent = RenderPassEvent.AfterRenderingTransparents;

        m_Pass.renderPassEvent = passEvent;

        if (settings.debugLogging && Time.frameCount % 60 == 0)
            Debug.Log($"[PosterizeFeature] Enqueue camera={camera.name} renderType={cameraData.renderType} postFX={cameraData.postProcessEnabled} event={passEvent} steps={settings.stepCount}");

        renderer.EnqueuePass(m_Pass);
    }

    class PosterizePass : ScriptableRenderPass
    {
        Material m_Material;
        int m_StepCount;
        bool m_DebugLogging;
        static readonly int StepCountId = Shader.PropertyToID("_StepCount");

        public void Setup(Material mat, int stepCount, bool debugLogging)
        {
            m_Material = mat;
            m_StepCount = stepCount;
            m_DebugLogging = debugLogging;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Material == null)
                return;

            UniversalResourceData resources = frameData.Get<UniversalResourceData>();
            TextureHandle source = resources.cameraColor;
            if (!source.IsValid())
                return;

            var descriptor = renderGraph.GetTextureDesc(source);
            descriptor.name = "_Posterize";
            descriptor.clearBuffer = false;
            TextureHandle destination = renderGraph.CreateTexture(descriptor);

            var properties = new MaterialPropertyBlock();
            properties.SetFloat(StepCountId, m_StepCount);
            renderGraph.AddBlitPass(
                new RenderGraphUtils.BlitMaterialParameters(source, destination, m_Material, 0)
                {
                    propertyBlock = properties
                },
                "Posterize");
            resources.cameraColor = destination;

            if (m_DebugLogging && Time.frameCount % 60 == 0)
                Debug.Log($"[PosterizeFeature] RenderGraph pass steps={m_StepCount}");
        }
    }
}
