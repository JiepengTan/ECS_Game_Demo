using System;
using System.Collections.Generic;
using GamesTan.ECS.Game;
using Lockstep.InternalUnsafeECS;
using Lockstep.UnsafeECS;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace GamesTan.Game.View {
    public unsafe class GameMain : MonoBehaviour {
        public bool IsCopyServiceFromThis;
        public float UpdateInterval = 0.03f;
        private float _updateTimer;

        private GameEcsWorld _curWorld;
        [SerializeField] private GameServices _curService;

        [SerializeField] private long _totalNativeMemoryUsedKB;

        public void Awake() {
#if UNITY_STANDALONE
            Application.targetFrameRate = 300;
#endif
            _updateTimer = 0;
            _curWorld = new GameEcsWorld();
            // just for debug
            if (IsCopyServiceFromThis) {
                _curWorld._services = _curService;
            }
            else {
                _curWorld.Services.ViewRoot = transform;
                _curService = _curWorld.Services;
            }

            _curWorld.DoAwake();
        }

        private void Update() {
            float dt = Time.deltaTime;
            _updateTimer += dt;
            while (_updateTimer > UpdateInterval) {
                _updateTimer -= UpdateInterval;
                Profiler.BeginSample("Update Frame=" + _curService.Frame);
                _curWorld.Update(UpdateInterval);
                Profiler.EndSample();
            }

            _totalNativeMemoryUsedKB = UnsafeUtility.TotalAllocSize / 1024;
        }

        private void OnDestroy() {
            _curWorld.DoDestroy();
            NativeUtil.FreeAll();
        }
    }
}