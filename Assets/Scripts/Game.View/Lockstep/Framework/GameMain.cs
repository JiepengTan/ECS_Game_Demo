using System;
using GamesTan.ECS.Game;
using GamesTan.Spatial;
using GamesTan.ECS;
using Lockstep.Game;
using UnityEngine;

namespace GamesTan.Game.View {
    public unsafe class GameMain : BaseMainScript {
        [Header("GameData")] 
        public bool IsRegionDebugMode;

        [Header("DebugData")] [SerializeField] private long _totalNativeMemoryUsedKB;

        private float _updateTimer;
        private int _lastDebugAreaCount = 0;
        private int _lastDebugEntityCount = 0;

        private GameGlobalStateService _stateService;
        [UnityEngine.Range(0, 40)] public int DebugAreaCount = 8;
        [UnityEngine.Range(1, 3000)] public int DebugEntityCount = 100;
        [UnityEngine.Range(1, 400)] public int DebugEntityBornPosRange = 100;
        protected override void DoAwake() {
#if UNITY_STANDALONE
            Application.targetFrameRate = 300;
#endif
            InitDebugInfo();
        }


        protected override ServiceContainer CreateServiceContainer() {
            return new UnityGameServiceContainer();
        }

        protected override object CreateWorld(IServiceContainer services) {
            return new GameEcsWorld(services);
        }
        protected override void DoUpdate() {
            _totalNativeMemoryUsedKB = UnsafeUtility.TotalAllocSize / 1024;
            UpdateDebugInfo();
        }

        protected override void DoDestroy() {
            NativeUtil.FreeAll();
        }

        private void InitDebugInfo() {
            Region.IsDebugMode = IsRegionDebugMode;
            _stateService = null;
        }
        private void UpdateDebugInfo() {
            Region.IsDebugMode = IsRegionDebugMode;
            if (_stateService == null) {
                _stateService = _serviceContainer.GetService<IGlobalStateService>() as GameGlobalStateService;
            }

            if (_stateService != null) {
                _stateService.DebugAreaCount = DebugAreaCount;
                _stateService.DebugEntityCount = DebugEntityCount;
                _stateService.DebugEntityBornPosRange = DebugEntityBornPosRange;
            }
        }
    }
}