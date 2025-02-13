using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experimental.Rendering.Universal
{
    /// <summary>
    /// Copy the given color buffer to the given destination color buffer.
    /// You can use this pass to copy a color buffer to the destination,
    /// so you can use it later in rendering. For example, you can copy
    /// the opaque texture to use it for distortion effects.
    /// </summary>
    internal class BlitMultiPass : ScriptableRenderPass
    {
        public enum RenderTarget
        {
            Color,
            RenderTexture,
        }

        public Material blitMaterial = null;
        public int blitShaderPassIndex = 0;
        public FilterMode filterMode { get; set; }

        private RTHandle source { get; set; }
        private RTHandle destination { get; set; }

        RTHandle m_TemporaryColorTexture;
        string m_ProfilerTag;

        private int blurPasses;
        private int downsample;

        RTHandle tmpRT1;
        RTHandle tmpRT2;

        /// <summary>
        /// Create the CopyColorPass
        /// </summary>
        public BlitMultiPass(RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag, int blurPasses, int downsample)
        {
            this.renderPassEvent = renderPassEvent;
            this.blitMaterial = blitMaterial;
            this.blitShaderPassIndex = blitShaderPassIndex;
            m_ProfilerTag = tag;

            this.blurPasses = blurPasses;
            this.downsample = downsample;

            m_TemporaryColorTexture = RTHandles.Alloc(
                "_TemporaryColorTexture",
                name: "_TemporaryColorTexture"
            );
        }

        /// <summary>
        /// Configure the pass with the source and destination to execute on.
        /// </summary>
        /// <param name="source">Source Render Target</param>
        /// <param name="destination">Destination Render Target</param>
        public void Setup(RTHandle source, RTHandle destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var width = cameraTextureDescriptor.width / downsample;
            var height = cameraTextureDescriptor.height / downsample;

            int tmpId1 = Shader.PropertyToID("tmpBlurRT1");
            int tmpId2 = Shader.PropertyToID("tmpBlurRT2");
            cmd.GetTemporaryRT(tmpId1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(tmpId2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            tmpRT1 = RTHandles.Alloc(tmpId1);
            tmpRT2 = RTHandles.Alloc(tmpId2);

            ConfigureTarget(tmpRT1);
            ConfigureTarget(tmpRT2);
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // First pass
            cmd.Blit(source, tmpRT1, blitMaterial);

            for (var i = 1; i < blurPasses - 1; i++)
            {
                cmd.Blit(tmpRT1, tmpRT2, blitMaterial);

                // Ping-pong
                var rttmp = tmpRT1;
                tmpRT1 = tmpRT2;
                tmpRT2 = rttmp;
            }

            // Final pass
            cmd.Blit(tmpRT1, source, blitMaterial);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            RTHandles.Release(tmpRT1);
            RTHandles.Release(tmpRT2);

        }
    }
}
