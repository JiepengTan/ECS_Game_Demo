using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GamesTan.Rendering {
    partial class IndirectRenderPass_PrepareCulling : IndirectDrawRenderPass {
        protected override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
            RuntimeData.RenderPrepare(renderingData.cameraData.camera);
            DoPrepare(context);
            DoCulling(context);
        }

        private void DoPrepare(ScriptableRenderContext context) {
            CommandBuffer cmd = CommandBufferPool.Get("IndirectDraw-DoPrepare");
            cmd.SetBufferData(m_instancesArgsBuffer, m_args);
            cmd.SetBufferData(m_shadowArgsBuffer, m_args);
            // upload entity's matrix
            //if (_rendererData.isDirty) 
            {
                _rendererData.isDirty = false;
                cmd.SetBufferData(m_transformDataBuffer, _rendererData.transformData);
                cmd.SetBufferData(m_instanceDataBuffer, _rendererData.bounds);
                cmd.SetBufferData(m_instancesDrawAnimData, _rendererData.animData);
                cmd.SetBufferData(m_instancesSortingData, _rendererData.sortingData);
                cmd.SetBufferData(m_instancesShadowSortingData, _rendererData.sortingData);
            }
            int groupX = Mathf.Max(1, m_numberOfInstances / (2 * SCAN_THREAD_GROUP_SIZE));
            cmd.DispatchCompute(createDrawDataBufferCS, m_createDrawDataBufferKernelID, groupX, 1, 1);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void DoCulling(ScriptableRenderContext context) {
            CommandBuffer cmd = CommandBufferPool.Get("IndirectDraw-DoCulling");
            // Input
            cmd.SetComputeFloatParam(occlusionCS, _ShadowDistance, shadowDistance);
            cmd.SetComputeMatrixParam(occlusionCS, _UNITY_MATRIX_MVP, m_MVP);
            cmd.SetComputeVectorParam(occlusionCS, _CamPosition, m_camPosition);

            // Dispatch
            cmd.DispatchCompute(occlusionCS, m_occlusionKernelID, m_occlusionGroupX, 1, 1);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}