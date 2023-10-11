using UnityEngine;

namespace GamesTan.Rendering {
    public partial class IndirectDrawRenderPass {
        protected bool runCompute => Config.runCompute;
        protected bool drawInstances => Config.drawInstances;
        protected bool drawInstanceShadows => Config.drawInstanceShadows;
        protected bool enableFrustumCulling => Config.enableFrustumCulling;
        protected bool enableOcclusionCulling => Config.enableOcclusionCulling;
        protected bool enableDetailCulling => Config.enableDetailCulling;
        protected bool enableLOD => Config.enableLOD;
        protected bool enableOnlyLOD02Shadows => Config.enableOnlyLOD02Shadows;
        protected float detailCullingPercentage => Config.detailCullingPercentage;
        protected float shadowDistance => Config.shadowDistance;
        protected float lod0Distance => Config.lod0Distance;
        protected float lod1Distance => Config.lod1Distance;

        protected bool debugShowUI => Config.debugShowUI;
        protected bool debugDrawLOD => Config.debugDrawLOD;
        protected bool debugDrawBoundsInSceneView => Config.debugDrawBoundsInSceneView;
        protected bool debugDrawHiZ => Config.debugDrawHiZ;
        protected int debugHiZLOD => Config.debugHiZLOD;
        protected GameObject debugUIPrefab => Config.debugUIPrefab;

        protected bool logInstanceAnimation {
            get => Config.logInstanceAnimation;
            set => Config.logInstanceAnimation = value;
        }

        protected bool logInstanceDrawMatrices {
            get => Config.logInstanceDrawMatrices;
            set => Config.logInstanceDrawMatrices = value;
        }

        protected bool logArgumentsAfterReset {
            get => Config.logArgumentsAfterReset;
            set => Config.logArgumentsAfterReset = value;
        }

        protected bool logSortingData {
            get => Config.logSortingData;
            set => Config.logSortingData = value;
        }

        protected bool logArgumentsAfterOcclusion {
            get => Config.logArgumentsAfterOcclusion;
            set => Config.logArgumentsAfterOcclusion = value;
        }

        protected bool logInstancesIsVisibleBuffer {
            get => Config.logInstancesIsVisibleBuffer;
            set => Config.logInstancesIsVisibleBuffer = value;
        }

        protected bool logArgsBufferAfterCopy {
            get => Config.logArgsBufferAfterCopy;
            set => Config.logArgsBufferAfterCopy = value;
        }

        protected bool logCulledInstancesDrawMatrices {
            get => Config.logCulledInstancesDrawMatrices;
            set => Config.logCulledInstancesDrawMatrices = value;
        }

        protected bool logCulledInstancesAnimData {
            get => Config.logCulledInstancesAnimData;
            set => Config.logCulledInstancesAnimData = value;
        }

        protected bool logDebugAll {
            get => Config.logDebugAll;
            set => Config.logDebugAll = value;
        }

        protected bool logAllArgBuffer {
            get => Config.logAllArgBuffer;
            set => Config.logAllArgBuffer = value;
        }

        protected int logAllArgBufferCount {
            get => Config.logAllArgBufferCount;
            set => Config.logAllArgBufferCount = value;
        }


        protected ComputeShader createDrawDataBufferCS => Config.createDrawDataBufferCS;
        protected ComputeShader sortingCS => Config.sortingCS;
        protected ComputeShader occlusionCS => Config.occlusionCS;

        protected ComputeShader copyInstanceDataCS => Config.copyInstanceDataCS;

//
        protected int NUMBER_OF_DRAW_CALLS => IndirectRendererConfig.NUMBER_OF_DRAW_CALLS;
        protected int NUMBER_OF_ARGS_PER_DRAW => IndirectRendererConfig.NUMBER_OF_ARGS_PER_DRAW;
        protected int NUMBER_OF_ARGS_PER_INSTANCE_TYPE => IndirectRendererConfig.NUMBER_OF_ARGS_PER_INSTANCE_TYPE;
        protected int ARGS_BYTE_SIZE_PER_DRAW_CALL => IndirectRendererConfig.ARGS_BYTE_SIZE_PER_DRAW_CALL;
        protected int ARGS_BYTE_SIZE_PER_INSTANCE_TYPE => IndirectRendererConfig.ARGS_BYTE_SIZE_PER_INSTANCE_TYPE;
        protected int SCAN_THREAD_GROUP_SIZE => IndirectRendererConfig.SCAN_THREAD_GROUP_SIZE;
        protected string DEBUG_UI_RED_COLOR => IndirectRendererConfig.DEBUG_UI_RED_COLOR;
        protected string DEBUG_UI_WHITE_COLOR => IndirectRendererConfig.DEBUG_UI_WHITE_COLOR;

