using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamesTan.Rendering {
    [CreateAssetMenu(menuName = "Assets/IndirectRendererConfig")]
    public partial class IndirectRendererConfig: ScriptableObject {
        
        public InstanceConfig InstanceConfig;
        public List<int> InitInstanceCount = new List<int>();
        
        
        [Header("References")]
        public ComputeShader createDrawDataBufferCS;
        public ComputeShader sortingCS;
        public ComputeShader occlusionCS;
        public ComputeShader copyInstanceDataCS;
        
        [Header("Settings")]
        public bool runCompute = true;
        public bool drawInstances = true;
        public bool drawInstanceShadows = true;
        public bool enableFrustumCulling = true;
        public bool enableOcclusionCulling = true;
        public bool enableDetailCulling = true;
        public bool enableLOD = true;
        public bool enableOnlyLOD02Shadows = false;
        [Range(00.00f, 00.2f)] public float detailCullingPercentage = 0.015f;
        [Range(10, 1000)] public float shadowDistance = 100;
        [Range(10, 300.0f)] public float lod0Distance = 20f;
        [Range(10, 300.0f)] public float lod1Distance = 60f;
        
        // Debugging Variables
        [Header("Debug")]
        public bool debugShowUI;
        public bool debugDrawLOD;
        public bool debugDrawBoundsInSceneView;
        public bool debugDrawHiZ;
        [Range(0, 10)] public int debugHiZLOD;
        public GameObject debugUIPrefab;
        
        [Header("Logging")]
        public bool logInstanceAnimation = false;
        public bool logInstanceDrawMatrices = false;
        public bool logArgumentsAfterReset = false;
        public bool logSortingData = false;
        public bool logArgumentsAfterOcclusion = false;
        public bool logInstancesIsVisibleBuffer = false;
        public bool logArgsBufferAfterCopy = false;
        public bool logCulledInstancesDrawMatrices = false;
        public bool logCulledInstancesAnimData = false;
        public bool logDebugAll = false;
        public bool logAllArgBuffer;
        [Range(3,100)] public int logAllArgBufferCount = 10;
        
        // Constants
        public const int NUMBER_OF_DRAW_CALLS = 3; // (LOD00 + LOD01 + LOD02)
        public const int NUMBER_OF_ARGS_PER_DRAW = 5; // (indexCount, instanceCount, startIndex, baseVertex, startInstance)
        public const int NUMBER_OF_ARGS_PER_INSTANCE_TYPE = NUMBER_OF_DRAW_CALLS * NUMBER_OF_ARGS_PER_DRAW; // 3draws * 5args = 15args
        public const int ARGS_BYTE_SIZE_PER_DRAW_CALL = NUMBER_OF_ARGS_PER_DRAW * sizeof(uint); // 5args * 4bytes = 20 bytes
        public const int ARGS_BYTE_SIZE_PER_INSTANCE_TYPE = NUMBER_OF_ARGS_PER_INSTANCE_TYPE * sizeof(uint); // 15args * 4bytes = 60bytes
        public const int SCAN_THREAD_GROUP_SIZE = 64;
        public const string DEBUG_UI_RED_COLOR =   "<color=#ff6666>";
        public const string DEBUG_UI_WHITE_COLOR = "<color=#ffffff>";
        public const string DEBUG_SHADER_LOD_KEYWORD = "INDIRECT_DEBUG_LOD";
        
        // Shader Property ID's
        public static readonly int _UseHiZ = Shader.PropertyToID("_UseHiZ");
        public static readonly int _Data = Shader.PropertyToID("_Data");
        public static readonly int _Input = Shader.PropertyToID("_Input");
        public static readonly int _ShouldFrustumCull = Shader.PropertyToID("_ShouldFrustumCull");
        public static readonly int _ShouldOcclusionCull = Shader.PropertyToID("_ShouldOcclusionCull");
        public static readonly int _ShouldLOD = Shader.PropertyToID("_ShouldLOD");
        public static readonly int _ShouldDetailCull = Shader.PropertyToID("_ShouldDetailCull");
        public static readonly int _ShouldOnlyUseLOD02Shadows = Shader.PropertyToID("_ShouldOnlyUseLOD02Shadows");
        public static readonly int _UNITY_MATRIX_MVP = Shader.PropertyToID("_UNITY_MATRIX_MVP");
        public static readonly int _CamPosition = Shader.PropertyToID("_CamPosition");
        public static readonly int _HiZTextureSize = Shader.PropertyToID("_HiZTextureSize");
        public static readonly int _Level = Shader.PropertyToID("_Level");
        public static readonly int _LevelMask = Shader.PropertyToID("_LevelMask");
        public static readonly int _Width = Shader.PropertyToID("_Width");
        public static readonly int _Height = Shader.PropertyToID("_Height");
        public static readonly int _ShadowDistance = Shader.PropertyToID("_ShadowDistance");
        public static readonly int _DetailCullingScreenPercentage = Shader.PropertyToID("_DetailCullingScreenPercentage");
        public static readonly int _Lod0Distance = Shader.PropertyToID("_Lod0Distance");
        public static readonly int _Lod1Distance = Shader.PropertyToID("_Lod1Distance");
        
        public static readonly int _HiZMap = Shader.PropertyToID("_HiZMap");
        public static readonly int _NumOfDrawcalls = Shader.PropertyToID("_NumOfDrawcalls");
        public static readonly int _ArgsOffset = Shader.PropertyToID("_ArgsOffset");
        public static readonly int _TransformData = Shader.PropertyToID("_TransformData");
        public static readonly int _ArgsBuffer = Shader.PropertyToID("_ArgsBuffer");
        public static readonly int _ShadowArgsBuffer = Shader.PropertyToID("_ShadowArgsBuffer");
        public static readonly int _IsVisibleBuffer = Shader.PropertyToID("_IsVisibleBuffer");
        public static readonly int _ShadowIsVisibleBuffer = Shader.PropertyToID("_ShadowIsVisibleBuffer");
        public static readonly int _DrawcallDataOut = Shader.PropertyToID("_DrawcallDataOut");
        public static readonly int _SortingData = Shader.PropertyToID("_SortingData");
        public static readonly int _ShadowSortingData = Shader.PropertyToID("_ShadowSortingData");
        public static readonly int _InstanceDataBuffer = Shader.PropertyToID("_InstanceDataBuffer");
        public static readonly int _InstancePredicatesIn = Shader.PropertyToID("_InstancePredicatesIn");
        public static readonly int _InstancesDrawMatrixRows = Shader.PropertyToID("_InstancesDrawMatrixRows");
        public static readonly int _InstancesCulledMatrixRows01 = Shader.PropertyToID("_InstancesCulledMatrixRows01");
        
        public static readonly int _InstancesCulledAnimData = Shader.PropertyToID("_InstancesCulledAnimData");
        public static readonly int _InstancesDrawAnimData = Shader.PropertyToID("_InstancesDrawAnimData");
        public static readonly int _InstancesCulledIndexRemap = Shader.PropertyToID("_InstancesCulledIndexRemap");
    }
}