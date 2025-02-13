using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SimpleBlitRenderPassFeature : ScriptableRendererFeature
{
    class SimpleBlitRenderPass : ScriptableRenderPass
    {
        public RTHandle source;
        private Material material;
        private RTHandle tempRenderTargetHandle;

        public SimpleBlitRenderPass(Material material) 
        {
            this.material = material;

            // Создаем RTHandle через Alloc
            tempRenderTargetHandle = RTHandles.Alloc(
                Vector2.one, 
                dimension: TextureDimension.Tex2D, 
                name: "_TemporaryColorTexture"
            );
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // Настроить рендер-таргет
            ConfigureTarget(tempRenderTargetHandle);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get("SimpleBlitRenderPass");

            // Используем новую версию Blit, принимающую RTHandle
            Blitter.BlitCameraTexture(commandBuffer, source, tempRenderTargetHandle, material, 0);
            Blitter.BlitCameraTexture(commandBuffer, tempRenderTargetHandle, source);

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            // Никаких дополнительных очисток не требуется
        }

        public void Dispose()
        {
            RTHandles.Release(tempRenderTargetHandle);
        }
    }

    [System.Serializable]
    public class SimpleBlitSettings 
    {
        public Material material = null;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public ClearFlag clearFlag = ClearFlag.None;
        public Color clearColor = Color.black;
    }

    public SimpleBlitSettings settings = new SimpleBlitSettings();
    SimpleBlitRenderPass simpleBlitRenderPass;

    public override void Create()
    {
        simpleBlitRenderPass = new SimpleBlitRenderPass(settings.material);
        simpleBlitRenderPass.renderPassEvent = settings.renderPassEvent;
        simpleBlitRenderPass.ConfigureClear(settings.clearFlag, settings.clearColor);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        simpleBlitRenderPass.source = renderer.cameraColorTargetHandle;
        renderer.EnqueuePass(simpleBlitRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            simpleBlitRenderPass?.Dispose();
        }
    }
}
