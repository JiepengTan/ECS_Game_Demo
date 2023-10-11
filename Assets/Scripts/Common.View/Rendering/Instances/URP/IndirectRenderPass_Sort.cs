using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GamesTan.Rendering {
    partial class IndirectRenderPass_Sort : IndirectDrawRenderPass {
        protected override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
            DoSort(context);
            DoCopyData(context);
        }
        private void DoSort(ScriptableRenderContext context) {
            CommandBuffer cmd = CommandBufferPool.Get("IndirectDraw-Sort");
            SortRenderData(cmd, (ComputeBuffer)m_instancesSortingData);
            SortRenderData(cmd, (ComputeBuffer)m_instancesShadowSortingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        private void DoCopyData(ScriptableRenderContext context) {
            CommandBuffer cmd = CommandBufferPool.Get("IndirectDraw-CopyData");
            // Normal
            cmd.SetComputeBufferParam(copyInstanceDataCS, m_copyInstanceDataKernelID, _SortingData,                  m_instancesSortingData);
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _InstancePredicatesIn,         m_instancesIsVisibleBuffer);
                
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _DrawcallDataOut,              m_instancesArgsBuffer);
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _InstancesCulledAnimData,      m_instancesCulledAnimData);
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _InstancesCulledMatrixRows01,  m_instancesCulledMatrixRows01);
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _InstancesCulledIndexRemap,    m_instancesCulledIndexRemap);
            cmd.DispatchCompute(copyInstanceDataCS, m_copyInstanceDataKernelID, m_copyInstanceDataGroupX, 1, 1);
                
                
            // Shadows
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _SortingData,                  m_instancesShadowSortingData);
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _InstancePredicatesIn,         m_shadowsIsVisibleBuffer);
            
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _DrawcallDataOut,              m_shadowArgsBuffer);
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _InstancesCulledAnimData,      m_shadowCulledAnimData);
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _InstancesCulledMatrixRows01,  m_shadowCulledMatrixRows01);
            cmd.SetComputeBufferParam(copyInstanceDataCS,m_copyInstanceDataKernelID, _InstancesCulledIndexRemap,    m_shadowCulledIndexRemap);
                
            cmd.DispatchCompute( copyInstanceDataCS,m_copyInstanceDataKernelID, m_copyInstanceDataGroupX, 1, 1);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void SortRenderData(CommandBuffer cmd, ComputeBuffer sortingDataBuffer) {
            uint instanceCount = (uint)m_numberOfInstances;
            uint transposeBlockSize = 8;
            var transposekernelID = MSortingTranspose_64_KernelID;
            uint sortBlockSize = 256;
            int sortKernelID = m_sorting_256_CSKernelID;
            RuntimeData.GetSortKernelInfo(instanceCount, out sortBlockSize, out sortKernelID);
            var sortCS = this.sortingCS;
            
            // Determine parameters.
            uint maxWidth = sortBlockSize;
            uint maxHeight = (uint)instanceCount / sortBlockSize;
            int groupXCount = (int)(instanceCount / sortBlockSize);
            // Sort the data
            // First sort the rows for the levels <= to the block size
            for (uint level = 2; level <= sortBlockSize; level <<= 1) {
                SetGPUSortConstants(cmd, (ComputeShader)sortCS, level, level, maxHeight, maxWidth);

                // Sort the row data
                cmd.SetComputeBufferParam(sortCS, sortKernelID, _Data, sortingDataBuffer);
                cmd.DispatchCompute(sortCS,sortKernelID, groupXCount, 1, 1);
            }

            // Then sort the rows and columns for the levels > than the block size
            // Transpose. Sort the Columns. Transpose. Sort the Rows.
            for (uint level = (sortBlockSize << 1); level <= instanceCount; level <<= 1) {
                // Transpose the data from buffer 1 into buffer 2
                uint l = (level / sortBlockSize);
                var inff = (level & ~instanceCount);
                uint lm = inff / sortBlockSize;
                SetGPUSortConstants(cmd, (ComputeShader)sortCS, l, lm, maxWidth, maxHeight);
                cmd.SetComputeBufferParam(sortCS, transposekernelID, _Input, sortingDataBuffer);
                cmd.SetComputeBufferParam(sortCS, transposekernelID, _Data, m_instancesSortingDataTemp);
                cmd.DispatchCompute(sortCS, transposekernelID, (int)(maxWidth / transposeBlockSize),
                    (int)(maxHeight / transposeBlockSize), 1);

                // Sort the transposed column data
                cmd.SetComputeBufferParam(sortCS, sortKernelID, _Data, m_instancesSortingDataTemp);
                cmd.DispatchCompute(sortCS, sortKernelID, groupXCount, 1, 1);

                // Transpose the data from buffer 2 back into buffer 1
                SetGPUSortConstants(cmd, (ComputeShader)sortCS, sortBlockSize, level, maxHeight, maxWidth);
                cmd.SetComputeBufferParam(sortCS, transposekernelID, _Input, m_instancesSortingDataTemp);
                cmd.SetComputeBufferParam(sortCS, transposekernelID, _Data, sortingDataBuffer);
                cmd.DispatchCompute(sortCS, transposekernelID, (int)(maxHeight / transposeBlockSize),
                    (int)(maxWidth / transposeBlockSize), 1);

                // Sort the row data
                cmd.SetComputeBufferParam(sortCS, sortKernelID, _Data, sortingDataBuffer);
                cmd.DispatchCompute(sortCS, sortKernelID, groupXCount, 1, 1);
            }
        }


        private void SetGPUSortConstants(CommandBuffer cmd, ComputeShader cs, uint level, uint levelMask, uint width,
            uint height) {
            cmd.SetComputeIntParam(cs, _Level, (int)level);
            cmd.SetComputeIntParam(cs, _LevelMask, (int)levelMask);
            cmd.SetComputeIntParam(cs, _Width, (int)width);
            cmd.SetComputeIntParam(cs, _Height, (int)height);
        }
    }
}