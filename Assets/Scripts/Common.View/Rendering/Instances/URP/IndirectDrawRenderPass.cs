using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GamesTan.Rendering {
    partial class IndirectDrawRenderPass : ScriptableRenderPass {
        public IndirectRendererConfig Config;
        public IndirectRendererRuntimeData RuntimeData;
       
        private Vector2Int size;
        public IndirectDrawRenderPass(RenderPassEvent evt, IndirectRendererConfig config,IndirectRendererRuntimeData runtimeData) {
            Config = config;
            RuntimeData =runtimeData;
            this.renderPassEvent = evt;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor) {
            size = new Vector2Int(cameraTextureDescripor.width, cameraTextureDescripor.height);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            var colorHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            CommandBuffer cmd = CommandBufferPool.Get("IndirectDrawRenderFeaturePass");

            //cmd.SetRandomWriteTarget(1, cbPoints);
            //cmd.GetTemporaryRT(m_ColorRTid, size.x, size.y, 24);
            //cmd.Blit(colorHandle, m_ColorRTid, mat, 0);
            //cmd.ClearRandomWriteTargets();
            //cmd.ReleaseTemporaryRT(m_ColorRTid);
            //cmd.SetRenderTarget(colorHandle);
            //cmd.CopyCounterValue(cbPoints, cbDrawArgs, 4);
            //cmd.DrawMeshInstancedIndirect(mesh, 0, mat, 1, cbDrawArgs, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}