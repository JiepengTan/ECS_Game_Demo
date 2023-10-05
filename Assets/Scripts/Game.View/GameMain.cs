using System;
using System.Collections.Generic;
using GamesTan.ECS.Game;
using NUnit.Framework;
using UnityEngine;

namespace GamesTan.Game.View {
    public unsafe class GameMain : MonoBehaviour {
        public float UpdateInterval = 0.03f;
        private float _updateTimer;

        private GameEcsWorld _curWorld;
        [SerializeField] private GameServices _curService;

        public void Start() {
            _updateTimer = 0;
            _curWorld = new GameEcsWorld();
            _curWorld.DoAwake();
            _curWorld.Services.transform = transform;
            _curService = _curWorld.Services;
        }

        private void Update() {
            float dt = Time.deltaTime;
            _updateTimer += dt;
            while (_updateTimer > UpdateInterval) {
                _updateTimer -= UpdateInterval;
                _curWorld.Update(UpdateInterval);
            }
        }

        private void OnDestroy() {
            _curWorld.DoDestroy();
        }
    }
}