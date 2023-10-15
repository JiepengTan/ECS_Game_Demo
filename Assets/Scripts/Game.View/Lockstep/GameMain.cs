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

        [UnityEngine.Range(0, 40)] public int DebugAreaCount = 8;
        [UnityEngine.Range(1, 30000)] public int DebugEntityCount = 1;
        [UnityEngine.Range(1, 1000)] public int DebugEntityBornPosRange = 100;

        private int _lastDebugAreaCount = 0;
        private int _lastDebugEntityCount = 0;

        public GameGlobalStateService GlobalStateService;
        protected override void DoAwake() {
#if UNITY_STANDALONE
            Application.targetFrameRate = 300;
#endif
            Region.IsDebugMode = IsRegionDebugMode;
        }
        protected override ServiceContainer CreateServiceContainer() {
            return new UnityGameServiceContainer();
        }

        protected override object CreateWorld(IServiceContainer services) {
            return new GameEcsWorld(services);
        }
        protected override void DoUpdate() {
            _totalNativeMemoryUsedKB = UnsafeUtility.TotalAllocSize / 1024;
            Region.IsDebugMode = IsRegionDebugMode;
            GlobalStateService = _serviceContainer.GetService<IGlobalStateService>() as GameGlobalStateService;
        }

        protected override void DoDestroy() {
            NativeUtil.FreeAll();
        }
    }
}