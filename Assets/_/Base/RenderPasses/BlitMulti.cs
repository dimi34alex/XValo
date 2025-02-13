using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
namespace UnityEngine.Experimental.Rendering.Universal

{
    public class BlitMulti : ScriptableRendererFeature
    {
        [System.Serializable]
        public class BlitSettings
        {
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

            public Material blitMaterial = null;
            public int blitMaterialPassIndex = -1;
            public Target destination = Target.Color;
            public string textureId = "_BlitPassTexture";

            [Range(1, 15)]
            public int blurPasses = 2;
            [Range(1, 4)]
            public int downsample = 2;
        }

        public enum Target
        {
            Color,
            Texture
        }

        public BlitSettings settings = new BlitSettings();
        RTHandle m_RenderTextureHandle;

        BlitMultiPass blitPass;

        public override void Create()
        {
            var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
            settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
            blitPass = new BlitMultiPass(settings.Event, settings.blitMaterial, settings.blitMaterialPassIndex, name, settings.blurPasses, settings.downsample);

            // Создаем RTHandle через Alloc
            m_RenderTextureHandle = RTHandles.Alloc(settings.textureId);
        }


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Используем новый API для получения цвета камеры
            var src = renderer.cameraColorTargetHandle;

            // Определяем, куда рендерить
            var dest = (settings.destination == Target.Color) ? renderer.cameraColorTargetHandle : m_RenderTextureHandle;

            if (settings.blitMaterial == null)
            {
                Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
                return;
            }

            blitPass.Setup(src, dest);
            renderer.EnqueuePass(blitPass);
        }

    }
}

