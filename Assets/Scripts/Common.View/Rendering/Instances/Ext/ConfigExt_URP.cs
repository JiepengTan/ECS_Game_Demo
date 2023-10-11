using UnityEngine;

namespace GamesTan.Rendering {
    public partial class IndirectDrawRenderPass {
        bool runCompute => Config.runCompute;
        bool drawInstances => Config.drawInstances;
        bool drawInstanceShadows => Config.drawInstanceShadows;
        bool enableFrustumCulling => Config.enableFrustumCulling;
        bool enableOcclusionCulling => Config.enableOcclusionCulling;
        bool enableDetailCulling => Config.enableDetailCulling;
        bool enableLOD => Config.enableLOD;
        bool enableOnlyLOD02Shadows => Config.enableOnlyLOD02Shadows;
        float detailCullingPercentage => Config.detailCullingPercentage;
        float shadowDistance => Config.shadowDistance;
        float lod0Distance => Config.lod0Distance;
        float lod1Distance => Config.lod1Distance;

        bool debugShowUI => Config.debugShowUI;
        bool debugDrawLOD => Config.debugDrawLOD;
        bool debugDrawBoundsInSceneView => Config.debugDrawBoundsInSceneView;
        bool debugDrawHiZ => Config.debugDrawHiZ;
        int debugHiZLOD => Config.debugHiZLOD;
        GameObject debugUIPrefab => Config.debugUIPrefab;

        bool logInstanceAnimation {
            get => Config.logInstanceAnimation;
            set => Config.logInstanceAnimation = value;
        }

        bool logInstanceDrawMatrices {
            get => Config.logInstanceDrawMatrices;
            set => Config.logInstanceDrawMatrices = value;
        }

        bool logArgumentsAfterReset {
            get => Config.logArgumentsAfterReset;
            set => Config.logArgumentsAfterReset = value;
        }

        bool logSortingData {
            get => Config.logSortingData;
            set => Config.logSortingData = value;
        }

        bool logArgumentsAfterOcclusion {
            get => Config.logArgumentsAfterOcclusion;
            set => Config.logArgumentsAfterOcclusion = value;
        }

        bool logInstancesIsVisibleBuffer {
            get => Config.logInstancesIsVisibleBuffer;
            set => Config.logInstancesIsVisibleBuffer = value;
        }

        bool logArgsBufferAfterCopy {
            get => Config.logArgsBufferAfterCopy;
            set => Config.logArgsBufferAfterCopy = value;
        }

        bool logCulledInstancesDrawMatrices {
            get => Config.logCulledInstancesDrawMatrices;
            set => Config.logCulledInstancesDrawMatrices = value;
        }

        bool logCulledInstancesAnimData {
            get => Config.logCulledInstancesAnimData;
            set => Config.logCulledInstancesAnimData = value;
        }

        bool logDebugAll {
            get => Config.logDebugAll;
            set => Config.logDebugAll = value;
        }

        bool logAllArgBuffer {
            get => Config.logAllArgBuffer;
            set => Config.logAllArgBuffer = value;
        }

        int logAllArgBufferCount {
            get => Config.logAllArgBufferCount;
            set => Config.logAllArgBufferCount = value;
        }


        ComputeShader createDrawDataBufferCS => Config.createDrawDataBufferCS;
        ComputeShader sortingCS => Config.sortingCS;
        ComputeShader occlusionCS => Config.occlusionCS;

