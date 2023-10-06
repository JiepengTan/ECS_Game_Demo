using System;
using System.Collections.Generic;
using GamesTan.ECS.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;

namespace GamesTan.Game.View {
    public unsafe class GameMain : MonoBehaviour {
        public bool IsCopyServiceFromThis;
        public float UpdateInterval = 0.03f;
        private float _updateTimer;

        private GameEcsWorld _curWorld;
        [SerializeField] private GameServices _curService;

        public void Awake() {
            _updateTimer = 0;
            _curWorld = new GameEcsWorld();
            // just for debug
            if (IsCopyServiceFromThis) {
                _curWorld._services = _curService;
            }
            else {
                _curWorld.Services.transform = transform;
                _curService = _curWorld.Services;
            }
            _curWorld.DoAwake();

        }

        private void Update() {
            float dt = Time.deltaTime;
            _updateTimer += dt;
            while (_updateTimer > UpdateInterval) {
                _updateTimer -= UpdateInterval;
                Profiler.BeginSample("Update Frame=" +_curService.Frame );
                _curWorld.Update(UpdateInterval);
                Profiler.EndSample();
            }
        }

        private void OnDestroy() {
            _curWorld.DoDestroy();
        }
        
        

    }
}