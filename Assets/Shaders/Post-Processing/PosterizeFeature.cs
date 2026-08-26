using UnityEngine;
using UnityEngine.Rendering;
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
        RTHandle m_Source;
        RTHandle m_TempRT;
        static readonly int StepCountId = Shader.PropertyToID("_StepCount");

        public void Setup(Material mat, int stepCount, bool debugLogging)
        {
            m_Material = mat;
            m_StepCount = stepCount;
            m_DebugLogging = debugLogging;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureInput(ScriptableRenderPassInput.Color);

            // Grab the target here (per-frame), NOT in AddRenderPasses.
            // On Auto intermediate-texture mode, the handle captured earlier
            // can be stale and the pass silently writes nowhere.
            m_Source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref m_TempRT, desc, name: "_PosterizeTempRT");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null)
                return;

            if (m_Source == null || m_TempRT == null)
                return;

            if (m_DebugLogging && Time.frameCount % 60 == 0)
                Debug.Log($"[PosterizeFeature] Execute camera={renderingData.cameraData.camera.name} steps={m_StepCount}");

            CommandBuffer cmd = CommandBufferPool.Get("Posterize");
            m_Material.SetFloat(StepCountId, m_StepCount);

            Blit(cmd, m_Source, m_TempRT, m_Material);
            Blit(cmd, m_TempRT, m_Source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }
}
