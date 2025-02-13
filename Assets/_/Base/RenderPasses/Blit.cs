using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.Universal
{
    public class Blit : ScriptableRendererFeature
    {
        [System.Serializable]
        public class BlitSettings
        {
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
            public Material blitMaterial = null;
            public int blitMaterialPassIndex = -1;
            public Target destination = Target.Color;
            public string textureId = "_BlitPassTexture";
        }

        public enum Target
        {
            Color,
            Texture
        }

        public BlitSettings settings = new BlitSettings();
        RTHandle m_RenderTextureHandle;
        BlitPass blitPass;

        public override void Create()
        {
            var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
            settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
            
            // Создание RTHandle
            if (settings.destination == Target.Texture)
                m_RenderTextureHandle = RTHandles.Alloc(settings.textureId, name: settings.textureId);

            blitPass = new BlitPass(settings.Event, settings.blitMaterial, settings.blitMaterialPassIndex, name);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var src = renderer.cameraColorTargetHandle;
            var dest = (settings.destination == Target.Color) 
                ? renderer.cameraColorTargetHandle 
                : m_RenderTextureHandle;

            if (settings.blitMaterial == null)
            {
                Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
                return;
            }

            blitPass.Setup(src, dest);
            renderer.EnqueuePass(blitPass);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (m_RenderTextureHandle != null)
            {
                RTHandles.Release(m_RenderTextureHandle);
            }
        }
    }

    public class BlitPass : ScriptableRenderPass
    {
        private Material blitMaterial;
        private int blitMaterialPassIndex;
        private RTHandle source;
        private RTHandle destination;

        public BlitPass(RenderPassEvent renderPassEvent, Material blitMaterial, int blitMaterialPassIndex, string profilerTag)
        {
            this.renderPassEvent = renderPassEvent;
            this.blitMaterial = blitMaterial;
            this.blitMaterialPassIndex = blitMaterialPassIndex;
        }

        public void Setup(RTHandle source, RTHandle destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Blit Pass");

            if (destination == renderingData.cameraData.renderer.cameraColorTargetHandle)
            {
                Blit(cmd, source, destination, blitMaterial, blitMaterialPassIndex);
            }
            else
            {
                Blit(cmd, source, destination, blitMaterial, blitMaterialPassIndex);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
