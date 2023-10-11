using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace GamesTan.Rendering {
    public partial class IndirectRenderer : MonoBehaviour {
        public IndirectRendererConfig Config;
        public IndirectRendererRuntimeData RuntimeData;
        
        public HiZBuffer hiZBuffer;
        public Camera mainCamera;
        public Camera debugCamera;
        
        private int _curFrameNum = 0;
        public bool IsSrp =>  GraphicsSettings.renderPipelineAsset != null;
        
        public void DoAwake(InstanceRenderData data,List<IndirectInstanceData> prefabInfos) {
            RuntimeData = new IndirectRendererRuntimeData(Config);
            RuntimeData.DoAwake(data, prefabInfos,hiZBuffer, transform);
            IndirectRendererRuntimeData.SetInstance(RuntimeData);
        }
        public void DoDestroy()
        {
            RuntimeData.DoDestroy();
            IndirectRendererRuntimeData.SetInstance(null);
        }
        public void DoUpdate()
        {
            RuntimeData.DoUpdate();
        }

        public void OnCameraPreCull()
        {
            if(IsSrp) return;
            if (!m_isEnabled
                || indirectMeshes == null
                || indirectMeshes.Length == 0
                || hiZBuffer.Texture == null
                )
            {
                return;
            }

            bool isNeedUpdate = _curFrameNum != Time.frameCount;
            _curFrameNum = Time.frameCount;
            if (runCompute && isNeedUpdate)
            {
                Profiler.BeginSample("CalculateVisibleInstances()");
                CalculateVisibleInstances();
                Profiler.EndSample();
            }
            
            if (drawInstances)
            {
                Profiler.BeginSample("DrawInstances()");
                DrawInstances();
                Profiler.EndSample();
            }
            
            if (drawInstanceShadows)
            {
                Profiler.BeginSample("DrawInstanceShadows()");
                DrawInstanceShadows();
                Profiler.EndSample();
            }
            
            if (debugDrawHiZ)
            {
                Vector3 pos = transform.position;
                pos.y = debugCamera.transform.position.y;
                debugCamera.transform.position = pos;
                debugCamera.Render();
            }
            RuntimeData.UpdateDebug();
        }

        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            if (debugDrawBoundsInSceneView) {
                var bound = this._rendererData.bounds;
                Gizmos.color = new Color(1f, 0f, 0f, 0.333f);
                for (int i = 0; i < bound.Length; i++)
                {
                    Gizmos.DrawWireCube(bound[i].boundsCenter, bound[i].boundsExtents * 2f);
                }
            }
        }
        
        private void DrawInstances()
        {
            for (int i = 0; i < indirectMeshes.Length; i++)
            {
                int argsIndex = i * ARGS_BYTE_SIZE_PER_INSTANCE_TYPE;
                IndirectRenderingMesh irm = indirectMeshes[i];
                
                // 0 - index count per instance, 
                // 1 - instance count
                // 2 - start index location
                // 3 - base vertex location
                // 4 - start instance location
                if (enableLOD)
                {
                    Graphics.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material, m_bounds, m_instancesArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 0, irm.lod00MatPropBlock, ShadowCastingMode.Off);
                    Graphics.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material, m_bounds, m_instancesArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 1, irm.lod01MatPropBlock, ShadowCastingMode.Off);
                }
                Graphics.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material, m_bounds, m_instancesArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 2, irm.lod02MatPropBlock, ShadowCastingMode.Off);
            }
        }
        
        private void DrawInstanceShadows()
        {
            for (int i = 0; i < indirectMeshes.Length; i++)
            {
                int argsIndex = i * ARGS_BYTE_SIZE_PER_INSTANCE_TYPE;
                IndirectRenderingMesh irm = indirectMeshes[i];
                
                if (!enableOnlyLOD02Shadows)
                {
                    Graphics.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material, m_bounds, m_shadowArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 0, irm.shadowLod00MatPropBlock, ShadowCastingMode.ShadowsOnly);
                    Graphics.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material, m_bounds, m_shadowArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 1, irm.shadowLod01MatPropBlock, ShadowCastingMode.ShadowsOnly);
                }
                Graphics.DrawMeshInstancedIndirect(irm.mesh, 0, irm.material, m_bounds, m_shadowArgsBuffer, argsIndex + ARGS_BYTE_SIZE_PER_DRAW_CALL * 2, irm.shadowLod02MatPropBlock, ShadowCastingMode.ShadowsOnly);
                
            }
        }

        private void CalculateVisibleInstances() {
            RuntimeData.RenderPrepare(mainCamera);
            
            //////////////////////////////////////////////////////
            // Reset the arguments buffer
            //////////////////////////////////////////////////////
            Profiler.BeginSample("Resetting args buffer");
            {
                m_instancesArgsBuffer.SetData(m_args);
                m_shadowArgsBuffer.SetData(m_args);
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("00 Calc Transform Matrix");
            // upload entity's matrix
            //if (_rendererData.isDirty) 
            {
                _rendererData.isDirty = false;
                m_transformDataBuffer.SetData(_rendererData.transformData);
            
                m_instanceDataBuffer.SetData(_rendererData.bounds ); // bounds
                
                m_instancesDrawAnimData.SetData(_rendererData.animData);// anim data
                m_instancesSortingData.SetData(_rendererData.sortingData);// sorting data
                m_instancesShadowSortingData.SetData(_rendererData.sortingData);// sorting data
            }

            
            int groupX = Mathf.Max(1, m_numberOfInstances / (2 * SCAN_THREAD_GROUP_SIZE));
            createDrawDataBufferCS.Dispatch(m_createDrawDataBufferKernelID, groupX, 1, 1);
            
            Profiler.EndSample();
            
            
            //////////////////////////////////////////////////////
            // Set up compute shader to perform the occlusion culling
            //////////////////////////////////////////////////////
            Profiler.BeginSample("01 Occlusion");
            {
                // Input
                occlusionCS.SetFloat(_ShadowDistance, shadowDistance);
                occlusionCS.SetMatrix(_UNITY_MATRIX_MVP, m_MVP);
                occlusionCS.SetVector(_CamPosition, m_camPosition);
                
                // Dispatch
                occlusionCS.Dispatch(m_occlusionKernelID, m_occlusionGroupX, 1, 1);
            }
            Profiler.EndSample();
            
            
            //////////////////////////////////////////////////////
            // Sort the position buffer based on distance from camera
            //////////////////////////////////////////////////////
            Profiler.BeginSample("02 LOD Sorting");
            {
                SortRenderData(m_instancesSortingData);
                SortRenderData(m_instancesShadowSortingData);
            }
            Profiler.EndSample();
     
            
            //////////////////////////////////////////////////////
            // Perform stream compaction 
            // Calculate instance offsets and store in drawcall arguments buffer
            //////////////////////////////////////////////////////
            Profiler.BeginSample("05 Copy Instance Data");
            {
                // Normal
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _SortingData,                  m_instancesSortingData);
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancePredicatesIn,         m_instancesIsVisibleBuffer);
                
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _DrawcallDataOut,              m_instancesArgsBuffer);
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledAnimData,      m_instancesCulledAnimData);
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledMatrixRows01,  m_instancesCulledMatrixRows01);
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledIndexRemap,    m_instancesCulledIndexRemap);
                copyInstanceDataCS.Dispatch(m_copyInstanceDataKernelID, m_copyInstanceDataGroupX, 1, 1);
                
                
                // Shadows
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _SortingData,                  m_instancesShadowSortingData);
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancePredicatesIn,         m_shadowsIsVisibleBuffer);
                
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _DrawcallDataOut,              m_shadowArgsBuffer);
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledAnimData,      m_shadowCulledAnimData);
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledMatrixRows01,  m_shadowCulledMatrixRows01);
                copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledIndexRemap,    m_shadowCulledIndexRemap);
                
                copyInstanceDataCS.Dispatch(m_copyInstanceDataKernelID, m_copyInstanceDataGroupX, 1, 1);
                        
            }
            Profiler.EndSample();
            if (logDebugAll) {
                logDebugAll = false;
            }
            if (logAllArgBuffer) {
                RuntimeData.LogAllBuffers(logAllArgBufferCount);
            }
        }

        private void SortRenderData(ComputeBuffer sortingDataBuffer ) {
            uint instanceCount = (uint)m_numberOfInstances;
            uint transposeBlockSize = 8;
            var transposekernelID = MSortingTranspose_64_KernelID;
            
            uint sortBlockSize = 256;
            var sortKernelID = m_sorting_256_CSKernelID;
            RuntimeData.  GetSortKernelInfo(instanceCount, out sortBlockSize, out sortKernelID);

            // Determine parameters.
            uint maxWidth = sortBlockSize;
            uint maxHeight = (uint)instanceCount / sortBlockSize;
            int groupXCount = (int)(instanceCount / sortBlockSize);
            // Sort the data
            // First sort the rows for the levels <= to the block size
            for (uint level = 2; level <= sortBlockSize; level <<= 1)
            {
                SetGPUSortConstants( sortingCS,  level,  level,  maxHeight,  maxWidth);

                // Sort the row data
                sortingCS.SetBuffer( sortKernelID, _Data, sortingDataBuffer);
                sortingCS.Dispatch(sortKernelID, groupXCount, 1, 1);
            }

            // Then sort the rows and columns for the levels > than the block size
            // Transpose. Sort the Columns. Transpose. Sort the Rows.
            for (uint level = (sortBlockSize << 1); level <= instanceCount; level <<= 1)
            {
                // Transpose the data from buffer 1 into buffer 2
                uint l = (level / sortBlockSize);
                var inff = (level & ~instanceCount);
                uint lm = inff / sortBlockSize;
                SetGPUSortConstants(sortingCS,  l,  lm,  maxWidth,  maxHeight);
                sortingCS.SetBuffer(transposekernelID, _Input, sortingDataBuffer);
                sortingCS.SetBuffer( transposekernelID, _Data, m_instancesSortingDataTemp);
                sortingCS.Dispatch( transposekernelID, (int)(maxWidth / transposeBlockSize), (int)(maxHeight / transposeBlockSize), 1);

                // Sort the transposed column data
                sortingCS.SetBuffer( sortKernelID, _Data, m_instancesSortingDataTemp);
                sortingCS.Dispatch(sortKernelID, groupXCount, 1, 1);

                // Transpose the data from buffer 2 back into buffer 1
                SetGPUSortConstants(sortingCS,  sortBlockSize,  level,  maxHeight,  maxWidth);
                sortingCS.SetBuffer( transposekernelID, _Input, m_instancesSortingDataTemp);
                sortingCS.SetBuffer( transposekernelID, _Data, sortingDataBuffer);
                sortingCS.Dispatch(transposekernelID, (int)(maxHeight / transposeBlockSize), (int)(maxWidth / transposeBlockSize), 1);

                // Sort the row data
                sortingCS.SetBuffer( sortKernelID, _Data, sortingDataBuffer);
                sortingCS.Dispatch(sortKernelID, groupXCount, 1, 1);
            }
        }

 

        private void SetGPUSortConstants( ComputeShader cs, uint level, uint levelMask, uint width, uint height)
        {
            cs.SetInt( _Level, (int)level);
            cs.SetInt( _LevelMask, (int)levelMask);
            cs.SetInt( _Width, (int)width);
            cs.SetInt( _Height, (int)height);
        }
        
    }    
}