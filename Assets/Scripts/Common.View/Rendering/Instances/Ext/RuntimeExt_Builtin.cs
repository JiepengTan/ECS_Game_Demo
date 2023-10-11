using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GamesTan.Rendering {
    public partial class IndirectRenderer {

        public IndirectRenderingMesh[] indirectMeshes=> RuntimeData.indirectMeshes;
        public InstanceRenderData _rendererData => RuntimeData._rendererData;
        ComputeBuffer m_instancesArgsBuffer => RuntimeData.m_instancesArgsBuffer;
        ComputeBuffer m_shadowArgsBuffer => RuntimeData.m_shadowArgsBuffer;

        ComputeBuffer m_instancesIsVisibleBuffer => RuntimeData.m_instancesIsVisibleBuffer;
        ComputeBuffer m_instanceDataBuffer => RuntimeData.m_instanceDataBuffer;
        ComputeBuffer m_instancesSortingData => RuntimeData.m_instancesSortingData;
        ComputeBuffer m_instancesShadowSortingData => RuntimeData.m_instancesShadowSortingData;
        ComputeBuffer m_instancesSortingDataTemp => RuntimeData.m_instancesSortingDataTemp;
        ComputeBuffer m_instancesMatrixRows01 => RuntimeData.m_instancesMatrixRows01;
        ComputeBuffer m_instancesCulledMatrixRows01 => RuntimeData.m_instancesCulledMatrixRows01;
        ComputeBuffer m_shadowsIsVisibleBuffer => RuntimeData.m_shadowsIsVisibleBuffer;
        ComputeBuffer m_shadowCulledMatrixRows01 => RuntimeData.m_shadowCulledMatrixRows01;

        ComputeBuffer m_transformDataBuffer => RuntimeData.m_transformDataBuffer;

        ComputeBuffer m_instancesDrawAnimData => RuntimeData.m_instancesDrawAnimData;
        ComputeBuffer m_instancesCulledAnimData => RuntimeData.m_instancesCulledAnimData;
        ComputeBuffer m_shadowCulledAnimData => RuntimeData.m_shadowCulledAnimData;

        ComputeBuffer m_instancesDrawIndexRemap => RuntimeData.m_instancesDrawIndexRemap;
        ComputeBuffer m_instancesCulledIndexRemap => RuntimeData.m_instancesCulledIndexRemap;
        ComputeBuffer m_shadowCulledIndexRemap => RuntimeData.m_shadowCulledIndexRemap;





        int m_createDrawDataBufferKernelID => RuntimeData.m_createDrawDataBufferKernelID;
        int m_sorting_64_CSKernelID => RuntimeData.m_sorting_64_CSKernelID;
        int m_sorting_256_CSKernelID => RuntimeData.m_sorting_256_CSKernelID;
        int m_sorting_512_CSKernelID => RuntimeData.m_sorting_512_CSKernelID;
        
        int m_sortingTransposeKernelID => RuntimeData.m_sortingTransposeKernelID;
        int m_occlusionKernelID => RuntimeData.m_occlusionKernelID;
        int m_copyInstanceDataKernelID => RuntimeData.m_copyInstanceDataKernelID;
        bool m_isInitialized => RuntimeData.m_isInitialized;


        int m_numberOfInstanceTypes => RuntimeData.m_numberOfInstanceTypes;
        int m_numberOfInstances => RuntimeData.m_numberOfInstances;
        int m_occlusionGroupX => RuntimeData.m_occlusionGroupX;
        int m_copyInstanceDataGroupX => RuntimeData.m_copyInstanceDataGroupX;
        bool m_debugLastDrawLOD => RuntimeData.m_debugLastDrawLOD;
        bool m_isEnabled => RuntimeData.m_isEnabled;
        uint[] m_args => RuntimeData.m_args;
        Bounds m_bounds => RuntimeData.m_bounds;
        Vector3 m_camPosition => RuntimeData.m_camPosition;
        Matrix4x4 m_MVP => RuntimeData.m_MVP;


        StringBuilder m_debugUIText => RuntimeData.m_debugUIText;
        Text m_uiText => RuntimeData.m_uiText;
        GameObject m_uiObj => RuntimeData.m_uiObj;
    }
}