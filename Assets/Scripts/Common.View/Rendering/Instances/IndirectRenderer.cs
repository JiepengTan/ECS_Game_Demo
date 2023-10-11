using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GamesTan.ECS;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace GamesTan.Rendering {
    public partial class IndirectRenderer : MonoBehaviour {
        public IndirectRendererConfig Config;
        public IndirectRendererRuntimeData RuntimeData;
        #region Variables
      
        
        public HiZBuffer hiZBuffer;
        public Camera mainCamera;
        public Camera debugCamera;
        
        
        #endregion

        #region MonoBehaviour

        public void SetRenderData(InstanceRenderData data) {
            RuntimeData = new IndirectRendererRuntimeData(Config);
            RuntimeData.DoInit(data);
        }
        private void OnDestroy()
        {
            RuntimeData. DoDestroy();
        }
        public void DoUpdate()
        {
            RuntimeData.DoUpdate();
        }

        private int curFrameNum = 0;
        public void OnCameraPreCull()
        {
            if (!m_isEnabled
                || indirectMeshes == null
                || indirectMeshes.Length == 0
                || hiZBuffer.Texture == null
                )
            {
                return;
            }

            bool isNeedUpdate = curFrameNum != Time.frameCount;
            curFrameNum = Time.frameCount;
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
        
        #endregion

        public int MaxRenderCount = 0;
        
        #region Public Functions
        
        public void Initialize(List<IndirectInstanceData> infos,int maxCount) {
            RuntimeData.Initialize(infos, maxCount,transform,hiZBuffer);
        }


        public void StartDrawing()
        {
            if (!m_isInitialized)
            {
                Debug.LogError("IndirectRenderer: Unable to start drawing because it's not initialized");
                return;
            }
            
            RuntimeData.m_isEnabled = true;
        }
        
        public void StopDrawing(bool shouldReleaseBuffers = false)
        {
            RuntimeData.m_isEnabled = false;
            
            if (shouldReleaseBuffers)
            {
                RuntimeData.ReleaseBuffers();
                RuntimeData.m_isInitialized = false;
                hiZBuffer.enabled = false;
            }
        }
        
        #endregion

        #region Private Functions
        
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

        private void CalculateVisibleInstances()
        {
            // Global data
            RuntimeData.m_camPosition =  mainCamera.transform.position;
            RuntimeData.m_bounds.center = m_camPosition;
            RuntimeData.m_bounds.extents = Vector3.one * 10000;
            
            //Matrix4x4 m = mainCamera.transform.localToWorldMatrix;
            Matrix4x4 v = mainCamera.worldToCameraMatrix;
            Matrix4x4 p = mainCamera.projectionMatrix;
            RuntimeData.m_MVP = p * v;//*m;
            if (logDebugAll) {
                Debug.Log("logDebugAll =================== " + Time.frameCount);
            }

            
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
                SortRenderDatas(m_instancesSortingData);
                SortRenderDatas(m_instancesShadowSortingData);
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
        
        
        private void SortRenderDatas(ComputeBuffer sortingDataBuffer )
        {
            uint BITONIC_BLOCK_SIZE = 256;
            uint TRANSPOSE_BLOCK_SIZE = 8;
            
            // Determine parameters.
            uint NUM_ELEMENTS = (uint)m_numberOfInstances;
            uint MATRIX_WIDTH = BITONIC_BLOCK_SIZE;
            uint MATRIX_HEIGHT = (uint)NUM_ELEMENTS / BITONIC_BLOCK_SIZE;
            
            // Sort the data
            // First sort the rows for the levels <= to the block size
            for (uint level = 2; level <= BITONIC_BLOCK_SIZE; level <<= 1)
            {
                SetGPUSortConstants( sortingCS, ref level, ref level, ref MATRIX_HEIGHT, ref MATRIX_WIDTH);

                // Sort the row data
                sortingCS.SetBuffer( m_sortingCSKernelID, _Data, sortingDataBuffer);
                sortingCS.Dispatch(m_sortingCSKernelID, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);
            }

            // Then sort the rows and columns for the levels > than the block size
            // Transpose. Sort the Columns. Transpose. Sort the Rows.
            for (uint level = (BITONIC_BLOCK_SIZE << 1); level <= NUM_ELEMENTS; level <<= 1)
            {
                // Transpose the data from buffer 1 into buffer 2
                uint l = (level / BITONIC_BLOCK_SIZE);
                var inff = (level & ~NUM_ELEMENTS);
                uint lm = inff / BITONIC_BLOCK_SIZE;
                SetGPUSortConstants(sortingCS, ref l, ref lm, ref MATRIX_WIDTH, ref MATRIX_HEIGHT);
                sortingCS.SetBuffer(m_sortingTransposeKernelID, _Input, sortingDataBuffer);
                sortingCS.SetBuffer( m_sortingTransposeKernelID, _Data, m_instancesSortingDataTemp);
                sortingCS.Dispatch( m_sortingTransposeKernelID, (int)(MATRIX_WIDTH / TRANSPOSE_BLOCK_SIZE), (int)(MATRIX_HEIGHT / TRANSPOSE_BLOCK_SIZE), 1);

                // Sort the transposed column data
                sortingCS.SetBuffer( m_sortingCSKernelID, _Data, m_instancesSortingDataTemp);
                sortingCS.Dispatch(m_sortingCSKernelID, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);

                // Transpose the data from buffer 2 back into buffer 1
                SetGPUSortConstants(sortingCS, ref BITONIC_BLOCK_SIZE, ref level, ref MATRIX_HEIGHT, ref MATRIX_WIDTH);
                sortingCS.SetBuffer( m_sortingTransposeKernelID, _Input, m_instancesSortingDataTemp);
                sortingCS.SetBuffer( m_sortingTransposeKernelID, _Data, sortingDataBuffer);
                sortingCS.Dispatch(m_sortingTransposeKernelID, (int)(MATRIX_HEIGHT / TRANSPOSE_BLOCK_SIZE), (int)(MATRIX_WIDTH / TRANSPOSE_BLOCK_SIZE), 1);

                // Sort the row data
                sortingCS.SetBuffer( m_sortingCSKernelID, _Data, sortingDataBuffer);
                sortingCS.Dispatch(m_sortingCSKernelID, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);
            }
        }
        
        private void SetGPUSortConstants( ComputeShader cs, ref uint level, ref uint levelMask, ref uint width, ref uint height)
        {
            cs.SetInt( _Level, (int)level);
            cs.SetInt( _LevelMask, (int)levelMask);
            cs.SetInt( _Width, (int)width);
            cs.SetInt( _Height, (int)height);
        }
        

        
        #endregion

    }    
}