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

// Preferrably want to have all buffer structs in power of 2...
// 6 * 4 bytes = 24 bytes
[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct InstanceBound
{
    public float3 boundsCenter;       // 3
    public float3 boundsExtents;      // 6
    public override string ToString() {
        return $"center:{boundsCenter} ext:{boundsExtents}";
    }
}

// 8 * 4 bytes = 32 bytes
[StructLayout(LayoutKind.Sequential)]
public struct Indirect2x2Matrix
{
    public Vector4 row0;    // 4
    public Vector4 row1;    // 8
    public Vector4 row2;    // 8
};

// 2 * 4 bytes = 8 bytes
[StructLayout(LayoutKind.Sequential)]
public struct SortingData
{
    public uint drawCallInstanceIndex; // 1
    public uint distanceToCam;         // 2
    public uint threadDispatchID;       //3

    public override string ToString() {
        return $"dist:{distanceToCam } instanceId:{drawCallInstanceIndex}  tid:{threadDispatchID}";
    }
};

[System.Serializable]
public class IndirectRenderingMesh
{
    public Mesh mesh;
    public Material material;
    public MaterialPropertyBlock lod00MatPropBlock;
    public MaterialPropertyBlock lod01MatPropBlock;
    public MaterialPropertyBlock lod02MatPropBlock;
    public MaterialPropertyBlock shadowLod00MatPropBlock;
    public MaterialPropertyBlock shadowLod01MatPropBlock;
    public MaterialPropertyBlock shadowLod02MatPropBlock;
    public uint numOfVerticesLod00;
    public uint numOfVerticesLod01;
    public uint numOfVerticesLod02;
    public uint numOfIndicesLod00;
    public uint numOfIndicesLod01;
    public uint numOfIndicesLod02;
    public Bounds originalBounds;
}


public class IndirectRenderer : MonoBehaviour
{
    #region Variables
    
    [Header("Settings")]
    public bool runCompute = true;
    public bool drawInstances = true;
    public bool drawInstanceShadows = true;
    public bool enableFrustumCulling = true;
    public bool enableOcclusionCulling = true;
    public bool enableDetailCulling = true;
    public bool enableLOD = true;
    public bool enableOnlyLOD02Shadows = true;
    [Range(00.00f, 00.2f)] public float detailCullingPercentage = 0.005f;
    [Range(10, 1000)] public float shadowDistance = 100;
    [Range(10, 300.0f)] public float lod0Distance = 20f;
    [Range(10, 300.0f)] public float lod1Distance = 100f;
    
    // Debugging Variables
    [Header("Debug")]
    public bool debugShowUI;
    public bool debugDrawLOD;
    public bool debugDrawBoundsInSceneView;
    public bool debugDrawHiZ;
    [Range(0, 10)] public int debugHiZLOD;
    public GameObject debugUIPrefab;
    
    
    [Header("Data")]
    [ReadOnly] public IndirectRenderingMesh[] indirectMeshes;
    private InstanceRenderData _rendererData = new InstanceRenderData();
    
    [Header("Logging")]
    public bool logInstanceAnimation = false;
    public bool logInstanceDrawMatrices = false;
    public bool logInstanceDrawCulledMatrices = false;
    public bool logArgumentsAfterReset = false;
    public bool logSortingData = false;
    public bool logArgumentsAfterOcclusion = false;
    public bool logInstancesIsVisibleBuffer = false;
    public bool logScannedPredicates = false;
    public bool logGroupSumArrayBuffer = false;
    public bool logScannedGroupSumsBuffer = false;
    public bool logArgsBufferAfterCopy = false;
    public bool logCulledInstancesDrawMatrices = false;
    public bool logCulledInstancesAnimData = false;
    public bool logDebugAll = false;
    public bool LogAllArgBuffer;
    public int LogAllArgBufferCount = 5;
    [Header("References")]
    public ComputeShader createDrawDataBufferCS;
    public ComputeShader sortingCS;
    public ComputeShader occlusionCS;
    public ComputeShader scanInstancesCS;
    public ComputeShader scanGroupSumsCS;
    public ComputeShader copyInstanceDataCS;
    public HiZBuffer hiZBuffer;
    public Camera mainCamera;
    public Camera debugCamera;
    // prefab buffers
    private ComputeBuffer m_instancesArgsBuffer;
    private ComputeBuffer m_shadowArgsBuffer;
    // Compute Buffers
    private ComputeBuffer m_instancesIsVisibleBuffer;
    private ComputeBuffer m_instancesGroupSumArrayBuffer;
    private ComputeBuffer m_instancesScannedGroupSumBuffer;
    private ComputeBuffer m_instancesScannedPredicates;
    private ComputeBuffer m_instanceDataBuffer;
    private ComputeBuffer m_instancesSortingData;
    private ComputeBuffer m_instancesSortingDataTemp;
    private ComputeBuffer m_instancesMatrixRows01;
    private ComputeBuffer m_instancesCulledMatrixRows01;
    private ComputeBuffer m_shadowsIsVisibleBuffer;
    private ComputeBuffer m_shadowGroupSumArrayBuffer;
    private ComputeBuffer m_shadowsScannedGroupSumBuffer;
    private ComputeBuffer m_shadowScannedInstancePredicates;
    private ComputeBuffer m_shadowCulledMatrixRows01;

    // update buffers
    private ComputeBuffer m_positionsBuffer;
    private ComputeBuffer m_scaleBuffer;
    private ComputeBuffer m_rotationBuffer;
    // animation
    private ComputeBuffer m_instancesDrawAnimData;
    private ComputeBuffer m_instancesCulledAnimData;
    private ComputeBuffer m_shadowCulledAnimData;
    // remap final index
    private ComputeBuffer m_instancesDrawIndexRemap;
    private ComputeBuffer m_instancesCulledIndexRemap;
    private ComputeBuffer m_shadowCulledIndexRemap;
    
    
    // Command Buffers
    
    // Kernel ID's
    private int m_createDrawDataBufferKernelID;
    private int m_sortingCSKernelID;
    private int m_sortingTransposeKernelID;
    private int m_occlusionKernelID;
    private int m_scanInstancesKernelID;
    private int m_scanGroupSumsKernelID;
    private int m_copyInstanceDataKernelID;
    private bool m_isInitialized;
    
    // Other
    private int m_numberOfInstanceTypes;
    private int m_numberOfInstances;
    private int m_occlusionGroupX;
    private int m_scanInstancesGroupX;
    private int m_scanThreadGroupsGroupX;
    private int m_copyInstanceDataGroupX;
    private bool m_debugLastDrawLOD = false;
    private bool m_isEnabled;
    private uint[] m_args;
    private Bounds m_bounds;
    private Vector3 m_camPosition = Vector3.zero;
    private Vector3 m_lastCamPosition = Vector3.zero;
    private Matrix4x4 m_MVP;
    
    // Debug
    private AsyncGPUReadbackRequest m_debugGPUArgsRequest;
    private AsyncGPUReadbackRequest m_debugGPUShadowArgsRequest;
    private StringBuilder m_debugUIText = new StringBuilder(1000);
    private Text m_uiText;
    private GameObject m_uiObj;
    
    // Constants
    private const int NUMBER_OF_DRAW_CALLS = 3; // (LOD00 + LOD01 + LOD02)
    private const int NUMBER_OF_ARGS_PER_DRAW = 5; // (indexCount, instanceCount, startIndex, baseVertex, startInstance)
    public const int NUMBER_OF_ARGS_PER_INSTANCE_TYPE = NUMBER_OF_DRAW_CALLS * NUMBER_OF_ARGS_PER_DRAW; // 3draws * 5args = 15args
    private const int ARGS_BYTE_SIZE_PER_DRAW_CALL = NUMBER_OF_ARGS_PER_DRAW * sizeof(uint); // 5args * 4bytes = 20 bytes
    private const int ARGS_BYTE_SIZE_PER_INSTANCE_TYPE = NUMBER_OF_ARGS_PER_INSTANCE_TYPE * sizeof(uint); // 15args * 4bytes = 60bytes
    private const int SCAN_THREAD_GROUP_SIZE = 64;
    private const string DEBUG_UI_RED_COLOR =   "<color=#ff6666>";
    private const string DEBUG_UI_WHITE_COLOR = "<color=#ffffff>";
    private const string DEBUG_SHADER_LOD_KEYWORD = "INDIRECT_DEBUG_LOD";
    
    // Shader Property ID's
    private static readonly int _Data = Shader.PropertyToID("_Data");
    private static readonly int _Input = Shader.PropertyToID("_Input");
    private static readonly int _ShouldFrustumCull = Shader.PropertyToID("_ShouldFrustumCull");
    private static readonly int _ShouldOcclusionCull = Shader.PropertyToID("_ShouldOcclusionCull");
    private static readonly int _ShouldLOD = Shader.PropertyToID("_ShouldLOD");
    private static readonly int _ShouldDetailCull = Shader.PropertyToID("_ShouldDetailCull");
    private static readonly int _ShouldOnlyUseLOD02Shadows = Shader.PropertyToID("_ShouldOnlyUseLOD02Shadows");
    private static readonly int _UNITY_MATRIX_MVP = Shader.PropertyToID("_UNITY_MATRIX_MVP");
    private static readonly int _CamPosition = Shader.PropertyToID("_CamPosition");
    private static readonly int _HiZTextureSize = Shader.PropertyToID("_HiZTextureSize");
    private static readonly int _Level = Shader.PropertyToID("_Level");
    private static readonly int _LevelMask = Shader.PropertyToID("_LevelMask");
    private static readonly int _Width = Shader.PropertyToID("_Width");
    private static readonly int _Height = Shader.PropertyToID("_Height");
    private static readonly int _ShadowDistance = Shader.PropertyToID("_ShadowDistance");
    private static readonly int _DetailCullingScreenPercentage = Shader.PropertyToID("_DetailCullingScreenPercentage");
    private static readonly int _Lod0Distance = Shader.PropertyToID("_Lod0Distance");
    private static readonly int _Lod1Distance = Shader.PropertyToID("_Lod1Distance");
    
    private static readonly int _HiZMap = Shader.PropertyToID("_HiZMap");
    private static readonly int _NumOfGroups = Shader.PropertyToID("_NumOfGroups");
    private static readonly int _NumOfDrawcalls = Shader.PropertyToID("_NumOfDrawcalls");
    private static readonly int _ArgsOffset = Shader.PropertyToID("_ArgsOffset");
    private static readonly int _Positions = Shader.PropertyToID("_Positions");
    private static readonly int _Scales = Shader.PropertyToID("_Scales");
    private static readonly int _Rotations = Shader.PropertyToID("_Rotations");
    private static readonly int _ArgsBuffer = Shader.PropertyToID("_ArgsBuffer");
    private static readonly int _ShadowArgsBuffer = Shader.PropertyToID("_ShadowArgsBuffer");
    private static readonly int _IsVisibleBuffer = Shader.PropertyToID("_IsVisibleBuffer");
    private static readonly int _ShadowIsVisibleBuffer = Shader.PropertyToID("_ShadowIsVisibleBuffer");
    private static readonly int _GroupSumArray = Shader.PropertyToID("_GroupSumArray");
    private static readonly int _ScannedInstancePredicates = Shader.PropertyToID("_ScannedInstancePredicates");
    private static readonly int _GroupSumArrayIn = Shader.PropertyToID("_GroupSumArrayIn");
    private static readonly int _GroupSumArrayOut = Shader.PropertyToID("_GroupSumArrayOut");
    private static readonly int _DrawcallDataOut = Shader.PropertyToID("_DrawcallDataOut");
    private static readonly int _SortingData = Shader.PropertyToID("_SortingData");
    private static readonly int _InstanceDataBuffer = Shader.PropertyToID("_InstanceDataBuffer");
    private static readonly int _InstancePredicatesIn = Shader.PropertyToID("_InstancePredicatesIn");
    private static readonly int _InstancesDrawMatrixRows01 = Shader.PropertyToID("_InstancesDrawMatrixRows01");
    private static readonly int _InstancesCulledMatrixRows01 = Shader.PropertyToID("_InstancesCulledMatrixRows01");
    
    private static readonly int _InstancesCulledAnimData = Shader.PropertyToID("_InstancesCulledAnimData");
    private static readonly int _InstancesDrawAnimData = Shader.PropertyToID("_InstancesDrawAnimData");
    private static readonly int _InstancesCulledIndexRemap = Shader.PropertyToID("_InstancesCulledIndexRemap");
    #endregion

    #region MonoBehaviour

    public void SetRenderData(InstanceRenderData data) {
        if (this._rendererData != null) {
            this._rendererData.OnLayoutChangedEvent -= OnRenderDataLayoutChanged;
        }
        this._rendererData = data;  
        this._rendererData.OnLayoutChangedEvent += OnRenderDataLayoutChanged;
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
        if (this._rendererData != null) {
            this._rendererData.OnLayoutChangedEvent -= OnRenderDataLayoutChanged;
        }
        if (debugDrawLOD)
        {
            for (int i = 0; i < indirectMeshes.Length; i++)
            {
                indirectMeshes[i].material.DisableKeyword(DEBUG_SHADER_LOD_KEYWORD);
            }
        }
    }
    public void DoUpdate()
    {
        if (m_isEnabled)
        {
            UpdateDebug();
        }
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
        UpdateDebug();
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
    
    public void Initialize(List<IndirectInstanceData> Infos,int maxCount) {
        if (!m_isInitialized)
        {
            if (!TryGetKernels())
            {
                Debug.LogError("Load compute shader core failed!");
            }
            m_numberOfInstances = maxCount;
            InitPrefabBuffers(Infos);
            InitInstanceBuffers();
            m_isInitialized = true;
        }
    }


    public void OnRenderDataLayoutChanged() {
        if (m_numberOfInstances < _rendererData.Capacity) {
            m_numberOfInstances = _rendererData.Capacity;
            InitInstanceBuffers();
        }
    }

    public void StartDrawing()
    {
        if (!m_isInitialized)
        {
            Debug.LogError("IndirectRenderer: Unable to start drawing because it's not initialized");
            return;
        }
        
        m_isEnabled = true;
    }
    
    public void StopDrawing(bool shouldReleaseBuffers = false)
    {
        m_isEnabled = false;
        
        if (shouldReleaseBuffers)
        {
            ReleaseBuffers();
            m_isInitialized = false;
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
        m_camPosition =  mainCamera.transform.position;
        m_bounds.center = m_camPosition;
        m_bounds.extents = Vector3.one * 10000;
        
        //Matrix4x4 m = mainCamera.transform.localToWorldMatrix;
        Matrix4x4 v = mainCamera.worldToCameraMatrix;
        Matrix4x4 p = mainCamera.projectionMatrix;
        m_MVP = p * v;//*m;
        if (logDebugAll) {
            Debug.Log("logDebugAll =================== " + Time.frameCount);
        }

        if (logInstanceDrawMatrices|| logDebugAll)
        {
            logInstanceDrawMatrices = false;
            LogInstanceDrawMatrices("LogInstanceDrawMatrices()");
        }
        
        if (logInstanceAnimation|| logDebugAll)
        {
            //logInstanceAnimation = false;
            LogInstanceAnimation("logInstanceAnimation()");
        }
        
        //////////////////////////////////////////////////////
        // Reset the arguments buffer
        //////////////////////////////////////////////////////
        Profiler.BeginSample("Resetting args buffer");
        {
            m_instancesArgsBuffer.SetData(m_args);
            m_shadowArgsBuffer.SetData(m_args);
            if (logArgumentsAfterReset || logDebugAll)
            {
                logArgumentsAfterReset = false;
                LogArgsBuffers("LogArgsBuffers() - Instances After Reset", "LogArgsBuffers() - Shadows After Reset");
            }
        }
        Profiler.EndSample();
        
        Profiler.BeginSample("00 Calc Transform Matrix");
        // upload entity's matrix
        //if (_rendererData.isDirty) 
        {
            _rendererData.isDirty = false;
            m_positionsBuffer.SetData(_rendererData.positions);
            m_scaleBuffer.SetData(_rendererData.scales);
            m_rotationBuffer.SetData(_rendererData.rotations);
        
            m_instanceDataBuffer.SetData(_rendererData.bounds ); // bounds
            
            m_instancesDrawAnimData.SetData(_rendererData.animData);// anim data
            m_instancesSortingData.SetData(_rendererData.sortingData);// sorting data
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
            
            if (logArgumentsAfterOcclusion|| logDebugAll)
            {
                logArgumentsAfterOcclusion = false;
                LogArgsBuffers("LogArgsBuffers() - Instances After Occlusion", "LogArgsBuffers() - Shadows After Occlusion");
            }
            
            if (logInstancesIsVisibleBuffer|| logDebugAll)
            {
                logInstancesIsVisibleBuffer = false;
                LogInstancesIsVisibleBuffers("LogInstancesIsVisibleBuffers() - Instances", "LogInstancesIsVisibleBuffers() - Shadows");
            }
        }
        Profiler.EndSample();
        
        
        //////////////////////////////////////////////////////
        // Sort the position buffer based on distance from camera
        //////////////////////////////////////////////////////
        Profiler.BeginSample("02 LOD Sorting");
        {
            m_lastCamPosition = m_camPosition;
            SortRenderDatas();
            if (logSortingData|| logDebugAll)
            {
                logSortingData = false;
                LogSortingData("LogSortingData())");
            }
        }
        Profiler.EndSample();
        
        //////////////////////////////////////////////////////
        // Perform scan of instance predicates
        //////////////////////////////////////////////////////
        Profiler.BeginSample("03 Scan Instances");
        {
            // Normal
            scanInstancesCS.SetBuffer(m_scanInstancesKernelID, _InstancePredicatesIn,         m_instancesIsVisibleBuffer);
            scanInstancesCS.SetBuffer(m_scanInstancesKernelID, _GroupSumArray,                m_instancesGroupSumArrayBuffer);
            scanInstancesCS.SetBuffer(m_scanInstancesKernelID, _ScannedInstancePredicates,    m_instancesScannedPredicates);
            scanInstancesCS.Dispatch(m_scanInstancesKernelID, m_scanInstancesGroupX, 1, 1);
            
            // Shadows
            scanInstancesCS.SetBuffer(m_scanInstancesKernelID, _InstancePredicatesIn,         m_shadowsIsVisibleBuffer);
            scanInstancesCS.SetBuffer(m_scanInstancesKernelID, _GroupSumArray,                m_shadowGroupSumArrayBuffer);
            scanInstancesCS.SetBuffer(m_scanInstancesKernelID, _ScannedInstancePredicates,    m_shadowScannedInstancePredicates);
            scanInstancesCS.Dispatch(m_scanInstancesKernelID, m_scanInstancesGroupX, 1, 1);
            
            if (logGroupSumArrayBuffer|| logDebugAll)
            {
                logGroupSumArrayBuffer = false;
                LogGroupSumArrayBuffer("LogGroupSumArrayBuffer() - Instances", "LogGroupSumArrayBuffer() - Shadows");
            }
            
            if (logScannedPredicates|| logDebugAll)
            {
                logScannedPredicates = false;
                LogScannedPredicates("LogScannedPredicates() - Instances", "LogScannedPredicates() - Shadows");
            }
        }
        Profiler.EndSample();
        
        //////////////////////////////////////////////////////
        // Perform scan of group sums
        //////////////////////////////////////////////////////
        Profiler.BeginSample("04 Scan Thread Groups");
        {
            // Normal
            scanGroupSumsCS.SetBuffer(m_scanGroupSumsKernelID, _GroupSumArrayIn,     m_instancesGroupSumArrayBuffer);
            scanGroupSumsCS.SetBuffer(m_scanGroupSumsKernelID, _GroupSumArrayOut,    m_instancesScannedGroupSumBuffer);
            scanGroupSumsCS.Dispatch(m_scanGroupSumsKernelID, m_scanThreadGroupsGroupX, 1, 1);
            
            // Shadows
            scanGroupSumsCS.SetBuffer(m_scanGroupSumsKernelID, _GroupSumArrayIn,     m_shadowGroupSumArrayBuffer);
            scanGroupSumsCS.SetBuffer(m_scanGroupSumsKernelID, _GroupSumArrayOut,    m_shadowsScannedGroupSumBuffer);
            scanGroupSumsCS.Dispatch(m_scanGroupSumsKernelID, m_scanThreadGroupsGroupX, 1, 1);
            
            if (logScannedGroupSumsBuffer|| logDebugAll)
            {
                logScannedGroupSumsBuffer = false;
                LogScannedGroupSumBuffer("LogScannedGroupSumBuffer() - Instances", "LogScannedGroupSumBuffer() - Shadows");
            }
        }
        Profiler.EndSample();
        
        //////////////////////////////////////////////////////
        // Perform stream compaction 
        // Calculate instance offsets and store in drawcall arguments buffer
        //////////////////////////////////////////////////////
        Profiler.BeginSample("05 Copy Instance Data");
        {
            // Normal
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancePredicatesIn,         m_instancesIsVisibleBuffer);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _GroupSumArray,                m_instancesScannedGroupSumBuffer);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _ScannedInstancePredicates,    m_instancesScannedPredicates);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledAnimData,      m_instancesCulledAnimData);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledMatrixRows01,  m_instancesCulledMatrixRows01);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _DrawcallDataOut,              m_instancesArgsBuffer);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledIndexRemap,    m_instancesCulledIndexRemap);
            copyInstanceDataCS.Dispatch(m_copyInstanceDataKernelID, m_copyInstanceDataGroupX, 1, 1);
            
            // Shadows
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancePredicatesIn,         m_shadowsIsVisibleBuffer);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _GroupSumArray,                m_shadowsScannedGroupSumBuffer);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _ScannedInstancePredicates,    m_shadowScannedInstancePredicates);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledAnimData,      m_shadowCulledAnimData);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledMatrixRows01,  m_shadowCulledMatrixRows01);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _DrawcallDataOut,              m_shadowArgsBuffer);
            copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesCulledIndexRemap,    m_shadowCulledIndexRemap);
            
            copyInstanceDataCS.Dispatch(m_copyInstanceDataKernelID, m_copyInstanceDataGroupX, 1, 1);
            
            if (logCulledInstancesDrawMatrices|| logDebugAll)
            {
                logCulledInstancesDrawMatrices = false;
                LogCulledInstancesDrawMatrices("LogCulledInstancesDrawMatrices() - Instances", "LogCulledInstancesDrawMatrices() - Shadows");
            }
            if (logCulledInstancesAnimData|| logDebugAll)
            {
                logCulledInstancesAnimData = false;
                LogCulledInstancesAnimData("LogCulledInstancesAnimData() - Instances", "LogCulledInstancesAnimData() - Shadows");
            }
                        
            
            if (logArgsBufferAfterCopy|| logDebugAll)
            {
                logArgsBufferAfterCopy = false;
                LogArgsBuffers("LogArgsBuffers() - Instances After Copy", "LogArgsBuffers() - Shadows After Copy");
            }
        }
        Profiler.EndSample();
        if (logDebugAll) {
            logDebugAll = false;
        }
        if (LogAllArgBuffer) {
            LogAllBuffers(LogAllArgBufferCount);
        }
    }
    
    
    private void SortRenderDatas()
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
            SetGPUSortConstants( ref sortingCS, ref level, ref level, ref MATRIX_HEIGHT, ref MATRIX_WIDTH);

            // Sort the row data
            sortingCS.SetBuffer( m_sortingCSKernelID, _Data, m_instancesSortingData);
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
            SetGPUSortConstants(ref sortingCS, ref l, ref lm, ref MATRIX_WIDTH, ref MATRIX_HEIGHT);
            sortingCS.SetBuffer(m_sortingTransposeKernelID, _Input, m_instancesSortingData);
            sortingCS.SetBuffer( m_sortingTransposeKernelID, _Data, m_instancesSortingDataTemp);
            sortingCS.Dispatch( m_sortingTransposeKernelID, (int)(MATRIX_WIDTH / TRANSPOSE_BLOCK_SIZE), (int)(MATRIX_HEIGHT / TRANSPOSE_BLOCK_SIZE), 1);

            // Sort the transposed column data
            sortingCS.SetBuffer( m_sortingCSKernelID, _Data, m_instancesSortingDataTemp);
            sortingCS.Dispatch(m_sortingCSKernelID, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);

            // Transpose the data from buffer 2 back into buffer 1
            SetGPUSortConstants(ref sortingCS, ref BITONIC_BLOCK_SIZE, ref level, ref MATRIX_HEIGHT, ref MATRIX_WIDTH);
            sortingCS.SetBuffer( m_sortingTransposeKernelID, _Input, m_instancesSortingDataTemp);
            sortingCS.SetBuffer( m_sortingTransposeKernelID, _Data, m_instancesSortingData);
            sortingCS.Dispatch(m_sortingTransposeKernelID, (int)(MATRIX_HEIGHT / TRANSPOSE_BLOCK_SIZE), (int)(MATRIX_WIDTH / TRANSPOSE_BLOCK_SIZE), 1);

            // Sort the row data
            sortingCS.SetBuffer( m_sortingCSKernelID, _Data, m_instancesSortingData);
            sortingCS.Dispatch(m_sortingCSKernelID, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);
        }
    }
    
    private void SetGPUSortConstants( ref ComputeShader cs, ref uint level, ref uint levelMask, ref uint width, ref uint height)
    {
        cs.SetInt( _Level, (int)level);
        cs.SetInt( _LevelMask, (int)levelMask);
        cs.SetInt( _Width, (int)width);
        cs.SetInt( _Height, (int)height);
    }
    
    private bool TryGetKernels()
    {
        return TryGetKernel("CSMain",           ref createDrawDataBufferCS, ref m_createDrawDataBufferKernelID)
            && TryGetKernel("BitonicSort",      ref sortingCS,            ref m_sortingCSKernelID)
            && TryGetKernel("MatrixTranspose",  ref sortingCS,            ref m_sortingTransposeKernelID)
            && TryGetKernel("CSMain",           ref occlusionCS,            ref m_occlusionKernelID)
            && TryGetKernel("CSMain",           ref scanInstancesCS,        ref m_scanInstancesKernelID)
            && TryGetKernel("CSMain",           ref scanGroupSumsCS,        ref m_scanGroupSumsKernelID)
            && TryGetKernel("CSMain",           ref copyInstanceDataCS,     ref m_copyInstanceDataKernelID)
        ;
    }



    private void InitInstanceBuffers()
    {
        Debug.LogWarning("InitInstanceBuffers " + m_numberOfInstances);
        int computeShaderInputSize = Marshal.SizeOf(typeof(InstanceBound));
        int computeShaderDrawMatrixSize = Marshal.SizeOf(typeof(Indirect2x2Matrix));
        int computeSortingDataSize = Marshal.SizeOf(typeof(SortingData));
        int computeAnimDataSize = Marshal.SizeOf(typeof(AnimRenderData));
        int computeIndexRemapDataSize = Marshal.SizeOf(typeof(uint));
        ReleaseInstanceBuffers();
        
        m_instancesDrawAnimData                = new ComputeBuffer(m_numberOfInstances, computeAnimDataSize, ComputeBufferType.Default);
        m_instancesCulledAnimData             = new ComputeBuffer(m_numberOfInstances, computeAnimDataSize, ComputeBufferType.Default);
        m_shadowCulledAnimData             = new ComputeBuffer(m_numberOfInstances, computeAnimDataSize, ComputeBufferType.Default);
        
        m_instancesDrawIndexRemap             = new ComputeBuffer(m_numberOfInstances, computeIndexRemapDataSize, ComputeBufferType.Default);
        m_instancesCulledIndexRemap             = new ComputeBuffer(m_numberOfInstances, computeIndexRemapDataSize, ComputeBufferType.Default);
        m_shadowCulledIndexRemap             = new ComputeBuffer(m_numberOfInstances, computeIndexRemapDataSize, ComputeBufferType.Default);
            
        m_instanceDataBuffer              = new ComputeBuffer(m_numberOfInstances, computeShaderInputSize, ComputeBufferType.Default);
        m_instancesSortingData            = new ComputeBuffer(m_numberOfInstances, computeSortingDataSize, ComputeBufferType.Default);
        m_instancesSortingDataTemp        = new ComputeBuffer(m_numberOfInstances, computeSortingDataSize, ComputeBufferType.Default);
        m_instancesMatrixRows01           = new ComputeBuffer(m_numberOfInstances, computeShaderDrawMatrixSize, ComputeBufferType.Default);
        m_instancesCulledMatrixRows01     = new ComputeBuffer(m_numberOfInstances, computeShaderDrawMatrixSize, ComputeBufferType.Default);
        m_instancesIsVisibleBuffer        = new ComputeBuffer(m_numberOfInstances, sizeof(uint), ComputeBufferType.Default);
        m_instancesScannedPredicates      = new ComputeBuffer(m_numberOfInstances, sizeof(uint), ComputeBufferType.Default);
        m_instancesGroupSumArrayBuffer    = new ComputeBuffer(m_numberOfInstances, sizeof(uint), ComputeBufferType.Default);
        m_instancesScannedGroupSumBuffer  = new ComputeBuffer(m_numberOfInstances, sizeof(uint), ComputeBufferType.Default);
        
        m_shadowCulledMatrixRows01        = new ComputeBuffer(m_numberOfInstances, computeShaderDrawMatrixSize, ComputeBufferType.Default);
        m_shadowsIsVisibleBuffer          = new ComputeBuffer(m_numberOfInstances, sizeof(uint), ComputeBufferType.Default);
        m_shadowScannedInstancePredicates = new ComputeBuffer(m_numberOfInstances, sizeof(uint), ComputeBufferType.Default);
        m_shadowGroupSumArrayBuffer       = new ComputeBuffer(m_numberOfInstances, sizeof(uint), ComputeBufferType.Default);
        m_shadowsScannedGroupSumBuffer    = new ComputeBuffer(m_numberOfInstances, sizeof(uint), ComputeBufferType.Default);
        
        // Setup the Material Property blocks for our meshes...
        for (int i = 0; i < indirectMeshes.Length; i++)
        {
            IndirectRenderingMesh irm = indirectMeshes[i];
            int argsIndex = i * NUMBER_OF_ARGS_PER_INSTANCE_TYPE;
            
            irm.lod00MatPropBlock = new MaterialPropertyBlock();
            irm.lod01MatPropBlock = new MaterialPropertyBlock();
            irm.lod02MatPropBlock = new MaterialPropertyBlock();
            irm.shadowLod00MatPropBlock = new MaterialPropertyBlock();
            irm.shadowLod01MatPropBlock = new MaterialPropertyBlock();
            irm.shadowLod02MatPropBlock = new MaterialPropertyBlock();
            
            irm.lod00MatPropBlock.SetInt(_ArgsOffset, argsIndex + 4);
            irm.lod01MatPropBlock.SetInt(_ArgsOffset, argsIndex + 9);
            irm.lod02MatPropBlock.SetInt(_ArgsOffset, argsIndex + 14);
            
            irm.shadowLod00MatPropBlock.SetInt(_ArgsOffset, argsIndex + 4);
            irm.shadowLod01MatPropBlock.SetInt(_ArgsOffset, argsIndex + 9);
            irm.shadowLod02MatPropBlock.SetInt(_ArgsOffset, argsIndex + 14);
            
            irm.lod00MatPropBlock.SetBuffer(_ArgsBuffer, m_instancesArgsBuffer);
            irm.lod01MatPropBlock.SetBuffer(_ArgsBuffer, m_instancesArgsBuffer);
            irm.lod02MatPropBlock.SetBuffer(_ArgsBuffer, m_instancesArgsBuffer);
            
            irm.shadowLod00MatPropBlock.SetBuffer(_ArgsBuffer, m_shadowArgsBuffer);
            irm.shadowLod01MatPropBlock.SetBuffer(_ArgsBuffer, m_shadowArgsBuffer);
            irm.shadowLod02MatPropBlock.SetBuffer(_ArgsBuffer, m_shadowArgsBuffer);
            
            irm.lod00MatPropBlock.SetBuffer(_InstancesDrawAnimData, m_instancesCulledAnimData);
            irm.lod01MatPropBlock.SetBuffer(_InstancesDrawAnimData, m_instancesCulledAnimData);
            irm.lod02MatPropBlock.SetBuffer(_InstancesDrawAnimData, m_instancesCulledAnimData);
            
            irm.lod00MatPropBlock.SetBuffer(_InstancesCulledIndexRemap, m_instancesCulledIndexRemap);
            irm.lod01MatPropBlock.SetBuffer(_InstancesCulledIndexRemap, m_instancesCulledIndexRemap);
            irm.lod02MatPropBlock.SetBuffer(_InstancesCulledIndexRemap, m_instancesCulledIndexRemap);
            
            
            irm.lod00MatPropBlock.SetBuffer(_InstancesDrawMatrixRows01, m_instancesCulledMatrixRows01);
            irm.lod01MatPropBlock.SetBuffer(_InstancesDrawMatrixRows01, m_instancesCulledMatrixRows01);
            irm.lod02MatPropBlock.SetBuffer(_InstancesDrawMatrixRows01, m_instancesCulledMatrixRows01);
            
            
            irm.shadowLod00MatPropBlock.SetBuffer(_InstancesDrawAnimData, m_shadowCulledAnimData);
            irm.shadowLod01MatPropBlock.SetBuffer(_InstancesDrawAnimData, m_shadowCulledAnimData);
            irm.shadowLod02MatPropBlock.SetBuffer(_InstancesDrawAnimData, m_shadowCulledAnimData);
            
            irm.shadowLod00MatPropBlock.SetBuffer(_InstancesCulledIndexRemap, m_shadowCulledIndexRemap);
            irm.shadowLod01MatPropBlock.SetBuffer(_InstancesCulledIndexRemap, m_shadowCulledIndexRemap);
            irm.shadowLod02MatPropBlock.SetBuffer(_InstancesCulledIndexRemap, m_shadowCulledIndexRemap);
            
            irm.shadowLod00MatPropBlock.SetBuffer(_InstancesDrawMatrixRows01, m_shadowCulledMatrixRows01);
            irm.shadowLod01MatPropBlock.SetBuffer(_InstancesDrawMatrixRows01, m_shadowCulledMatrixRows01);
            irm.shadowLod02MatPropBlock.SetBuffer(_InstancesDrawMatrixRows01, m_shadowCulledMatrixRows01);
            
        }
        
        //-----------------------------------
        // InitializeDrawData
        //-----------------------------------
        
        
        // Create the buffer containing draw data for all instances
        m_positionsBuffer = new ComputeBuffer(m_numberOfInstances, Marshal.SizeOf(typeof(Vector3)), ComputeBufferType.Default);
        m_scaleBuffer = new ComputeBuffer(m_numberOfInstances, Marshal.SizeOf(typeof(Vector3)), ComputeBufferType.Default);
        m_rotationBuffer = new ComputeBuffer(m_numberOfInstances, Marshal.SizeOf(typeof(Vector3)), ComputeBufferType.Default);
        
        
        createDrawDataBufferCS.SetBuffer(m_createDrawDataBufferKernelID, _Positions, m_positionsBuffer);
        createDrawDataBufferCS.SetBuffer(m_createDrawDataBufferKernelID, _Scales, m_scaleBuffer);
        createDrawDataBufferCS.SetBuffer(m_createDrawDataBufferKernelID, _Rotations, m_rotationBuffer);
        createDrawDataBufferCS.SetBuffer(m_createDrawDataBufferKernelID, _InstancesDrawMatrixRows01, m_instancesMatrixRows01);

        
        //-----------------------------------
        // InitConstantComputeVariables
        //-----------------------------------
        
        m_occlusionGroupX = Mathf.Max(1, m_numberOfInstances / 64);
        m_scanInstancesGroupX = Mathf.Max(1, m_numberOfInstances / (2 * SCAN_THREAD_GROUP_SIZE));
        m_scanThreadGroupsGroupX = 1;
        m_copyInstanceDataGroupX = Mathf.Max(1, m_numberOfInstances / (2 * SCAN_THREAD_GROUP_SIZE));
        
        occlusionCS.SetInt(_ShouldFrustumCull,          enableFrustumCulling    ? 1 : 0);
        occlusionCS.SetInt(_ShouldOcclusionCull,        enableOcclusionCulling  ? 1 : 0);
        occlusionCS.SetInt(_ShouldDetailCull,           enableDetailCulling     ? 1 : 0);
        occlusionCS.SetInt(_ShouldLOD,                  enableLOD               ? 1 : 0);
        occlusionCS.SetInt(_ShouldOnlyUseLOD02Shadows,  enableOnlyLOD02Shadows  ? 1 : 0);
        occlusionCS.SetFloat(_ShadowDistance, shadowDistance);
        occlusionCS.SetFloat(_DetailCullingScreenPercentage, detailCullingPercentage);
        occlusionCS.SetFloat(_Lod0Distance, lod0Distance);
        occlusionCS.SetFloat(_Lod1Distance, lod1Distance);
        occlusionCS.SetVector(_HiZTextureSize, hiZBuffer.TextureSize);
        occlusionCS.SetBuffer(m_occlusionKernelID, _InstanceDataBuffer, m_instanceDataBuffer);
        occlusionCS.SetBuffer(m_occlusionKernelID, _ArgsBuffer, m_instancesArgsBuffer);
        occlusionCS.SetBuffer(m_occlusionKernelID, _ShadowArgsBuffer, m_shadowArgsBuffer);
        occlusionCS.SetBuffer(m_occlusionKernelID, _IsVisibleBuffer, m_instancesIsVisibleBuffer);
        occlusionCS.SetBuffer(m_occlusionKernelID, _ShadowIsVisibleBuffer, m_shadowsIsVisibleBuffer);
        occlusionCS.SetTexture(m_occlusionKernelID, _HiZMap, hiZBuffer.Texture);
        occlusionCS.SetBuffer(m_occlusionKernelID, _SortingData, m_instancesSortingData);
        
        scanGroupSumsCS.SetInt(_NumOfGroups, m_numberOfInstances / (2 * SCAN_THREAD_GROUP_SIZE));
        
        copyInstanceDataCS.SetInt(_NumOfDrawcalls, m_numberOfInstanceTypes * NUMBER_OF_DRAW_CALLS);
        copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstanceDataBuffer, m_instanceDataBuffer);
        copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesDrawMatrixRows01, m_instancesMatrixRows01);
        copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _SortingData, m_instancesSortingData);
        copyInstanceDataCS.SetBuffer(m_copyInstanceDataKernelID, _InstancesDrawAnimData, m_instancesDrawAnimData);
        
        //CreateCommandBuffers();
        
    }

    private void InitPrefabBuffers(List<IndirectInstanceData> _instances) {
        m_numberOfInstanceTypes = _instances.Count;
        hiZBuffer.enabled = true;
        hiZBuffer.InitializeTexture();
        indirectMeshes = new IndirectRenderingMesh[m_numberOfInstanceTypes];
        m_args = new uint[m_numberOfInstanceTypes * NUMBER_OF_ARGS_PER_INSTANCE_TYPE];
        for (int i = 0; i < m_numberOfInstanceTypes; i++) {
            IndirectRenderingMesh irm = new IndirectRenderingMesh();
            IndirectInstanceData iid = _instances[i];

            // Initialize Mesh
            irm.numOfVerticesLod00 = (uint)iid.lod00Mesh.vertexCount;
            irm.numOfVerticesLod01 = (uint)iid.lod01Mesh.vertexCount;
            irm.numOfVerticesLod02 = (uint)iid.lod02Mesh.vertexCount;
            irm.numOfIndicesLod00 = iid.lod00Mesh.GetIndexCount(0);
            irm.numOfIndicesLod01 = iid.lod01Mesh.GetIndexCount(0);
            irm.numOfIndicesLod02 = iid.lod02Mesh.GetIndexCount(0);

            irm.mesh = new Mesh();
            irm.mesh.name = iid.prefab.name;
            irm.mesh.CombineMeshes(
                new CombineInstance[] {
                    new CombineInstance() { mesh = iid.lod00Mesh },
                    new CombineInstance() { mesh = iid.lod01Mesh },
                    new CombineInstance() { mesh = iid.lod02Mesh }
                },
                true, // Merge Submeshes 
                false, // Use Matrices
                false // Has lightmap data
            );

            // Arguments
            int argsIndex = i * NUMBER_OF_ARGS_PER_INSTANCE_TYPE;

            // Buffer with arguments has to have five integer numbers
            // LOD00
            m_args[argsIndex + 0] = irm.numOfIndicesLod00; // 0 - index count per instance, 
            m_args[argsIndex + 1] = 0; // 1 - instance count
            m_args[argsIndex + 2] = 0; // 2 - start index location
            m_args[argsIndex + 3] = 0; // 3 - base vertex location
            m_args[argsIndex + 4] = 0; // 4 - start instance location

            // LOD01
            m_args[argsIndex + 5] = irm.numOfIndicesLod01; // 0 - index count per instance, 
            m_args[argsIndex + 6] = 0; // 1 - instance count
            m_args[argsIndex + 7] = m_args[argsIndex + 0] + m_args[argsIndex + 2]; // 2 - start index location
            m_args[argsIndex + 8] = 0; // 3 - base vertex location
            m_args[argsIndex + 9] = 0; // 4 - start instance location

            // LOD02
            m_args[argsIndex + 10] = irm.numOfIndicesLod02; // 0 - index count per instance, 
            m_args[argsIndex + 11] = 0; // 1 - instance count
            m_args[argsIndex + 12] = m_args[argsIndex + 5] + m_args[argsIndex + 7]; // 2 - start index location
            m_args[argsIndex + 13] = 0; // 3 - base vertex location
            m_args[argsIndex + 14] = 0; // 4 - start instance location

            // Materials
            irm.material = iid.indirectMaterial; //new Material(iid.indirectMaterial);
            irm.originalBounds = CalculateBounds(iid.prefab);
            // Add the data to the renderer list
            indirectMeshes[i] = irm;
            _rendererData.prefabSize[i] = irm.originalBounds.size;
        }

        ReleasePrefabBuffer();
        m_instancesArgsBuffer = new ComputeBuffer(m_numberOfInstanceTypes * NUMBER_OF_ARGS_PER_INSTANCE_TYPE, sizeof(uint),
            ComputeBufferType.IndirectArguments);
        m_shadowArgsBuffer = new ComputeBuffer(m_numberOfInstanceTypes * NUMBER_OF_ARGS_PER_INSTANCE_TYPE, sizeof(uint),
            ComputeBufferType.IndirectArguments);

        m_instancesArgsBuffer.SetData(m_args);
        m_shadowArgsBuffer.SetData(m_args);
    }

    private void ReleasePrefabBuffer() {
        ReleaseComputeBuffer(ref m_instancesArgsBuffer);
        ReleaseComputeBuffer(ref m_shadowArgsBuffer);
    }

    private Bounds CalculateBounds(GameObject _prefab)
    {
        GameObject obj = Instantiate(_prefab);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.Euler(Vector3.zero);
        obj.transform.localScale = Vector3.one;
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds();
        if (rends.Length > 0)
        {
            b = new Bounds(rends[0].bounds.center, rends[0].bounds.size);
            for (int r = 1; r < rends.Length; r++)
            {
                b.Encapsulate(rends[r].bounds);
            }
        }
        b.center = Vector3.zero;
        DestroyImmediate(obj);
        
        return b;
    }

    private void ReleaseBuffers() {
        ReleasePrefabBuffer();
        ReleaseInstanceBuffers();
    }

    private void ReleaseInstanceBuffers()
    {
        //ReleaseCommandBuffer(ref visibleInstancesCB);
        
        ReleaseComputeBuffer(ref m_instancesIsVisibleBuffer);
        ReleaseComputeBuffer(ref m_instancesGroupSumArrayBuffer);
        ReleaseComputeBuffer(ref m_instancesScannedGroupSumBuffer);
        ReleaseComputeBuffer(ref m_instancesScannedPredicates);
        ReleaseComputeBuffer(ref m_instanceDataBuffer);
        ReleaseComputeBuffer(ref m_instancesSortingData);
        ReleaseComputeBuffer(ref m_instancesSortingDataTemp);
        ReleaseComputeBuffer(ref m_instancesMatrixRows01);
        ReleaseComputeBuffer(ref m_instancesCulledMatrixRows01);
        
        ReleaseComputeBuffer(ref m_shadowsIsVisibleBuffer);
        ReleaseComputeBuffer(ref m_shadowGroupSumArrayBuffer);
        ReleaseComputeBuffer(ref m_shadowsScannedGroupSumBuffer);
        ReleaseComputeBuffer(ref m_shadowScannedInstancePredicates);
        ReleaseComputeBuffer(ref m_shadowCulledMatrixRows01);
        
        ReleaseComputeBuffer(ref m_positionsBuffer);
        ReleaseComputeBuffer(ref m_scaleBuffer);
        ReleaseComputeBuffer(ref m_rotationBuffer);

        ReleaseComputeBuffer(ref m_instancesDrawAnimData);
        ReleaseComputeBuffer(ref m_instancesCulledAnimData);
        ReleaseComputeBuffer(ref m_shadowCulledAnimData);
        
        ReleaseComputeBuffer(ref m_instancesDrawIndexRemap);
        ReleaseComputeBuffer(ref m_instancesCulledIndexRemap );
        ReleaseComputeBuffer(ref m_shadowCulledIndexRemap    );
        
    }
    
    private static void ReleaseComputeBuffer(ref ComputeBuffer _buffer)
    {
        if (_buffer == null)
        {
            return;
        }

        _buffer.Release();
        _buffer = null;
    }
    
    private static void ReleaseCommandBuffer(ref CommandBuffer _buffer)
    {
        if (_buffer == null)
        {
            return;
        }

        _buffer.Release();
        _buffer = null;
    }
    
    private static bool TryGetKernel(string kernelName, ref ComputeShader cs, ref int kernelID)
    {
        if (!cs.HasKernel(kernelName))
        {
            Debug.LogError(kernelName + " kernel not found in " + cs.name + "!");
            return false;
        }
        
        kernelID = cs.FindKernel(kernelName);
        return true;
    }
    
    #endregion

    #region Debug & Logging
    
    private void UpdateDebug()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        occlusionCS.SetInt(_ShouldFrustumCull,          enableFrustumCulling    ? 1 : 0);
        occlusionCS.SetInt(_ShouldOcclusionCull,        enableOcclusionCulling  ? 1 : 0);
        occlusionCS.SetInt(_ShouldDetailCull,           enableDetailCulling     ? 1 : 0);
        occlusionCS.SetInt(_ShouldLOD,                  enableLOD               ? 1 : 0);
        occlusionCS.SetInt(_ShouldOnlyUseLOD02Shadows,  enableOnlyLOD02Shadows  ? 1 : 0);
        occlusionCS.SetFloat(_DetailCullingScreenPercentage, detailCullingPercentage);
        
        if (debugDrawLOD != m_debugLastDrawLOD)
        {
            m_debugLastDrawLOD = debugDrawLOD;
            
            if (debugDrawLOD)
            {
                for (int i = 0; i < indirectMeshes.Length; i++)
                {
                    indirectMeshes[i].material.EnableKeyword(DEBUG_SHADER_LOD_KEYWORD);
                }
            }
            else
            {
                for (int i = 0; i < indirectMeshes.Length; i++)
                {
                    indirectMeshes[i].material.DisableKeyword(DEBUG_SHADER_LOD_KEYWORD);
                }
            }
        }

        if (logDebugAll) {
            
        }

        UpdateDebugUI();
    }
    
    private void UpdateDebugUI()
    {
        if (!debugShowUI)
        {
            if (m_uiObj != null)
            {
                Destroy(m_uiObj);
            }
            return;
        }
        
        if (m_uiObj == null)
        {
            m_uiObj = Instantiate(debugUIPrefab);
            m_uiObj.transform.parent = transform;
            m_uiText = m_uiObj.transform.GetComponentInChildren<Text>();
        }
        
        if (m_debugGPUArgsRequest.hasError || m_debugGPUShadowArgsRequest.hasError)
        {
            m_debugGPUArgsRequest = AsyncGPUReadback.Request(m_instancesArgsBuffer);
            m_debugGPUShadowArgsRequest = AsyncGPUReadback.Request(m_shadowArgsBuffer);
        }
        else if (m_debugGPUArgsRequest.done && m_debugGPUShadowArgsRequest.done)
        {
            NativeArray<uint> argsBuffer = m_debugGPUArgsRequest.GetData<uint>();
            NativeArray<uint> shadowArgsBuffer = m_debugGPUShadowArgsRequest.GetData<uint>();
            
            m_debugUIText.Length = 0;
            
            uint totalCount = 0;
            uint totalLod00Count = 0;
            uint totalLod01Count = 0;
            uint totalLod02Count = 0;
            
            uint totalShadowCount = 0;
            uint totalShadowLod00Count = 0;
            uint totalShadowLod01Count = 0;
            uint totalShadowLod02Count = 0;
            
            uint totalIndices = 0;
            uint totalLod00Indices = 0;
            uint totalLod01Indices = 0;
            uint totalLod02Indices = 0;
            
            uint totalShadowIndices = 0;
            uint totalShadowLod00Indices = 0;
            uint totalShadowLod01Indices = 0;
            uint totalShadowLod02Indices = 0;
            
            uint totalVertices = 0;
            uint totalLod00Vertices = 0;
            uint totalLod01Vertices = 0;
            uint totalLod02Vertices = 0;
            
            uint totalShadowVertices = 0;
            uint totalShadowLod00Vertices = 0;
            uint totalShadowLod01Vertices = 0;
            uint totalShadowLod02Vertices = 0;
            
            int instanceIndex = 0;
            uint normMultiplier = (uint) (drawInstances ? 1 : 0);
            uint shadowMultiplier = (uint) (drawInstanceShadows && QualitySettings.shadows != ShadowQuality.Disable ? 1 : 0);
            int cascades = QualitySettings.shadowCascades;
            
            
            m_debugUIText.AppendLine(
                $"<color=#ffffff>Name {Time.frameCount}".PadRight(32)//.Substring(0, 58)
                + $"Instances".PadRight(25)//.Substring(0, 25)
                + $"Shadow Instances".PadRight(25)//.Substring(0, 25)
                + $"Vertices".PadRight(31)//.Substring(0, 25)
                + $"Indices</color>"
            );
            
            for (int i = 0; i < argsBuffer.Length; i = i + NUMBER_OF_ARGS_PER_INSTANCE_TYPE)
            {
                IndirectRenderingMesh irm = indirectMeshes[instanceIndex];
                
                uint lod00Count = argsBuffer[i +  1] * normMultiplier;
                uint lod01Count = argsBuffer[i +  6] * normMultiplier;
                uint lod02Count = argsBuffer[i + 11] * normMultiplier;
                
                uint lod00ShadowCount = shadowArgsBuffer[i +  1] * shadowMultiplier;
                uint lod01ShadowCount = shadowArgsBuffer[i +  6] * shadowMultiplier;
                uint lod02ShadowCount = shadowArgsBuffer[i + 11] * shadowMultiplier;
                
                uint lod00Indices = argsBuffer[i +  0] * normMultiplier;
                uint lod01Indices = argsBuffer[i +  5] * normMultiplier;
                uint lod02Indices = argsBuffer[i + 10] * normMultiplier;
                
                uint shadowLod00Indices = shadowArgsBuffer[i +  0] * shadowMultiplier;
                uint shadowLod01Indices = shadowArgsBuffer[i +  5] * shadowMultiplier;
                uint shadowLod02Indices = shadowArgsBuffer[i + 10] * shadowMultiplier;
                
                uint lod00Vertices = irm.numOfVerticesLod00 * normMultiplier;
                uint lod01Vertices = irm.numOfVerticesLod01 * normMultiplier;
                uint lod02Vertices = irm.numOfVerticesLod02 * normMultiplier;
                
                uint shadowLod00Vertices = irm.numOfVerticesLod00 * shadowMultiplier;
                uint shadowLod01Vertices = irm.numOfVerticesLod01 * shadowMultiplier;
                uint shadowLod02Vertices = irm.numOfVerticesLod02 * shadowMultiplier;
                
                // Output...
                string lod00VertColor = (lod00Vertices > 10000 ? DEBUG_UI_RED_COLOR : DEBUG_UI_WHITE_COLOR);
                string lod01VertColor = (lod01Vertices > 5000 ? DEBUG_UI_RED_COLOR : DEBUG_UI_WHITE_COLOR);
                string lod02VertColor = (lod02Vertices > 1000 ? DEBUG_UI_RED_COLOR : DEBUG_UI_WHITE_COLOR);
                
                string lod00IndicesColor = (lod00Indices > (lod00Vertices * 3.33f) ? DEBUG_UI_RED_COLOR : DEBUG_UI_WHITE_COLOR);
                string lod01IndicesColor = (lod01Indices > (lod01Vertices * 3.33f) ? DEBUG_UI_RED_COLOR : DEBUG_UI_WHITE_COLOR);
                string lod02IndicesColor = (lod02Indices > (lod02Vertices * 3.33f) ? DEBUG_UI_RED_COLOR : DEBUG_UI_WHITE_COLOR);
                
                m_debugUIText.AppendLine(
                    $"<b><color=#809fff>{instanceIndex}. {irm.mesh.name}".PadRight(200).Substring(0, 35) + "</color></b>"
                    
                    + $"({lod00Count}, {lod01Count}, {lod02Count})"
                        .PadRight(200).Substring(0, 25)
                    
                    + $"({lod00ShadowCount},{lod01ShadowCount}, {lod02ShadowCount})"
                        .PadRight(200).Substring(0, 25)
                    
                    + $"({lod00VertColor}{lod00Vertices,5}</color>, {lod01VertColor}{lod01Vertices,5}</color>, {lod02VertColor}{lod02Vertices,5})</color>"
                        .PadRight(200).Substring(0, 100)
                    
                    + $"({lod00IndicesColor}{lod00Indices,5}</color>, {lod01IndicesColor}{lod01Indices,5}</color>, {lod02IndicesColor}{lod02Indices,5})</color>"
                        .PadRight(5)
                );
                
                // Total
                uint sumCount = lod00Count + lod01Count + lod02Count;
                uint sumShadowCount = lod00ShadowCount + lod01ShadowCount + lod02ShadowCount;
                
                uint sumLod00Indices = lod00Count * lod00Indices;
                uint sumLod01Indices = lod01Count * lod01Indices;
                uint sumLod02Indices = lod02Count * lod02Indices;
                uint sumIndices = sumLod00Indices + sumLod01Indices + sumLod02Indices;
                
                uint sumShadowLod00Indices = lod00ShadowCount * shadowLod00Indices;
                uint sumShadowLod01Indices = lod01ShadowCount * shadowLod01Indices;
                uint sumShadowLod02Indices = lod02ShadowCount * shadowLod02Indices;
                uint sumShadowIndices = sumShadowLod00Indices + sumShadowLod01Indices + sumShadowLod02Indices;
                
                uint sumLod00Vertices = lod00Count * lod00Vertices;
                uint sumLod01Vertices = lod01Count * lod01Vertices;
                uint sumLod02Vertices = lod02Count * lod02Vertices;
                uint sumVertices = sumLod00Vertices + sumLod01Vertices + sumLod02Vertices;
                
                uint sumShadowLod00Vertices = lod00ShadowCount * shadowLod00Vertices;
                uint sumShadowLod01Vertices = lod01ShadowCount * shadowLod01Vertices;
                uint sumShadowLod02Vertices = lod02ShadowCount * shadowLod02Vertices;
                uint sumShadowVertices = sumShadowLod00Vertices + sumShadowLod01Vertices + sumShadowLod02Vertices;
                
                totalCount += sumCount;
                totalLod00Count += lod00Count;
                totalLod01Count += lod01Count;
                totalLod02Count += lod02Count;
                
                totalShadowCount += sumShadowCount;
                totalShadowLod00Count += lod00ShadowCount;
                totalShadowLod01Count += lod01ShadowCount;
                totalShadowLod02Count += lod02ShadowCount;
                
                totalIndices += sumIndices;
                totalLod00Indices += sumLod00Indices;
                totalLod01Indices += sumLod01Indices;
                totalLod02Indices += sumLod02Indices;
                
                totalShadowIndices += sumShadowIndices;
                totalShadowLod00Indices += sumShadowLod00Indices;
                totalShadowLod01Indices += sumShadowLod01Indices;
                totalShadowLod02Indices += sumShadowLod02Indices;
                
                totalVertices += sumVertices;
                totalLod00Vertices += sumLod00Vertices;
                totalLod01Vertices += sumLod01Vertices;
                totalLod02Vertices += sumLod02Vertices;
                
                totalShadowVertices += sumShadowVertices;
                totalShadowLod00Vertices += sumShadowLod00Vertices;
                totalShadowLod01Vertices += sumShadowLod01Vertices;
                totalShadowLod02Vertices += sumShadowLod02Vertices;
                
                
                instanceIndex++;
            }
            
            m_debugUIText.AppendLine();
            m_debugUIText.AppendLine("<b>Total</b>");
            m_debugUIText.AppendLine(
                string.Format(
                    "Instances:".PadRight(10).Substring(0,10) + " {0, 8} ({1, 8}, {2, 8}, {3, 8})",
                    totalCount,
                    totalLod00Count,
                    totalLod01Count,
                    totalLod02Count,
                    totalShadowCount
                )
            );
            m_debugUIText.AppendLine(
                string.Format(
                    "Vertices:".PadRight(10).Substring(0,10) + " {0, 8} ({1, 8}, {2, 8}, {3, 8})",
                    totalVertices,
                    totalLod00Vertices,
                    totalLod01Vertices,
                    totalLod02Vertices
                )
            );
            m_debugUIText.AppendLine(
                string.Format(
                    "Indices:".PadRight(10).Substring(0,10) + " {0, 8} ({1, 8}, {2, 8}, {3, 8})",
                    totalIndices,
                    totalLod00Indices,
                    totalLod01Indices,
                    totalLod02Indices
                )
            );
            
            m_debugUIText.AppendLine();
            m_debugUIText.AppendLine("<b>Shadow</b>");
            m_debugUIText.AppendLine(
                string.Format(
                    "Instances:".PadRight(10).Substring(0,10) + " {0, 8} ({1, 8}, {2, 8}, {3, 8}) * " + cascades + " Cascades"
                    + " ==> {4, 8} ({5, 8}, {6, 8}, {7, 8})",
                    totalShadowCount,
                    totalShadowLod00Count,
                    totalShadowLod01Count,
                    totalShadowLod02Count,
                    totalShadowCount * cascades,
                    totalShadowLod00Count * cascades,
                    totalShadowLod01Count * cascades,
                    totalShadowLod02Count * cascades
                )
            );
            
            m_debugUIText.AppendLine(
                string.Format(
                    "Vertices:".PadRight(10).Substring(0,10) + " {0, 8} ({1, 8}, {2, 8}, {3, 8}) * " + cascades + " Cascades"
                    + " ==> {4, 8} ({5, 8}, {6, 8}, {7, 8})",
                    totalShadowVertices,
                    totalShadowLod00Vertices,
                    totalShadowLod01Vertices,
                    totalShadowLod02Vertices,
                    totalShadowVertices * cascades,
                    totalShadowLod00Vertices * cascades,
                    totalShadowLod01Vertices * cascades,
                    totalShadowLod02Vertices * cascades
                )
            );
            m_debugUIText.AppendLine(
                string.Format(
                    "Indices:".PadRight(10).Substring(0,10) + " {0, 8} ({1, 8}, {2, 8}, {3, 8}) * " + cascades + " Cascades"
                    + " ==> {4, 8} ({5, 8}, {6, 8}, {7, 8})",
                    totalShadowIndices,
                    totalShadowLod00Indices,
                    totalShadowLod01Indices,
                    totalShadowLod02Indices,
                    totalShadowIndices * cascades,
                    totalShadowLod00Indices * cascades,
                    totalShadowLod01Indices * cascades,
                    totalShadowLod02Indices * cascades
                )
            );
            
            m_uiText.text = m_debugUIText.ToString();
            m_debugGPUArgsRequest = AsyncGPUReadback.Request(m_instancesArgsBuffer);
            m_debugGPUShadowArgsRequest = AsyncGPUReadback.Request(m_shadowArgsBuffer);
        }
    }
    
    
    private void LogSortingData(string prefix = "")
    {
        SortingData[] sortingData = new SortingData[m_numberOfInstances];
        m_instancesSortingData.GetData(sortingData);
        
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(prefix)) { sb.AppendLine(prefix); }
        
        uint lastDrawCallIndex = 0;
        for (int i = 0; i < sortingData.Length; i++)
        {
            uint drawCallIndex = (sortingData[i].drawCallInstanceIndex >> 16);
            uint instanceIndex = (sortingData[i].drawCallInstanceIndex) & 0xFFFF;
            if (i == 0) { lastDrawCallIndex = drawCallIndex; }
            sb.AppendLine("(" + drawCallIndex + ") --> " + sortingData[i].distanceToCam + " instanceIndex:" + instanceIndex);
            
            if (lastDrawCallIndex != drawCallIndex)
            {
                Debug.Log(sb.ToString());
                sb = new StringBuilder();
                lastDrawCallIndex = drawCallIndex;
            }
        }

        Debug.Log(sb.ToString());
    }

    private void LogInstanceAnimation(string prefix = "") {
        AnimRenderData[] infos = new AnimRenderData[m_numberOfInstances];
        m_instancesDrawAnimData.GetData(infos);
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(prefix)) { sb.AppendLine(prefix); }

        for (int i = 0; i < Mathf.Min(infos.Length,5); i++) {
            sb.Append("" + infos[i].AnimInfo0.z + ",");
        }

        sb.AppendLine();
        for (int i = 0; i < infos.Length; i++)
        {
            sb.AppendLine(
                i + "\n"  + infos[i].ToString() + "\n"
            );
        }

        Debug.Log(sb.ToString());
    }

    private string _lastStr;
    private void LogAllBuffers(int count = 5) {
        count = Mathf.Min(m_numberOfInstances, count);
        StringBuilder sb = new StringBuilder();
        {
            //sb.AppendLine("IndexCountPerInstance InstanceCount StartIndexLocation BaseVertexLocation StartInstanceLocation");
            uint[] instancesArgs = new uint[m_numberOfInstanceTypes * NUMBER_OF_ARGS_PER_INSTANCE_TYPE];
            m_instancesArgsBuffer.GetData(instancesArgs);
            for (int i = 0; i < instancesArgs.Length; i++)
            {
                sb.Append(instancesArgs[i] + " ");
                if ((i + 1) % 5 == 0)
                {
                    sb.AppendLine("");
                }
            }
        }
        sb.AppendLine("======== positions ===========");
        {
            var datas = _rendererData.positions;
            for (int i = 0; i < count; i++)
            {
                sb.Append(i+": "+datas[i] +  " instId: " + _rendererData.sortingData[i].drawCallInstanceIndex+ "\n");
            }
            sb.AppendLine();
        }
        
        sb.AppendLine("======== VisibleBuffer ===========");
        {
            uint[] indexRemap = new uint[m_numberOfInstances ];
            m_instancesIsVisibleBuffer.GetData(indexRemap);
            for (int i = 0; i < count; i++)
            {
                sb.Append(indexRemap[i] + " ");
            }
            sb.AppendLine();
        }
        
        sb.AppendLine("======== SumGroup ===========");
        {
            sb.Append("visibleSum: ");
            uint[] visibleAry = new uint[m_numberOfInstances];
            m_instancesScannedPredicates.GetData(visibleAry);
            for (int i = 0; i < count; i++)
            {
                sb.Append(visibleAry[i] + " ");
            }
            sb.AppendLine();
            sb.Append("chunkSum: ");
            uint[] chunkSum = new uint[m_numberOfInstances];
            m_instancesGroupSumArrayBuffer.GetData(chunkSum);
            for (int i = 0; i < count; i++)
            {
                sb.Append(chunkSum[i] + " ");
            }
            sb.AppendLine();
            sb.Append("groupSum: ");
            uint[] groupSum = new uint[m_numberOfInstances];
            m_instancesScannedGroupSumBuffer.GetData(groupSum);
            for (int i = 0; i < count; i++)
            {
                sb.Append(groupSum[i] + " ");
            }
            sb.AppendLine();
            sb.AppendLine("----shadow below----");
            uint[] shadowsScannedData = new uint[m_numberOfInstances];
            m_shadowScannedInstancePredicates.GetData(shadowsScannedData);
            for (int i = 0; i < count; i++)
            {
                sb.Append(shadowsScannedData[i] + " ");
            }
            sb.AppendLine();
        }
        sb.AppendLine("======== SortedInstanceId ===========");
        {
            SortingData[] sortingData = new SortingData[m_numberOfInstances];
            m_instancesSortingData.GetData(sortingData);
            for (int i = 0; i < count; i++) {
                uint instanceIndex = (sortingData[i].drawCallInstanceIndex) & 0xFFFF;
                sb.Append(instanceIndex + " ");
            }
            sb.AppendLine();
        }
        
        sb.AppendLine("======== IndexRemap ===========");
        {
            uint[] indexRemap = new uint[m_numberOfInstances];
            m_instancesCulledIndexRemap.GetData(indexRemap);
            for (int i = 0; i < count; i++)
            {
                sb.Append(indexRemap[i] + " ");
            }
            sb.AppendLine();
        }
        sb.AppendLine("======== SortData ===========");
        {
            SortingData[] sortingData = new SortingData[m_numberOfInstances];
            m_instancesSortingData.GetData(sortingData);
        
            uint lastDrawCallIndex = 0;
            for (int i = 0; i < count; i++)
            {
                sb.AppendLine(sortingData[i].ToString());
            
            }
        }
        sb.AppendLine("======== CulledMatrix ===========");
        {
            Indirect2x2Matrix[] matrix1 = new Indirect2x2Matrix[m_numberOfInstances];
            m_instancesCulledMatrixRows01.GetData(matrix1);
            
            for (int i = 0; i < count; i++)
            {
                sb.AppendLine(
                    i + "\n" 
                      + matrix1[i].row0 + "\n"
                      + matrix1[i].row1 + "\n"
                      + matrix1[i].row2 + "\n"
                      + "\n"
                );
            }
        }        
        sb.AppendLine("======== RawMatrix ===========");
        string prefix = "";
        {
            Indirect2x2Matrix[] matrix1 = new Indirect2x2Matrix[m_numberOfInstances];
            m_instancesMatrixRows01.GetData(matrix1);
        
            for (int i = 0; i < count; i++)
            {
                sb.AppendLine(
                    i + "\n" 
                      + matrix1[i].row0 + "\n"
                      + matrix1[i].row1 + "\n"
                      + matrix1[i].row2 + "\n"
                      + "\n"
                );
            }
        }
        var str = sb.ToString();
        if (str != _lastStr) {
            _lastStr = str;
            Debug.Log(_lastStr);
        }
    }
    private void LogInstanceDrawMatrices(string prefix = "")
    {
        Indirect2x2Matrix[] matrix1 = new Indirect2x2Matrix[m_numberOfInstances];
        m_instancesMatrixRows01.GetData(matrix1);
        
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(prefix)) { sb.AppendLine(prefix); }
        
        for (int i = 0; i < matrix1.Length; i++)
        {
            sb.AppendLine(
                i + "\n" 
                + matrix1[i].row0 + "\n"
                + matrix1[i].row1 + "\n"
                + matrix1[i].row2 + "\n"
                + "\n"
            );
        }

        Debug.Log(sb.ToString());
    }



    private void LogArgsBuffers(string instancePrefix = "", string shadowPrefix = "")
    {
        uint[] instancesArgs = new uint[m_numberOfInstanceTypes * NUMBER_OF_ARGS_PER_INSTANCE_TYPE];
        uint[] shadowArgs = new uint[m_numberOfInstanceTypes * NUMBER_OF_ARGS_PER_INSTANCE_TYPE];
        m_instancesArgsBuffer.GetData(instancesArgs);
        m_shadowArgsBuffer.GetData(shadowArgs);
        
        StringBuilder instancesSB = new StringBuilder();
        StringBuilder shadowsSB = new StringBuilder();
        
        if (!string.IsNullOrEmpty(instancePrefix)){ instancesSB.AppendLine(instancePrefix); }
        if (!string.IsNullOrEmpty(shadowPrefix))  { shadowsSB.AppendLine(shadowPrefix); }
        
        instancesSB.AppendLine("");
        shadowsSB.AppendLine("");
        
        instancesSB.AppendLine("IndexCountPerInstance InstanceCount StartIndexLocation BaseVertexLocation StartInstanceLocation");
        shadowsSB.AppendLine("IndexCountPerInstance InstanceCount StartIndexLocation BaseVertexLocation StartInstanceLocation");

        int counter = 0;
        instancesSB.AppendLine(indirectMeshes[counter].mesh.name);
        shadowsSB.AppendLine(indirectMeshes[counter].mesh.name);
        for (int i = 0; i < instancesArgs.Length; i++)
        {
            instancesSB.Append(instancesArgs[i] + " ");
            shadowsSB.Append(shadowArgs[i] + " ");

            if ((i + 1) % 5 == 0)
            {
                instancesSB.AppendLine("");
                shadowsSB.AppendLine("");

                if ((i + 1) < instancesArgs.Length
                    && (i + 1) % NUMBER_OF_ARGS_PER_INSTANCE_TYPE == 0)
                {
                    instancesSB.AppendLine("");
                    shadowsSB.AppendLine("");

                    counter++;
                    IndirectRenderingMesh irm = indirectMeshes[counter];
                    Mesh m = irm.mesh;
                    instancesSB.AppendLine(m.name);
                    shadowsSB.AppendLine(m.name);
                }
            }
        }
        
        Debug.Log(instancesSB.ToString());
        Debug.Log(shadowsSB.ToString());
    }
    
    private void LogInstancesIsVisibleBuffers(string instancePrefix = "", string shadowPrefix = "")
    {
        uint[] instancesIsVisible = new uint[m_numberOfInstances];
        uint[] shadowsIsVisible = new uint[m_numberOfInstances];
        m_instancesIsVisibleBuffer.GetData(instancesIsVisible);
        m_shadowsIsVisibleBuffer.GetData(shadowsIsVisible);
        
        StringBuilder instancesSB = new StringBuilder();
        StringBuilder shadowsSB = new StringBuilder();
        
        if (!string.IsNullOrEmpty(instancePrefix)){ instancesSB.AppendLine(instancePrefix); }
        if (!string.IsNullOrEmpty(shadowPrefix))  { shadowsSB.AppendLine(shadowPrefix); }
        
        for (int i = 0; i < instancesIsVisible.Length; i++)
        {
            instancesSB.AppendLine(i + ": " + instancesIsVisible[i]);
            shadowsSB.AppendLine(i + ": " + shadowsIsVisible[i]);
        }
        
        Debug.Log(instancesSB.ToString());
        Debug.Log(shadowsSB.ToString());
    }
    
    private void LogScannedPredicates(string instancePrefix = "", string shadowPrefix = "")
    {
        uint[] instancesScannedData = new uint[m_numberOfInstances];
        uint[] shadowsScannedData = new uint[m_numberOfInstances];
        m_instancesScannedPredicates.GetData(instancesScannedData);
        m_shadowScannedInstancePredicates.GetData(shadowsScannedData);
        
        StringBuilder instancesSB = new StringBuilder();
        StringBuilder shadowsSB = new StringBuilder();
        
        if (!string.IsNullOrEmpty(instancePrefix)){ instancesSB.AppendLine(instancePrefix); }
        if (!string.IsNullOrEmpty(shadowPrefix))  { shadowsSB.AppendLine(shadowPrefix); }
        
        for (int i = 0; i < instancesScannedData.Length; i++)
        {
            instancesSB.AppendLine(i + ": " + instancesScannedData[i]);
            shadowsSB.AppendLine(i + ": " + shadowsScannedData[i]);
        }

        Debug.Log(instancesSB.ToString());
        Debug.Log(shadowsSB.ToString());
    }
    
    private void LogGroupSumArrayBuffer(string instancePrefix = "", string shadowPrefix = "")
    {
        uint[] instancesScannedData = new uint[m_numberOfInstances];
        uint[] shadowsScannedData = new uint[m_numberOfInstances];
        m_instancesGroupSumArrayBuffer.GetData(instancesScannedData);
        m_shadowsScannedGroupSumBuffer.GetData(shadowsScannedData);
        
        StringBuilder instancesSB = new StringBuilder();
        StringBuilder shadowsSB = new StringBuilder();
        
        if (!string.IsNullOrEmpty(instancePrefix)){ instancesSB.AppendLine(instancePrefix); }
        if (!string.IsNullOrEmpty(shadowPrefix))  { shadowsSB.AppendLine(shadowPrefix); }
        
        for (int i = 0; i < instancesScannedData.Length; i++)
        {
            instancesSB.AppendLine(i + ": " + instancesScannedData[i]);
            shadowsSB.AppendLine(i + ": " + shadowsScannedData[i]);
        }

        Debug.Log(instancesSB.ToString());
        Debug.Log(shadowsSB.ToString());
    }
    
    private void LogScannedGroupSumBuffer(string instancePrefix = "", string shadowPrefix = "")
    {
        uint[] instancesScannedData = new uint[m_numberOfInstances];
        uint[] shadowsScannedData = new uint[m_numberOfInstances];
        m_instancesScannedPredicates.GetData(instancesScannedData);
        m_shadowScannedInstancePredicates.GetData(shadowsScannedData);
        
        StringBuilder instancesSB = new StringBuilder();
        StringBuilder shadowsSB = new StringBuilder();
        
        if (!string.IsNullOrEmpty(instancePrefix)){ instancesSB.AppendLine(instancePrefix); }
        if (!string.IsNullOrEmpty(shadowPrefix))  { shadowsSB.AppendLine(shadowPrefix); }
        
        for (int i = 0; i < instancesScannedData.Length; i++)
        {
            instancesSB.AppendLine(i + ": " + instancesScannedData[i]);
            shadowsSB.AppendLine(i + ": " + shadowsScannedData[i]);
        }

        Debug.Log(instancesSB.ToString());
        Debug.Log(shadowsSB.ToString());
    }
    private void LogCulledInstancesAnimData(string instancePrefix = "", string shadowPrefix = "")
    {
        Indirect2x2Matrix[] instancesMatrix1 = new Indirect2x2Matrix[m_numberOfInstances];
        m_instancesCulledMatrixRows01.GetData(instancesMatrix1);
        
        Indirect2x2Matrix[] shadowsMatrix1 = new Indirect2x2Matrix[m_numberOfInstances];
        m_shadowCulledMatrixRows01.GetData(shadowsMatrix1);
        
        StringBuilder instancesSB = new StringBuilder();
        StringBuilder shadowsSB = new StringBuilder();
        
        if (!string.IsNullOrEmpty(instancePrefix)){ instancesSB.AppendLine(instancePrefix); }
        if (!string.IsNullOrEmpty(shadowPrefix))  { shadowsSB.AppendLine(shadowPrefix); }
        
        for (int i = 0; i < instancesMatrix1.Length; i++)
        {
            instancesSB.AppendLine(
                i + "\n" 
                + instancesMatrix1[i].row0 + "\n"
                + instancesMatrix1[i].row1 + "\n"
                + instancesMatrix1[i].row2 + "\n"
                + "\n"
            );
            
            shadowsSB.AppendLine(
                i + "\n" 
                + shadowsMatrix1[i].row0 + "\n"
                + shadowsMatrix1[i].row1 + "\n"
                + shadowsMatrix1[i].row2 + "\n"
                + "\n"
            );
        }

        Debug.Log(instancesSB.ToString());
        Debug.Log(shadowsSB.ToString());
    }
        
    private void LogCulledInstancesDrawMatrices(string instancePrefix = "", string shadowPrefix = "")
    {
        Indirect2x2Matrix[] instancesMatrix1 = new Indirect2x2Matrix[m_numberOfInstances];
        m_instancesCulledMatrixRows01.GetData(instancesMatrix1);
        
        Indirect2x2Matrix[] shadowsMatrix1 = new Indirect2x2Matrix[m_numberOfInstances];
        m_shadowCulledMatrixRows01.GetData(shadowsMatrix1);
        
        StringBuilder instancesSB = new StringBuilder();
        StringBuilder shadowsSB = new StringBuilder();
        
        if (!string.IsNullOrEmpty(instancePrefix)){ instancesSB.AppendLine(instancePrefix); }
        if (!string.IsNullOrEmpty(shadowPrefix))  { shadowsSB.AppendLine(shadowPrefix); }
        
        for (int i = 0; i < instancesMatrix1.Length; i++)
        {
            instancesSB.AppendLine(
                i + "\n" 
                + instancesMatrix1[i].row0 + "\n"
                + instancesMatrix1[i].row1 + "\n"
                + instancesMatrix1[i].row2 + "\n"
                + "\n"
            );
            
            shadowsSB.AppendLine(
                i + "\n" 
                + shadowsMatrix1[i].row0 + "\n"
                + shadowsMatrix1[i].row1 + "\n"
                + shadowsMatrix1[i].row2 + "\n"
                + "\n"
            );
        }

        Debug.Log(instancesSB.ToString());
        Debug.Log(shadowsSB.ToString());
    }
    
    #endregion
}