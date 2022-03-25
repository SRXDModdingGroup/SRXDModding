using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderFeature : ScriptableRendererFeature {
    class CustomRenderPass : ScriptableRenderPass {
        private RenderTargetIdentifier cameraColorTarget;
        private Material[] materials;
        private RenderTargetHandle tempTexture1;
        private RenderTargetHandle tempTexture2;

        public CustomRenderPass(Material[] materials) {
            this.materials = materials;
            tempTexture1.Init("_tempTexture1");
            tempTexture2.Init("_tempTexture2");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            var cmd = CommandBufferPool.Get("CustomRenderFeature");
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;

            descriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(tempTexture1.id, descriptor, FilterMode.Bilinear);
            cmd.GetTemporaryRT(tempTexture2.id, descriptor, FilterMode.Bilinear);
            Blit(cmd, cameraColorTarget, tempTexture1.Identifier());

            var source = tempTexture1.Identifier();
            var destination = tempTexture2.Identifier();

            for (int i = 0; i < materials.Length; i++) {
                Blit(cmd, source, destination, materials[i]);
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

    [SerializeField] private Material[] materials;

    private CustomRenderPass pass;

    /// <inheritdoc/>
    public override void Create() {
        pass = new CustomRenderPass(materials);

        // Configures where the render pass should be injected.
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(pass);
    }
}


