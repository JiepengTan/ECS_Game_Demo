using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GamesTan.Rendering {
    partial class IndirectRenderPass_DrawOpacity : IndirectDrawRenderPass {
        const int  ShadowPassIdx = 0;
        protected override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get("IndirectDraw-DrawOpacity");
            for (int i = 0; i < indirectMeshes.Length; i++)
            {
                int argsIndex = i * ARGS_BYTE_SIZE_PER_INSTANCE_TYPE;
                IndirectRenderingMesh irm = indirectMeshes[i];
                if (enableLOD)
                {
                    cmd.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material,ShadowPassIdx,  m_instancesArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 0, irm.lod00MatPropBlock);
                    cmd.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material,ShadowPassIdx,  m_instancesArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 1, irm.lod01MatPropBlock);
                }
                cmd.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material, ShadowPassIdx, m_instancesArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 2, irm.lod02MatPropBlock);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            RuntimeData.CheckLogBuffers();
        }
    }
}