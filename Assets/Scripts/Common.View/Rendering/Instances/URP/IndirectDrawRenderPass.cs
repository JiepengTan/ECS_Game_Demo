using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GamesTan.Rendering {
    partial class IndirectDrawRenderPass : ScriptableRenderPass {
        public IndirectRendererConfig Config;
        private int maxCount;
        private Mesh mesh;
        private Material mat;
        private Vector2Int size;

        private ComputeBuffer cbDrawArgs;
        private ComputeBuffer cbPoints;
        private int m_ColorRTid = Shader.PropertyToID("_CameraScreenTexture");

        public IndirectDrawRenderPass(RenderPassEvent renderPassEvent, int count, Mesh mesh, Material material,
            ComputeBuffer cbDrawArgs, ComputeBuffer cbPoints) {
            this.maxCount = count;
            this.mesh = mesh;
            this.mat = material;
            this.renderPassEvent = renderPassEvent;
            this.cbDrawArgs = cbDrawArgs;
            this.cbPoints = cbPoints;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor) {
            size = new Vector2Int(cameraTextureDescripor.width, cameraTextureDescripor.height);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            var colorHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            //Camera camera = renderingData.cameraData.camera;

            CommandBuffer cmd = CommandBufferPool.Get("IndirectDrawRenderFeaturePass");

            //This binds the buffer we want to store the filtered star positions
            //Match the id with shader
            cmd.SetRandomWriteTarget(1, cbPoints);
            cmd.GetTemporaryRT(m_ColorRTid, size.x, size.y, 24);
            //This blit will send the screen texture to shader and do the filtering
            //If the pixel is bright enough we take the pixel position
            cmd.Blit(colorHandle, m_ColorRTid, mat, 0);
            cmd.ClearRandomWriteTargets();
            cmd.ReleaseTemporaryRT(m_ColorRTid);
            cmd.SetRenderTarget(colorHandle);
            //Tells actually how many stars we need to draw
            //Copy the filtered star count to cbDrawArgs[1], which is at 4bytes int offset
            cmd.CopyCounterValue(cbPoints, cbDrawArgs, 4);
            //Draw the stars
            cmd.DrawMeshInstancedIndirect(mesh, 0, mat, 1, cbDrawArgs, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}