        ComputeShader copyInstanceDataCS => Config.copyInstanceDataCS;

//
        int NUMBER_OF_DRAW_CALLS => IndirectRendererConfig.NUMBER_OF_DRAW_CALLS;
        int NUMBER_OF_ARGS_PER_DRAW => IndirectRendererConfig.NUMBER_OF_ARGS_PER_DRAW;
        int NUMBER_OF_ARGS_PER_INSTANCE_TYPE => IndirectRendererConfig.NUMBER_OF_ARGS_PER_INSTANCE_TYPE;
        int ARGS_BYTE_SIZE_PER_DRAW_CALL => IndirectRendererConfig.ARGS_BYTE_SIZE_PER_DRAW_CALL;
        int ARGS_BYTE_SIZE_PER_INSTANCE_TYPE => IndirectRendererConfig.ARGS_BYTE_SIZE_PER_INSTANCE_TYPE;
        int SCAN_THREAD_GROUP_SIZE => IndirectRendererConfig.SCAN_THREAD_GROUP_SIZE;
        string DEBUG_UI_RED_COLOR => IndirectRendererConfig.DEBUG_UI_RED_COLOR;
        string DEBUG_UI_WHITE_COLOR => IndirectRendererConfig.DEBUG_UI_WHITE_COLOR;

        string DEBUG_SHADER_LOD_KEYWORD => IndirectRendererConfig.DEBUG_SHADER_LOD_KEYWORD;

//
        int _Data => IndirectRendererConfig._Data;
        int _Input => IndirectRendererConfig._Input;
        int _ShouldFrustumCull => IndirectRendererConfig._ShouldFrustumCull;
        int _ShouldOcclusionCull => IndirectRendererConfig._ShouldOcclusionCull;
        int _ShouldLOD => IndirectRendererConfig._ShouldLOD;
        int _ShouldDetailCull => IndirectRendererConfig._ShouldDetailCull;
        int _ShouldOnlyUseLOD02Shadows => IndirectRendererConfig._ShouldOnlyUseLOD02Shadows;
        int _UNITY_MATRIX_MVP => IndirectRendererConfig._UNITY_MATRIX_MVP;
        int _CamPosition => IndirectRendererConfig._CamPosition;
        int _HiZTextureSize => IndirectRendererConfig._HiZTextureSize;
        int _Level => IndirectRendererConfig._Level;
        int _LevelMask => IndirectRendererConfig._LevelMask;
        int _Width => IndirectRendererConfig._Width;
        int _Height => IndirectRendererConfig._Height;
        int _ShadowDistance => IndirectRendererConfig._ShadowDistance;
        int _DetailCullingScreenPercentage => IndirectRendererConfig._DetailCullingScreenPercentage;
        int _Lod0Distance => IndirectRendererConfig._Lod0Distance;
        int _Lod1Distance => IndirectRendererConfig._Lod1Distance;

        int _HiZMap => IndirectRendererConfig._HiZMap;
        int _NumOfDrawcalls => IndirectRendererConfig._NumOfDrawcalls;
        int _ArgsOffset => IndirectRendererConfig._ArgsOffset;
        int _TransformData => IndirectRendererConfig._TransformData;
        int _ArgsBuffer => IndirectRendererConfig._ArgsBuffer;
        int _ShadowArgsBuffer => IndirectRendererConfig._ShadowArgsBuffer;
        int _IsVisibleBuffer => IndirectRendererConfig._IsVisibleBuffer;
        int _ShadowIsVisibleBuffer => IndirectRendererConfig._ShadowIsVisibleBuffer;
        int _DrawcallDataOut => IndirectRendererConfig._DrawcallDataOut;
        int _SortingData => IndirectRendererConfig._SortingData;
        int _ShadowSortingData => IndirectRendererConfig._ShadowSortingData;
        int _InstanceDataBuffer => IndirectRendererConfig._InstanceDataBuffer;
        int _InstancePredicatesIn => IndirectRendererConfig._InstancePredicatesIn;
        int _InstancesDrawMatrixRows => IndirectRendererConfig._InstancesDrawMatrixRows;

        int _InstancesCulledMatrixRows01 => IndirectRendererConfig._InstancesCulledMatrixRows01;

//
        int _InstancesCulledAnimData => IndirectRendererConfig._InstancesCulledAnimData;
        int _InstancesDrawAnimData => IndirectRendererConfig._InstancesDrawAnimData;
        int _InstancesCulledIndexRemap => IndirectRendererConfig._InstancesCulledIndexRemap;
    }
}