        protected string DEBUG_SHADER_LOD_KEYWORD => IndirectRendererConfig.DEBUG_SHADER_LOD_KEYWORD;

//
        protected int _Data => IndirectRendererConfig._Data;
        protected int _Input => IndirectRendererConfig._Input;
        protected int _ShouldFrustumCull => IndirectRendererConfig._ShouldFrustumCull;
        protected int _ShouldOcclusionCull => IndirectRendererConfig._ShouldOcclusionCull;
        protected int _ShouldLOD => IndirectRendererConfig._ShouldLOD;
        protected int _ShouldDetailCull => IndirectRendererConfig._ShouldDetailCull;
        protected int _ShouldOnlyUseLOD02Shadows => IndirectRendererConfig._ShouldOnlyUseLOD02Shadows;
        protected int _UseHiZ => IndirectRendererConfig._UseHiZ;
        protected int _UNITY_MATRIX_MVP => IndirectRendererConfig._UNITY_MATRIX_MVP;
        protected int _CamPosition => IndirectRendererConfig._CamPosition;
        protected int _HiZTextureSize => IndirectRendererConfig._HiZTextureSize;
        protected int _Level => IndirectRendererConfig._Level;
        protected int _LevelMask => IndirectRendererConfig._LevelMask;
        protected int _Width => IndirectRendererConfig._Width;
        protected int _Height => IndirectRendererConfig._Height;
        protected int _ShadowDistance => IndirectRendererConfig._ShadowDistance;
        protected int _DetailCullingScreenPercentage => IndirectRendererConfig._DetailCullingScreenPercentage;
        protected int _Lod0Distance => IndirectRendererConfig._Lod0Distance;
        protected int _Lod1Distance => IndirectRendererConfig._Lod1Distance;

        protected int _HiZMap => IndirectRendererConfig._HiZMap;
        protected int _NumOfDrawcalls => IndirectRendererConfig._NumOfDrawcalls;
        protected int _ArgsOffset => IndirectRendererConfig._ArgsOffset;
        protected int _TransformData => IndirectRendererConfig._TransformData;
        protected int _ArgsBuffer => IndirectRendererConfig._ArgsBuffer;
        protected int _ShadowArgsBuffer => IndirectRendererConfig._ShadowArgsBuffer;
        protected int _IsVisibleBuffer => IndirectRendererConfig._IsVisibleBuffer;
        protected int _ShadowIsVisibleBuffer => IndirectRendererConfig._ShadowIsVisibleBuffer;
        protected int _DrawcallDataOut => IndirectRendererConfig._DrawcallDataOut;
        protected int _SortingData => IndirectRendererConfig._SortingData;
        protected int _ShadowSortingData => IndirectRendererConfig._ShadowSortingData;
        protected int _InstanceDataBuffer => IndirectRendererConfig._InstanceDataBuffer;
        protected int _InstancePredicatesIn => IndirectRendererConfig._InstancePredicatesIn;
        protected int _InstancesDrawMatrixRows => IndirectRendererConfig._InstancesDrawMatrixRows;

        protected int _InstancesCulledMatrixRows01 => IndirectRendererConfig._InstancesCulledMatrixRows01;

//
        protected int _InstancesCulledAnimData => IndirectRendererConfig._InstancesCulledAnimData;
        protected int _InstancesDrawAnimData => IndirectRendererConfig._InstancesDrawAnimData;
        protected int _InstancesCulledIndexRemap => IndirectRendererConfig._InstancesCulledIndexRemap;
    }
}