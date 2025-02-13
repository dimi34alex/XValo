using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experimental.Rendering.Universal

{
    /// <summary>
    /// Copy the given color buffer to the given destination color buffer.
    ///
    /// You can use this pass to copy a color buffer to the destination,
    /// so you can use it later in rendering. For example, you can copy
    /// the opaque texture to use it for distortion effects.
    /// </summary>
    internal class BlitPass1 : ScriptableRenderPass
    {
        public enum RenderTarget
        {
            Color,
            RenderTexture,
        }

        private Material _blitMaterial = null;
        private int _blitShaderPassIndex = 0;
        public FilterMode filterMode { get; set; }

        private RTHandle _source { get; set; }
        private RTHandle _destination { get; set; }

        private RTHandle _temporaryColorTexture;
        private string _profilerTag;

        /// <summary>
        /// Create the CopyColorPass
        /// </summary>
        public BlitPass1(RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            _blitMaterial = blitMaterial;
            _blitShaderPassIndex = blitShaderPassIndex;
            _profilerTag = tag;

            // Создаем RTHandle через Alloc вместо Init
            _temporaryColorTexture = RTHandles.Alloc(
                Vector2.one, 
                dimension: TextureDimension.Tex2D, 
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
            _source = source;
            _destination = destination;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);
            
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // Заменяем RTHandle.CameraTarget на cameraColorTargetHandle
            if (_destination == renderingData.cameraData.renderer.cameraColorTargetHandle)
            {
                Blitter.BlitCameraTexture(cmd, _source, _temporaryColorTexture, _blitMaterial, _blitShaderPassIndex);
                Blitter.BlitCameraTexture(cmd, _temporaryColorTexture, _source);
            }
            else
            {
                Blitter.BlitCameraTexture(cmd, _source, _destination, _blitMaterial, _blitShaderPassIndex);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (_temporaryColorTexture != null)
                RTHandles.Release(_temporaryColorTexture);
        }
    }
}
