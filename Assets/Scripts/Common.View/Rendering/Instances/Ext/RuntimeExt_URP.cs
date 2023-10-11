using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GamesTan.Rendering {
    public partial class IndirectDrawRenderPass {

        public IndirectRenderingMesh[] indirectMeshes=> RuntimeData.indirectMeshes;
        public InstanceRenderData _rendererData => RuntimeData._rendererData;
        protected ComputeBuffer m_instancesArgsBuffer => RuntimeData.m_instancesArgsBuffer;
        protected ComputeBuffer m_shadowArgsBuffer => RuntimeData.m_shadowArgsBuffer;

        protected ComputeBuffer m_instancesIsVisibleBuffer => RuntimeData.m_instancesIsVisibleBuffer;
        protected ComputeBuffer m_instanceDataBuffer => RuntimeData.m_instanceDataBuffer;
        protected ComputeBuffer m_instancesSortingData => RuntimeData.m_instancesSortingData;
        protected ComputeBuffer m_instancesShadowSortingData => RuntimeData.m_instancesShadowSortingData;
        protected ComputeBuffer m_instancesSortingDataTemp => RuntimeData.m_instancesSortingDataTemp;
        protected ComputeBuffer m_instancesMatrixRows01 => RuntimeData.m_instancesMatrixRows01;
        protected ComputeBuffer m_instancesCulledMatrixRows01 => RuntimeData.m_instancesCulledMatrixRows01;
        protected ComputeBuffer m_shadowsIsVisibleBuffer => RuntimeData.m_shadowsIsVisibleBuffer;
        protected ComputeBuffer m_shadowCulledMatrixRows01 => RuntimeData.m_shadowCulledMatrixRows01;

        protected ComputeBuffer m_transformDataBuffer => RuntimeData.m_transformDataBuffer;

        protected ComputeBuffer m_instancesDrawAnimData => RuntimeData.m_instancesDrawAnimData;
        protected ComputeBuffer m_instancesCulledAnimData => RuntimeData.m_instancesCulledAnimData;
        protected ComputeBuffer m_shadowCulledAnimData => RuntimeData.m_shadowCulledAnimData;

        protected ComputeBuffer m_instancesDrawIndexRemap => RuntimeData.m_instancesDrawIndexRemap;
        protected ComputeBuffer m_instancesCulledIndexRemap => RuntimeData.m_instancesCulledIndexRemap;
        protected ComputeBuffer m_shadowCulledIndexRemap => RuntimeData.m_shadowCulledIndexRemap;

        protected int m_createDrawDataBufferKernelID => RuntimeData.m_createDrawDataBufferKernelID;
        protected int m_sorting_128_CSKernelID => RuntimeData.m_sorting_128_CSKernelID;
        protected int m_sorting_256_CSKernelID => RuntimeData.m_sorting_256_CSKernelID;
        protected int m_sorting_512_CSKernelID => RuntimeData.m_sorting_512_CSKernelID;
        
        protected int MSortingTranspose_64_KernelID => RuntimeData.m_sortingTranspose_64_KernelID;
        protected int m_occlusionKernelID => RuntimeData.m_occlusionKernelID;
        protected int m_copyInstanceDataKernelID => RuntimeData.m_copyInstanceDataKernelID;
        protected bool m_isInitialized => RuntimeData.m_isInitialized;


        protected int m_numberOfInstanceTypes => RuntimeData.m_numberOfInstanceTypes;
        protected int m_numberOfInstances => RuntimeData.m_numberOfInstances;
        protected int m_occlusionGroupX => RuntimeData.m_occlusionGroupX;
        protected int m_copyInstanceDataGroupX => RuntimeData.m_copyInstanceDataGroupX;
        protected bool m_debugLastDrawLOD => RuntimeData.m_debugLastDrawLOD;
        protected bool m_isEnabled => RuntimeData.m_isEnabled;
        protected uint[] m_args => RuntimeData.m_args;
        protected Bounds m_bounds => RuntimeData.m_bounds;
        protected Vector3 m_camPosition => RuntimeData.m_camPosition;
        protected Matrix4x4 m_MVP => RuntimeData.m_MVP;


        protected StringBuilder m_debugUIText => RuntimeData.m_debugUIText;
        protected Text m_uiText => RuntimeData.m_uiText;
        protected GameObject m_uiObj => RuntimeData.m_uiObj;
    }
}