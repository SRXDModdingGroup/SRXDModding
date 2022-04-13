using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderFeature : ScriptableRendererFeature {
    [SerializeField] private Material[] materials;

    private CustomRenderPass pass;

    private static readonly int ASPECT = Shader.PropertyToID("_Aspect");

    public override void Create() {
        pass = new CustomRenderPass(materials);
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(pass);
    }

    class CustomRenderPass : ScriptableRenderPass {
        private RenderTargetIdentifier cameraColorTarget;
        private Material[] materials;
        private RenderTargetHandle tempTexture1;
        private RenderTargetHandle tempTexture2;
        private float aspect;

        public CustomRenderPass(Material[] materials) {
            this.materials = materials;
            tempTexture1.Init("_tempTexture1");
            tempTexture2.Init("_tempTexture2");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            aspect = renderingData.cameraData.camera.aspect;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            var cmd = CommandBufferPool.Get("CustomRenderFeature");
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;

            descriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(tempTexture1.id, descriptor, FilterMode.Bilinear);
            cmd.GetTemporaryRT(tempTexture2.id, descriptor, FilterMode.Bilinear);
            Blit(cmd, cameraColorTarget, tempTexture1.Identifier());

            var source = tempTexture1.Identifier();
            var destination = tempTexture2.Identifier();

            foreach (var material in materials) {
                material.SetFloat(ASPECT, aspect);
                Blit(cmd, source, destination, material);
                (source, destination) = (destination, source);
            }

            Blit(cmd, source, cameraColorTarget);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(tempTexture1.id);
            cmd.ReleaseTemporaryRT(tempTexture2.id);
        }
    }
}


