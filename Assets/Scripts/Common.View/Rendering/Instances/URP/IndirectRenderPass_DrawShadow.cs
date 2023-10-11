using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GamesTan.Rendering {
    partial class IndirectRenderPass_DrawShadow : IndirectDrawRenderPass {
        const int ShadowPassIdx = 1;
        protected override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get("IndirectDraw-DrawShadow");
            for (int i = 0; i < indirectMeshes.Length; i++)
            {
                int argsIndex = i * ARGS_BYTE_SIZE_PER_INSTANCE_TYPE;
                IndirectRenderingMesh irm = indirectMeshes[i];
                if (!enableOnlyLOD02Shadows)
                {
                    cmd.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material, ShadowPassIdx, m_shadowArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 0, irm.shadowLod00MatPropBlock);
                    cmd.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material, ShadowPassIdx, m_shadowArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 1, irm.shadowLod01MatPropBlock);
                }
                cmd.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material, ShadowPassIdx, m_shadowArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 2, irm.shadowLod02MatPropBlock);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

}